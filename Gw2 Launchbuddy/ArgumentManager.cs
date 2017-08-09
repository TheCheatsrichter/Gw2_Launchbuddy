using System;
using System.Collections.Generic;
using System.Linq;
using static Gw2_Launchbuddy.MainWindow;

namespace Gw2_Launchbuddy
{
    public static class ArgumentManager
    {
        private static List<Argument> argumentList = new List<Argument>();

        static ArgumentManager()
        {
            Add("-32", "Forces the game to run in 32 bit.").IsActive().IsSelectable();
            Add("-bmp", "Forces the game to create lossless screenshots as .BMP files. Use for creating high-quality screenshots at the expense of much larger files.").IsActive().IsSelectable();
            Add("-diag", "Instead of launching the game, this command creates a detailed diagnostic file that contains diagnostic data that can be used for troubleshooting. The file, NetworkDiag.log, will be located in your game directory or Documents/Guild Wars . If you want to use this feature, be sure to create a separate shortcut for it.").IsActive().IsSelectable().IsBlocker();
            Add("-dx9single", "Enables the Direct3D 9c renderer in single-threaded mode. Improves performance in Wine with CSMT.").IsActive().IsSelectable();
            Add("-forwardrenderer", "Uses Forward Rendering instead of Deferred Rendering (unfinished). This currently may lead to shadows and lighting to not appear as expected.It may increase the framerate and responsiveness when using AMD graphics card").IsActive().IsSelectable();
            Add("-image", "Runs the patch UI only in order to download any available updates)); closes immediately without loading the login form. ").IsActive().IsSelectable().IsBlocker();
            Add("-log", "Enables the creation of a log file, used mostly by Support. The path for the generated file usually is found in the APPDATA folder").IsActive().IsSelectable().IsBlocker();
            Add("-mce", "Start the client with Windows Media Center compatibility, switching the game to full screen and restarting Media Center (if available) after the client is closed.").IsActive().IsSelectable();
            Add("-nomusic", "Disables music and background music.").IsActive().IsSelectable();
            Add("-noui", "Disables the user interface. This does the same thing as pressing Ctrl+Shift+H in the game.").IsActive().IsSelectable();
            Add("-nosound", "Disables audio system completely.").IsActive().IsSelectable();
            Add("-prefreset", "Resets game settings.").IsActive().IsSelectable();
            Add("-repair", "Start the client, checks the files for errors and repairs them as needed. This can take a long time (1/2 hour or an hour) to run as it checks the entire contents of the 20-30 gigabyte archive.").IsActive().IsSelectable().IsBlocker();
            Add("-uispanallmonitors", "Spreads user interface across all monitors in a triple monitor setup.").IsActive().IsSelectable();
            Add("-uninstall", "Presents the uninstall dialog. If uninstall is accepted, it deletes the contents of the Guild Wars 2 installation folder except GW2.EXE itself and any manually created subfolders. Contents in subfolders (if any) are not deleted.").IsActive().IsSelectable();
            Add("-useOldFov", "Disables the widescreen field-of-view enhancements and restores the original field-of-view.").IsActive().IsSelectable();
            Add("-verify", "Used to verify the .dat file.").IsActive().IsSelectable().IsBlocker();
            Add("-windowed", "Forces Guild Wars 2 to run in windowed mode. In game, you can switch to windowed mode by pressing Alt + Enter or clicking the window icon in the upper right corner.").IsActive().IsSelectable();
            Add("-umbra gpu", "Forces the use of umbra's GPU accelerated culling. In most cases, using this results in higher cpu usage and lower gpu usage decreasing the frame-rate.").IsActive().IsSelectable();
            Add("-maploadinfo", "Shows diagnostic information during map loads, including load percentages and elapsed time.").IsActive().IsSelectable();
            Add("-shareArchive", "Opens the Gw2.dat file in shared mode so that it can be accessed from other processes while the game is running.").IsActive();//.IsSelected();
            Add("-nopatchui", "Hides the user interface during the update process.").IsActive();
            Add("-email", null).IsActive().IsSensitive();
            Add("-password", null).IsActive().IsSensitive();

            //IsBlocker needs to be added to -exit and -allowinstall when added.
        }

        public static Argument Add(Argument Argument)
        {
            argumentList.Add(Argument);
            return Argument;
        }
        public static Argument Add(string Flag, string Description = null) { return Add(new Argument(Flag, Description)); }

        public static Argument Argument(string Flag) {
            return argumentList.Where(a => a.Flag == Flag).SingleOrDefault() ?? Add(Flag, "??????????").IsActive();
        }

        public static void Remove(this Argument argument) { argumentList.Remove(argument); }

        public static List<Argument> ToList() {  return argumentList; }

        public static Dictionary<string, Argument> ToDictionary(bool IncludeSensitive = false)
        {
            return argumentList.Where(a => a.Active).ToDictionary(a => a.Flag, a => a);
        }
    }

    public class Argument
    {
        public Argument(string Flag, string Description = null)
        {
            this.Flag = Flag;
            this.Description = Description;
        }

        public string Flag { get; private set; }
        public string Description { get; private set; }

        public Argument IsSensitive(bool Sensitive = true) { this.Sensitive = Sensitive; return this; }
        public Argument IsActive(bool Active = true) { this.Active = Active; return this; }
        public Argument IsBlocker(bool Blocker = true) { this.Blocker = Blocker; return this; }
        public Argument IsTemporary (bool Temporary = true) { this.Temporary = Temporary; return this; }
        public Argument IsSelectable(bool Selectable = true) { this.Selectable = Selectable; return this; }

        public bool Sensitive { get; private set; }
        public bool Active { get; private set; }
        public bool Blocker { get; private set; }
        public bool Temporary { get; private set; }
        public bool Selectable { get; private set; }
    }
    #region Scrapped Argument
    /* Scrapped for method chaining.
    public class Argument
    {
            public Argument(string Flag, string Description, Is Properties = Is.None)
            {
                properties = Properties;
                this.Flag = Flag;
                this.Description = Description;
            }

            public string String { get; set; }
            public bool Selected { get { return properties.HasFlag(Is.Selected); } set { properties &= value ? Is.Selected : ~Is.Selected; } }

            public string Flag { get; private set; }
            public string Description { get; private set; }

            public bool Sensitive { get { return properties.HasFlag(Is.Sensitive); } }
            public bool Active { get { return properties.HasFlag(Is.Active); } }
            public bool Blocker { get { return properties.HasFlag(Is.Blocker); } }
            public bool Temporary { get { return properties.HasFlag(Is.Temporary); } }
            public bool Selectable { get { return properties.HasFlag(Is.Selectable); } }

            [Flags]
            public enum Is
            {
                None = 0,
                Sensitive = 1 << 0,
                Active = 1 << 1,
                Blocker = 1 << 2,
                Temporary = 1 << 3,
                Selectable = 1 << 4,
                Selected = 1 << 5,
            }
            private Is properties;
    }*/
#endregion
            /*
            public class ArgumentsDead
            {
                public Arguments()
                {
                    arguments = new List<ArgumentOld>();
                    // Normal
                    Arguments.Add(new ArgumentOld("-32", "Forces the game to run in 32 bit."));
                    //Arguments.Add(new Argument("-64", "Forces the game to run in 64 bit."));
                    Arguments.Add(new ArgumentOld("-bmp", "Forces the game to create lossless screenshots as .BMP files. Use for creating high-quality screenshots at the expense of much larger files."));
                    Arguments.Add(new ArgumentOld("-diag", "Instead of launching the game, this command creates a detailed diagnostic file that contains diagnostic data that can be used for troubleshooting. The file, NetworkDiag.log, will be located in your game directory or Documents/Guild Wars . If you want to use this feature, be sure to create a separate shortcut for it."));
                    Arguments.Add(new ArgumentOld("-dx9single", "Enables the Direct3D 9c renderer in single-threaded mode. Improves performance in Wine with CSMT."));
                    Arguments.Add(new ArgumentOld("-forwardrenderer", "Uses Forward Rendering instead of Deferred Rendering (unfinished). This currently may lead to shadows and lighting to not appear as expected.It may increase the framerate and responsiveness when using AMD graphics card"));
                    Arguments.Add(new ArgumentOld("-image", "Runs the patch UI only in order to download any available updates)); closes immediately without loading the login form. "));
                    Arguments.Add(new ArgumentOld("-log", "Enables the creation of a log file, used mostly by Support. The path for the generated file usually is found in the APPDATA folder"));
                    Arguments.Add(new ArgumentOld("-mce", "Start the client with Windows Media Center compatibility, switching the game to full screen and restarting Media Center (if available) after the client is closed."));
                    Arguments.Add(new ArgumentOld("-nomusic", "Disables music and background music."));
                    Arguments.Add(new ArgumentOld("-noui", "Disables the user interface. This does the same thing as pressing Ctrl+Shift+H in the game."));
                    Arguments.Add(new ArgumentOld("-nosound", "Disables audio system completely."));
                    Arguments.Add(new ArgumentOld("-prefreset", "Resets game settings."));
                    Arguments.Add(new ArgumentOld("-repair", "Start the client, checks the files for errors and repairs them as needed. This can take a long time (1/2 hour or an hour) to run as it checks the entire contents of the 20-30 gigabyte archive."));
                    Arguments.Add(new ArgumentOld("-uispanallmonitors", "Spreads user interface across all monitors in a triple monitor setup."));
                    Arguments.Add(new ArgumentOld("-uninstall", "Presents the uninstall dialog. If uninstall is accepted, it deletes the contents of the Guild Wars 2 installation folder except GW2.EXE itself and any manually created subfolders. Contents in subfolders (if any) are not deleted."));
                    Arguments.Add(new ArgumentOld("-useOldFov", "Disables the widescreen field-of-view enhancements and restores the original field-of-view."));
                    Arguments.Add(new ArgumentOld("-verify", "Used to verify the .dat file."));
                    Arguments.Add(new ArgumentOld("-windowed", "Forces Guild Wars 2 to run in windowed mode. In game, you can switch to windowed mode by pressing Alt + Enter or clicking the window icon in the upper right corner."));
                    Arguments.Add(new ArgumentOld("-umbra gpu", "Forces the use of umbra's GPU accelerated culling. In most cases, using this results in higher cpu usage and lower gpu usage decreasing the frame-rate."));
                    Arguments.Add(new ArgumentOld("-maploadinfo", "Shows diagnostic information during map loads, including load percentages and elapsed time."));
                    Arguments.Add(new ArgumentOld("-shareArchive", "Opens the Gw2.dat file in shared mode so that it can be accessed from other processes while the game is running.", true, false, false));
                    Arguments.Add(new ArgumentOld("-nopatchui", "Hides the user interface during the update process.", true, false, false));
                    // Secure
                    Arguments.Add(new ArgumentOld("-email", null, sensitive: true));
                    Arguments.Add(new ArgumentOld("-password", null, sensitive: true));
                    Arguments.OrderBy(a => a.Sensitive == false).ThenBy(a => a.Flag);
                }
                public Dictionary<string, ArgumentOld> ToDictionary(bool secure)
                {
                    return Arguments.Where(a => secure == false ? a.Sensitive == false : true).ToDictionary(a => a.Flag, a => a);
                }
                public List<ArgumentOld> arguments { get; set; }
                public string Print(int? i = null)
                {
                    //if (Globals.ClientIsUptodate) AddTemp("-shareArchive");
                    try
                    {
                        if (i == null)
                            return String.Join(" ", Arguments.Where(a => a.Selected == true).Select(a => a.Print));
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
                    //AddTemp("-nopatchui");
                    return String.Join(" ", Arguments.Where(a => a.Selected).Select(a => a.Print));
                }
                public string PrintSterile(int? i = null)
                {
                    if (i == null)
                        return String.Join(" ", Arguments.Where(a => a.Selected).Where(a => !a.Sensitive).Select(a => a.Print)) + (Properties.Settings.Default.use_autologin ? " -AutoLogin" : "");
                    else
                        return PrintSterile(Globals.selected_accs.Count > i ? Globals.selected_accs[i.Value] : new Account());
                }
                public string PrintSterile(Account account)
                {
                    return String.Join(" ", Arguments.Where(a => a.Selected).Where(a => !a.Sensitive).Select(a => a.Print)) + (Properties.Settings.Default.use_autologin ? " -AutoLogin" : "");
                }
                public void Argument(string flag, string description) { Argument(flag, description, null); }
                public void Argument(string flag, string option, bool? sensitive)
                {
                    var arg = new ArgumentOld(flag, option, sensitive: sensitive ?? false);
                    if (Arguments.Any(a => a.Flag == flag))
                        arg = Arguments.Where(a => a.Flag == flag).First();
                    else
                        Arguments.Add(arg);
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
                    if (!Arguments.Any(a => a.Flag == flag))
                        Arguments.Add(new ArgumentOld(flag, sensitive: true));
                    var arg = Arguments.Where(a => a.Flag == flag).FirstOrDefault();

                    arg.Selected = true;
                    arg.Option = option;
                    arg.Temporary = temp ?? false;
                }
                public void AddTemp(string flag, string option = null)
                {
                    Add(flag, option, true);
                }
                public void Remove(string flag)
                {
                    Arguments.Where(a => a.Flag == flag).FirstOrDefault().Selected = false;
                }
                public void RemoveTemps()
                {
                    Arguments.Where(a => a.Temporary).All(a => { a.Selected = false; a.Temporary = false; return true; });
                }
            }
            public class ArgumentOld
            {
                public string Flag { get; set; }
                public string Option { get; set; }
                public bool Selected { get; set; }
                public string Description { get; set; }
                public string Print { get { return Flag != null ? Flag + (Option != null ? " " + Option : "") : ""; } set { } }
                public bool Sensitive { get; set; }
                public bool Temporary { get; set; }
                public bool Selectable { get; set; }
                public bool Active { get; set; }


                public ArgumentOld(string flag) : this(flag, null, active: true, selectable: true, sensitive: false, load: true) { }
                public ArgumentOld(string flag, bool sensitive) : this(flag, null, active: true, selectable: true, sensitive: sensitive, load: true) { }
                public ArgumentOld(string flag, string description) : this(flag, description, active: true, selectable: true, sensitive: null, load: true) { }
                public ArgumentOld(string flag, string description, bool sensitive) : this(flag, description, active: true, selectable: true, sensitive: sensitive, load: true) { }
                public ArgumentOld(string flag, string description, bool? active, bool? selectable, bool? sensitive, bool? load)
                {
                    Flag = flag;
                    Description = description;
                    Sensitive = sensitive ?? false;
                    Selected = false;
                    Temporary = false;
                    Selectable = selectable ?? true;
                    Active = active ?? true;
                    //Load = load ?? false;
                }

            }
            public class ArgumentProperties
            {
                public bool IsSensitive { get; set; }
                public bool IsActive { get; set; }
                public bool AllowLoad { get; set; }
                public bool IsTemporary { get; set; }
                public bool IsSelectable { get; set; }
            }
            public class ArgumentInfo
            {
                public string Flag { get; set; }
                public string Description { get; set; }
            }

            public class Argumented
            {
                public ArgumentInfo ArgumentInfo { get; set; }
                public ArgumentProperties ArgumentProperties { get; set; }
                public bool IsSelected { get; set; }
                public string Options { get; set; }
            }

            public class PropertyAttribute : Attribute
            {
                public PropertyAttribute()
                {
                    IsSensitive = false;
                    IsActive = true;
                    AllowLoad = true;
                    IsTemporary = false;
                    IsSelectable = true;
                }
            }*/
        }
