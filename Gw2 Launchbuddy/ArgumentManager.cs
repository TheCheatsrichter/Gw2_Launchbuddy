using System;
using System.Collections.Generic;
using System.Linq;
using static Gw2_Launchbuddy.MainWindow;

namespace Gw2_Launchbuddy
{
    public class Arguments
    {
        public Arguments()
        {
            arguments = new List<Argument>();
            // Normal
            arguments.Add(new Argument("-32", "Forces the game to run in 32 bit."));
            //arguments.Add(new Argument("-64", "Forces the game to run in 64 bit."));
            arguments.Add(new Argument("-bmp", "Forces the game to create lossless screenshots as .BMP files. Use for creating high-quality screenshots at the expense of much larger files."));
            arguments.Add(new Argument("-diag", "Instead of launching the game, this command creates a detailed diagnostic file that contains diagnostic data that can be used for troubleshooting. The file, NetworkDiag.log, will be located in your game directory or Documents/Guild Wars . If you want to use this feature, be sure to create a separate shortcut for it."));
            arguments.Add(new Argument("-dx9single", "Enables the Direct3D 9c renderer in single-threaded mode. Improves performance in Wine with CSMT."));
            arguments.Add(new Argument("-forwardrenderer", "Uses Forward Rendering instead of Deferred Rendering (unfinished). This currently may lead to shadows and lighting to not appear as expected.It may increase the framerate and responsiveness when using AMD graphics card"));
            arguments.Add(new Argument("-image", "Runs the patch UI only in order to download any available updates)); closes immediately without loading the login form. "));
            arguments.Add(new Argument("-log", "Enables the creation of a log file, used mostly by Support. The path for the generated file usually is found in the APPDATA folder"));
            arguments.Add(new Argument("-mce", "Start the client with Windows Media Center compatibility, switching the game to full screen and restarting Media Center (if available) after the client is closed."));
            arguments.Add(new Argument("-nomusic", "Disables music and background music."));
            arguments.Add(new Argument("-noui", "Disables the user interface. This does the same thing as pressing Ctrl+Shift+H in the game."));
            arguments.Add(new Argument("-nosound", "Disables audio system completely."));
            arguments.Add(new Argument("-prefreset", "Resets game settings."));
            arguments.Add(new Argument("-repair", "Start the client, checks the files for errors and repairs them as needed. This can take a long time (1/2 hour or an hour) to run as it checks the entire contents of the 20-30 gigabyte archive."));
            arguments.Add(new Argument("-uispanallmonitors", "Spreads user interface across all monitors in a triple monitor setup."));
            arguments.Add(new Argument("-uninstall", "Presents the uninstall dialog. If uninstall is accepted, it deletes the contents of the Guild Wars 2 installation folder except GW2.EXE itself and any manually created subfolders. Contents in subfolders (if any) are not deleted."));
            arguments.Add(new Argument("-useOldFov", "Disables the widescreen field-of-view enhancements and restores the original field-of-view."));
            arguments.Add(new Argument("-verify", "Used to verify the .dat file."));
            arguments.Add(new Argument("-windowed", "Forces Guild Wars 2 to run in windowed mode. In game, you can switch to windowed mode by pressing Alt + Enter or clicking the window icon in the upper right corner."));
            arguments.Add(new Argument("-umbra gpu", "Forces the use of umbra's GPU accelerated culling. In most cases, using this results in higher cpu usage and lower gpu usage decreasing the frame-rate."));
            arguments.Add(new Argument("-maploadinfo", "Shows diagnostic information during map loads, including load percentages and elapsed time."));
            //arguments.Add(new Argument("-shareArchive", "Opens the Gw2.dat file in shared mode so that it can be accessed from other processes while the game is running."));
            // Secure
            arguments.Add(new Argument("-email", null, true));
            arguments.Add(new Argument("-password", null, true));
            arguments.OrderBy(a => a.Sensitive == false).ThenBy(a => a.Flag);
        }
        public Dictionary<string, string> ToDictionary(bool secure)
        {
            return arguments.Where(a => secure == false ? a.Sensitive == false : true).ToDictionary(a => a.Flag, a => a.Description);
        }
        public List<Argument> arguments { get; set; }
        public string Print(int? i = null)
        {
            AddTemp("-shareArchive");
            try
            {
                if (i == null)
                    return String.Join(" ", arguments.Where(a => a.Active == true).Select(a => a.Print));
                else
                    return Print(Globals.selected_accs.Count > i ? Globals.selected_accs[i.Value] : new Account());
            }
            finally
            {
                RemoveTemps();
            }
        }
        public string Print(Account account)
        {
            AddTemp("-email ", '"' + account.Email + '"');
            AddTemp("-password ", '"' + account.Password + '"');
            AddTemp("-nopatchui");
            return String.Join(" ", arguments.Where(a => a.Active).Select(a => a.Print));
        }
        public string PrintSterile(int? i = null)
        {
            if (i == null)
                return String.Join(" ", arguments.Where(a => a.Active).Where(a => !a.Sensitive).Select(a => a.Print)) + (Properties.Settings.Default.use_autologin ? " -AutoLogin" : "");
            else
                return PrintSterile(Globals.selected_accs.Count > i ? Globals.selected_accs[i.Value] : new Account());
        }
        public string PrintSterile(Account account)
        {
            return String.Join(" ", arguments.Where(a => a.Active).Where(a => !a.Sensitive).Select(a => a.Print)) + (Properties.Settings.Default.use_autologin ? " -AutoLogin" : "");
        }
        public void Argument(string flag, string description) { Argument(flag, description, null); }
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

        public void Add(string flag)
        {
            Add(flag, null);
        }
        public void Add(string flag, string option, bool? temp = null)
        {
            // If flag does not exist, add it as secure and select it.
            // Shouldn't really happen but nopatchui for instance is not in list and is needed.
            if (!arguments.Any(a => a.Flag == flag))
                arguments.Add(new Argument(flag, null, true));
            var arg = arguments.Where(a => a.Flag == flag).FirstOrDefault();

            arg.Active = true;
            arg.Option = option;
            arg.Temporary = temp ?? false;
        }
        public void AddTemp(string flag, string option = null)
        {
            Add(flag, option, true);
        }
        public void Remove(string flag)
        {
            arguments.Where(a => a.Flag == flag).FirstOrDefault().Active = false;
        }
        public void RemoveTemps()
        {
            arguments.Where(a => a.Temporary).All(a => { a.Active = false; a.Temporary = false; return true; });
        }
    }
    public class Argument
    {
        public string Flag { get; set; }
        public string Option { get; set; }
        public bool Active { get; set; }
        public string Description { get; set; }
        public string Print { get { return Flag != null ? Flag + (Option != null ? " " + Option : "") : ""; } set { } }
        public bool Sensitive { get; set; }
        public bool Temporary { get; set; }

        public Argument(string flag) : this(flag, null, false) { }
        public Argument(string flag, bool sensitive) : this(flag, null, sensitive) { }
        public Argument(string flag, string description) : this(flag, description, null) { }
        public Argument(string flag, string description, bool? sensitive)
        {
            Flag = flag;
            Description = description;
            Sensitive = sensitive ?? false;
            Active = false;
            Temporary = false;
        }
    }
}
