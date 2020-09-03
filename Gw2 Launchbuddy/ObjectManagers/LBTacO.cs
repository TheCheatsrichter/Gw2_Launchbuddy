using Gw2_Launchbuddy.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class LBTacO
    {
        static string path = "";
        public static string TacoPath
        {
            get { return path; }
        }

        public static bool IsValid
        {
            get
            {
                return File.Exists(path);
            }
        }

        public static void Init()
        {
            path = LBConfiguration.Config.taco_path;
        }

        public static void SetPath()
        {
            Builders.FileDialog.DefaultExt(".exe")
                            .Filter("EXE Files(*.exe)|*.exe")
                            .EnforceExt(".exe")
                            .ShowDialog((Helpers.FileDialog fileDialog) =>
                            {
                                if (fileDialog.FileName != "")
                                {
                                    path = fileDialog.FileName;
                                }
                            });
            LBConfiguration.Config.taco_path = path;
            LBConfiguration.Save();
        }

        public static void LaunchTacoInstance(Account acc)
        {
            if(IsValid)
            {
                ProcessStartInfo proinfo = new ProcessStartInfo();
                proinfo.FileName = path;
                proinfo.WorkingDirectory = Path.GetDirectoryName(path);
                if(ClientManager.ActiveClients.Count>1) proinfo.Arguments = $"-mumble GW2MumbleLink{acc.ID} -forcenewinstance";
                Process.Start(proinfo);

                Action sleep = () => Thread.Sleep(1500);
                Helpers.BlockerInfo.Run("TacO Launch", $"TacO is launched for account {acc.Nickname}.", sleep);
            }
            else
            {
                MessageBox.Show("TacO .exe file location seems to have changed or was not set correctly! Please reconfigure the path in the TacO Tab.");
            }

        }
    }
}

