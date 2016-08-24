using System;
using System.Collections.Generic;
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



namespace Gw2_Serverselection
{

    /// <summary>
    /// Gw2 Launchbuddy 
    /// </summary>
    public partial class MainWindow : Window
    {
        //List<String> authlist = new List<String>();
        //List<String> assetlist = new List<String>()
        ObservableCollection<Server> assetlist = new ObservableCollection<Server>();
        ObservableCollection<Server> authlist = new ObservableCollection<Server>();
        ObservableCollection<Account> accountlist = new ObservableCollection<Account>();

        Server selected_authsv = new Server();
        Server selected_assetsv = new Server();
        Account selected_acc = new Account();

        string exepath,exename;

        [Serializable]
        public class Server
        {
            public string IP { get; set; }
            public string Port { get; set; }
            public string Ping { get; set; }
            public string Type { get; set; }
            public string Location { get; set; }
        }

        [Serializable]
        public class Account
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public DateTime Time { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            accountlist.Clear();
            //createlist();
            loadconfig();
            loadaccounts();
            
        }

        void createlist()
        {
            authlist.Clear();
            assetlist.Clear();

            string default_auth1port = "6112";
            string default_auth2port = "6112";
            string default_assetport = "80";

            try {
                IPAddress[] auth1ips = Dns.GetHostAddresses("auth1.101.ArenaNetworks.com");
                IPAddress[] auth2ips = Dns.GetHostAddresses("auth2.101.ArenaNetworks.com");
                IPAddress[] assetips = Dns.GetHostAddresses("assetcdn.101.ArenaNetworks.com");

                foreach (IPAddress ip in auth1ips)
                {
                    authlist.Add(new Server { IP = ip.ToString(), Port = default_auth1port, Type = "auth1", Ping = getping(ip.ToString()).ToString() });
                }

                foreach (IPAddress ip in auth2ips)
                {

                    authlist.Add(new Server { IP = ip.ToString(), Port = default_auth1port, Type = "auth2", Ping = getping(ip.ToString()).ToString() });

                }

                foreach (IPAddress ip in assetips)
                {

                    assetlist.Add(new Server { IP = ip.ToString(), Port = default_assetport, Type = "asset", Ping = getping(ip.ToString()).ToString(), Location = getlocation(ip.ToString()) });
                }

                listview_auth.ItemsSource = authlist;
                listview_assets.ItemsSource = assetlist;
                lab_authserverlist.Content = "Athentication Servers (" + authlist.Count + " servers found):";
                lab_assetserverlist.Content = "Asset Servers APLHA (" + assetlist.Count + " servers found):";


            }
            catch
            {
                MessageBox.Show("Could not fetch Serverlist using hardcoded Serverlist!");

                try
                {
                    //(OLD VERSION) Harcoded server lists. Will only be used if DNS query could not be resolved


                    // Listed as auth1 servers (NA?)
                    authlist.Add(new Server {IP= "64.25.38.51",Port= default_auth1port, Ping = getping("64.25.38.51").ToString() } );
                    authlist.Add(new Server { IP = "64.25.38.54", Port = default_auth1port, Ping = getping("64.25.38.54").ToString() });
                    authlist.Add(new Server { IP = "64.25.38.205", Port = default_auth1port, Ping = getping("64.25.38.205").ToString() });
                    authlist.Add(new Server { IP = "64.25.38.171", Port = default_auth1port, Ping = getping("64.25.38.171").ToString() });
                    authlist.Add(new Server { IP = "64.25.38.172", Port = default_auth1port, Ping = getping("64.25.38.172").ToString() });

                    // Listed as auth2 servers (EU?)
                    authlist.Add(new Server { IP = "206.127.146.73", Port = default_auth2port, Ping = getping("206.127.146.73").ToString() });
                    authlist.Add(new Server { IP = "206.127.159.107", Port = default_auth2port, Ping = getping("206.127.159.107").ToString() });
                    authlist.Add(new Server { IP = "206.127.146.74", Port = default_auth2port, Ping = getping("206.127.146.74").ToString() });
                    authlist.Add(new Server { IP = "206.127.159.109", Port = default_auth2port, Ping = getping("206.127.159.109").ToString() });
                    authlist.Add(new Server { IP = "206.127.159.108", Port = default_auth2port, Ping = getping("206.127.159.108").ToString() });
                    authlist.Add(new Server { IP = "206.127.159.77", Port = default_auth2port, Ping = getping("206.127.159.77").ToString() });

                    // Assets servers 
                    assetlist.Add(new Server { IP = "54.192.201.89", Port = default_assetport, Ping = getping("54.192.201.89").ToString() });
                    assetlist.Add(new Server { IP = "54.192.201.14", Port = default_assetport, Ping = getping("54.192.201.14").ToString() });
                    assetlist.Add(new Server { IP = "54.192.201.65", Port = default_assetport, Ping = getping("54.192.201.65").ToString() });
                    assetlist.Add(new Server { IP = "54.192.201.68", Port = default_assetport, Ping = getping("54.192.201.68").ToString() });
                    assetlist.Add(new Server { IP = "54.192.201.41", Port = default_assetport, Ping = getping("54.192.201.41").ToString() });
                    assetlist.Add(new Server { IP = "54.192.201.155", Port = default_assetport, Ping = getping("54.192.201.155").ToString() });
                    assetlist.Add(new Server { IP = "54.192.201.83", Port = default_assetport, Ping = getping("54.192.201.83").ToString() });
                    assetlist.Add(new Server { IP = "54.192.201.5", Port = default_assetport, Ping = getping("54.192.201.5").ToString() });
                }
                catch(Exception err)
                {
                    MessageBox.Show("Could not create serverlists with hardcoded ips.\n" + err.Message);
                }                


            }


            

        }

        void loadconfig()
        {

            // Importing the XML file at AppData\Roaming\Guild Wars 2\
            // This file also contains infos about the graphic settings


            //Find the newest xml file in APPDATA (the xml files share the same name as their exe files -> multiple .xml files possible!)

            string[] configfiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\Guild Wars 2\","*.exe.xml");
            string sourcepath="";
            long max = 0;

            foreach (string config in configfiles)
            {
                if (System.IO.File.GetLastWriteTime(config).Ticks > max)
                {
                    max = System.IO.File.GetLastWriteTime(config).Ticks;
                    sourcepath = config;
                }  
            }

            // Read the xml file

            try
            {
                StreamReader stream = new System.IO.StreamReader(sourcepath);
                XmlTextReader reader = null;
                reader = new XmlTextReader(stream);

                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "VERSIONNAME":

                            lab_version.Content = "Client Version: " + getvalue(reader);
                            break;


                        case "INSTALLPATH":

                            exepath = getvalue(reader);
                            lab_path.Content = exepath;
                            break;

                        case "EXECUTABLE":

                            exename = getvalue(reader);
                            lab_path.Content += exename;
                            break;

                        case "EXECCMD":

                            lab_para.Content = "Latest Startparameters: ";
                            string[] parameters = getvalue(reader).Split('"')[2].Split(' ');
                            
                            foreach (string parameter in parameters)
                            {
                                lab_para.Content = lab_para.Content + " "+ parameter;
                                
                            }
                            // Automatically  set checks of previously used arguments

                            foreach (CheckBox entry in arglistbox.Items)
                            {
                                foreach (string parameter in parameters)
                                {                                   
                                    if (entry.Content.ToString() == parameter)
                                    {
                                        entry.IsChecked = true;
                                    }

                                }
                            }
                            
                            break;


                    }

                }

            }
            catch
            {
                MessageBox.Show("Gw2 info file not found! Please choose the Directory manualy!");
            }
        }

        string getvalue(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                string value = reader.Value;
                return value;
            }
            return null;
        }

        
        string getlocation(string ip)
        {
            //Getting the geolocation of the asset CDN servers
            try {
                using (var objClient = new System.Net.WebClient())
                {
                    var strFile = objClient.DownloadString("http://freegeoip.net/xml/" + ip);

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
        

        void pingservers()
        { 
            /*
            // Check the ping for each Server

            listview_auth.Items.Clear();
            listview_assets.Items.Clear();

            foreach (string server in authlist)
            {
                //Ugliest method to define servers. (Will be updated with upcoming xml serverlists)
                string ip = server.Split(':')[0];
                string port = server.Split(':')[1];
                string ping = getping(ip).ToString();
                string location = server.Split(':')[2];


                //string location = getlocation(ip);    //location seems always to be  texas 

                if (ping == "0")
                {
                    ping = "timeout";
                }

                listview_auth.Items.Add(
                    new
                    {
                        IP = ip,
                        Port = port,
                        Ping = ping,
                        Location = location,
                    });

            }

            foreach (string server in assetlist)
            {
                string ip = server.Split(':')[0];
                string port = server.Split(':')[1];
                string ping = getping(ip).ToString();
                string location = getlocation(ip); //geolocation of the CDN servers, rarely vary but still do vary


                if (ping == "0")
                {
                    ping = "timeout";
                }

                listview_assets.Items.Add(
                    new
                    {
                        IP = ip,
                        Port = port,
                        Ping = ping,
                        Location = location,
                    });

            }
            */

        }

        long getping(string ip)
        {
            // Get Ping in ms
            Ping pingsender = new Ping();
            return pingsender.Send(ip).RoundtripTime;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            createlist();
        }

        private void listview_assets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UI Handling for selected Asset Server
            if (listview_assets.Items.Count != 0)
            {
                var selectedItem = (dynamic)listview_assets.SelectedItems[0];
                selected_assetsv.IP = selectedItem.IP;
                selected_assetsv.Port = selectedItem.Port;
                selected_assetsv.Ping = selectedItem.Ping;
                tb_assetsport.Text = selectedItem.Port;

                checkb_assets.Content = "Use Assets Server : " + selected_assetsv.IP;
                checkb_assets.IsEnabled = true;
            }
            else
            {
                checkb_assets.IsChecked = false;
            }
        }
        private void listview_auth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UI Handling for selected Auth Server
            if (listview_auth.Items.Count != 0)
            {
                var selectedItem = (dynamic)listview_auth.SelectedItems[0];
                selected_authsv.IP = selectedItem.IP;
                selected_authsv.Port = selectedItem.Port;
                selected_authsv.Ping= selectedItem.Ping;
                tb_authport.Text = selectedItem.Port;

                checkb_auth.Content = "Use Authentication Server : " + selected_authsv.IP;
                checkb_auth.IsEnabled = true;
            } else
            {
                checkb_auth.IsChecked = false;
            }
        }

        private void checkb_auth_Checked(object sender, RoutedEventArgs e)
        {

            lab_port_auth.IsEnabled = true;
            tb_authport.IsEnabled = true;
            tb_authport.Text = selected_authsv.Port;

        }

        private void checkb_assets_Checked(object sender, RoutedEventArgs e)
        {

            lab_port_assets.IsEnabled = true;
            tb_assetsport.IsEnabled = true;
            tb_assetsport.Text = selected_assetsv.Port;

        }
        private void checkb_auth_Unchecked(object sender, RoutedEventArgs e)
        {
            lab_port_auth.IsEnabled = false;
            tb_authport.IsEnabled = false;
        }

        private void checkb_assets_Unchecked(object sender, RoutedEventArgs e)
        {
            lab_port_assets.IsEnabled = false;
            tb_assetsport.IsEnabled = false;
        }

        private void tb_authport_TextChanged(object sender, TextChangedEventArgs e)
        {
            selected_authsv.Port = tb_authport.Text;
        }

        private void tb_assetsport_TextChanged(object sender, TextChangedEventArgs e)
        {
            selected_assetsv.Port = tb_authport.Text;
        }


        private void bt_launch_Click(object sender, RoutedEventArgs e)
        {
            //Launching the application with arguments

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = exepath+exename;
            startInfo.Arguments = getarguments();

            try
            {
                System.Diagnostics.Process.Start(startInfo);
            }
            catch
            {
                System.Windows.MessageBox.Show("Could not launch Gw2. Invalid path?");
            }
        }



        private void bt_installpath_Click(object sender, RoutedEventArgs e)
        {
            //Alternative Path selection (when xml import fails)
            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.DefaultExt = "exe";
            filedialog.Multiselect = false;
            filedialog.Filter = "Exe Files(*.exe) | *.exe";
            filedialog.ShowDialog();


            if (filedialog.FileName != "")
            { 
                exepath = Path.GetDirectoryName(filedialog.FileName)+@"\";
                exename = Path.GetFileName(filedialog.Fi‌​leName);
                lab_path.Content = exepath+exename;
                Gw2_Launchbuddy.Properties.Settings.Default.exename = exename;
            }
        }

        

        public void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            // Modified Shortcut script by "CooLMinE" at http://www.fluxbytes.com/
            try
            {
                string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
                shortcut.Arguments = getarguments();
                shortcut.IconLocation = Assembly.GetExecutingAssembly().Location;

                shortcut.Description = "Created with Gw2 Launchbuddy, © TheCheatsrichter";

                shortcut.TargetPath = targetFileLocation;
                shortcut.Save();
                System.Windows.MessageBox.Show("Custom Launcher created at : " + exepath);
            }
            catch
            {
                MessageBox.Show("Error when creating shortcut. Invalid Path?");
            }                                

        }



        private void arglistbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Description switch. Should have used extern file / dictionary
            if (arglistbox.SelectedItem != null)
            {

                object selecteditem = arglistbox.SelectedItem;
                System.Windows.Controls.CheckBox item = (System.Windows.Controls.CheckBox)selecteditem;
                lab_descr.Content = "Description ("+ item.Content.ToString() + "):";

                switch (item.Content.ToString())
                {
                    case "-32":

                        textblock_descr.Text = "Forces the game to run in 32 bit.";
                        break;

                    case "-bmp":
                        textblock_descr.Text = "Forces the game to create lossless screenshots as .BMP files. Use for creating high-quality screenshots at the expense of much larger files.";
                        break;

                    case "-diag":
                        textblock_descr.Text = "Instead of launching the game, this command creates a detailed diagnostic file that contains diagnostic data that can be used for troubleshooting. The file, NetworkDiag.log, will be located in your game directory or Documents/Guild Wars . If you want to use this feature, be sure to create a separate shortcut for it.";
                        break;

                    case "-dx9single":
                        textblock_descr.Text = "Enables the Direct3D 9c renderer in single-threaded mode. Improves performance in Wine with CSMT.";
                        break;

                    case "-forwardrenderer":
                        textblock_descr.Text = "Uses Forward Rendering instead of Deferred Rendering (unfinished). This currently may lead to shadows and lighting to not appear as expected.It may increase the framerate and responsiveness when using AMD graphics card";
                        break;

                    case "-image":
                        textblock_descr.Text = "Runs the patch UI only in order to download any available updates; closes immediately without loading the login form. ";
                        break;

                    case "-log":
                        textblock_descr.Text = ("Enables the creation of a log file, used mostly by Support. The path for the generated file usually is found in the APPDATA folder");
                        break;

                    case "-mce":
                        textblock_descr.Text = "Start the client with Windows Media Center compatibility, switching the game to full screen and restarting Media Center (if available) after the client is closed.";
                        break;

                    case "-nomusic":
                        textblock_descr.Text = "Disables music and background music.";
                        break;

                    case "-noui":
                        textblock_descr.Text = "Disables the user interface. This does the same thing as pressing Ctrl+Shift+H in the game.";
                        break;

                    case "-nosound":
                        textblock_descr.Text = "Disables audio system completely.";
                        break;

                    case "-prefreset":
                        textblock_descr.Text = "Resets game settings.";
                        break;

                    case "-repair":
                        textblock_descr.Text = "Start the client, checks the files for errors and repairs them as needed. This can take a long time (1/2 hour or an hour) to run as it checks the entire contents of the 20-30 gigabyte archive.";
                        break;

                    case "-shareArchive":
                        textblock_descr.Text = "Opens the Gw2.dat file in shared mode so that it can be accessed from other processes while the game is running.";
                        break;

                    case "-uispanallmonitors":
                        textblock_descr.Text = "Spreads user interface across all monitors in a triple monitor setup.";
                        break;

                    case "-uninstall":
                        textblock_descr.Text = "Presents the uninstall dialog. If uninstall is accepted, it deletes the contents of the Guild Wars 2 installation folder except GW2.EXE itself and any manually created subfolders. Contents in subfolders (if any) are not deleted.";
                        break;

                    case "-useOldFov":
                        textblock_descr.Text = "Disables the widescreen field-of-view enhancements and restores the original field-of-view.";
                        break;

                    case "-verify":
                        textblock_descr.Text = "Used to verify the .dat file.";
                        break;

                    case "-windowed":
                        textblock_descr.Text = "Forces Guild Wars 2 to run in windowed mode. In game, you can switch to windowed mode by pressing Alt + Enter or clicking the window icon in the upper right corner.";
                        break;

                    case "-umbra gpu":
                        textblock_descr.Text = "Forces the use of umbra's GPU accelerated culling. In most cases, using this results in higher cpu usage and lower gpu usage decreasing the frame-rate.";
                        break;

                    case "-maploadinfo":
                        textblock_descr.Text = "Shows diagnostic information during map loads, including load percentages and elapsed time.";
                        break;

                    default:
                        textblock_descr.Text = "Description missing!. (PLS REPORT)";
                        break;
                }

            }

        }

        private void bt_shortcut_Click(object sender, RoutedEventArgs e)
        {
            CreateShortcut("Gw2_Custom_Launcher",exepath,exepath+exename);
            try
            {
                Process.Start(exepath);
            }
            catch(Exception err)
            {
                MessageBox.Show("Could not open file directory\n"+err.Message);
            }
        }

        private void checkb_clientport_Checked(object sender, RoutedEventArgs e)
        {
            tb_clientport.IsEnabled = true;
            lab_port_client.IsEnabled = true;

        }

        private void checkb_clientport_Unchecked(object sender, RoutedEventArgs e)
        {
            tb_clientport.IsEnabled = false;
            lab_port_client.IsEnabled = false;
        }


        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        void safeaccounts()
        {
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\Launchbuddy.bin";
                using (Stream stream = System.IO.File.Open(path, FileMode.Create))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    bformatter.Serialize(stream, accountlist);
                }
            }
            catch (Exception e)
            { MessageBox.Show(e.Message); }
        }

        void loadaccounts()
        {
            try {

                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\Launchbuddy.bin";

                if (System.IO.File.Exists(path) == true)
                {
                    using (Stream stream = System.IO.File.Open(path, FileMode.Open))
                    {
                        var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();


                        accountlist = (ObservableCollection<Account>)bformatter.Deserialize(stream);
                        listview_acc.ItemsSource = accountlist;

                    }
                }
            }
            catch(Exception e)
            { MessageBox.Show(e.Message); }
            
        }

        private void bt_addacc_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidEmail(tb_email.Text))
            {
                if (tb_passw.Text.Length > 4)
                {
                    Account acc = new Account { Email= tb_email.Text, Password= tb_passw.Text, Time = DateTime.Now};
                    accountlist.Add(acc);
                    listview_acc.ItemsSource = accountlist;
                }
                else
                {
                    MessageBox.Show("Invalid password");
                }
            
            }
            else
            {
                MessageBox.Show("Invalid Email!");
            }
        }

        private void cb_login_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Autologin does only function when no second Authentication (SMS,Email,App) is used on this account.\n To avoid this make sure that you current network is an authorized network (check always trust this network at login) or deactivate the second authentication!", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Warning);
            listview_acc.IsEnabled = true;
            lab_email.IsEnabled = true;
            lab_passw.IsEnabled = true;
            tb_passw.IsEnabled = true;
            tb_email.IsEnabled = true;
            bt_addacc.IsEnabled = true;
            bt_remacc.IsEnabled = true;
        }

        private void cb_login_Unchecked(object sender, RoutedEventArgs e)
        {
            listview_acc.IsEnabled = false;
            lab_email.IsEnabled = false;
            lab_passw.IsEnabled = false;
            tb_passw.IsEnabled = false;
            tb_email.IsEnabled = false;
            bt_addacc.IsEnabled = false;
            bt_remacc.IsEnabled = false;
        }

        private void listview_acc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_acc.SelectedItem != null)
            {
                var selectedItem = (dynamic)listview_acc.SelectedItem;
                cb_login.Content = "Use Autologin : " + selectedItem.Email;
                selected_acc.Email = selectedItem.Email;
                selected_acc.Password = selectedItem.Password;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            safeaccounts();
        }

        private void bt_remacc_Click(object sender, RoutedEventArgs e)
        {
            if (listview_acc.SelectedItem != null)
            {
                accountlist.Remove(listview_acc.SelectedItem as Account);
                listview_acc.SelectedIndex = -1;
            }
                
        }

        private string getarguments()
        {
            //Gathers all arguments and converts them into a formated single string

            string arguments = "";

            if (checkb_assets.IsChecked == true)
            {
                arguments += " - assetsrv " + selected_assetsv.IP + ":" + tb_assetsport.Text;
            }

            if (checkb_auth.IsChecked == true)
            {
                arguments += " -authsrv " + selected_authsv.IP + ":" + tb_authport.Text;
            }

            if (checkb_clientport.IsChecked == true)
            {
                arguments += " -clientport " + tb_clientport.Text;
            }

            if (cb_login.IsChecked == true)
            {
               
                if (selected_acc.Email != null && selected_acc.Password != null)
                {
                    arguments += "-nopatchui -email " + selected_acc.Email + " -password " + selected_acc.Password;
                }
                else
                {
                    MessageBox.Show("No Account selected! Launching without autologin.");
                }
                    
                
            }

            foreach (System.Windows.Controls.CheckBox entry in arglistbox.Items)
            {
                if (entry.IsChecked==true)
                {
                    arguments += " " + entry.Content;
                }
                
            }
            return arguments;
        }
    }

}
