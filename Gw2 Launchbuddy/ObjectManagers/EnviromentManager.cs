using Gw2_Launchbuddy.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Net;
using System.Reflection;
//using CommandLine;
using Gw2_Launchbuddy.Modifiers;
using System.Threading;
using CommandLine;

namespace Gw2_Launchbuddy.ObjectManagers
{
    //Global collection of Paths and Setups for all other Managers

    public static class EnviromentManager
    {
        public static Version LBVersion = new Version("3.0.9");
        public static LaunchOptions LaunchOptions;

        public static string LBAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Gw2 Launchbuddy\";
        public static string LBActiveClientsPath = LBAppdataPath + "lbac.txt";
        public static string LBIconsPath = LBAppdataPath + "Icons\\";
        public static string LBAccPath = LBAppdataPath + "Accs.xml";
        public static string LBConfigPath = LBAppdataPath + "LBConfig.xml";
        public static string LBPluginsPath = LBAppdataPath + "Plugins\\";
        private static string LBRightTestPath = LBAppdataPath + "RightTest.txt";

        public static string CustomMusic_Folderpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Guild Wars 2\Music\";
        public static string CustomMusicDisabled_Folderpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Guild Wars 2\MusicDisabled\";

        //public static bool LBUseClientGUI = LBConfiguration.Config.useinstancegui;
        //public static bool LBUseLoadingGUI = Properties.Settings.Default.useloadingui;

        private static GUI_ApplicationManager lbInstanceGUI;

        public static GUI_ApplicationManager LBInstanceGUI
        {
            get {
                if(lbInstanceGUI==null)
                {
                    lbInstanceGUI= new GUI_ApplicationManager();
                }
                return lbInstanceGUI;
            }
            set { lbInstanceGUI = value; }
        }
        public static LoadingScreen LBLoadingGUI = new LoadingScreen();

        public static string GwAppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\";
        public static string GwClientXmlPath;
        public static string GwClientVersion;
        public static string GwClientExeName;
        public static string GwClientExeNameWithoutExtension { get { return Path.GetFileNameWithoutExtension(GwClientExeName); } }
        public static string GwClientPath;
        public static string GwClientTmpPath;
        public static string GwClientExePath { get { return GwClientPath + GwClientExeName; } }
        public static bool? GwClientUpToDate = null;

        public static string GwLocaldatPath = GwAppdataPath + "Local.dat";
        public static string GwLocaldatBakPath = GwAppdataPath + "Local.dat.bak";

        public static string GwCacheFoldersPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\";

        public static string LBLocaldatsPath = LBAppdataPath +"Loginfiles\\";


        public static string TMP_GFXConfig = GwAppdataPath + "TMP_GFX.xml";
        public static string TMP_BackupGFXConfig = GwAppdataPath + "TMP_GFX.bak";
        public static string MASTER_GFXConfig = GwAppdataPath + "MASTER_GFX.xml";
        public static string MASTER_Localdat = GwAppdataPath + "MASTER_Local.dat";

        public static string LBAddonPath = LBAppdataPath + "Addons.xml";

        public static Rect GwLoginwindow_UIOffset = new Rect(20, 195, 0, 0);

        public static MainWindow MainWin = null;

        public static void Init()
        {
            //LBInstanceGUI = new GUI_ApplicationManager();

            //Path Setup
            DirectorySetup();
            LoadLBConfig();
            LoadGwClientInfo();

            //Rights Check :P
            CheckApplicationRights();

            //Updater Network Protocol
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            VersionSwitcher.DeleteUpdater();

            //Account Import Export
            AccountManager.ImportAccounts();
            AccountManager.AccountSelfClean();
            ClientManager.ImportActiveClients();
            CrashAnalyzer.ReadCrashLogs();
            IconManager.Init();

            //Temp AddonManager
            AddOnManager.LoadAddons(LBAddonPath);
            AddOnManager.LaunchLbAddons();

            Hotkeys.RegisterAll();

            //Cleanup Plugin folder
            PluginManager.RemoveUninstalledPlugins();
            PluginManager.AddToInstallPlugins();

            //Cleanup CacheFolder
            CacheCleaner.Clean();

            //Musicplaylists
            MusicManager.Init();

        }

        private static bool CreateMasterFiles()
        {
            bool success = true;

            try
            {
                if (File.Exists(MASTER_Localdat)) File.Delete(MASTER_Localdat);
                IORepeater.FileCopy(GwLocaldatPath,MASTER_Localdat);
            }
            catch
            {
                success = false;
            }

            try
            {
                if (File.Exists(MASTER_GFXConfig)) File.Delete(MASTER_GFXConfig);
                IORepeater.FileCopy(GwClientXmlPath, MASTER_GFXConfig);
            }
            catch
            {
                success = false;
            }
            return success;
        }

        private static bool RestoreMasterFiles()
        {
            bool success = true;
            try
            {
                if (File.Exists(GwLocaldatPath)) File.Delete(GwLocaldatPath);
                IORepeater.FileCopy( MASTER_Localdat, GwLocaldatPath);
            }
            catch
            {
                success = false;
            }

            try
            {
                if (File.Exists(GwClientXmlPath)) File.Delete(GwClientXmlPath);
                IORepeater.FileCopy( MASTER_GFXConfig,GwClientXmlPath);
            }
            catch
            {
                success = false;
            }
            return success;
        }

        public static void AfterUI_Inits()
        {
            CheckGwClientVersion();
            //After Game Updates Create Master Files
            CreateMasterFiles();

            if (LBConfiguration.Config.autoupdatedatfiles)
            {
                UpdateAccounts();
            }
            PluginManager.LoadPlugins();
            PluginManager.InitPlugins();
            PluginManager.AutoUpdatePlugins();
            PluginManager.OnLBStart(null);

            if(ClientManager.ActiveClients.Count>0 && LBConfiguration.Config.useinstancegui)
            {
                LBInstanceGUI.Show();
            }

            LBTacO.Init();
            LBBlish.Init();
        }

        public static void Close()
        {
            AddOnManager.SaveAddons(LBAddonPath);
            AccountManager.SaveAccounts();
            Hotkeys.UnregisterAll();
            //Local Dat Cleanup
            LocalDatManager.CleanUp();
            RestoreMasterFiles();
            PluginManager.OnLBClose(null);
        }

        public static void Reboot()
        {
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C ping 127.0.0.1 -n 2 && \"" + System.Reflection.Assembly.GetEntryAssembly().Location + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Close();
            Process.Start(Info);
            Application.Current.MainWindow.Close();
        }

        private static void UpdateAccounts()
        {
            AccountManager.UpdateAccountFiles();
        }

        public static void CheckApplicationRights()
        {
            /*
            if (File.Exists(LBRightTestPath)) File.Delete(LBRightTestPath);
            File.WriteAllText(LBRightTestPath," ");
            if(System.Environment.OSVersion.Version.Major >=10)
            {
                if (!Gw2_Launchbuddy.Modifiers.LocalDatManager.CreateSymbolicLink(LBAppdataPath + "LinkTest", LBRightTestPath, LocalDatManager.SymbolicLink.Unprivileged | LocalDatManager.SymbolicLink.File))
                {
                    System.Diagnostics.Process.Start("ms-settings:developers");
                    System.Diagnostics.Process.Start("https://docs.microsoft.com/en-us/windows/uwp/get-started/enable-your-device-for-development");
                    File.Delete(LBRightTestPath);
                    MessageBox.Show("Launchbuddy requires additional rights to work properly. Please run Launchbuddy as Admin OR activate Windows Developer Mode!");
                }
            }else
            {
                if (!Gw2_Launchbuddy.Modifiers.LocalDatManager.CreateSymbolicLink(LBAppdataPath + "LinkTest", LBRightTestPath, LocalDatManager.SymbolicLink.File))
                {
                    File.Delete(LBRightTestPath);
                    MessageBox.Show("Launchbuddy requires additional rights to work properly. Please run Launchbuddy as Admin!");
                }

            }

            try
            {
                File.Delete(LBAppdataPath + "LinkTest");
                File.Delete(LBRightTestPath);
            }
            catch
            {

            }
            */

        }

        private static void LoadLBConfig()
        {
            LBConfiguration.Load();
        }

        private static void DirectorySetup()
        {
            if (!Directory.Exists(LBAppdataPath)) Directory.CreateDirectory(LBAppdataPath);

            FieldInfo[] props = typeof(EnviromentManager).GetFields();

            foreach(FieldInfo prop in props.Where(p=>p.FieldType== typeof(string)))
            {
                string pvalue=prop.GetValue(null) as string;
                if(pvalue!=null)
                {
                    if (pvalue.EndsWith("\\") && !Directory.Exists(pvalue))
                    {
                        Directory.CreateDirectory(pvalue);
                    }
                }
            }
        }

        public static void ResetPropertySettings()
        {
            LBConfiguration.Reset();
            LBConfiguration.Save();
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
                MessageBox.Show("Guild Wars 2 may not be installed. \n " + e.Message);
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
                MessageBox.Show("Guild Wars 2 info file not found! Please choose the directory manually / launch gw2 once!");
            }

            GwClientTmpPath = GwClientPath + GwClientExeName.Replace(".exe",".tmp");
        }

        public static void CheckGwClientVersion()
        {
            if (LBConfiguration.Config.forcegameclientupdate)
            {
                UpdateGwClient(true);
                return;
            }

            if (GwClientVersion != null)
            {
                //GwClientUpToDate defaults to null on startup
                if (GwClientUpToDate == null)
                {
                    if(Api.Online)
                    {
                        try
                        {
                            Debug.WriteLine($"API Buildversion:{Api.ClientBuild} Local version:{GwClientVersion}");
                            GwClientUpToDate = int.Parse(Api.ClientBuild) <= int.Parse(GwClientVersion);
                        }
                        catch
                        {
                            GwClientUpToDate = Api.ClientBuild.ToLower() == GwClientVersion.ToLower();
                        }
                    }else
                    {
                        MessageBox.Show("Guild Wars 2 API is not reachable. Therefore Launchbuddy defaults to updating the game client.");
                        GwClientUpToDate = false;
                    }
             
                    if (!(bool)GwClientUpToDate)
                    {
                        UpdateGwClient();
                    }
                }
            }
            else
            {
                
                MessageBox.Show("Could not read current Guild Wars2 client version. Please manually check the Guild Wars 2 gameclient for updates and only then proceed with usgae of Launchbuddy");
            }
        }

        private static void UpdateGwClient(bool isforced=false)
        {
            while (Process.GetProcessesByName("*Gw2*.exe").Length == 1)
            {
                MessageBox.Show("Please close all running Guild Wars 2 game instances to update the game.");
            }

            GwGameProcess pro = new GwGameProcess();
            pro.StartInfo = new ProcessStartInfo { FileName = EnviromentManager.GwClientExePath, Arguments = "-image" };

            try
            {
                pro.Start();
                pro.Refresh();
            }
            catch
            {
                MessageBox.Show("Launchbuddy could not find your Gw2 executable. If it has been moved please restart the game once without launchbuddy to fix this issue.");
            }


            Action gameupdateprocess = () =>
            {
                pro.WaitForExit();
                if (pro.exitstatus == ProcessExtension.ExitStatus.self_update)
                {
                    while (Process.GetProcessesByName(pro.ProcessName).Length <= 0)
                    {
                        Thread.Sleep(10);
                    }
                    Process newpro = Process.GetProcessesByName(pro.ProcessName)[0];

                    newpro.WaitForExit();
                }
            };

            Helpers.BlockerInfo.Run("Gw2 Game Update", "Guild Wars 2 is updating. Please wait.", gameupdateprocess);

            /*
            //Overwrite xml file because -image does not update xml file
            string xmldata = File.ReadAllText(EnviromentManager.GwClientXmlPath);
            xmldata= xmldata.Replace(EnviromentManager.GwClientVersion, Api.ClientBuild);
            File.WriteAllText(EnviromentManager.GwClientXmlPath, xmldata);
            */

            if (!isforced)
            {
                string oldbuild = EnviromentManager.GwClientVersion;
                //Reimport xml file
                LoadGwClientInfo();
                if (int.Parse(oldbuild) >= int.Parse(EnviromentManager.GwClientVersion))
                {
                    MessageBox.Show($"The gameclient did not change its version when updating (Previous Version:{oldbuild} New Version: {EnviromentManager.GwClientVersion})." +
                        $" Was the gameupdate successful? Please update Gw2 manually and only then proceed with Launchbuddys usage.");
                }
            }
            Thread.Sleep(2000);
            LoadGwClientInfo();
        }

        public static void Show_LBInstanceGUI()
        {
            if (LBConfiguration.Config.useinstancegui)
            {
                if (LBInstanceGUI.IsLoaded == false) LBInstanceGUI = new GUI_ApplicationManager();
                if (LBInstanceGUI.WindowState == WindowState.Minimized) LBInstanceGUI.WindowState = WindowState.Normal;
                LBInstanceGUI.Show();
            }
        }

        public static string Create_Environment_Report()
        {
            string report = "";

            report += "\n\n########LB Folder########\n";
            report += "Path:" + LBAppdataPath+"\n";
            report += DirectoryFilesToString(LBAppdataPath);
            report += "#########################\n";

            report += "########GW Folder########\n";
            report += "Path:" + GwAppdataPath + "\n";
            report += DirectoryFilesToString(GwAppdataPath);
            report += "#########################\n\n";

            try
            {
                report += FileUtil.WhoIsLockingAsString(GwLocaldatPath);
                report += FileUtil.WhoIsLockingAsString(GwLocaldatBakPath);
                report += FileUtil.WhoIsLockingAsString(GwClientXmlPath);
                report += FileUtil.WhoIsLockingAsString(GwClientPath);
            }catch
            {
                report += "Could not fetch file handle protocol.";
                report += "#########################\n\n";
            }


            return report;
        }

        private static string DirectoryFilesToString(string path)
        {
            string report = "";
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    report += "\t-" + file+"\n";
                }
            }
            else
            {
                report += $"Folder does not exist! Tried to find it at: {path}\n";
            }
            return report;
        }
    }


    public class LaunchOptions
    {
        [Option('q', "silent", HelpText = "Run Launchbuddy silently.")]
        public bool Silent { get; set; }

        [Option("settings", HelpText = "Use Settings.json instead of command line arguments.")]
        public bool Settings { get; set; }

        [Option('l', "launch", Separator = ':', HelpText = "Launch with nicknames of saved accounts. Use : as a separator.")]
        public IEnumerable<string> Launch { get; set; }

        [Option('m', "minimized", HelpText = "Start Launchbuddy minimized.")]
        public string Minimized { get; set; }

        [Option('s', "safe", HelpText = "Do not load plugins.")]
        public bool Safe { get; set; }

        [Option('a', "args", Separator = ':', HelpText = "Arguments to use when launching with -launch. Use : as a separator, no arguments with additional input.")]
        public IEnumerable<string> Args { get; set; }

        [Option("delaymutex", HelpText = "Delay in milliseconds between mutex close attempts. Higher values increase the time between retries. (Up to 9 retries will be attempted)", Hidden = true)]
        public int? Delay { get; set; }
    }


    #region Plugin

    public class AccPluginCalls : PluginContracts.ObjectInterfaces.IAcc
    {
        private Account acc { set; get; }

        public AccPluginCalls(Account acc)
        {
            this.acc = acc;
        }

        public string Nickname { get { return acc.Nickname; } }
        public int ID { get { return acc.ID; } }

        public void Launch() => acc.Client.Launch();
        public void Suspend() => acc.Client.Suspend();
        public void Resume() => acc.Client.Resume();
        public void Maximize() => acc.Client.Maximize();
        public void Minimize() => acc.Client.Minimize();
        public void Focus() => acc.Client.Focus();
        public void Inject(string dllpath) => acc.Client.Inject(dllpath);
        public void Window_Move(int posx, int posy) => acc.Client.Window_Move(posx, posy);
        public void Window_Scale(int width, int height) => acc.Client.Window_Scale(width, height);
        public void Close() => acc.Client.Close();
        public PluginContracts.ObjectInterfaces.ClientStatus Status { get { return ((PluginContracts.ObjectInterfaces.ClientStatus)acc.Client.Status); } }
    }

    #endregion Plugin
}
