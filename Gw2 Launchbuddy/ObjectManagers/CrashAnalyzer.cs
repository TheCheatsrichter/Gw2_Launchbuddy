using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;
using System.Xml.Serialization;
using Gw2_Launchbuddy.ObjectManagers;

namespace Gw2_Launchbuddy
{
    public class CrashFilter
    {
        public string[] keywords;
        public string solution;
        public CrashFilter(string[] Keywords,string Solution)
        {
            keywords = Keywords;
            solution = Solution;
        }
        public int Matchcount(string assertion)
        {
            int matches = 0;
            foreach (string key in keywords)
            {
                matches += Regex.Matches(assertion, key).Count;
            }
            return matches;
        }
    }

    static public class CrashLibrary
    {
        public static List<CrashFilter> CrashFilters = new List<CrashFilter> {
            new CrashFilter(new string[] { "c0000005", "Memory at address","could not be read" }, "mem_read"),
            new CrashFilter(new string[] { "c0000005", "Memory at address","could not be written" }, "mem_write"),
            new CrashFilter(new string[] { "Coherent", "host", "crashed" }, "host_crash"),
            new CrashFilter(new string[] { "Client needs to be patched", "shareArchive", "or noPatch" }, "outdated_client"),
            new CrashFilter(new string[] { "Model", "leaks","detected" }, "model_leaks"),
            new CrashFilter(new string[] { "Raw manifest not found.","Is your archive up to date?","shareArchive","isRelaunch"}, "readonly_write"),
        };

        public static Dictionary<string, string> SolutionInfo = new Dictionary<string, string>
        {
            { "unknown","Cooooo...\nQuaggan doesn't know what to do with this crash. :(" },
            { "host_crash","Cooo!\nQuaggan sees that your launcher crashed. You should download a new version from the Arenanet website!" },
            { "model_leaks","Cooo!\nQuaggan remembers that this crash happened very often when the old 32 Bit Client of Guild Wars 2 was used! You should NOT run the client with the -32 parameter if possible!" },
            { "mem_read","Cooo!\nSeems like a memory read error happended to you!\nQuaggan knows that this sometimes happens when your Gw2.dat file gets corrupted.\nSometimes using the -repair argument will help youuuu!" },
            { "outdated_client","Cooo!\nYour client seems to be outdated and you tried to launch the game with autologin!\n Quaggan would update your client for youu!" },
            { "mem_write","Cooo!\nSeems like a memory write error happended to you!\nQuaggan knows that this sometimes happens when your Gw2.dat file gets corrupted.\nSometimes using the -repair argument will help youuuu!" },
            { "readonly_write","BooOOooo!\nIt looks like your client may have tried to update while in share mode!\nQuaggan needs to close all your clients and update them!" }
        };


        static public string ClassifyCrash(Crashlog log)
        {
            int highestmatch = 0;
            int hm_index=0;
            for (int i = 0; i <= CrashLibrary.CrashFilters.Count - 1; i++)
            {
                if (CrashLibrary.CrashFilters[i].Matchcount(log.Assertion) > highestmatch && CrashLibrary.CrashFilters[i].Matchcount(log.Assertion) != 0)
                {
                    highestmatch = CrashLibrary.CrashFilters[i].Matchcount(log.Assertion);
                    hm_index = i;
                }
            }
            if (highestmatch>1)
            return CrashFilters[hm_index].solution;
            return "unknown";
        }

        public static void ApplySolution(string solutionkey)
        {
            switch (solutionkey)
            {
                case "mem_read":
                    mem_read();
                    break;
                case "mem_write":
                    mem_write();
                    break;
                case "host_crash":
                    host_crash();
                    break;
                case "model_leaks":
                    model_leaks();
                    break;
                case "outdated_client":
                    outdated_client();
                    break;
                case "readonly_write":
                    readonly_write();
                    break;
            }
        }

        //Solutions
        private static void mem_read()
        {
            Unhandled_Launch("-repair");
            System.Windows.Forms.MessageBox.Show("The Guild Wars 2 Launcher is trying to fix the mem_read error!\nPlease wait for completion before you continue.");
        }
        private static void mem_write()
        {
            Unhandled_Launch("-repair");
            System.Windows.Forms.MessageBox.Show("The Guild Wars 2 Launcher is trying to fix the mem_write error!\nPlease wait for completion before you continue.");
        }
        private static void model_leaks()
        {
            System.Windows.Forms.MessageBox.Show("Argument -32 deactivated!");
        }

        private static void host_crash()
        {
            Process.Start("https://account.arena.net/login");
        }

        private static void outdated_client()
        {
            Unhandled_Launch("-image");
            System.Windows.Forms.MessageBox.Show("The Guild Wars 2 Launcher is trying to update!\nPlease wait for completion before you continue.");
        }

        private static void readonly_write()
        {
            /*
            foreach(Client client in Client.GetClients())
            {
                client.Stop();
            }
            outdated_client();
            */
        }

        private static void Unhandled_Launch(string argus)
        {
            Process pro = new Process();
            pro.StartInfo = new ProcessStartInfo(EnviromentManager.GwClientExePath, argus);
            pro.Start();
        }
    }

    static public class CrashAnalyzer
    {
        //Reading/Managing Crashlogs
        public static ObservableCollection<Crashlog> Crashlogs= new ObservableCollection<Crashlog>();


        static public Crashlog GetLatestCrashlog()
        {
            return Crashlogs[Crashlogs.Count-1];
        }

        static public Crashlog GetCrashByIndex(int index)
        {
            return Crashlogs[index];
        }

        static public void ReadCrashLogs(string path=null)
        {
            Crashlogs.Clear();
           if (path == null)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Guild Wars 2\\Arenanet.log";
            }

           try
            {
                if (!File.Exists(path)) File.Create(path).Close();
                long crashlogs_size = new FileInfo(path).Length / 1000000;

                if (crashlogs_size > 10)
                {
                    //Average crashlog size = 12.288 kb
                    MessageBoxResult win = MessageBox.Show("Your crashlog file has an overall size of " + crashlogs_size+ " Mb! (est. " + Math.Round((crashlogs_size*1000/12.288),0) + " Crashlogs!)\nUsing crashlogs this big could decrease performance drastically!\n Should launchbuddy reduce its size?", "Crashlog File too big", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (win.ToString() == "Yes")
                    {
                        string[] lines =File.ReadLines(path).Take(5000).ToArray();
                        File.Delete(path);
                        File.WriteAllLines(path,lines);
                    }
                }

                string[] data = Regex.Split(File.ReadAllText(path), @"\*--> Crash <--\*");
                for(int i=1;i< data.Length; i++)
                {
                    Crashlogs.Add(new Crashlog(data[i]));
                }

                Crashlogs = new ObservableCollection<Crashlog>(from i in Crashlogs orderby i.CrashTime select i);
                Crashlogs = new ObservableCollection<Crashlog>(Crashlogs.Reverse());

                //Clean up Crashlog      
                 if (Crashlogs.Count >= 25)
                {
                    try
                    {
                        string logs = "";

                        for (int i=2;i<data.Length;i++)
                        {
                            logs += @"*--> Crash <--*" + data[i];
                        }

                        File.Delete(path);
                        File.WriteAllText(path,logs);
                    }catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Could not open Crashlog!\n"+ e.Message);
                if(File.Exists(path))File.Delete(path);
                File.Create(path).Close();
            }
        }
    }

    public class Crashlog
    {
        // Crashinfos
        public string Assertion { set; get; }
        string Filename;
        string Exename;
        uint Pid;
        string[] Arguments;
        string BaseAddr;
        string ProgramID;
        public string Build { set; get; }
        string crashtime;
        string rawdata;
        public string CrashTime
        {
            get { return crashtime ; }
            set {
                Match match = Regex.Match(value, @"(?<Date>\d{4}-\d\d-\d\d)T(?<Time>\d\d:\d\d:\d\d)\+");
                crashtime = match.Groups["Date"].Value + " " + match.Groups["Time"].Value;
            }
        }
        string UpTime;

        //System
        string Username;
        string IPAdress;
        string Processors;
        string OS;

        //DLLS
        string[] DllList;

        //Outputs
        public string Quickinfo
        {
            get {
                string tmp = "Crashtime: " + CrashTime;
                tmp += "\nBuild: " + Build;
                tmp += "\nAssertion: " + Assertion;
                tmp += "Used Arguments:" + String.Join(",", Arguments);
                tmp += "\n\nFull report:" + rawdata;
                return tmp;
            }
        }

        //Solution
        public string Solutioninfo;
        public string Solutionkey { set; get; }
        public void Solve() { if (IsSolveable) CrashLibrary.ApplySolution(Solutionkey); }
        public bool IsSolveable {
            get { if (Solutionkey != "unknown") return true;
                return false;
            } }

        public Crashlog(string crashdata)
        {
            //Only use splitted Crash data!
            rawdata = crashdata;

            Assertion = Regex.Match(crashdata, @"Assertion: ?(?<data>.*)").Groups["data"].Value;
            if (Assertion=="") Assertion= Regex.Match(crashdata, @"Exception: ?(?<data>.*\n.*)").Groups["data"].Value;

            Filename = Regex.Match(crashdata, @"File: ?(?<data>.*)").Groups["data"].Value;
            Exename = Regex.Match(crashdata, @"App: ?(?<data>.*)").Groups["data"].Value;
            Pid = UInt16.Parse(Regex.Match(crashdata, @"Pid: ?(?<data>.*)").Groups["data"].Value);
            Arguments = Regex.Match(crashdata, @"Cmdline: ?(?<data>.*)").Groups["data"].Value.Split('-');
            BaseAddr = Regex.Match(crashdata, @"BaseAddr: ?(?<data>.*)").Groups["data"].Value;
            ProgramID = Regex.Match(crashdata, @"ProgramId: ?(?<data>.*)").Groups["data"].Value;
            Build = Regex.Match(crashdata, @"Build: ?(?<data>.*)").Groups["data"].Value;
            CrashTime = Regex.Match(crashdata, @"When: ?(?<data>.*)").Groups["data"].Value;
            UpTime = Regex.Match(crashdata, @"Uptime: ?(?<data>.*)").Groups["data"].Value;

            Username= Regex.Match(crashdata, @"Name: ?(?<data>.*)").Groups["data"].Value;
            IPAdress= Regex.Match(crashdata, @"IpAddr: ?(?<data>.*)").Groups["data"].Value;
            Processors= Regex.Match(crashdata, @"Processors: ?(?<data>.*)").Groups["data"].Value;
            OS = Regex.Match(crashdata, @"OSVersion: ?(?<data>.*)").Groups["data"].Value;

            DllList = Regex.Matches(crashdata, @"\w:\\.*.dll").Cast<Match>().Select(m => m.Value).ToArray();
            Solutionkey = CrashLibrary.ClassifyCrash(this);
            Solutioninfo = CrashLibrary.SolutionInfo[Solutionkey];
        }
    }


}
