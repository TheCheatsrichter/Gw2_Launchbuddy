using System;
using System.Runtime.InteropServices;
using System.Windows;
using Gw2_Launchbuddy.ObjectManagers;
using Gw2_Launchbuddy.Modifiers;
using System.Reflection;
using System.Linq;

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
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            AttachConsole(-1);
            RunParsed();
        }

        /*
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");

            dllName = dllName.Replace(".", "_");

            foreach (string str in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Console.WriteLine(str);
            }

            System.Resources.ResourceManager rm = new System.Resources.ResourceManager("Gw2_Launchbuddy.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

            if (dllName.EndsWith("_resources")) return null;

            /*
            string test = "";
            object test2 = rm.GetObject("CommandLine");
            object test3 = rm.GetObject("NHotkey_Wpf");
            
            byte[] bytes = null;

            //MessageBox.Show("Loading " + dllName);
            bytes = (byte[])rm.GetObject(dllName);

            if (bytes == null || bytes.Length == 0)
            {
                //MessageBox.Show("Missing: " + dllName);
                return null;
            }
            //MessageBox.Show("Loaded " + dllName);

            return System.Reflection.Assembly.Load(bytes);
        }
    */

        
        public static void RunParsed(LaunchOptions options)
        {
        EnviromentManager.Init();

            EnviromentManager.LaunchOptions = options;

            if (!options.Silent)
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
        
        public static void RunParsed()
        {
            //CrashReporter
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionReport);
#endif

            EnviromentManager.Init();

            var app = new App();
            app.InitializeComponent();
            app.Run();

        }

        private static void UnhandledExceptionReport(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            CrashReporter.ReportCrashToAll(e);
        }
    }
}