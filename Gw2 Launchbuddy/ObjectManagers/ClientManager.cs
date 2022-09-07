using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Gw2_Launchbuddy;
using System.Windows.Threading;
using Gw2_Launchbuddy.Modifiers;
using Gw2_Launchbuddy.Extensions;

namespace Gw2_Launchbuddy.ObjectManagers
{

    public static class ClientManager
    {
        public static Client.ClientStatus ActiveStatus_Threshold = Client.ClientStatus.Running;

        public static ObservableCollection<Client> Clients = new ObservableCollection<Client>();
        public static readonly ObservableCollection<Client> ActiveClients = new ObservableCollection<Client>(); //initialize one instance, get{} would create a new instance every statusupdate (bad for ui)

        public static void Add(Client client)
        {
            Clients.Add(client);
            client.StatusChanged += UpdateActiveList;
        }

        public static void UpdateActiveList(object sender, EventArgs e)
        {
            Client client = sender as Client;

            if(client == null)
            {
                return;
            }

            Console.WriteLine(client.account.Nickname + " status changed to: " + client.Status + " = " + ((int)client.Status).ToString());

            if (ActiveClients.Contains(client) && client.Status < ActiveStatus_Threshold)
            {
                Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                  new Action(() =>
                  {
                      ActiveClients.Remove(client);
                  }));
            }
            if (!ActiveClients.Contains(client) && (client.Status.HasFlag(ActiveStatus_Threshold)))
            {
                Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                  new Action(() =>
                  {
                      ActiveClients.Add(client);
                      SaveActiveClients();
                  }));
            }
        }

        private static void SaveActiveClients()
        {
            //Saving all active clients to a file within the Appdata Lb folder, rather than registry, to minimize errors by privileges
            //Format:"Nickname,Client.Status,Client.Process.Id,Client.Process.StartTime
            if (File.Exists(EnviromentManager.LBActiveClientsPath))
                File.Delete(EnviromentManager.LBActiveClientsPath);
            var filewriter = File.AppendText(EnviromentManager.LBActiveClientsPath);
            foreach (Client client in ActiveClients)
            {
                filewriter.WriteLine(client.account.Nickname + "," + (int)client.Status + "," + client.Process.Id + "," + client.Process.StartTime.ToString());
            }
            filewriter.Close();
        }

        private static bool SearchForeignClients()
        {
            foreach (Process pro in Process.GetProcessesByName(EnviromentManager.GwClientExeNameWithoutExtension))
            {
                if (ActiveClients.Where<Client>(c => c.Process.Id == pro.Id).Count<Client>() == 0)
                {
                    MessageBox.Show("A Guild Wars 2 instance has been found which is not configured for multiboxing.\n Please close the application or wait for the update to be completed to avoid unexpected behavior.");
                    return false;
                }
            }
            return true;
        }

        public static bool ImportActiveClients()
        {
            if (File.Exists(EnviromentManager.LBActiveClientsPath))
            {
                var lines_client = File.ReadLines(EnviromentManager.LBActiveClientsPath);
                foreach (string line in lines_client)
                {
                    Account acc;
                    try
                    {
                        acc = AccountManager.Accounts.First(a => a.Nickname == line.Split(',')[0]);
                    }
                    catch
                    {
                        continue;
                    }
                    GwGameProcess pro;
                    try
                    {
                        pro = Process.GetProcessById(Int32.Parse(line.Split(',')[2])) as GwGameProcess;
                    }
                    catch
                    {
                        continue;
                    }
                    if (pro != null)
                    {
                        String processStartTime;
                        try
                        {
                            processStartTime = pro.StartTime.ToString();
                        }
                        catch
                        {
                            continue;
                        }
                        if (processStartTime == line.Split(',')[3])
                        {
                            Client.ClientStatus status = (Client.ClientStatus)Int32.Parse(line.Split(',')[1]);
                            acc.Client.SetProcess(pro,status);
                            if (status.HasFlag(Client.ClientStatus.Running)) ActiveClients.Add(acc.Client);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            SearchForeignClients();

            return true;
        }
    }

    public class Client : INotifyPropertyChanged
    {
        public readonly Account account;
        private ClientStatus status = ClientStatus.None;
        public GwGameProcess Process = new GwGameProcess();
        //public Process CoherentUIProcess;  Could be used to improve login timings; needs more testing
        public event EventHandler StatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        //public ObservableCollection<string> HotkeyCommands { get { return GetCommands(typeof(Client)); } }

        public Account Account { set {} get { return account; } }

        protected virtual void OnStatusChanged(EventArgs e)
        {
            account.Settings.ClientStatusChanged();
            OnPropertyChanged("StatusToIcon");
#if DEBUG
            Console.WriteLine("Account: " + account + " Status: " + Status);
#endif
            PluginManager.OnClientStatusChanged(new PluginContracts.EventArguments.ClientStatusEventArgs(account.ID,(PluginContracts.ObjectInterfaces.ClientStatus)Status)); //Plugincall

            EventHandler handler = StatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        [Flags]
        public enum ClientStatus
        {
            None = 0x00,
            Configured = 0x01,
            Created = 0x01 << 1,
            Injected = 0x01 << 2,
            MutexClosed = 0x01 << 3,
            Login = 0x01 <<4,
            Running = 0x01 << 5,
            Closed = 0x01 << 6,
            Crash= 0x01 << 7
        };

        public ClientStatus Status
        {
            set
            {
                status = status | value;
                if (value == ClientStatus.None || value == ClientStatus.Closed)
                { status = ClientStatus.None; }
                OnStatusChanged(EventArgs.Empty);
            }
            get { return status; }
        }

        public void SetProcess(GwGameProcess pro, ClientStatus prostatus)
        {
            Process = pro;
            try
            {
                if (pro == null) return;
                if (pro.HasExited) return;
                Process.EnableRaisingEvents = true;
                Process.Exited += OnClientClose;
                status = status | prostatus;
                if (prostatus == ClientStatus.None || prostatus == ClientStatus.Closed)
                    status = ClientStatus.None;
            }
            catch
            {
                status = ClientStatus.Crash;
            }

        }

        public Client(Account acc)
        {
            account = acc;
            Process.Exited += OnClientClose;
            ClientManager.Add(this);
        }

        private bool ProcessIsClosed()
        {
            if (Process.HasExited)
            {
                this.status = ClientStatus.None;
                return true;
            }
            return false;
        }

        public void Close()
        {
            //May need more gracefully close function
            if(Status> ClientStatus.Created)
            {
                Process.Kill();
                Account.Settings.RelaunchesLeft = 0;
                Status = ClientStatus.Closed;
            }
        }

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void Suspend()
        {
            if (ProcessIsClosed()) return;

            foreach (ProcessThread pT in Process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public void Minimize()
        {
            if (ProcessIsClosed()) return;
            Process.Refresh();
            ShowWindow(Process.MainWindowHandle,0x6);
        }

        public void Maximize()
        {
            if (ProcessIsClosed()) return;
            Process.Refresh();
            ShowWindow(Process.MainWindowHandle, 0x3);
        }

        public void Resume()
        {
            if (ProcessIsClosed()) return;
            foreach (ProcessThread pT in Process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                uint suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public void Focus()
        {
            if (ProcessIsClosed()) return;
            Process.Refresh();
            IntPtr hwndMain = Process.MainWindowHandle;
            SetForegroundWindow(hwndMain);
        }

        protected virtual void OnClientClose(object sender, EventArgs e)
        {
            Process pro = sender as Process;
            Status = ClientStatus.Closed;
            if (Account.Settings.RelaunchesLeft > 0)
            {
                Account.Settings.RelaunchesLeft--;
                Launch();
            }
            account.Settings.AccountInformation.SetLastClose();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        public void Window_MoveTo_Default()
        {
            WindowConfig config = account.Settings.WinConfig;
            Window_Move(config.WinPos_X,config.WinPos_Y);
        }

        public void Window_ScaleTo_Default()
        {
            WindowConfig config = account.Settings.WinConfig;
            Window_Scale(config.Win_Width,config.Win_Height);
        }

        public void Window_Move(int posx,int posy)
        {
            if (ProcessIsClosed()) return;
            Process.Refresh();
            IntPtr handle = Process.MainWindowHandle;
            RECT Rect = new RECT();
            if (GetWindowRect(handle, ref Rect))
                MoveWindow(handle, posx, posy, Rect.right - Rect.left,Rect.bottom-Rect.top,true);
        }

        public void Window_Scale(int width,int height)
        {
            if (ProcessIsClosed()) return;
            Process.Refresh();
            IntPtr handle = Process.MainWindowHandle;
            RECT Rect = new RECT();
            if (GetWindowRect(handle, ref Rect))
                MoveWindow(handle, Rect.left ,Rect.top, width, height, true);
        }

        private void Window_Init()
        {
            new Thread (Th_Window_Init).Start();
        }

        private async void Th_Window_Init()
        {
            // icm32.dll
            await Process.WaitForStateAsynch(GwGameProcess.GameStatus.game_charscreen);
            Console.WriteLine($"{account.Nickname} configuring Windowsize");

            Window_MoveTo_Default();
            Window_ScaleTo_Default();

            int i = 0;
            while (i<3 && account.Settings.WinConfig.IsConfigured(Process)!=true)
            {
                try
                {
                    Window_MoveTo_Default();
                    Window_ScaleTo_Default();
                }
                catch { }

                Thread.Sleep(500);
                i++;
            }


        }

        private void ConfigureProcess()
        {

            if (Process == null) Process = new GwGameProcess();
            Process.EnableRaisingEvents = true;
            try { Process.Exited -= OnClientClose; } catch { };
            Process.Exited += OnClientClose;

            string args = "";


            try
            {
                Argument dx9arg = account.Settings.Arguments.FirstOrDefault(x => x.ToString() == "-dx9");
                Argument dx11arg = account.Settings.Arguments.FirstOrDefault(x => x.ToString() == "-dx11");

                if (dx9arg.IsActive && dx11arg.IsActive)
                {
                    MessageBox.Show($"On account {account.Nickname} both DirectX 9 and DirectX 11 was enabled. Please disable one of both options in the future. This setting got reset to DirectX 9 for this account.");
                    dx11arg.IsActive = false;
                }
            }catch
            {
                Console.WriteLine("Could not fetch direct X settings");
            }


            try
            {
                foreach (Argument arg in account.Settings.Arguments.GetActive())
                {
                    args += arg.ToString() + " ";
                }
            }
            catch
            {
                Console.WriteLine("Account arguments corrupted");
            }

            args += "-shareArchive ";
            account.CustomMumbleLink = false;

            if (ClientManager.ActiveClients.Count != 0 || account.Settings.AlwaysUseCustomMumbleLink)
            {
                args += $"-mumble GW2MumbleLink{account.ID} ";
                account.CustomMumbleLink = true;
            }


            if (account.Settings.WinConfig != null && !args.Contains("-windowed")) args += "-windowed ";

            //Add Login Credentials
            /*

            if (account.Settings.HasLoginCredentials)
            {
                args += "-nopatchui -autologin ";
                args += "-email \"" + account.Settings.Email+"\" ";
                args += "-password \"" + account.Settings.Password + "\" ";
            }
            */

            /*
            if(account.Settings.Loginfile!=null && account.Settings.Loginfile.Gw2Build==EnviromentManager.GwClientVersion)
            {
                args += "-autologin ";
            }
            */

            //Add Server Options
            if (ServerManager.SelectedAssetserver != null)
                if(ServerManager.SelectedAssetserver.Enabled)
                args += "-assetsrv " + ServerManager.SelectedAssetserver.ToArgument;
            if (ServerManager.SelectedAuthserver != null)
                if (ServerManager.SelectedAuthserver.Enabled)
                    args += "-authsrv " + ServerManager.SelectedAuthserver.ToArgument;
            if(ServerManager.clientport!=null)
            {
                args += "-clientport " + ServerManager.clientport;
            }

            Process.StartInfo = new ProcessStartInfo { FileName = EnviromentManager.GwClientExePath, Arguments=args };
        }

        private void SetProcessPriority()
        {
            if (Account.Settings.ProcessPriority != ProcessPriorityClass.Normal)
            {
                try
                {
                    Process.PriorityClass = Account.Settings.ProcessPriority;
                }
                catch
                {
                    MessageBox.Show("Could not set Process priority");
                }
            }
        }

        private void SwapLocalDat()
        {
            if (account.Settings.Loginfile != null)
            {
                LocalDatManager.Apply(account.Settings.Loginfile);
            }else
            {
                LocalDatManager.ApplyNullLoginfile(); //Create symbolic link to local dat itself --> closes file handle on gamestart
            }

        }

        private void CloseMutex()
        {
            for (var i = 0; i < 10; i++)
            {
#if DEBUG
                System.Diagnostics.Debug.Print("Mutex Kill Attempt Nr" + i);
#endif
                try
                {
                    //if (HandleManager.ClearMutex(ClientManager.ClientInfo.ProcessName, "AN-Mutex-Window-Guild Wars 2", ref nomutexpros)) i = 10;
                    if (HandleManager.KillHandle(Process, "AN-Mutex-Window-Guild Wars 2", false)) i = 10;
                }
                catch (Exception err)
                {
                    MessageBox.Show("Mutex release failed, will try again. Please provide the following if you want to help fix this problem: \r\n" + err.GetType().ToString() + "\r\n" + err.Message + "\r\n" + err.StackTrace);
                }

                //Maxtime 10 secs
                Thread.Sleep((int)(Math.Pow(i, 2) * 25 + 50));

            }
        }

        private void SwapGFX()
        {
            if(account.Settings.GFXFile!=null)
            {
                GFXManager.UseGFX(account.Settings.GFXFile);
            }
        }
        private void RestoreGFX()
        {
            GFXManager.RestoreDefault();
        }

        private void InjectDlls()
        {
            foreach(string dll in account.Settings.DLLs)
            {
                DllInjector.Inject((uint)Process.Id,dll);
            }
        }

        public void Inject(string dllname)
        {
            DllInjector.Inject((uint)Process.Id, dllname);
        }

        private bool ProcessExists()
        {
            try
            {
                object res = System.Diagnostics.Process.GetProcessById(Process.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void FillLogin()
        {
            if(account.Settings.Email!=null && account.Settings.Password!=null)
            {
                Loginfiller.Login(account.Settings.Email, account.Settings.Password, this.Process,true);
            }
        }

        private void PressLoginButton()
        {
            //Wait between clients

            int timetowait = 0;

            //Get all accounts that were active in the last 4 hours

            int active_accounts = AccountManager.Accounts.Count(x => x.Settings.AccountInformation.HadLoginInPastMinutes(240) == true);

            switch (active_accounts)
            {
                case int _ when active_accounts >= 36:
                    timetowait = 120 * 1000;
                    break;

                case int _ when active_accounts >= 31:
                    timetowait = 60 * 1000;
                    break;

                case int _ when active_accounts >= 21:
                    timetowait = 40 * 1000;
                    break;

                case int _ when active_accounts >= 13:
                    timetowait = 20 * 1000;
                    break;

                default:
                    timetowait = 1800 + (active_accounts * active_accounts * 80);
                    break;
            }

            if(LBConfiguration.Config.uselogindelays)
            {
                Action loginwait = () => { Thread.Sleep(timetowait); };
                Helpers.BlockerInfo.Run("Delaying Login", $"Launchbuddy is currently delaying the login for {timetowait / 1000} sec(s). This is a safety measure to avoid triggering GW2 DDOS protection. Press cancel to skip.", loginwait, false);
            }
 
            Loginfiller.PressLoginButton(Account);
        }

        private void WaitForLogin()
        {
            Action blockefunc = () => Process.WaitForState(GwGameProcess.GameStatus.loginwindow_authentication);
            Helpers.BlockerInfo.Run($"{account.Nickname} Login pending", $"{account.Nickname} Login requiring additional information.", blockefunc);
        }

        public async void Launch()
        {

            try
            {

                while (Status < ClientStatus.Running)
                {

                    //Check if it crashed/closed in between a step
                    if (!Process.IsRunning && Status > ClientStatus.Created)
                    {
                        try
                        {
                            Process.Stop();
                        }
                        catch { }

                        RestoreGameSettings();
                        MessageBoxResult win = MessageBox.Show($"Client {account.Nickname} got closed or crashed before a clean Start (Errorcode: {Convert.ToString((int)Status, 16)}). Do you want to retry to start this Client?", "Client Retry", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (win.ToString() == "Yes")
                        {
                            Status = ClientStatus.None;
                        }
                        else
                        {
                            Account.Settings.RelaunchesLeft = 0;
                            Status = ClientStatus.Crash;
                        }
                    }


                    switch (Status)
                    {
                        case var expression when (Status < ClientStatus.Configured):
                            ConfigureProcess();
                            SwapGFX();
                            SwapLocalDat();
                            Status = ClientStatus.Configured;
                            break;

                        case var expression when (Status < ClientStatus.Created):
                            Process.Start();
                            Status = ClientStatus.Created;
                            break;

                        case var expression when (Status < ClientStatus.Injected):
                            Process.WaitForState(GwGameProcess.GameStatus.loginwindow_prelogin);
                            InjectDlls();
                            Status = ClientStatus.Injected;
                            break;

                        case var expression when (Status < ClientStatus.MutexClosed):
                            CloseMutex();
                            Status = ClientStatus.MutexClosed;
                            break;

                        case var expression when (Status < ClientStatus.Login):
                            if(!Process.WaitForState(GwGameProcess.GameStatus.loginwindow_prelogin))
                            {
                                Status = ClientStatus.Crash;
                            }

                            if (Account.Settings.Loginfile != null)
                            {
                                if(LBConfiguration.Config.pushgameclientbuttons) PressLoginButton();
                                LocalDatManager.ToDefault(account.Settings.Loginfile);
                            }else
                            {
                                LocalDatManager.ToDefault(null); // Reset symbolic link
                            }
                            
                            Status = ClientStatus.Login;
                            break;

                        case var expression when (Status < ClientStatus.Running):
                            RestoreGFX();
                            SetProcessPriority();
                            Status = ClientStatus.Running;
                            try { Focus(); } catch { }
                            try {if(account.Settings.WinConfig!=null)new Thread(Window_Init).Start();} catch { }
                            account.Settings.AccountInformation.SetLastLogin();

                            // Launch TacO & BlisH
                            try
                            {
                                if (account.Settings.StartBlish) LBBlish.LaunchBlishInstance(account);
                                if (account.Settings.StartTaco) LBTacO.LaunchTacoInstance(account);
                            } catch { }
                            
                            break;
                    }
                }

            }
            catch (Exception e)
            {
                RestoreGameSettings();
                MessageBox.Show("Account: "+account.Nickname+"\n\n"+ e.Message);
                Status = ClientStatus.None;
                CrashReporter.ReportCrashToAll(e);
            }
            if (Status.HasFlag(ClientStatus.Crash)) Status = ClientStatus.None;
        }

        void RestoreGameSettings()
        {
            try
            {
                GFXManager.RestoreDefault();
            }
            catch
            {

            }

            try
            {
                LocalDatManager.ToDefault(account.Settings.Loginfile);
            }
            catch
            {

            }
        }

        //UI functions
        public BitmapImage StatusToIcon
        {
            get
            {
                if (Status >= ClientStatus.Running)
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/running.png", UriKind.RelativeOrAbsolute));
                if (Status > ClientStatus.None)
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/loading.png", UriKind.RelativeOrAbsolute));
                return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/idle.png", UriKind.RelativeOrAbsolute));
            }
            set
            {
                StatusToIcon = value;
            }
        }
    }
}