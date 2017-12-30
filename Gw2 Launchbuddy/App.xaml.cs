using CommandLine;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using Gw2_Launchbuddy.ObjectManagers;

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
            Globals.options = options;
            if (!options.Silent)
            {
                var app = new App();
                app.InitializeComponent();
#if DEBUG
                app.Run();
#endif
            }
            else
            {
                AccountManager.ImportExport.LoadAccountInfo();
                Config.LoadConfig();
            }
            if(!String.IsNullOrWhiteSpace(options.Launch))
            {
                LaunchManager.LaunchGW2(AccountManager.Account(options.Launch));
            }
        }
    }
}