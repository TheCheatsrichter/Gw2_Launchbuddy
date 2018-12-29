using Gw2_Launchbuddy.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Net;

namespace Gw2_Launchbuddy.ObjectManagers
{
    //Global collection of Paths and Setups for all other Managers

    public static class EnviromentManager
    {
        public static string LBAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Gw2 Launchbuddy\";
        public static string LBActiveClientsPath = LBAppdataPath + "lbac.txt";
        public static string LBAccountPath = LBAppdataPath + "lb_acc.bin";
        public static string LBIconsPath = LBAppdataPath + "Icons\\";
        public static bool LBUseClientGUI = Properties.Settings.Default.useinstancegui;
        public static bool LBUseLoadingGUI = Properties.Settings.Default.useloadingui;

        public static GUI_ApplicationManager LBInstanceGUI = new GUI_ApplicationManager();
        public static LoadingScreen LBLoadingGUI = new LoadingScreen();

        public static string GwAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\";
        public static string GwClientXmlPath;
        public static string GwClientVersion;
        public static string GwClientExeName;
        public static string GwClientPath;
        public static string GwClientExePath { get { return GwClientPath + GwClientExeName; } }
        public static bool? GwClientUpToDate = null;

        public static string TMP_GFXConfig = GwAppdataPath + "TMP_GFX.xml";
        public static string TMP_BackupGFXConfig = GwAppdataPath + "TMP_GFX.bak";

        public static void Init()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            VersionSwitcher.DeleteUpdater();

            LoadGwClientInfo();
            CheckGwClientVersion();
            VersionSwitcher.CheckForUpdate();

            AccountManager.ImportAccounts();
            ClientManager.ImportActiveClients();
            CrashAnalyzer.ReadCrashLogs();
            IconManager.Init();
        }

        public static void LoadGwClientInfo()
        {
            //Find the newest XML file in APPDATA (the XML files share the same name as their XML files -> multiple .xml files possible!)
            string[] configfiles = new string[] { };
            try
            {
                configfiles = Directory.GetFiles(GwAppdataPath, "*.exe.xml");
            }
            catch (Exception e)
            {
                MessageBox.Show("Guild Wars may not be installed. \n " + e.Message);
                return;
            }
            GwClientXmlPath = "";
            long max = 0;
            foreach (string config in configfiles)
            {
                if (System.IO.File.GetLastWriteTime(config).Ticks > max)
                {
                    max = System.IO.File.GetLastWriteTime(config).Ticks;
                    GwClientXmlPath = config;
                }
            }

            // Read the XML file
            try
            {
                StreamReader stream = new System.IO.StreamReader(GwClientXmlPath);
                XmlTextReader reader = null;
                reader = new XmlTextReader(stream);
                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "VERSIONNAME":
                            Regex filter = new Regex(@"\d*\d");
                            GwClientVersion = filter.Match(reader.GetValue()).Value;
                            break;

                        case "INSTALLPATH":
                            GwClientPath = reader.GetValue();
                            break;

                        case "EXECUTABLE":
                            GwClientExeName = reader.GetValue();
                            break;

                        case "EXECCMD":
                            //Argument import not needed, now Account bound
                            /*
                            Regex regex = new Regex(@"(?<=^|\s)-(umbra.(\w)*|\w*)");
                            string input = reader.GetValue();
                            MatchCollection matchList = regex.Matches(input);

                            foreach (Argument argument in ArgumentManager.ArgumentCollection)
                                foreach (Match parameter in matchList)
                                    if (argument.Flag == parameter.Value && !argument.Blocker)
                                        AccountArgumentManager.StopGap.IsSelected(parameter.Value, true);
                            */
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
        public static void CheckGwClientVersion()
        {
            if (GwClientVersion != null)
            {
                if (GwClientUpToDate == null && Api.Online)
                {
                    GwClientUpToDate = Api.ClientBuild == GwClientVersion;
                    if (!(bool)GwClientUpToDate)
                    {
                        MessageBox.Show("Your Gw2 Client is outdated. Update startet now");
                        UpdateGwClient();
                    }
                        
                }
            }
            else
            {
                LoadGwClientInfo();
            }
        }
        private static void UpdateGwClient()
        {
            if (Process.GetProcessesByName("*Gw2*.exe").Length == 0)
            {
                Process pro = new Process();
                pro.StartInfo = new ProcessStartInfo { Arguments = "-image" };
                pro.Start();
            }
            else
            {
                MessageBox.Show("Please close all running Gw2 game instances to update the game.");
            }
        }
    }
}
