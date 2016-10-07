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
using System.IO.Compression;


namespace Gw2_Launchbuddy
{
    public partial class MainWindow : Window
    {

        ///Gw2 Launchbuddy by TheCheatsrichter 2016
        ///
        ///Argument generator and shortcut creator for Guild Wars 2

        /// Object prefix:
        ///     bt= button
        ///     lab = label
        ///     cb = checkbox
        /// 
        ///##########################################
        /// 

        SetupInfo winsetupinfo = new SetupInfo();
        private SortAdorner listViewSortAdorner = null;
        private GridViewColumnHeader listViewSortCol = null;

        ObservableCollection<Server> assetlist = new ObservableCollection<Server>();
        ObservableCollection<Server> authlist = new ObservableCollection<Server>();
        ObservableCollection<Account> accountlist = new ObservableCollection<Account>();

        Server selected_authsv = new Server();
        Server selected_assetsv = new Server();

        List<Account> selected_accs = new List<Account>();
        List<int> nomutexpros = new List<int>();

        string exepath, exename , unlockerpath, version_client, version_api;
        string AppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Gw2 Launchbuddy\\";
        bool ismultibox = false;

        AES crypt = new AES();

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
            public string DisplayPW {
                get
                {
                    string stars = "";
                    foreach (char ch in Password.ToCharArray())
                    {
                        stars += "*";
                    }
                    return stars;
                }
                set {
                    DisplayPW = value;
                }
            }
            public DateTime Time { get; set; }
            public string Nick { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(AppdataPath))
            {
                Directory.CreateDirectory(AppdataPath);
            }

            accountlist.Clear(); //clearing accountlist
            checksetup();
            loadconfig(); // loading the gw2 xml config file from appdata and loading user settings
            loadaccounts(); // loading saved accounts from launchbuddy
            Thread checkver = new Thread(checkversion);
            checkver.IsBackground = true;
            checkver.Start();

        }

        void checkversion()
        {
            try
            {
                if (!isclientuptodate())
                {

                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBoxResult win = MessageBox.Show("A new Build of Gw2 is available! Not updating can cause Gw2 Launchbuddy to not work! Update now?", "Client Build Info", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (win.ToString() == "Yes")
                        {
                            updateclient();
                            System.Windows.Forms.Application.Restart();
                            Application.Current.Shutdown();   
                        }
                    })); 
                }
                string versioninfo = "Build Version: " + version_client;

                Dispatcher.Invoke(new Action(() =>
                {
                    if (version_api == version_client)
                    {
                        versioninfo += "\tStatus: up to date!";
                        lab_version.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        versioninfo += "\tStatus: outdated!";
                        lab_version.Foreground = new SolidColorBrush(Colors.Red);
                    }

                    lab_version.Content = versioninfo;

                }));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        bool isclientuptodate()
        {
            WebClient downloader = new WebClient();
            Regex filter = new Regex(@"\d*\d");
            version_api = filter.Match(downloader.DownloadString("https://api.guildwars2.com/v2/build")).Value;

            if (version_api == version_client) return true;
            return false;
        }

        void updateclient()
        {
            Process progw2 = new Process();
            ProcessStartInfo infoprogw2 = new ProcessStartInfo { FileName = exepath + exename, Arguments = "-image" };
            progw2.StartInfo = infoprogw2;
            progw2.Start();
            progw2.WaitForExit();
        }

        void checksetup()
        {
            try {
                if (!System.IO.File.Exists(AppdataPath+"handle64.exe") || !System.IO.File.Exists(AppdataPath + "handle.exe"))
                {
                    winsetupinfo.WindowStyle = WindowStyle.None;
                    winsetupinfo.Width = 300;
                    winsetupinfo.Height = 200;
                    winsetupinfo.Show();
                    myWindow.Visibility = Visibility.Hidden;
                    Thread th_gethandleexe = new Thread(gethandleexe);
                    th_gethandleexe.IsBackground = true;
                    th_gethandleexe.Start();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        void gethandleexe()
        {
            try
            {
                System.IO.File.Delete(AppdataPath + "handle64.exe");
                System.IO.File.Delete(AppdataPath + "handle.exe");
                System.IO.File.Delete(AppdataPath + "Eula.txt");
                WebClient downloadclient = new WebClient();
                downloadclient.DownloadFile("https://download.sysinternals.com/files/Handle.zip", AppdataPath + "Handle.zip");

                ZipFile.ExtractToDirectory(AppdataPath + "Handle.zip", AppdataPath);
                ProcessStartInfo prohandleinfo = new ProcessStartInfo();
                prohandleinfo.UseShellExecute = false;
                prohandleinfo.CreateNoWindow = true;
                prohandleinfo.Arguments = "-accepteula";


                if (Environment.Is64BitOperatingSystem)
                {
                    prohandleinfo.FileName = AppdataPath + "handle64.exe";
                }else
                {
                    prohandleinfo.FileName = AppdataPath + "handle.exe";
                }
                Process prohandle = new Process{ StartInfo = prohandleinfo };
                prohandle.Start();

                System.IO.File.Delete(AppdataPath + "Handle.zip");
                System.IO.File.Delete(AppdataPath + "Eula.txt");

                Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() => setupend()));

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        void setupend()
        {
            try {
                myWindow.Visibility = Visibility.Visible;
                winsetupinfo.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
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
            lab_assetserverlist.Content = "Asset Servers (" + assetlist.Count + " servers found):";
            bt_checkservers.Content = "Check Servers (Last update: "+ DateTime.Now.ToString("h:mm:ss tt") + ")";


            // Sorting  servers (ping).
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listview_auth.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Descending));
            CollectionView sview = (CollectionView)CollectionViewSource.GetDefaultView(listview_assets.ItemsSource);
            sview.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Descending));
            sview.Refresh();


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

            try
            {
                if (Properties.Settings.Default.use_reshade && cb_reshade.IsEnabled == true) cb_reshade.IsChecked = true;
                if (Properties.Settings.Default.use_autologin == true) cb_login.IsChecked = true;

                listview_acc.SelectedIndex = Properties.Settings.Default.selected_acc;

            }
            catch (Exception err)
            {
                MessageBox.Show("Error in UI setup. \n " + err.Message);
            }
            

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
                            Regex filter = new Regex(@"\d*\d");
                            version_client= filter.Match(getvalue(reader)).Value;
                            lab_version.Content = "Client Version: " + version_client;
                            break;


                        case "INSTALLPATH":

                            exepath = getvalue(reader);
                            lab_path.Content = "Install Path: " + exepath;
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
                                if (parameter.Value != "-shareArchive") lab_para.Content = lab_para.Content + " " + parameter.Value;
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
                return reader.Value;
            }
            return null;
        }


        string getlocation(string ip)
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


        long getping(string ip)
        {
            // Get Ping in ms
            Ping pingsender = new Ping();
            return (int)pingsender.Send(ip).RoundtripTime;
        }

        private void bt_checkservers_Click(object sender, RoutedEventArgs e)
        {
            //Starting servercheck thread
            bt_checkservers.Content = "Loading Serverlist";
            bt_checkservers.IsEnabled = false;
            Thread serverthread = new Thread(createlist);
            serverthread.IsBackground = true;
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
            //Checking for existing Gw2 instances
            Process[] prolist = Process.GetProcesses();

            foreach (Process pro in prolist)
            {
                if (!nomutexpros.Contains(pro.Id) && pro.ProcessName == "Gw2")
                {
                    MessageBox.Show("One instance of Gw2 is allready running!\n If that instance was not launched with -shareArchive (when launched with gw2 launchbuddy this argument gets added automatically) then additional instances will crash!");
                    closemutex(pro.Id, "AN-Mutex-Window-Guild Wars 2", "Mutant");
                }
            }


            //Launching the application with arguments
            if (ismultibox)
            {
                for (int i = 0; i <= selected_accs.Count - 1; i++) launchgw2(i);
            }else
            {
                launchgw2(0);
            }
        }

        void killmutex(int proid)
        {
            
            
        }


        void closemutex (int proid, string handlename , string handletype)
        {
            StreamReader outputReader = null;
            Process prohandle = new Process();
            ProcessStartInfo prohandle_info = new ProcessStartInfo();

            if (Environment.Is64BitOperatingSystem)
            {
                prohandle_info.FileName= AppdataPath + "handle64.exe";
            } else
            {
                prohandle_info.FileName= AppdataPath + "handle.exe";
            }
            prohandle_info.UseShellExecute = false;
            prohandle_info.RedirectStandardInput = true;
            prohandle_info.RedirectStandardOutput = true;
            prohandle_info.CreateNoWindow = true;
            prohandle_info.Arguments = "-p "+proid+" -a \""+ handlename + "\"";
            prohandle.StartInfo = prohandle_info;
            prohandle.Start();
            outputReader = prohandle.StandardOutput;

            string output = outputReader.ReadToEnd();
            Regex regfilter = new Regex("....(:)");
            MatchCollection matches = regfilter.Matches(output);

            string tmp="";
            string handlehexid = "";
            foreach (Match entry in matches)
            {
                tmp= tmp + entry.Value + "\n";
            }

            try
            {
                handlehexid = matches[matches.Count - 1].Value.Trim(':');
            } catch (Exception err)
            {
                MessageBox.Show("No Mutex found on process : " + proid.ToString() + "\n" + err.Message);
            }

            
            try
            {
                prohandle.Close();
            }
            catch
            {

            }
            
            prohandle_info.Arguments = "-p " + proid + " -c " + handlehexid + " -y";
            prohandle.StartInfo = prohandle_info;
            prohandle.Start();
            nomutexpros.Add(proid);
            output = outputReader.ReadToEnd();


        }


        void launchgw2(int accnr)
        {
            try
            {
                ProcessStartInfo gw2proinfo = new ProcessStartInfo();
                gw2proinfo.FileName = exepath + exename;
                gw2proinfo.Arguments = getarguments(accnr);
                Process gw2pro = new Process { StartInfo = gw2proinfo };

                try
                {
                    gw2pro.Start();
                }
                catch (Exception err)
                {
                    System.Windows.MessageBox.Show("Could not launch Gw2. Invalid path?\n" + err.Message);
                }
                try
                {
                    gw2pro.WaitForInputIdle();
                    closemutex(gw2pro.Id, "AN-Mutex-Window-Guild Wars 2", "Mutant");

                    // OLD method, sadly only working on Win7
                    /*
                    MutexCloser mutexcloser = new MutexCloser();
                    mutexcloser.CloseMutex(gw2pro.Id, "AN-Mutex-Window-Guild Wars 2");
                    */
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
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
            catch(Exception err)
            {
                MessageBox.Show(err.Message);
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
                string arguments = getarguments(0);
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
            // Description switch. Should have used extern file / dictionary?
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
                    CreateShortcut("Gw2_Launcher_" + selected_accs[0].Nick, exepath, exepath + exename);
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
            
            ObservableCollection<Account> aes_accountlist = new ObservableCollection<Account>();
            try
            {
                aes_accountlist.Clear();
                foreach (Account acc in accountlist)
                {
                    aes_accountlist.Add(new Account {Nick= acc.Nick, Email = acc.Email, Password = crypt.Encrypt(acc.Password), Time = acc.Time });
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("Could not encrypt passwords\n" + err.Message);
            }

            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\Launchbuddy.bin";
                using (Stream stream = System.IO.File.Open(path, FileMode.Create))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bformatter.Serialize(stream, aes_accountlist);
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
                    accountlist.Clear();
                    using (Stream stream = System.IO.File.Open(path, FileMode.Open))
                    {
                        var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        ObservableCollection<Account> aes_accountlist = (ObservableCollection<Account>)bformatter.Deserialize(stream);
                        
                        foreach (Account acc in aes_accountlist)
                        {
                            accountlist.Add(new Account{Nick = acc.Nick,Email=acc.Email,Password=crypt.Decrypt(acc.Password),Time=acc.Time});
                        }

                        listview_acc.ItemsSource = accountlist;
                    }
                }

                //listview_acc.ItemsSource = accountlist;
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
                    Account acc = new Account {Nick= tb_nick.Text , Email = tb_email.Text, Password = tb_passw.Text, Time = DateTime.Now };
                    accountlist.Add(acc);
                    listview_acc.ItemsSource = accountlist;
                    tb_email.Clear();
                    tb_passw.Clear();
                    tb_nick.Clear();
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
            if (Properties.Settings.Default.use_autologin == false) MessageBox.Show("Autologin does only function when no second Authentication (SMS,Email,App) is used on this account.\n Make sure that your current network is an authorized network (check always trust this network at login,recommended) or deactivate the second authentication!(not recommended)\n\n ATTENTION: Invalid inputs result in a black/white screen and the game freezes!", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Warning);
            listview_acc.IsEnabled = true;
            lab_email.IsEnabled = true;
            lab_passw.IsEnabled = true;
            tb_passw.IsEnabled = true;
            tb_email.IsEnabled = true;
            bt_addacc.IsEnabled = true;
            bt_remacc.IsEnabled = true;
            tb_nick.IsEnabled = true;
            lab_nick.IsEnabled = true;

            Properties.Settings.Default.use_autologin = true;
            Properties.Settings.Default.Save();
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
            tb_nick.IsEnabled = false;
            lab_nick.IsEnabled = false;
            cb_login.Content = "Use Autologin:";

            listview_acc.SelectedIndex = -1;
            Properties.Settings.Default.use_autologin = false;
            Properties.Settings.Default.Save();
        }

        private void listview_acc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected_accs = listview_acc.SelectedItems.Cast<Account>().ToList();

            Properties.Settings.Default.selected_acc = listview_acc.SelectedIndex;
            Properties.Settings.Default.Save();

            if (listview_acc.SelectedItems.Count != 0 && listview_acc.SelectedItems.Count<=1)
            {
                var selectedItems = (dynamic)listview_acc.SelectedItems;
                cb_login.Content = "Use Autologin : " + selectedItems[0].Email;
                selected_accs[0].Email = selectedItems[0].Email;
                selected_accs[0].Password = selectedItems[0].Password;
                bt_shortcut.IsEnabled = true;
                ismultibox = false;
            }

            if (listview_acc.SelectedItems.Count != 0 && listview_acc.SelectedItems.Count > 1)
            {
                var selectedItem = (dynamic)listview_acc.SelectedItem;
                cb_login.Content = "Use Autologin (Multiboxing): " + listview_acc.SelectedItems.Count + " Accounts selected";
                selected_accs[0].Email = selectedItem.Email;
                selected_accs[0].Password = selectedItem.Password;
                bt_shortcut.IsEnabled = false;
                ismultibox = true;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.use_reshade = (bool)cb_reshade.IsChecked;
            Properties.Settings.Default.Save();
            safeaccounts();
            Environment.Exit(Environment.ExitCode);

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

        private void exp_server_Collapsed(object sender, RoutedEventArgs e)
        {
            ServerUI.Height = new GridLength(30);
            Application.Current.MainWindow.Height=585;
            
        }

        private void exp_server_Expanded(object sender, RoutedEventArgs e)
        {
            ServerUI.Height = new GridLength(290);
            Application.Current.MainWindow.Height = 845;
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listview_auth.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Ascending));
            CollectionView sview = (CollectionView)CollectionViewSource.GetDefaultView(listview_assets.ItemsSource);
            sview.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Ascending));
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

        private string getarguments(int accnr)
        {
            //Gathers all arguments and returns them as single string

            string arguments = " -shareArchive";

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

            try
            {
                if (cb_login.IsChecked == true)
                {
                    if (selected_accs[accnr].Email != null && selected_accs[accnr].Password != null)
                    {
                        
                        arguments += " -nopatchui -email \"" + selected_accs[accnr].Email + "\" -password \"" + selected_accs[accnr].Password +"\" ";
                    }
                }

            }
            catch
            {

                    MessageBox.Show("No Account selected! Launching without autologin.");
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

        void sortbycolum (ListView list , object sender)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                list.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            list.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {


        }

        void mutexkiller(int ProId)
        {
            if (!nomutexpros.Contains(ProId))
            {
                MutexCloser mutexcloser = new MutexCloser();
                mutexcloser.CloseMutex(ProId, "AN-Mutex-Window-Guild Wars 2");
            }
        }

        private void bt_donate_Click(object sender, RoutedEventArgs e)
        {
            string url = "";

            string business = "thecheatsrichter@gmx.at";  // your paypal email
            string description = "Gw2 Launchbuddy Donation";            // '%20' represents a space. remember HTML!
            string currency = "EUR";                 // AUD, USD, etc.

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                //"&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
        }


        private void listview_auth_Click(object sender, RoutedEventArgs e)
        {
            sortbycolum(listview_auth,sender);
        }
        private void listview_assets_Click(object sender, RoutedEventArgs e)
        {
            sortbycolum(listview_assets, sender);
        }

    }




    public class SortAdorner : Adorner
    {
        private static Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
        {
            this.Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform
                    (
                            AdornedElement.RenderSize.Width - 15,
                            (AdornedElement.RenderSize.Height - 5) / 2
                    );
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.Direction == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(Brushes.Black, null, geometry);

            drawingContext.Pop();
        }
    }
}
