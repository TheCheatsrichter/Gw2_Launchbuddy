using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.NetworkInformation;
using System.Xml;
using System.IO;
using System.Diagnostics;
using IWshRuntimeLibrary;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.IO.Compression;
using System.Windows.Media.Animation;
using System.Xml.Serialization;
using System.Net.Sockets;

namespace Gw2_Launchbuddy
{

    public class Server
    {
        public string IP { get; set; }
        public string Port { get; set; }
        public string Ping { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
    }

    public static class Serverselector
    {
        public static ObservableCollection<Server> fetch_authserverlist()
        {
            ObservableCollection<Server> tmp_authlist = new ObservableCollection<Server>();
            ObservableCollection<Server> tmp_assetlist = new ObservableCollection<Server>();
            string default_auth1port = "6112";
            string default_auth2port = "6112";

            try
            {
                IPAddress[] auth1ips = Dns.GetHostAddresses("auth1.101.ArenaNetworks.com");
                IPAddress[] auth2ips = Dns.GetHostAddresses("auth2.101.ArenaNetworks.com");

                foreach (IPAddress ip in auth1ips)
                {
                    tmp_authlist.Add(new Server { IP = ip.ToString(), Port = default_auth1port, Type = "auth1", Ping = tcpping(ip,default_auth1port).ToString() });
                }

                foreach (IPAddress ip in auth2ips)
                {
                    tmp_authlist.Add(new Server { IP = ip.ToString(), Port = default_auth2port, Type = "auth2", Ping = tcpping(ip, default_auth2port).ToString() });
                }
                return tmp_authlist;
            }
            catch
            {
                return null;
            }
        }

        public static ObservableCollection<Server> fetch_assetserverlist()
        {
            ObservableCollection<Server> tmp_assetlist = new ObservableCollection<Server>();
            string default_assetport = "80";

            try
            {
                IPAddress[] assetips = Dns.GetHostAddresses("assetcdn.101.ArenaNetworks.com");

                foreach (IPAddress ip in assetips)
                {
                    tmp_assetlist.Add(new Server { IP = ip.ToString(), Port = default_assetport, Type = "asset", Ping = getping(ip.ToString()).ToString(), Location = getlocation(ip.ToString()) });
                }

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

        public static double tcpping(IPAddress ip, string port)
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Blocking = true;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            sock.Connect(ip,Int32.Parse(port));
            stopwatch.Stop();
            double t = stopwatch.Elapsed.TotalMilliseconds;
            sock.Close();

            return Math.Round(t,0);
        }

        public static string getlocation(string ip)
        {
            //Getting the geolocation of the asset CDN servers
            //This might be the origin of AV flagging the exe!
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
