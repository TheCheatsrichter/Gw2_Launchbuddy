using CommandLine;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using Gw2_Launchbuddy.ObjectManagers;
using System.Net;

namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    }

    public class EntryPoint
    {
        [DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);

        [STAThread]
        public static void Main(string[] args)
        {
            AttachConsole(-1);
            var result = Parser.Default.ParseArguments<Options>(args).WithParsed(options => RunParsed(options));
        }

        public static void RunParsed(Options options)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Globals.options = options;

            // Load things before MainWindow
            AccountManager.ImportExport.LoadAccountInfo();
            ClientManager.ClientInfo.LoadClientInfo();
            //PluginManager.DoInits();

            foreach (var account in options.Launch)
                AccountManager.Account(account).IsSelected();
            foreach(var arg in options.Args)
                AccountArgumentManager.StopGap.IsSelected("-" + arg, true);
            //LaunchManager.Launch();

            if (!options.Silent)
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}