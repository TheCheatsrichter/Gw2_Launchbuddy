using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace Update_Helper
{
    class Program
    {
        /* Args:
         *       0: pid of old LB
         *       1: new version
         *       2: URL to download
         *       3: old LB name
         */
        static void Main(string[] args)
        {
            try
            {
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

                //Start new LB
                Process newLB = new Process { StartInfo = new ProcessStartInfo(dest) };
                newLB.Start();
            }
            catch { }

            //Kill and delete updater (Self)
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
        }
    }
}
