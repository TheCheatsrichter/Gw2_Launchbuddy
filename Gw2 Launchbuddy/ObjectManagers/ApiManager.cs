using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class Api
    {
        static Api()
        {
            WebClient api = new WebClient();
            Regex filter = new Regex(@"\d*\d");
            try
            {
                ClientBuild = filter.Match(api.DownloadString("https://api.guildwars2.com/v2/build")).Value;
                Online = true;
            }
            catch
            {
                Online = false;
                MessageBox.Show("The official Gw2 API is not reachable / down! Launchbuddy can't make sure that your game client is up to date.\nPlease keep your game manually up to date to avoid crashes!");
            }
        }

        public static bool Online { get; set; }
        public static string ClientBuild { get; set; }
    }
}
