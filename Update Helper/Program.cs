using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace Update_Helper
{
    internal class Program
    {
        /* Args:
         *       0: pid of old LB
         *       1: new version
         *       2: URL to download
         *       3: old LB name
         */

        private static void Main(string[] args)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //Find and kill old LB
                Process oldLB = null;
                if (Process.GetProcesses().Any(x => x.Id == Int32.Parse(args[0])))
                {
                    oldLB = Process.GetProcessById(Int32.Parse(args[0]));
                    oldLB.Kill();
                    oldLB.WaitForExit();
                }

                //Gather Info
                var dir = System.IO.Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "\\";
                var dest = dir + args[3];
                var bakdest = dest + ".bak";
                var tempdest = dir + "Gw2_Launchbuddy_" + args[1] + ".exe";

                //Download new LB
                WebClient wc = new WebClient();
                wc.DownloadFile(args[2], tempdest);

                //Delete existing backup and create new one
                if (!System.IO.File.Exists(bakdest)) File.Delete(bakdest);
                File.Move(dest, bakdest);

                //Rename new LB to old LB name
                File.Move(tempdest, dest);

                //Cleanup
                File.Delete(bakdest);

                //Open Directory
                new Process { StartInfo = new ProcessStartInfo(dir) }.Start();
            }
            catch { }

            //Kill and delete updater (Self)
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C timeout /T 3 & Del \"" + new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            
        }
    }
}