using CommandLine;
using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

namespace Gw2_Launchbuddy
{
    internal static class Globals
    {
        public static GUI_ApplicationManager Appmanager = new GUI_ApplicationManager(); //Should not be created here/should have its own controller.

        public static ObservableCollection<AccountClient> LinkedAccs = new ObservableCollection<AccountClient>(); //Should nto be needed?

        //public static List<Account> selected_accs = new List<Account>();
        public static string unlockerpath, version_api; //Should be split out into respective managers

        public static Server selected_authsv = new Server(); //Should be removed/Unneeded with Manager
        public static Server selected_assetsv = new Server(); //Should be removed/Unneeded with Manager

        public static GFXConfig SelectedGFX = new GFXConfig(); //Should be removed/Unneeded with Manager
        public static string ClientXmlpath; //Should be part of Client

        public static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Gw2 Launchbuddy\\";
        public static Version LBVersion = new Version("1.4.2");

        public static Options options;
    }

    public class Options
    {
        [Option('s', "silent", HelpText = "Run Launchbuddy silently.")]
        public bool Silent { get; set; }

        [Option("settings", HelpText = "Use Settings.json instead of command line Arguments.")]
        public bool Settings { get; set; }

        [Option('l', "launch", HelpText = "Launch with Nickname of saved account.")]
        public string Launch { get; set; }
    }

    public static class Config
    {
        public static void LoadConfig()
        {
            //Checking if path for ReShade Unlocker is saved

            //if (Properties.Settings.Default.reshadepath != "")
            //{
            //    try
            //    {
            //        Globals.unlockerpath = Properties.Settings.Default.reshadepath;
            //    }
            //    catch { }
            //    cb_reshade.IsEnabled = true;
            //}

            try
            {
                //if (Properties.Settings.Default.use_reshade && cb_reshade.IsEnabled == true) cb_reshade.IsChecked = true;
                //if (Properties.Settings.Default.use_autologin == true) cb_login.IsChecked = true;

                //if (Properties.Settings.Default.selected_acc != 0) listview_acc.SelectedIndex = Cinema_Accountlist.SelectedIndex = Properties.Settings.Default.selected_acc;
            }
            catch (Exception err)
            {
                MessageBox.Show("Error in UI setup. \n " + err.Message);
            }

            // Importing the XML file at AppData\Roaming\Guild Wars 2\
            // This file also contains infos about the graphic settings

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
            Globals.ClientXmlpath = "";
            long max = 0;

            foreach (string config in configfiles)
            {
                if (System.IO.File.GetLastWriteTime(config).Ticks > max)
                {
                    max = System.IO.File.GetLastWriteTime(config).Ticks;
                    Globals.ClientXmlpath = config;
                }
            }

            //Read the GFX Settings
            Globals.SelectedGFX = GFXManager.ReadFile(Globals.ClientXmlpath);
            //lv_gfx.ItemsSource = Globals.SelectedGFX.Config;
            //lv_gfx.Items.Refresh();

            // Read the XML file
            try
            {
                //if (Properties.Settings.Default.use_reshade) cb_reshade.IsChecked = true;

                StreamReader stream = new System.IO.StreamReader(Globals.ClientXmlpath);
                XmlTextReader reader = null;
                reader = new XmlTextReader(stream);

                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "VERSIONNAME":
                            Regex filter = new Regex(@"\d*\d");
                            ClientManager.ClientInfo.Version = filter.Match(getvalue(reader)).Value;
                            //lab_version.Content = "Client Version: " + ClientManager.ClientInfo.Version;
                            break;

                        case "INSTALLPATH":

                            ClientManager.ClientInfo.InstallPath = getvalue(reader);
                            //lab_path.Content = "Install Path: " + ClientManager.ClientInfo.InstallPath;
                            break;

                        case "EXECUTABLE":

                            ClientManager.ClientInfo.Executable = getvalue(reader);
                            //lab_path.Content += ClientManager.ClientInfo.Executable;
                            break;

                        case "EXECCMD":
                            //Filter arguments from path
                            //lab_para.Content = "Latest Start Parameters: ";
                            Regex regex = new Regex(@"(?<=^|\s)-(umbra.(\w)*|\w*)");
                            string input = getvalue(reader);
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
                MessageBox.Show("Gw2 info file not found! Please choose the Directory manually!");
            }
        }
        private static string getvalue(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                return reader.Value;
            }
            return null;
        }
    }
}