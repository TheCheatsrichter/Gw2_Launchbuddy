using CommandLine;
using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.Generic;
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

        public static ObservableCollection<AccountClient> LinkedAccs = new ObservableCollection<AccountClient>(); //Should not be needed?

        //public static List<Account> selected_accs = new List<Account>();
        public static string unlockerpath, version_api; //Should be split out into respective managers

        public static Server selected_authsv = new Server(); //Should be removed/Unneeded with Manager
        public static Server selected_assetsv = new Server(); //Should be removed/Unneeded with Manager

        public static GFXConfig SelectedGFX = new GFXConfig(); //Should be removed/Unneeded with Manager
        public static string ClientXmlpath; //Should be part of Client

        public static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Gw2 Launchbuddy\\";
        public static Version LBVersion = new Version("1.7.0");

        public static Options options;
    }

    public class Options
    {
        [Option('q', "silent", HelpText = "Run Launchbuddy silently.")]
        public bool Silent { get; set; }

        [Option("settings", HelpText = "Use Settings.json instead of command line Arguments.")]
        public bool Settings { get; set; }

        [Option('l', "launch", Separator = ':', HelpText = "Launch with Nicknames of saved accounts. Use : as a separator.")]
        public IEnumerable<string> Launch { get; set; }

        [Option('m', "minimized", HelpText = "Run Launchbuddy but open minimized.")]
        public string Minimized { get; set; }

        [Option('s', "safe", HelpText = "Do not load plugins.")]
        public bool Safe { get; set; }

        [Option('a', "args", Separator = ':', HelpText = "Arguments to use when launching with -launch. Use : as a separator, no arguments with input.")]
        public IEnumerable<string> Args { get; set; }

        [Option("delaymutex", HelpText = "Delay in miliseconds between mutex close attempts. Higher values increase the time between retries. (Up to 9 retries will be attempted)", Hidden = true)]
        public int? Delay { get; set; }
    }
}