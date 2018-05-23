using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Xml;
using System.Text.RegularExpressions;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public class Server
    {
        public string IP { get; set; }
        public string Port { get; set; }
        public string Ping { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
    }

    public static class ServerManager
    {
        public static ObservableCollection<Server> authservers= new ObservableCollection<Server>();
        public static ObservableCollection<Server> assetservers= new ObservableCollection<Server>();

        private static string default_auth1port = "6112";
        private static string default_auth2port = "6112";
        private static string default_assetport = "80";

        public static void UpdateServerlists()
        {
            assetservers=fetch_assetserverlist();
            authservers = fetch_authserverlist();
        }

        public static void AddAuthServer(string ip)
        {
            if (Regex.Match(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")!=null)
            {
                authservers.Add(new Server { IP = ip.ToString(), Port = default_auth1port, Type = "-", Ping = tcpping(ip.ToString(), default_auth1port).ToString() });
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid IP input.");
            }
        }

        public static void AddAssetServer(string ip)
        {
            if (Regex.Match(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}") != null)
            {
                assetservers.Add(new Server { IP = ip.ToString(), Port = default_assetport, Type = "-", Ping = getping(ip).ToString(), Location = getlocation(ip.ToString()) });
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid IP input.");
            }
        }


        private static ObservableCollection<Server> fetch_authserverlist()
        {
            ObservableCollection<Server> tmp_authlist = new ObservableCollection<Server>();
            ObservableCollection<Server> tmp_assetlist = new ObservableCollection<Server>();
           

            try
            {
                IPAddress[] auth1ips = Dns.GetHostAddresses("auth1.101.ArenaNetworks.com");
                IPAddress[] auth2ips = Dns.GetHostAddresses("auth2.101.ArenaNetworks.com");

                foreach (IPAddress ip in auth1ips)
                {
                    tmp_authlist.Add(new Server { IP = ip.ToString(), Port = default_auth1port, Type = "auth1", Ping = tcpping(ip.ToString(), default_auth1port).ToString() });
                }

                foreach (IPAddress ip in auth2ips)
                {
                    tmp_authlist.Add(new Server { IP = ip.ToString(), Port = default_auth2port, Type = "auth2", Ping = tcpping(ip.ToString(), default_auth2port).ToString() });
                }

                /*Temporary disabled
                foreach (Server Server in Globals.manual_authlist)
                {
                    tmp_authlist.Add(new Server { IP = Server.IP, Port = default_auth2port, Type = "Manual", Ping = tcpping(Server.IP, default_auth2port).ToString() });
                }
                */

                return tmp_authlist;
            }
            catch
            {
                return null;
            }
        }

        private static ObservableCollection<Server> fetch_assetserverlist()
        {
            ObservableCollection<Server> tmp_assetlist = new ObservableCollection<Server>();

            try
            {
                IPAddress[] assetips = Dns.GetHostAddresses("assetcdn.101.ArenaNetworks.com");

                foreach (IPAddress ip in assetips)
                {
                    tmp_assetlist.Add(new Server { IP = ip.ToString(), Port = default_assetport, Type = "asset", Ping = getping(ip.ToString()).ToString(), Location = getlocation(ip.ToString()) });
                }
                /*Temporary disabled
                foreach (Server Server in Globals.manual_assetlist)
                {
                    tmp_assetlist.Add(new Server { IP = Server.IP, Port = default_assetport, Type = "Manual", Ping = tcpping(Server.IP, default_assetport).ToString() });
                }
                */

                return tmp_assetlist;
            }
            catch
            {
                return null;
            }
        }

        public static long getping(string ip)
        {
            // Get Ping in ms
            Ping pingsender = new Ping();
            return (int)pingsender.Send(ip).RoundtripTime;
        }

        public static double tcpping(string ip, string port)
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Blocking = true;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                sock.Connect(ip, Int32.Parse(port));
            }
            catch
            {
                return 9999;
            }

            stopwatch.Stop();
            double t = stopwatch.Elapsed.TotalMilliseconds;
            sock.Close();

            return Math.Round(t, 0);
        }

        public static string getlocation(string ip)
        {
            //Getting the geolocation of the asset CDN servers
            //This might be one origin of AV flagging the exe!
            try
            {
                using (var objClient = new System.Net.WebClient())
                {
                    var strFile = objClient.DownloadString("http://freegeoip.net/xml/" + ip); // limited to 100 requests / hour !

                    using (XmlReader reader = XmlReader.Create(new StringReader(strFile)))
                    {
                        reader.ReadToFollowing("RegionName");
                        return reader.ReadInnerXml();
                    }
                }
            }
            catch
            {
                return "-";
            }
        }
    }
}
