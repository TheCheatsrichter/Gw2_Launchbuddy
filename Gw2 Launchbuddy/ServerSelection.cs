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

namespace Gw2_Launchbuddy
{ 
    [Serializable]
    public class Server
    {
        public string IP { get; set; }
        public string Port { get; set; }
        public string Ping { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
    }


    class ServerSelection
    {
        static ObservableCollection<Server> assetlist = new ObservableCollection<Server>();
        static ObservableCollection<Server> authlist = new ObservableCollection<Server>();

        private long getping(string ip)
        {
            // Get Ping in ms
            Ping pingsender = new Ping();
            return (int)pingsender.Send(ip).RoundtripTime;
        }

        private string getlocation(string ip)
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


        public void CreateList()
        {
            ObservableCollection<Server> tmp_authlist = new ObservableCollection<Server>();
            ObservableCollection<Server> tmp_assetlist = new ObservableCollection<Server>();

            string default_auth1port = "6112";
            string default_auth2port = "6112";
            string default_assetport = "80";

            try
            {
                IPAddress[] auth1ips = Dns.GetHostAddresses("auth1.101.ArenaNetworks.com");
                IPAddress[] auth2ips = Dns.GetHostAddresses("auth2.101.ArenaNetworks.com");
                IPAddress[] assetips = Dns.GetHostAddresses("assetcdn.101.ArenaNetworks.com");

                foreach (IPAddress ip in auth1ips)
                {
                    tmp_authlist.Add(new Server { IP = ip.ToString(), Port = default_auth1port, Type = "auth1", Ping = getping(ip.ToString()).ToString() });
                }

                foreach (IPAddress ip in auth2ips)
                {
                    tmp_authlist.Add(new Server { IP = ip.ToString(), Port = default_auth1port, Type = "auth2", Ping = getping(ip.ToString()).ToString() });
                }

                foreach (IPAddress ip in assetips)
                {
                    tmp_assetlist.Add(new Server { IP = ip.ToString(), Port = default_assetport, Type = "asset", Ping = getping(ip.ToString()).ToString(), Location = getlocation(ip.ToString()) });
                }
            }
            catch
            {
                MessageBox.Show("Could not fetch Serverlist.");
            }

            try
            {
                Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => updateserverlist(tmp_authlist, tmp_assetlist)));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }



        }

        public void updateserverlist(ObservableCollection<Server> newauthlist, ObservableCollection<Server> newassetlist)
        {
            MainWindow.bt_checkservers.IsEnabled = true;

            authlist.Clear();
            assetlist.Clear();
            authlist = newauthlist;
            assetlist = newassetlist;
            listview_auth.ItemsSource = authlist;
            listview_assets.ItemsSource = assetlist;
            lab_authserverlist.Content = "Authentication Servers (" + authlist.Count + " servers found):";
            lab_assetserverlist.Content = "Asset Servers (" + assetlist.Count + " servers found):";
            bt_checkservers.Content = "Check Servers (Last update: " + DateTime.Now.ToString("h:mm:ss tt") + ")";

            // Sorting  servers (ping).
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listview_auth.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Descending));
            CollectionView sview = (CollectionView)CollectionViewSource.GetDefaultView(listview_assets.ItemsSource);
            sview.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Descending));
            sview.Refresh();
        }



    }



}
