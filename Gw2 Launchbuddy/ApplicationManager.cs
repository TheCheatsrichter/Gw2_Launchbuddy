namespace Gw2_Launchbuddy
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Security.Cryptography;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using log4net;

    public class ProAccBinding
    {
        public Process pro { set; get; }
        public MainWindow.Account acc { set; get; }

        public ProAccBinding(Process tpro, MainWindow.Account tacc = null)
        {
            this.pro = tpro;
            this.acc = tacc;
        }
    }

    static class LaunchManager
    {
        private static ILog Log { get; } = LogManager.GetLogger(typeof(LaunchManager));

        private static List<int> nomutexpros = new List<int>();
        public static async Task launch_click()
        {
            //Checking for existing Gw2 instances. Do not continue until closed.
            //if (Process.GetProcesses().ToList().Where(a => !nomutexpros.Contains(a.Id) && a.ProcessName == Regex.Replace(exename, @"\.exe(?=[^.]*$)", "", RegexOptions.IgnoreCase)).Any())
            if (!checkRegClients())
            {
                MessageBox.Show("At least one instance of Guild Wars is running that was not opened by LaunchBuddy. That instance will need to be closed.");
                return;
                //HandleManager.ClearMutex(exename, "AN-Mutex-Window-Guild Wars 2", ref nomutexpros);
            }

            if (Gw2_Launchbuddy.Properties.Settings.Default.useinstancegui)
            {
                if (Globals.Appmanager != null && Globals.Appmanager.Visibility == Visibility.Visible)
                {
                    Globals.Appmanager.Topmost = true;
                    Globals.Appmanager.Focus();
                    Globals.Appmanager.WindowState = WindowState.Normal;
                }
                else
                {
                    Globals.Appmanager.Show();
                }

            }

            //Launching the application with arguments
            if (Globals.selected_accs.Count > 0)
            {
                for (int i = 0; i <= Globals.selected_accs.Count - 1; i++) await launchgw2(i, i * 1000);
            }
            else
            {
                await launchgw2();
            }

            GFXManager.RestoreDefault();

            //Launching AddOns
            try
            {
                AddOnManager.LaunchAll();
            }
            catch (Exception e) // logged
            {
                Log.Error("Unable to launch add ons", e);
                MessageBox.Show($"Add-Ons are causing problems. We were unable to launch at least one of them. See log for more info.\nMore Technical information:\n{e}");
            }
        }
        static bool checkRegClients()
        {
            return RegClients();
        }
        static bool updateRegClients(string created)
        {
            return RegClients(created);
        }
        static bool RegClients(string created = null)
        {
            var key = Globals.LBRegKey;
            List<string> listClients = new List<string>();
            try
            {
                listClients = ((string[])key.GetValue("Clients")).ToList();
            }
            catch (Exception e) // logged
            {
                Log.Error($"Unable to retrieve list of clients from registry key {key}", e);
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Reg key likely does not exist: " + e.Message);
#endif
            }
            var gw2Procs = Process.GetProcesses().ToList().Where(a => a.ProcessName == Regex.Replace(Globals.exename, @"\.exe(?=[^.]*$)", "", RegexOptions.IgnoreCase)).ToList().ConvertAll<string>(new Converter<Process, string>(procMD5));
            var running = gw2Procs.Count();
            var temp = listClients.Where(a => !gw2Procs.Contains(a)).ToList();
            foreach (var t in temp) listClients.Remove(t);
            if (created != null) listClients.Add(created);
            key.SetValue("Clients", listClients.ToArray(), Microsoft.Win32.RegistryValueKind.MultiString);
            var logged = listClients.Count();
            key.Close();
            return running == logged;
        }

        static async Task launchgw2(int? accnr = null, int delay = 0)
        {
            try
            {
                if (delay > 0) await Task.Delay(delay);
                ProcessStartInfo gw2proinfo = new ProcessStartInfo();
                gw2proinfo.FileName = Globals.exepath + Globals.exename;
                gw2proinfo.Arguments = Globals.args.Print(accnr);
                gw2proinfo.WorkingDirectory = Globals.exepath;
                Process gw2pro = new Process { StartInfo = gw2proinfo };
                if (accnr != null)
                {
                    Globals.LinkedAccs.Add(new ProAccBinding(gw2pro, Globals.selected_accs[(int)accnr]));
                    GFXManager.UseGFX(Globals.selected_accs[(int)accnr].Configpath);
                }
                else
                {
                    MainWindow.Account undefacc = new MainWindow.Account { Email = "-", Nick = "Acc Nr" + Globals.LinkedAccs.Count };
                    Globals.LinkedAccs.Add(new ProAccBinding(gw2pro, undefacc));
                }
                try
                {
                    gw2pro.Start();
                }
                catch (Exception err) // logged
                {
                    Log.Error($"Unable to launch GW2. Process info: (Filename:{gw2pro.StartInfo.FileName}|Working Directory:{gw2pro.StartInfo.WorkingDirectory}). Arguments not logged for security reasons.", err);
                    MessageBox.Show($"Guild Wars 2 does not seem to be working. Maybe the path is wrong?\nMore technical information:\n{err}");
                    throw;
                }
                var mutexName = "AN-Mutex-Window-Guild Wars 2";
                var exProcIDsString =
                    nomutexpros == null
                    ? string.Empty
                    : string.Join(",", nomutexpros.Select(id => id.ToString().ToArray()));
                var processStartInfoString = $"(Proc:{Globals.exename}|Mutex name:{mutexName}|excProcIDs:{exProcIDsString})";
                try
                {
                    HandleManager.ClearMutex(Globals.exename, mutexName, ref nomutexpros);
                }
                catch (Exception err) // logged
                {
                    Log.Error($"Unable to clear Mutex. {processStartInfoString}", err);
                    MessageBox.Show($"Something went wrong while we tried to trick GW2 into allowing a second instance. Bummer.\nMore technical information:\n{err}");
                    throw;
                }
                try
                {
                    gw2pro.WaitForInputIdle(10000);
                }
                catch (Exception err) // logged
                {
                    Log.Error($"Error while waiting for idle input from GW2 process.", err);
                    MessageBox.Show($"Guild Wars 2 seems to be stuck. Are you on a very old computer?\nMore technical information:\n{err}");
                    throw;
                }
                string processMD5String = null;
                try
                {
                    processMD5String = procMD5(gw2pro);
                }
                catch (Exception e) // logged
                {
                    Log.Error($"Unable to get process MD5. Process Start Info: {processStartInfoString}", e);
                    MessageBox.Show($"Something went wrong trying to get more info from the GW2 process. What is happening, dude?\nMore technical information:\n{e}");
                    throw;
                }
                try
                {
                    //Register the new client to prevent problems.
                    updateRegClients(procMD5(gw2pro));
                }
                catch (Exception err) // logged
                {
                    Log.Error($"Unable to register the new client. Process MD5: {processMD5String}", err);
                    MessageBox.Show($"Something went sideways while we were trying to prevent future issues. Isn't that ironic?\nMore technical information:\n{err}");
                    throw;
                }
                Thread.Sleep(3000);
                if (Properties.Settings.Default.use_reshade)
                {
                    var reshadeUnlockerProcess = GetReshadeUnlockerProcess();
                    var reshadeUnlockerStartInfo = $"(Filename:{reshadeUnlockerProcess.FileName}|Working Directory:{reshadeUnlockerProcess.WorkingDirectory})";
                    try
                    {
                        Process.Start(reshadeUnlockerProcess);
                    }
                    catch (Exception err) // logged
                    {
                        Log.Error($"Unable to register the new client. Process MD5: {processMD5String}", err);
                        MessageBox.Show($"Reshade Unlocker does not seem to be working. Maybe the path is wrong?\nMore technical information:\n{err}");
                        throw;
                    }
                }
            }
            catch // logged
            {
                MessageBox.Show("Something went wrong trying to launch Gw2. See log file for more information.");
            }
        }

        private static ProcessStartInfo GetReshadeUnlockerProcess()
        {
            var reshadeUnlockerProcess = new ProcessStartInfo();
            reshadeUnlockerProcess.FileName = Globals.unlockerpath;
            reshadeUnlockerProcess.WorkingDirectory = Path.GetDirectoryName(Globals.unlockerpath);
            return reshadeUnlockerProcess;
        }

        public static string CalculateMD5(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString();
        }

        public static string procMD5(Process proc)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Start: " + proc.StartTime + " ID: " + proc.Id + " MD5: " + CalculateMD5(proc.StartTime.ToString() + proc.Id.ToString()));
#endif
            return CalculateMD5(proc.StartTime.ToString() + proc.Id.ToString());
        }
    }
}
