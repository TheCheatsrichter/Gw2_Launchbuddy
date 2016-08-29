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



namespace Gw2_Launchbuddy
{


    /// <summary>
    /// Gw2 Launchbuddy 
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Object prefix keys:
        /// 
        ///     bt= button
        ///     lab = label
        ///     
        /// 
        /// 
        /// </summary>

        //List<String> authlist = new List<String>();
        //List<String> assetlist = new List<String>()
        ObservableCollection<Server> assetlist = new ObservableCollection<Server>();
        ObservableCollection<Server> authlist = new ObservableCollection<Server>();
        ObservableCollection<Account> accountlist = new ObservableCollection<Account>();

        Server selected_authsv = new Server();
        Server selected_assetsv = new Server();
        Account selected_acc = new Account();

        string exepath, exename , unlockerpath;

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
            
            accountlist.Clear(); //clearing accountlist
            loadconfig(); // loading the gw2 xml config file from appdata
            loadaccounts(); // loading saved accounts from launchbuddy

        }

        void createlist()
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
                MessageBox.Show("Could not fetch Serverlist. Using hardcoded Serverlist!");

                try
                {
                    //(OLD VERSION) Harcoded server lists. Will only be used if DNS query could not be resolved


                    // Listed as auth1 servers (NA?)
                    tmp_authlist.Add(new Server { IP = "64.25.38.51", Port = default_auth1port, Ping = getping("64.25.38.51").ToString() });
                    tmp_authlist.Add(new Server { IP = "64.25.38.54", Port = default_auth1port, Ping = getping("64.25.38.54").ToString() });
                    tmp_authlist.Add(new Server { IP = "64.25.38.205", Port = default_auth1port, Ping = getping("64.25.38.205").ToString() });
                    tmp_authlist.Add(new Server { IP = "64.25.38.171", Port = default_auth1port, Ping = getping("64.25.38.171").ToString() });
                    tmp_authlist.Add(new Server { IP = "64.25.38.172", Port = default_auth1port, Ping = getping("64.25.38.172").ToString() });

                    // Listed as auth2 servers (EU?)
                    tmp_authlist.Add(new Server { IP = "206.127.146.73", Port = default_auth2port, Ping = getping("206.127.146.73").ToString() });
                    tmp_authlist.Add(new Server { IP = "206.127.159.107", Port = default_auth2port, Ping = getping("206.127.159.107").ToString() });
                    tmp_authlist.Add(new Server { IP = "206.127.146.74", Port = default_auth2port, Ping = getping("206.127.146.74").ToString() });
                    tmp_authlist.Add(new Server { IP = "206.127.159.109", Port = default_auth2port, Ping = getping("206.127.159.109").ToString() });
                    tmp_authlist.Add(new Server { IP = "206.127.159.108", Port = default_auth2port, Ping = getping("206.127.159.108").ToString() });
                    tmp_authlist.Add(new Server { IP = "206.127.159.77", Port = default_auth2port, Ping = getping("206.127.159.77").ToString() });

                    // Assets servers 
                    tmp_assetlist.Add(new Server { IP = "54.192.201.89", Port = default_assetport, Ping = getping("54.192.201.89").ToString() });
                    tmp_assetlist.Add(new Server { IP = "54.192.201.14", Port = default_assetport, Ping = getping("54.192.201.14").ToString() });
                    tmp_assetlist.Add(new Server { IP = "54.192.201.65", Port = default_assetport, Ping = getping("54.192.201.65").ToString() });
                    tmp_assetlist.Add(new Server { IP = "54.192.201.68", Port = default_assetport, Ping = getping("54.192.201.68").ToString() });
                    tmp_assetlist.Add(new Server { IP = "54.192.201.41", Port = default_assetport, Ping = getping("54.192.201.41").ToString() });
                    tmp_assetlist.Add(new Server { IP = "54.192.201.155", Port = default_assetport, Ping = getping("54.192.201.155").ToString() });
                    tmp_assetlist.Add(new Server { IP = "54.192.201.83", Port = default_assetport, Ping = getping("54.192.201.83").ToString() });
                    tmp_assetlist.Add(new Server { IP = "54.192.201.5", Port = default_assetport, Ping = getping("54.192.201.5").ToString() });
                }
                catch (Exception err)
                {
                    MessageBox.Show("Could not create serverlists with hardcoded ips.\n" + err.Message);
                }


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

        void updateserverlist(ObservableCollection<Server> newauthlist, ObservableCollection<Server> newassetlist)
        {
            bt_checkservers.IsEnabled = true;

            authlist.Clear();
            assetlist.Clear();
            authlist = newauthlist;
            assetlist = newassetlist;

            listview_auth.ItemsSource = authlist;
            listview_assets.ItemsSource = assetlist;
            lab_authserverlist.Content = "Authentication Servers (" + authlist.Count + " servers found):";
            lab_assetserverlist.Content = "Asset Servers APLHA (" + assetlist.Count + " servers found):";
            bt_checkservers.Content = "Check Servers (Last update: "+ DateTime.Now.ToString("h:mm:ss tt") + ")";


            // Sorting authentication servers (ping). Not needed for assetservers because they use CDN (ping nealy doesnt differ)
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listview_auth.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("IP", ListSortDirection.Ascending));
        }

        void loadconfig()
        {
            //Checking if path for reshade unlocker is saved

            if (Properties.Settings.Default.reshadepath != "")
            {
                try
                {
                    unlockerpath = Properties.Settings.Default.reshadepath;
                }
                catch { }
                cb_reshade.IsEnabled = true;
            }
            if (Properties.Settings.Default.use_reshade && cb_reshade.IsEnabled == true) cb_reshade.IsChecked=true;

            // Importing the XML file at AppData\Roaming\Guild Wars 2\
            // This file also contains infos about the graphic settings

            //Find the newest xml file in APPDATA (the xml files share the same name as their exe files -> multiple .xml files possible!)

            string[] configfiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\", "*.exe.xml");
            string sourcepath = "";
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
                if (Properties.Settings.Default.use_reshade) cb_reshade.IsChecked = true;

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

                            //Filter arguments from path
                            lab_para.Content = "Latest Startparameters: ";
                            
                            Regex regex = new Regex(@"-\w*");
                            string input = getvalue(reader);
                            MatchCollection matchList = regex.Matches(input);
                            
                            foreach (Match parameter in matchList)
                            {
                                lab_para.Content = lab_para.Content + " " + parameter.Value;

                            }
                            // Automatically  set checks of previously used arguments

                            foreach (CheckBox entry in arglistbox.Items)
                            {
                                foreach (Match parameter in matchList)
                                {
                                    if (entry.Content.ToString() == parameter.Value)
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


        long getping(string ip)
        {
            // Get Ping in ms
            Ping pingsender = new Ping();
            return pingsender.Send(ip).RoundtripTime;
        }

        private void bt_checkservers_Click(object sender, RoutedEventArgs e)
        {
            bt_checkservers.Content = "Loading Serverlist";
            bt_checkservers.IsEnabled = false;
            Thread serverthread = new Thread(createlist);
            serverthread.Start();
        }

        private void listview_assets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UI Handling for selected Asset Server
            if (listview_assets.Items.Count != 0)
            {
                selected_assetsv = (Server)listview_assets.SelectedItem;
                tb_assetsport.Text = selected_assetsv.Port;
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
                selected_authsv = (Server)listview_auth.SelectedItem;

                tb_authport.Text = selected_authsv.Port;

                checkb_auth.Content = "Use Authentication Server : " + selected_authsv.IP;
                checkb_auth.IsEnabled = true;
            }
            else
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

        private void bt_launch_Click(object sender, RoutedEventArgs e)
        {
            //Launching the application with arguments

            ProcessStartInfo gw2pro = new ProcessStartInfo();
            gw2pro.FileName = exepath + exename;
            gw2pro.Arguments = getarguments();

            try
            {
                System.Diagnostics.Process.Start(gw2pro);
            }
            catch (Exception err)
            {
                System.Windows.MessageBox.Show("Could not launch Gw2. Invalid path?\n" + err.Message);
            }

            if (cb_reshade.IsChecked == true)
            {
                try
                {
                    ProcessStartInfo unlockerpro = new ProcessStartInfo();
                    unlockerpro.FileName = unlockerpath;
                    Process.Start(unlockerpro);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Could not launch ReshadeUnlocker. Invalid path?\n" + err.Message);
                }
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
                exepath = Path.GetDirectoryName(filedialog.FileName) + @"\";
                exename = Path.GetFileName(filedialog.Fi‌​leName);
                lab_path.Content = exepath + exename;
                //Gw2_Launchbuddy.Properties.Settings.Default.reshadepath = exename;
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
                string arguments = getarguments();
                shortcut.IconLocation = Assembly.GetExecutingAssembly().Location;
                shortcut.Description = "Created with Gw2 Launchbuddy, © TheCheatsrichter";

                if (cb_reshade.IsChecked == true)
                {
                    // Using commandline to launch both exe files from the link file
                    // EXAMPLE: cmd.exe /c start "" "C:\Program Files (x86)\Guild Wars 2\ReshadeUnlocker" && start "" "C:\Program Files (x86)\Guild Wars 2\Gw2"
                    shortcut.Arguments = " /c start \"\" \"" + unlockerpath + "\" && start \"\" \"" + exepath+exename + "\" " +arguments;
                    MessageBox.Show(shortcut.Arguments);
                    shortcut.TargetPath = "cmd.exe"; // win will automatically extend this to the cmd path
                    shortcut.Save();
                }else
                {
                    shortcut.Arguments = arguments;
                    shortcut.TargetPath = targetFileLocation;
                    shortcut.Save();
                }
                

                string dynamicinfo = "";
                foreach (string arg in arguments.Split(' '))
                {
                    dynamicinfo += arg + "\n\t\t";
                }


                System.Windows.MessageBox.Show("Custom Launcher created at : " + exepath + "\nUse ReshadeUnlocker: "+ cb_reshade.IsChecked.ToString() +"\nUsed arguments:" + dynamicinfo);
            }
            catch (Exception err)
            {
                MessageBox.Show("Error when creating shortcut. Invalid Path?\n\n" + err.Message);
            }
     
            

        }



        private void arglistbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Description switch. Should have used extern file / dictionary
            if (arglistbox.SelectedItem != null)
            {

                object selecteditem = arglistbox.SelectedItem;
                System.Windows.Controls.CheckBox item = (System.Windows.Controls.CheckBox)selecteditem;
                lab_descr.Content = "Description (" + item.Content.ToString() + "):";

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

            if (cb_login.IsChecked == true)
            {
                try
                {
                    CreateShortcut("Gw2_Launcher_" + selected_acc.Email.Split('@')[0], exepath, exepath + exename);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }

            }
            else
            {
                CreateShortcut("Gw2_Custom_Launcher", exepath, exepath + exename);
            }

            try
            {
                Process.Start(exepath);
            }
            catch (Exception err)
            {
                MessageBox.Show("Could not open file directory\n" + err.Message);
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
            try
            {

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
            catch (Exception e)
            { MessageBox.Show(e.Message); }

        }

        private void bt_addacc_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidEmail(tb_email.Text))
            {
                if (tb_passw.Text.Length > 4)
                {
                    Account acc = new Account { Email = tb_email.Text, Password = tb_passw.Text, Time = DateTime.Now };
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
            MessageBox.Show("Autologin does only function when no second Authentication (SMS,Email,App) is used on this account.\n Make sure that you current network is an authorized network (check always trust this network at login,recommended) or deactivate the second authentication!(not recommended)", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            Properties.Settings.Default.use_reshade = (bool)cb_reshade.IsChecked;
            Properties.Settings.Default.Save();
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

        private void bt_quaggan_Click(object sender, RoutedEventArgs e)
        {
            if (exepath != "")
            {
                Clientfix clientfix = new Clientfix();
                clientfix.exepath = exepath;
                clientfix.exename = exename;
                clientfix.Show();
            }
            else
            {
                MessageBox.Show("Gw2.exe  installpath is empty!");
            }

        }


        private void tb_authport_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            selected_authsv.Port = tb_authport.Text;
        }

        private void tb_assetsport_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            selected_assetsv.Port = tb_assetsport.Text;
        }

        private void cb_reshade_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void bt_reshadepath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.DefaultExt = "exe";
            filedialog.Multiselect = false;
            filedialog.Filter = "Exe Files(*.exe) | *.exe";
            filedialog.ShowDialog();

            if (filedialog.FileName == "" || !filedialog.FileName.EndsWith(".exe"))
            {
                MessageBox.Show("Invalid .exe file selected!");
                cb_reshade.IsChecked = false;
            }
            else
            {
                unlockerpath = filedialog.FileName;
                cb_reshade.IsEnabled = true;
                Gw2_Launchbuddy.Properties.Settings.Default.reshadepath = unlockerpath;
            }

        }


        private void cb_reshade_Checked(object sender, RoutedEventArgs e)
        {
            if (!System.IO.File.Exists(unlockerpath))
            {
                cb_reshade.IsChecked = false;
                MessageBox.Show("Reshade Unlocker exe not found at :\n" + exepath + "\nPlease select the ReshadeUnlocker.exe manually!");
                System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
                filedialog.DefaultExt = "exe";
                filedialog.Multiselect = false;
                filedialog.Filter = "Exe Files(*.exe) | *.exe";
                filedialog.ShowDialog();

                if (filedialog.FileName == "" || !filedialog.FileName.EndsWith(".exe"))
                {
                    MessageBox.Show("Invalid .exe file selected!");

                } else
                {
                    unlockerpath = filedialog.FileName;
                    try
                    {
                        Gw2_Launchbuddy.Properties.Settings.Default.reshadepath = unlockerpath;
                    }
                    catch { }
                }
            }
        }

        private string getarguments()
        {
            //Gathers all arguments and returns them as single string

            string arguments = "";

            if (checkb_assets.IsChecked == true)
            {
                arguments += " -assetsrv " + selected_assetsv.IP + ":" + tb_assetsport.Text;
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
                if (entry.IsChecked == true)
                {
                    arguments += " " + entry.Content;
                }

            }
            return arguments;
        }
    }
}
