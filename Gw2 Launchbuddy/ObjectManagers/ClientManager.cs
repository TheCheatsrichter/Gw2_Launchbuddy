﻿using Gw2_Launchbuddy.Interfaces;
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
            if (!ActiveClients.Contains(client) && (client.Status >= ActiveStatus_Threshold))
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
                    MessageBox.Show("A Guild Wars 2 instance has been found which was not launched by Launchbuddy.\n Please close the application to avoid unexpected behavior.");
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
                    
                    Process pro;
                    try
                    {
                        pro = Process.GetProcessById(Int32.Parse(line.Split(',')[2]));
                    }
                    catch
                    {
                        continue;
                    }
                    
                    if (pro != null)
                    {
                        if (pro.StartTime.ToString() == line.Split(',')[3])
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
        public Process Process = new Process();
        public event EventHandler StatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        //public ObservableCollection<string> HotkeyCommands { get { return GetCommands(typeof(Client)); } }

        public Account Account { set {} get { return account; } }

        protected virtual void OnStatusChanged(EventArgs e)
        {
            OnPropertyChanged("StatusToIcon");
#if DEBUG
            Console.WriteLine("Account: " + account + " Status: " + Status);
#endif
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
            Running = 0x01 << 4,
            Closed = 0x01 << 5,
            Crash= 0x01 << 6
        };

        public ClientStatus Status
        {
            set
            {
                status = status | value;
                if (value == ClientStatus.None || value == ClientStatus.Closed)
                    status = ClientStatus.None;
                OnStatusChanged(EventArgs.Empty);
            }
            get { return status; }
        }

        public void SetProcess(Process pro, ClientStatus prostatus)
        {
            Process = pro;
            Process.EnableRaisingEvents = true;
            Process.Exited += OnClientClose;
            status = status | prostatus;
            if (prostatus == ClientStatus.None || prostatus == ClientStatus.Closed)
                status = ClientStatus.None;
        }

        public Client(Account acc)
        {
            account = acc;
            Process.Exited += OnClientClose;
            ClientManager.Add(this);
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
            ShowWindow(Process.MainWindowHandle,0x6);
        }

        public void Maximize()
        {
            ShowWindow(Process.MainWindowHandle, 0x3);
        }

        public void Resume()
        {
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
        }

        private void ConfigureProcess()
        {
            Process.EnableRaisingEvents = true;
            try { Process.Exited -= OnClientClose; } catch { };
            Process.Exited += OnClientClose;
            string args = "";
            foreach (Argument arg in account.Settings.Arguments.GetActive())
            {
                args += arg.ToString() + " ";
            }


            //Add Login Credentials
            if (account.Settings.HasLoginCredentials)
            {
                args += "-autologin ";
                if (!Properties.Settings.Default.datlaunching)
                {
                    args += "-shareArchive ";
                    args += "-nopatchui ";
                    args += "-email \"" + account.Settings.Email + "\" ";
                    args += "-password \"" + account.Settings.Password + "\" ";
                }
            }

            //Add Server Options
            if (ServerManager.SelectedAssetserver != null)
                if (ServerManager.SelectedAssetserver.Enabled)
                    args += "-assetserver " + ServerManager.SelectedAssetserver.ToArgument;
            if (ServerManager.SelectedAuthserver != null)
                if (ServerManager.SelectedAuthserver.Enabled)
                    args += "-authserver " + ServerManager.SelectedAuthserver.ToArgument;

            // Deltra Code
            if (Properties.Settings.Default.datlaunching)
            {
                args += "-autologin ";

                try
                {
                    Debug.WriteLine("DeltraCodeLauncing: " + Properties.Settings.Default.datlaunching.ToString());
                    string SymLinkingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\";
                    string target = Path.Combine(SymLinkingPath, "Local.dat");
                    string source = Path.Combine(SymLinkingPath, "Local-" + account.Nickname + ".dat");
                    var stringCommand = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/c mklink {0}{1}{2}", "", "\"" + source + "\"" + " ", "\"" + target + "\"");
                    Debug.WriteLine(stringCommand);

                    if (!File.Exists(target)) {
                        CreateSymLink(source, target);
                    }

                    if(File.Exists(target))
                    {
                        FileInfo DatFile = new FileInfo(target);
                        if (IsFileLocked(DatFile) == false)
                        {
                            if (DatFile.Length == 0)
                            {
                                DatFile.Delete();
                                CreateSymLink(source, target);
                            }
                            else
                            {
                                MessageBox.Show("Dat file isn't a symlink.");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Source + e.Message);
                }
            }

            Process.StartInfo = new ProcessStartInfo { FileName = EnviromentManager.GwClientExePath, Arguments = args };
        }

        private static bool IsFileLocked(FileInfo file) // https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
        {
            if(!File.Exists(file.FullName)) { return false; }
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static void CreateSymLink(string source, string target) // https://github.com/amd989/Symlinker/blob/master/Symlink%20Creator/MainWindow.cs
        {
            try
            {
                // concatenates a pair of "", this is to make folders with spaces to work
                var stringCommand = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/c mklink {0}{1}{2}{3}", "", "", "\"" + target + "\"" + " ", "\"" + source + "\"");
                Debug.WriteLine(stringCommand);
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = stringCommand,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };
                process.ErrorDataReceived += process_ErrorDataReceived;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                process.Close();
                process.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error!");
            }
        }

        private static void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                MessageBox.Show(e.Data, "Error!");
            }
        }
		
		// DeltraCode End

        private void SetProcessPriority()
        {
            if (Account.Settings.ProcessPriority != null | Account.Settings.ProcessPriority != ProcessPriorityClass.Normal)
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

        public void Launch()
        {
            while (Status < ClientStatus.Running)
            {
                try
                {
                    //Check if it crashed/closed in between a step
                    if(Status > ClientStatus.Created)
                        if (!ProcessExists())
                        {
                            MessageBoxResult win = MessageBox.Show("Client " + account.Nickname + " got closed or crashed before a clean Start. Do you want to retry to start this Client?", "Client Retry", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                            Status = ClientStatus.Configured;
                            break;

                        case var expression when (Status < ClientStatus.Created):
							if (!Properties.Settings.Default.datlaunching) { Process.Start(); }
							else { if (File.Exists(@"C:\Users\Harrison\AppData\Roaming\Guild Wars 2\local.dat")) { Process.Start(); } } // Need to add some sort of error check to see if a .dat file is actually linked here, this is buggy.
                            Suspend();
                            Status = ClientStatus.Created;
                            break;

                        case var expression when (Status < ClientStatus.Injected):
                            InjectDlls();
                            Resume();
                            Status = ClientStatus.Injected;
                            break;

                        case var expression when (Status < ClientStatus.MutexClosed):
                            if (!Properties.Settings.Default.datlaunching)
                            { CloseMutex(); }
                            Status = ClientStatus.MutexClosed;
                            break;

                        case var expression when (Status < ClientStatus.Running):
                            RestoreGFX();
                            SetProcessPriority();
                            Status = ClientStatus.Running;
                            break;

                        default:
                            //Undeclared Status Close Process and create new one
                            Close();
                            Status = ClientStatus.None;
                            break;
                    }
                    
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    Status = ClientStatus.None;
                }
            }
            try
            {
                Focus();
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
                    return new BitmapImage(new Uri("Resources/Icons/running.png", UriKind.Relative));
                if (Status > ClientStatus.None)
                    return new BitmapImage(new Uri("Resources/Icons/loading.png", UriKind.Relative));
                return new BitmapImage(new Uri("Resources/Icons/idle.png", UriKind.Relative));
            }
            set
            {
                StatusToIcon = value;
            }
        }
    }
}