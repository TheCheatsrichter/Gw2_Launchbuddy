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


namespace Gw2_Serverselection
{

    /// <summary>
    /// Gw2 Launchbuddy 
    /// </summary>
    public partial class MainWindow : Window
    {
        List<String> authlist = new List<String>();
        List<String> assetlist = new List<String>();
        //List<String> arguments = new List<String>();

        Server selected_authsv = new Server();
        Server selected_assetsv = new Server();

        string exepath,exename;

        class Server
        {
            public string IP;
            public string Port;
            public string Ping;
        }

        public MainWindow()
        {
            InitializeComponent();
            createlist();
            loadconfig();
            
        }

        void createlist()
        {
            //Harcoded server lists. Should later use .xml files

            // Listed as auth1 servers (NA?)
            authlist.Add("64.25.38.51:6112:Auth1");
            authlist.Add("64.25.38.54:6112:Auth1");
            authlist.Add("64.25.38.205:6112:Auth1");
            authlist.Add("64.25.38.171:6112:Auth1");
            authlist.Add("64.25.38.172:6112:Auth1");

            // Listed as auth2 servers (EU?)
            authlist.Add("206.127.146.73:6112:Auth2");
            authlist.Add("206.127.159.107:6112:Auth2");
            authlist.Add("206.127.146.74:6112:Auth2");
            authlist.Add("206.127.159.109:6112:Auth2");
            authlist.Add("206.127.159.108:6112:Auth2");
            authlist.Add("206.127.159.77:6112:Auth2");

            // Assets servers (CDN Servers -> Ip changes and newer Images might not be on the same old servers) -> Get Assetlist from -diag (takes about 10 mins :( )
            assetlist.Add("54.192.201.89:80");
            assetlist.Add("54.192.201.14:80");
            assetlist.Add("54.192.201.65:80");
            assetlist.Add("54.192.201.68:80");
            assetlist.Add("54.192.201.41:80");
            assetlist.Add("54.192.201.155:80");
            assetlist.Add("54.192.201.83:80");
            assetlist.Add("54.192.201.5:80");

        }

        void loadconfig()
        {

            // Importing the XML file at AppData\Roaming\Guild Wars 2\
            // This file also contains infos about the graphic settings


            //Find the newest xml file in APPDATA (the xml files share the same name as their exe files -> multiple .xml files possible!)

            string[] configfiles = Directory.GetFiles(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Guild Wars 2\","*.exe.xml");
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

        }

        long getping(string ip)
        {
            // Get Ping in ms
            Ping pingsender = new Ping();
            return pingsender.Send(ip).RoundtripTime;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            pingservers();
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

            object selecteditem = arglistbox.SelectedItem;
            System.Windows.Controls.CheckBox item = (System.Windows.Controls.CheckBox)selecteditem;
            switch(item.Content.ToString())
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

        private void bt_shortcut_Click(object sender, RoutedEventArgs e)
        {
            CreateShortcut("Gw2_Custom_Launcher",exepath,exepath+exename);
            try
            {
                Process.Start(exepath);
            }
            catch
            {

            }
        }

        private string getarguments()
        {
            //Gathers all arguments and converts them into a formated single string

            string arguments = "";

            if (checkb_assets.IsChecked == true)
            {
                arguments += " -assetsrv " + selected_assetsv.IP + ":" + tb_assetsport.Text;
            }

            if (checkb_auth.IsChecked == true)
            {
                arguments += " -authsrv " + selected_authsv.IP + ":" + tb_authport.Text;
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
