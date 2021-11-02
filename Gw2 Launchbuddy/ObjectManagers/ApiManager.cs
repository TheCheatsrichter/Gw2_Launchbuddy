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
            int newest_build = int.MinValue;

            try
            {
                newest_build = int.Parse(filter_officialapi.Match(api.DownloadString("https://api.guildwars2.com/v2/build")).Value);
                Online = true;
            }
            catch
            {
                Online = false;
            }


            Regex filter_assetserver = new Regex(@"\d+");
            //Reserverd when more servers are online
            /*
            IPAddress[] assetips = Dns.GetHostAddresses("assetcdn.101.ArenaNetworks.com");
            
            try
            {
                foreach(var assetip in assetips)
                {
                    int assetversion= int.Parse(filter_assetserver.Match(api.DownloadString($"http://{assetip.ToString()}/latest64/101")).Value);
                    if (assetversion> newest_build)
                    {
                        newest_build = assetversion;
                    }
                }
            }
            catch
            {

            }
            */

            try
            {
                int assetversion = int.Parse(filter_assetserver.Match(api.DownloadString($"http://assetcdn.101.ArenaNetworks.com/latest64/101")).Value);
                if(assetversion > newest_build)
                {
                    newest_build = assetversion;
                }
            }
            catch
            {

            }

            if(newest_build > 115267)
            {
                ClientBuild = newest_build.ToString();
            }else
            {
                MessageBox.Show("The official Guild Wars 2 API is unreachable or down and the asset servers not reachable! Launchbuddy can't make sure that your game client is up to date.\nPlease keep your game manually up to date to avoid crashes!");
            }
        }

        public static bool Online { get; set; }
        public static string ClientBuild { get; set; }
    }
}
