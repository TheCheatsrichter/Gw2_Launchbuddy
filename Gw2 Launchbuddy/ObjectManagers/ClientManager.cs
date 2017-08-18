using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace Gw2_Launchbuddy.ObjectManagers
{
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

        public static string CalculateProcessMD5(Process Process)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(Process.StartTime.ToString() + Process.Id.ToString());
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString();
        }

        public static class ClientInfo
        {
            //Info about the client
            private static string executable;

            public static bool IsUpToDate { get; set; }
            public static string InstallPath { get; set; }
            public static string Executable { get { return executable; } set { executable = value; ProcessName = Regex.Replace(value, @"\.exe(?=$)", "", RegexOptions.IgnoreCase); } }
            public static string FullPath { get { return InstallPath + Executable; } }
            public static string ProcessName { get; private set; }
            public static string Version { get; set; }
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

                    var gw2Proc = Process.GetProcesses().ToList().Where(a => a.ProcessName == ClientInfo.ProcessName).ToList(); //Bad, but processes named same as client executable
                    var md5Gw2Proc = gw2Proc.ConvertAll(new Converter<Process, string>(CalculateProcessMD5));
                    foreach (var t in listClients.Where(a => !md5Gw2Proc.Contains(a)).ToList()) listClients.Remove(t);
                    key.SetValue("Clients", listClients.ToArray(), Microsoft.Win32.RegistryValueKind.MultiString);
                }
                catch (Exception ex)
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

                    var currentProcesses = Process.GetProcesses().ToList().Where(a => a.ProcessName == ClientInfo.ProcessName).ToList();
                    var gw2Procs = currentProcesses.ConvertAll(new Converter<Process, string>(CalculateProcessMD5));
                    return gw2Procs.Count() == listClients.Count();
                }
                catch (Exception ex)
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

                    listClients.Add(Client.md5);
                    key.SetValue("Clients", listClients.ToArray(), Microsoft.Win32.RegistryValueKind.MultiString);
                }
                catch (Exception ex)
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
                    return new List<string>();
#endif
                }
            }
        }
    }

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

        public void SetPriority(ProcessPriorityClass priority)
        {
            Process.PriorityClass = priority;
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
    }
}