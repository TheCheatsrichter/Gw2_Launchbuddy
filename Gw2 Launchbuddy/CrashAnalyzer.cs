using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Gw2_Launchbuddy
{
    static public class CrashAnalyzer
    {
        public static List<Crashlog> Crashlogs= new List<Crashlog>();


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
           if (path == null)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Guild Wars 2\\Arenanet.log";
            }

           try
            {
                string[] data = Regex.Split(File.ReadAllText(path), @"\*--> Crash <--\*");
                for(int i=1;i< data.Length; i++)
                {
                    Crashlogs.Add(new Crashlog(data[i]));
                }

                //Clean up Crashlog
                if (Crashlogs.Count > 25)
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
                System.Windows.Forms.MessageBox.Show("Could not find Crashlog!\n"+ e.Message);
            }
        }
    }

    public class Crashlog
    {
        // Crashinfos
        string Assertion;
        string Filename;
        string Exename;
        uint Pid;
        string[] Arguments;
        string BaseAddr;
        string ProgramID;
        uint Build;
        string crashtime;
        string CrashTime
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

        public Crashlog(string crashdata)
        {
            //Only use splitted Crash data!
            Assertion = Regex.Match(crashdata, @"Assertion: ?(?<data>.*)").Groups["data"].Value;
            Filename = Regex.Match(crashdata, @"File: ?(?<data>.*)").Groups["data"].Value;
            Exename = Regex.Match(crashdata, @"App: ?(?<data>.*)").Groups["data"].Value;
            Pid = UInt16.Parse(Regex.Match(crashdata, @"Pid: ?(?<data>.*)").Groups["data"].Value);
            Arguments = Regex.Match(crashdata, @"Cmdline: ?(?<data>.*)").Groups["data"].Value.Split('-');
            BaseAddr = Regex.Match(crashdata, @"BaseAddr: ?(?<data>.*)").Groups["data"].Value;
            ProgramID = Regex.Match(crashdata, @"ProgramId: ?(?<data>.*)").Groups["data"].Value;
            Build = UInt32.Parse(Regex.Match(crashdata, @"Build: ?(?<data>.*)").Groups["data"].Value);
            CrashTime = Regex.Match(crashdata, @"When: ?(?<data>.*)").Groups["data"].Value;
            UpTime = Regex.Match(crashdata, @"Uptime: ?(?<data>.*)").Groups["data"].Value;

            Username= Regex.Match(crashdata, @"Name: ?(?<data>.*)").Groups["data"].Value;
            IPAdress= Regex.Match(crashdata, @"IpAddr: ?(?<data>.*)").Groups["data"].Value;
            Processors= Regex.Match(crashdata, @"Processors: ?(?<data>.*)").Groups["data"].Value;
            OS = Regex.Match(crashdata, @"OSVersion: ?(?<data>.*)").Groups["data"].Value;

            DllList = Regex.Matches(crashdata, @"\w:\\.*.dll").Cast<Match>().Select(m => m.Value).ToArray();
        }
    }
}
