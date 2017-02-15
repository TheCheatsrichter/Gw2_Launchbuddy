using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Gw2_Launchbuddy.MainWindow;
using System;


namespace Gw2_Launchbuddy
{

    static class Globals
    {
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

    }
}
