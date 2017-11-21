using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Gw2_Launchbuddy.MainWindow;
using System;
using CommandLine;
using System.Windows.Controls;


namespace Gw2_Launchbuddy
{
    static class Globals
    {
        public static GUI_ApplicationManager Appmanager = new GUI_ApplicationManager();

        public static Microsoft.Win32.RegistryKey LBRegKey { get { return Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE").CreateSubKey("LaunchBuddy"); } set { } }

        public static ObservableCollection<ProAccBinding> LinkedAccs = new ObservableCollection<ProAccBinding>();
        public static List<Account> selected_accs = new List<Account>();
        public static string exepath, exename, unlockerpath, version_client, version_api;

        public static bool ClientIsUptodate = false;
        public static Server selected_authsv = new Server();
        public static Server selected_assetsv = new Server();
        public static Arguments args = new Arguments();

        public static GFXConfig SelectedGFX = new GFXConfig();
        public static string ClientXmlpath;

        public static string AppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Gw2 Launchbuddy\\";
        public static Version LBVersion = new Version("1.4.2");

        //Arguments
        public static ObservableCollection<CheckBox> arg_checkboxlist = new ObservableCollection<CheckBox>();

        //Serverlists
        public static ObservableCollection<Server> authlist = new ObservableCollection<Server>();
        public static ObservableCollection<Server> assetlist = new ObservableCollection<Server>();

    }
    public class Options
    {
        [Option('s', "silent", HelpText = "Run Launchbuddy silently.")]
        public bool Silent { get; set; }
    }

}
