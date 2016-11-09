using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gw2_Launchbuddy.MainWindow;

namespace Gw2_Launchbuddy
{

    static class Globals
    {
        public static Microsoft.Win32.RegistryKey LBRegKey { get { return Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE").CreateSubKey("LaunchBuddy"); } set { } }

        public static List<Account> selected_accs = new List<Account>();
        public static string exepath, exename, unlockerpath, version_client, version_api;

        public static Server selected_authsv = new Server();
        public static Server selected_assetsv = new Server();
        public static Arguments args = new Arguments();

        public static Dictionary<string, string> ArgList
        {
            // Add/Remove arguments with ease.
            get
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();
                temp.Add("-32", "Forces the game to run in 32 bit.");
                temp.Add("-64", "Forces the game to run in 64 bit.");
                temp.Add("-bmp", "Forces the game to create lossless screenshots as .BMP files. Use for creating high-quality screenshots at the expense of much larger files.");
                temp.Add("-diag", "Instead of launching the game, this command creates a detailed diagnostic file that contains diagnostic data that can be used for troubleshooting. The file, NetworkDiag.log, will be located in your game directory or Documents/Guild Wars . If you want to use this feature, be sure to create a separate shortcut for it.");
                temp.Add("-dx9single", "Enables the Direct3D 9c renderer in single-threaded mode. Improves performance in Wine with CSMT.");
                temp.Add("-forwardrenderer", "Uses Forward Rendering instead of Deferred Rendering (unfinished). This currently may lead to shadows and lighting to not appear as expected.It may increase the framerate and responsiveness when using AMD graphics card");
                temp.Add("-image", "Runs the patch UI only in order to download any available updates); closes immediately without loading the login form. ");
                temp.Add("-log", "Enables the creation of a log file, used mostly by Support. The path for the generated file usually is found in the APPDATA folder");
                temp.Add("-mce", "Start the client with Windows Media Center compatibility, switching the game to full screen and restarting Media Center (if available) after the client is closed.");
                temp.Add("-nomusic", "Disables music and background music.");
                temp.Add("-noui", "Disables the user interface. This does the same thing as pressing Ctrl+Shift+H in the game.");
                temp.Add("-nosound", "Disables audio system completely.");
                temp.Add("-prefreset", "Resets game settings.");
                temp.Add("-repair", "Start the client, checks the files for errors and repairs them as needed. This can take a long time (1/2 hour or an hour) to run as it checks the entire contents of the 20-30 gigabyte archive.");
                temp.Add("-uispanallmonitors", "Spreads user interface across all monitors in a triple monitor setup.");
                temp.Add("-uninstall", "Presents the uninstall dialog. If uninstall is accepted, it deletes the contents of the Guild Wars 2 installation folder except GW2.EXE itself and any manually created subfolders. Contents in subfolders (if any) are not deleted.");
                temp.Add("-useOldFov", "Disables the widescreen field-of-view enhancements and restores the original field-of-view.");
                temp.Add("-verify", "Used to verify the .dat file.");
                temp.Add("-windowed", "Forces Guild Wars 2 to run in windowed mode. In game, you can switch to windowed mode by pressing Alt + Enter or clicking the window icon in the upper right corner.");
                temp.Add("-umbra gpu", "Forces the use of umbra's GPU accelerated culling. In most cases, using this results in higher cpu usage and lower gpu usage decreasing the frame-rate.");
                temp.Add("-maploadinfo", "Shows diagnostic information during map loads, including load percentages and elapsed time.");
                temp.Add("-shareArchive", "Opens the Gw2.dat file in shared mode so that it can be accessed from other processes while the game is running.");
                return temp;
            }
            set { }
        }
    }

    public class Arguments
    {
        public List<Argument> arguments { get; set; }
        public string Print(int i) { return Print(Globals.selected_accs.Count > i ? Globals.selected_accs[i] : new Account()); }
        public string Print(Account account)
        {
            Argument("-email", account.Email, true);
            Argument("-password", account.Password, true);
            Argument("-nopatchui", true);
            return String.Join(" ", arguments.Select(a => a.Print));
        }
        public string PrintSterile(int i) { return PrintSterile(Globals.selected_accs.Count > i ? Globals.selected_accs[i] : new Account()); }
        public string PrintSterile(Account account)
        {
            return String.Join(" ", arguments.Where(a => !a.Sensitive).Select(a => a.Print)) + (Properties.Settings.Default.use_autologin ? " -AutoLogin" : "");
        }
        public void Argument(string flag) { Argument(flag, null, null); }
        public void Argument(string flag, string option) { Argument(flag, option, null); }
        public void Argument(string flag, bool? sensitive) { Argument(flag, null, sensitive); }
        public void Argument(string flag, string option, bool? sensitive)
        {
            var arg = new Argument(flag, option, sensitive ?? false);
            if (arguments.Any(a => a.Flag == flag))
                arg = arguments.Where(a => a.Flag == flag).First();
            else
                arguments.Add(arg);
            arg.Option = option ?? arg.Option;
            arg.Sensitive = sensitive ?? arg.Sensitive;
        }

        public void Remove(string flag)
        {
            arguments.Remove(arguments.Where(a => a.Flag == flag).FirstOrDefault());
        }

        public Arguments()
        {
            arguments = new List<Gw2_Launchbuddy.Argument>();
            Argument("-shareArchive", true);
        }
    }
    public class Argument
    {
        public string Flag { get; set; }
        public string Option { get; set; }
        public string Print { get { return Flag != null ? Flag + (Option != null ? " " + Option : "") : ""; } set { } }
        public bool Sensitive { get; set; }

        public Argument(string flag) : this(flag, null, false) { }
        public Argument(string flag, bool sensitive) : this(flag, null, sensitive) { }
        public Argument(string flag, string option) : this(flag, option, false) { }
        public Argument(string flag, string option, bool sensitive)
        {
            Flag = flag;
            Option = option;
            Sensitive = sensitive;
        }
    }
}
