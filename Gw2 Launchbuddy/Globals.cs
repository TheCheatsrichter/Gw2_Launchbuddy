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
        private static Microsoft.Win32.RegistryKey _LBRegKey = null;
        public static Microsoft.Win32.RegistryKey LBRegKey
        {
            get
            {
                return Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE").CreateSubKey("LaunchBuddy");
            }
            set { }
        }
        
        public static List<Account> selected_accs = new List<Account>();
        public static string exepath, exename, unlockerpath, version_client, version_api;

        public static Server selected_authsv = new Server();
        public static Server selected_assetsv = new Server();
        public static Arguments args = new Arguments();
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
            return String.Join(" ", arguments.Where(a => !a.Sensitive).Select(a => a.Print)) + (Properties.Settings.Default.use_autologin ? " -AutoLogin" : "") ;
        }
        public void Argument(string flag) { Argument(flag, null, null); }
        public void Argument(string flag, string option) { Argument(flag, option, null); }
        public void Argument(string flag, bool? sensitive) { Argument(flag, null, sensitive); }
        public void Argument(string flag, string option, bool? sensitive)
        {
            var arg = new Argument(flag,option,sensitive ?? false);
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
