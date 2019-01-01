using Gw2_Launchbuddy.Interfaces;
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
        private static string filepath = "gw2lb_ac.txt"; //Change this to Appdata Lb folder
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
                  }));
                //SaveActiveClients();
            }          
        }

        private static void SaveActiveClients()
        {
            //Saving all active clients to a file within the Appdata Lb folder, rather than registry, to minimize errors by privileges
            //Format:"Nickname,Client.Status,Client.Process.Id,Client.Process.StartTime
            if (File.Exists(filepath))
                File.Delete(filepath);
            var filewriter = File.AppendText(filepath);
            foreach (Client client in ActiveClients)
            {
                filewriter.WriteLine(client.account.Nickname + "," + (int)client.Status + "," + client.Process.Id + "," + client.Process.StartTime.ToString());
            }
            filewriter.Close();
        }

        public static bool ImportActiveClients()
        {
            if (File.Exists(filepath))
            {
                var lines_client = File.ReadLines(filepath);
                foreach (string line in lines_client)
                {
                    Account acc = AccountManager.Accounts.First(a => a.Nickname == line.Split(',')[0]);
                    Process pro = Process.GetProcessById(Int32.Parse(line.Split(',')[2]));
                    if (pro != null)
                    {
                        if (pro.StartTime.ToString() == line.Split(',')[3])
                        {
                            acc.Client.Process = pro;
                        }
                        else
                        {
                            continue;
                        }
                        acc.Client.Status = (Client.ClientStatus)Int32.Parse(line.Split(',')[1]);
                    }
                }
            }
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
            Closed = 0x01 << 5
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

        public Client(Account acc)
        {
            account = acc;
            Process.Exited += OnClientClose;
            ClientManager.Add(this);
        }

        public void Close()
        {
            //May need more gracefully close function
            Process.Kill();
        }


        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);

        public void Suspend()
        {
            SuspendThread(Process.Handle);
        }

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        public void Resume()
        {
            ResumeThread(Process.Handle);
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
            if (pro == this.Process)
                this.Status = ClientStatus.Closed;
        }

        private void ConfigureProcess()
        {
            Process.EnableRaisingEvents = true;
            Process.Exited += OnClientClose;
            string args = "";
            foreach (Argument arg in account.Settings.Arguments.GetActive())
            {
                args += arg.ToString() + " ";
            }
            args += "-shareArchive ";
            
            //Add Login Credentials
            if (account.Settings.Email!=null && account.Settings.Password!=null)
            {
                args += "-nopatchui -autologin ";
                args += "-email " + account.Settings.Email+" ";
                args += "-password " + account.Settings.Password + " ";
            }

            //Add Server Options
            if (ServerManager.SelectedAssetserver != null)
                if(ServerManager.SelectedAssetserver.Enabled)
                args += "-assetserver " + ServerManager.SelectedAssetserver.ToArgument;
            if (ServerManager.SelectedAuthserver != null)
                if (ServerManager.SelectedAuthserver.Enabled)
                    args += "-authserver " + ServerManager.SelectedAuthserver.ToArgument;

            Process.StartInfo = new ProcessStartInfo { FileName = EnviromentManager.GwClientExePath, Arguments=args };
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
                MessageBox.Show("Client " + account.Nickname + " got closed or crashed before a clean Start. Retry started!");
                return false;
            }
        }

        public void Launch()
        {
            while (Status < ClientStatus.Running)
            {
                try
                {
                    if(Status > ClientStatus.Created)
                        if (!ProcessExists()) Status = ClientStatus.None;
                    switch (Status)
                    {
                        case var expression when (Status < ClientStatus.Configured):
                            ConfigureProcess();
                            SwapGFX();
                            Status = ClientStatus.Configured;
                            break;

                        case var expression when (Status < ClientStatus.Created):
                            Process.Start();
                            Suspend();
                            Status = ClientStatus.Created;
                            break;

                        case var expression when (Status < ClientStatus.Injected):
                            InjectDlls();
                            Status = ClientStatus.Injected;
                            break;

                        case var expression when (Status < ClientStatus.MutexClosed):
                            CloseMutex();
                            Status = ClientStatus.MutexClosed;
                            break;

                        case var expression when (Status < ClientStatus.Running):
                            RestoreGFX();
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