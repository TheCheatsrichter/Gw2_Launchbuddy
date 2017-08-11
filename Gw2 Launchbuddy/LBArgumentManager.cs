using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace Gw2_Launchbuddy
{
    public class LBArgument
    {
        public string command;
        public string data=null;
        public LBArgument(string input)
        {
            foreach(Match match in Regex.Matches(input, @"-(?<command>\w+)( ?""(?<data>\w+)""?)?"))
            {
                command = match.Groups["command"].Value;
                data = match.Groups["data"].Value;
            }
        }
    }

    static class LBArgumentManager
    {
        static public ObservableCollection<LBArgument> SetArgumentList(string input)
        {
            ObservableCollection<LBArgument> arglist = new ObservableCollection<LBArgument>();
            MatchCollection matches = Regex.Matches(input, @"(-\w+ ?(""[\w\d]+"")?)");
            foreach (Match match in matches)
            {
                arglist.Add(new LBArgument(match.Value));
            }
            return arglist;
        }

        static public void Execute(ObservableCollection<LBArgument> args)
        {
            foreach (LBArgument arg in args)
            {
                //Execute each command via switch
                switch (arg.command)
                {
                    case "nocinema":
                        Gw2_Launchbuddy.Properties.Settings.Default.cinema_use = false;
                        break;

                    case "autologin":
                        ArgCMD_Autologin(arg.data);
                        break;

                    default:
                        break;
                }
            }
        }

        static private void ArgCMD_Autologin(string accnick)
        {
            
        }

    }
}
