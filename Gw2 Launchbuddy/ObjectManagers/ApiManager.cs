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
            Regex filter_officialapi = new Regex(@"\d*\d");
            try
            {
                ClientBuild = filter_officialapi.Match(api.DownloadString("https://api.guildwars2.com/v2/build")).Value;

                if(ClientBuild == "115267")
                {
                    Regex filter_assetserver = new Regex(@"\d+");
                    ClientBuild = filter_assetserver.Match(api.DownloadString("http://assetcdn.101.arenanetworks.com/latest64/101")).Value;
                    
                }

                Online = true;
            }
            catch
            {
                Online = false;
                MessageBox.Show("The official Guild Wars 2 API is unreachable or down! Launchbuddy can't make sure that your game client is up to date.\nPlease keep your game manually up to date to avoid crashes!");
            }
        }

        public static bool Online { get; set; }
        public static string ClientBuild { get; set; }
    }
}
