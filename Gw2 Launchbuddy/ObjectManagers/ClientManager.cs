using Gw2_Launchbuddy.Interfaces;
using Gw2_Launchbuddy.Wrappers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;

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
            if (ActiveClients.Contains(client) && !(client.Status < ActiveStatus_Threshold))
            {
                ActiveClients.Remove(client);
            }
            if (!ActiveClients.Contains(client) && (client.Status >= ActiveStatus_Threshold))
            {
                ActiveClients.Add(client);
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

    public class Client
    {
        public readonly Account account;
        private ClientStatus status = ClientStatus.None;
        public Process Process = new Process();

        public event EventHandler StatusChanged;

        protected virtual void OnStatusChanged(EventArgs e)
        {
#if DEBUG
            Console.WriteLine("Account: " + account + " Status: " + Status);
#endif
            EventHandler handler = StatusChanged;
            if (handler != null)
            {
                handler(this, e);
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
            if (Process.Responding)
                Process.Close();
            else
            {
                Resume();
                Process.Close();
            }
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
            //Add Arguments here
            Process.StartInfo = new ProcessStartInfo { FileName = EnviromentManager.GwClientExePath, Arguments="-shareArchive" };
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

        public void Launch()
        {
            while (Status < ClientStatus.Running)
            {
                switch (Status)
                {
                    case var expression when (Status < ClientStatus.Configured):
                        ConfigureProcess();
                        Status = ClientStatus.Configured;
                        break;

                    case var expression when (Status < ClientStatus.Created):
                        Process.Start();
                        Suspend();
                        Status = ClientStatus.Created;
                        break;

                    case var expression when (Status < ClientStatus.Injected):
                        Status = ClientStatus.Injected;
                        break;

                    case var expression when (Status < ClientStatus.MutexClosed):
                        CloseMutex();
                        Status = ClientStatus.MutexClosed;
                        break;

                    case var expression when (Status < ClientStatus.Running):
                        Status = ClientStatus.Running;
                        break;

                    default:
                        //Undeclared Status Close Process and create new one
                        Close();
                        Status = ClientStatus.None;
                        break;
                }
            }
            Focus();
        }
    }

    /*
    public static class ClientManager
    {
        private static ObservableCollection<Client> clientCollection { get; set; }
        public static ReadOnlyObservableCollection<Client> ClientCollection { get; set; }

        static ClientManager()
        {
            clientCollection = new ObservableCollection<Client>();
            ClientCollection = new ReadOnlyObservableCollection<Client>(clientCollection);
        }

        public static Client CreateClient()
        {
            var createdClient = new Client();
            clientCollection.Add(createdClient);
            return createdClient;
        }

        public static class ClientInfo
        {
            //Info about the client
            private static string executable;

            private static bool? _IsUpToDate;
            public static bool IsUpToDate
            {
                get
                {
                    if (!_IsUpToDate.HasValue && Api.Online)
                    {
                        _IsUpToDate = Api.ClientBuild == Version;
                    }
                    return _IsUpToDate.Value;
                }
            }
            public static string InstallPath { get; set; }
            public static string Executable { get { return executable; } set { executable = value; ProcessName = Regex.Replace(value, @"\.exe(?=$)", "", RegexOptions.IgnoreCase); } }
            public static string FullPath { get { return InstallPath + Executable; } }
            public static string ProcessName { get; private set; }
            public static string Version { get; private set; }

            public static void LoadClientInfo()
            {
                //Find the newest XML file in APPDATA (the XML files share the same name as their XML files -> multiple .xml files possible!)
                string[] configfiles = new string[] { };
                try
                {
                    configfiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\", "*.exe.xml");
                }
                catch (Exception e)
                {
                    //TODO: Handle corrupt/missing Guild Wars install
                    MessageBox.Show("Guild Wars may not be installed. \n " + e.Message);
                    return;
                }
                EnviromentManager.GwClientXmlPath = "";
                long max = 0;

                foreach (string config in configfiles)
                {
                    if (System.IO.File.GetLastWriteTime(config).Ticks > max)
                    {
                        max = System.IO.File.GetLastWriteTime(config).Ticks;
                        EnviromentManager.GwClientXmlPath = config;
                    }
                }

                //Read the GFX Settings
                Globals.SelectedGFX = GFXManager.ReadFile(EnviromentManager.GwClientXmlPath);
                //lv_gfx.ItemsSource = Globals.SelectedGFX.Config;
                //lv_gfx.Items.Refresh();

                // Read the XML file
                try
                {
                    //if (Properties.Settings.Default.use_reshade) cb_reshade.IsChecked = true;

                    StreamReader stream = new System.IO.StreamReader(EnviromentManager.GwClientXmlPath);
                    XmlTextReader reader = null;
                    reader = new XmlTextReader(stream);

                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "VERSIONNAME":
                                Regex filter = new Regex(@"\d*\d");
                                Version = filter.Match(reader.GetValue()).Value;
                                //lab_version.Content = "Client Version: " + ClientManager.ClientInfo.Version;
                                break;

                            case "INSTALLPATH":
                                InstallPath = reader.GetValue();
                                //lab_path.Content = "Install Path: " + EnviromentManager.GwClientPath;
                                break;

                            case "EXECUTABLE":
                                Executable = reader.GetValue();
                                //lab_path.Content += EnviromentManager.GwClientExePath;
                                break;

                            case "EXECCMD":
                                //Filter arguments from path
                                //lab_para.Content = "Latest Start Parameters: ";
                                Regex regex = new Regex(@"(?<=^|\s)-(umbra.(\w)*|\w*)");
                                string input = reader.GetValue();
                                MatchCollection matchList = regex.Matches(input);

                                foreach (Argument argument in ArgumentManager.ArgumentCollection)
                                    foreach (Match parameter in matchList)
                                        if (argument.Flag == parameter.Value && !argument.Blocker)
                                            AccountArgumentManager.StopGap.IsSelected(parameter.Value, true);

                                //RefreshUI();
                                break;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Gw2 info file not found! Please choose the directory manually!");
                }
            }
        }

        public static class ClientReg
        {
            static ClientReg()
            {
                UpdateRegClients();
            }

            private static RegistryKey key { get { return Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE").CreateSubKey("LaunchBuddy"); } }

            public static void UpdateRegClients()
            {
                try
                {
                    List<string> listClients = getClientListFromReg();

                    var gw2Proc = Client.GetClients();// Process.GetProcesses().ToList().Where(a => a.ProcessName == ClientInfo.ProcessName).ToList(); //Bad, but processes named same as client executable
                    var md5Gw2Proc = gw2Proc.ToList().Select(a => a.CalculateProcessMD5());
                    foreach (var t in listClients.Where(a => !md5Gw2Proc.Contains(a)).ToList()) listClients.Remove(t);
                    key.SetValue("Clients", listClients.ToArray(), Microsoft.Win32.RegistryValueKind.MultiString);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    key.Close();
                }
            }

            public static bool CheckRegClients()
            {
                try
                {
                    UpdateRegClients();
                    List<string> listClients = getClientListFromReg();

                    var currentProcesses = Client.GetClients();
                    var gw2Procs = currentProcesses.ToList().Select(a => a.CalculateProcessMD5());
                    return gw2Procs.Count() == listClients.Count();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    key.Close();
                }
            }

            public static void RegClient(Client Client)
            {
                try
                {
                    List<string> listClients = getClientListFromReg();

                    listClients.Add(Client.MD5);
                    key.SetValue("Clients", listClients.ToArray(), Microsoft.Win32.RegistryValueKind.MultiString);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    key.Close();
                }
            }

            private static List<string> getClientListFromReg()
            {
                try
                {
                    return ((string[])key.GetValue("Clients")).ToList();
                }
                catch (Exception e)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Reg key likely does not exist: " + e.Message);
#endif
                    return new List<string>();
                }
            }
        }

        public static void UpdateClient()
        {
            // Needs much more:
            //   - Check for other clients running
            //   - Close other clients after prompt
            //   - After first exit wait for new client to start
            //   - Force close new client
            //   - Run -image again so we can wait for update to end
            var client = new Client();
            client.Arguments = "-image";
            client.StartAndWait();
        }
    }

    public class Client : ProcessWrapper, IProcessWrapper<Client>
    {
        public Client()
        {
            StartInfo = new ProcessStartInfo(ClientManager.ClientInfo.FullPath);
            Started += Client_Started;
        }

        public bool Launch()
        {
            //if (EnableRaisingEvents == true) return false;
            EnableRaisingEvents = true;

            var AccountClient = AccountClientManager.AccountClientCollection.Where(a => a.Client == this).Single();

            if (AccountClient.Account != AccountManager.DefaultAccount)
                AccountClient.Account = new Account(AccountClient.Account.Nickname, null, null);

            Globals.LinkedAccs.Add(AccountClient);
           // GFXManager.UseGFX(AccountClient.Account.ConfigurationPath);

            return Start();
        }

        private void Client_Started(object sender, EventArgs e)
        {
            ClientManager.ClientReg.RegClient(this);

            EnableRaisingEvents = true;
            Exited += Client_Exited;
            foreach (var plugin in PluginManager.PluginCollection) plugin.Client_PostLaunch();
        }

        new public bool Start()
        {
            foreach (var plugin in PluginManager.PluginCollection) plugin.Client_PreLaunch();

            try
            {
                return base.StartAndWait();
            }
            catch (Exception err)
            {
                System.Windows.MessageBox.Show("Could not launch Gw2. Invalid path?\n" + err.Message);
                Application.Current.Dispatcher.Invoke(delegate { AccountClientManager.Remove(this); });
                return false;
            }
        }

        private void Client_Exited(object sender, EventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(delegate { AccountClientManager.Remove(this); });
            }
            catch (Exception err) { }
            foreach (var plugin in PluginManager.PluginCollection) plugin.Client_Exit();
            Dispose();
        }

        public static Process[] GetClients()
        {
            return GetProcessesByName(ClientManager.ClientInfo.ProcessName);
            //return temp.Select(a => ToDerivedHelper.ToDerived<Process, Client>(a)).ToArray();
        }
    }
    /*
    public class Client
    {
        public Process Process;
        public string md5;

        public Client()
        {
            Process = new Process() { StartInfo = new ProcessStartInfo(ClientManager.ClientInfo.FullPath) };
        }

        public string Arguments { get => Process.StartInfo.Arguments; set => Process.StartInfo.Arguments = value; }

        public void Start()
        {
            Process.EnableRaisingEvents = true;
            Process.Exited += ClientProcess_Exited;
            Process.Start();
            md5 = ClientManager.CalculateProcessMD5(this.Process);
            ClientManager.ClientReg.RegClient(this);
        }

        public void StartAndWait()
        {
            Start();
            Process.WaitForExit();
        }

        private void ClientProcess_Exited(object sender, EventArgs e)
        {
            Process.Exited -= ClientProcess_Exited;
            Application.Current.Dispatcher.Invoke(delegate { AccountClientManager.Remove(this); });
            Process.Dispose();
        }

        public void Stop()
        {
            try
            {
                if (!Process.CloseMainWindow())
                    Process.Kill();
                if (!Process.WaitForExit(1000))
                    Process.Kill();
            }
            catch { }
        }
    }*/
}