using CommandLine;
using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.ObjectModel;

namespace Gw2_Launchbuddy
{
    internal static class Globals
    {
        public static GUI_ApplicationManager Appmanager = new GUI_ApplicationManager(); //Should not be created here/should have its own controller.

        public static ObservableCollection<AccountClient> LinkedAccs = new ObservableCollection<AccountClient>(); //Should nto be needed?

        //public static List<Account> selected_accs = new List<Account>();
        public static string version_api; //Should be split out into respective managers

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
    }
}