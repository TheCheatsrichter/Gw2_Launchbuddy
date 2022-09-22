using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Gw2_Launchbuddy.ObjectManagers;
using System.Windows;

namespace Gw2_Launchbuddy.Helpers
{
    public static class PublicIPFetcher
    {

        static IPAddress last_ipaddress
        {
            set { LBConfiguration.Config.last_ipaddress = value.ToString(); LBConfiguration.Save(); }
            get {
                try
                {
                    return IPAddress.Parse(LBConfiguration.Config.last_ipaddress);
                }
                catch
                {
                    return null;
                }
                
            }
        }
        static DateTime timestamp_ipchange= DateTime.MinValue;

        public static DateTime Time_LastIpChange { get { return timestamp_ipchange; } }

        public static IPAddress UpdateIP()
        {
            List<string> services = new List<string>()
            {
                "https://ipv4.icanhazip.com/",
                "https://ipinfo.io/ip",
                "https://checkip.amazonaws.com",
                "https://wtfismyip.com/text",
                "https://api.ipify.org",
                "http://icanhazip.com"
            };
            using (var webclient = new WebClient { Proxy = WebRequest.GetSystemWebProxy() })
                foreach (var service in services)
                {
                    try {
                        var ipaddress = IPAddress.Parse(webclient.DownloadString(service));
                        if (!ipaddress.Equals(last_ipaddress) && last_ipaddress != null)
                        {
                            timestamp_ipchange = DateTime.Now; // Doesnt update when VPN disbaled --> enabled but the other way arround
                        }
                        last_ipaddress = ipaddress;
                        return ipaddress;
                    } 
                    catch { }
                }
            return null;
        }
    }
}
