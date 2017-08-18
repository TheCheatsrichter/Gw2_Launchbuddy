using Gw2_Launchbuddy.ObjectManagers;
using IWshRuntimeLibrary;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;

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

        private bool cinemamode = false;
        private bool slideshowthread_isrunning = false;

        private int reso_x, reso_y;

        private SetupInfo winsetupinfo = new SetupInfo();
        private SortAdorner listViewSortAdorner = null;
        private GridViewColumnHeader listViewSortCol = null;

        private ObservableCollection<Server> assetlist = new ObservableCollection<Server>();
        private ObservableCollection<Server> authlist = new ObservableCollection<Server>();

        private AES crypt = new AES();

        public class CinemaImage
        {
            public string Name
            {
                set { }
                get { return System.IO.Path.GetFileName(Path); }
            }

            public string Path { set; get; }

            public CinemaImage(string Path)
            {
                this.Path = Path;
            }
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                if (!Directory.Exists(Globals.AppDataPath))
                {
                    Directory.CreateDirectory(Globals.AppDataPath);
                }
            }
            catch
            {
                Properties.Settings.Default.Reset();
            }

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionReport);

            //LB statistics
            Properties.Settings.Default.counter_launches += 1;
            Properties.Settings.Default.Save();
#else
            System.Diagnostics.Debug.WriteLine("Compiled without crash handler, and always first run.");
            Properties.Settings.Default.counter_launches = 1;
#endif

            //Setup
            donatepopup();

            AccountManager.ImportExport.LoadAccountInfo(); // Load saved accounts from XML
            LoadConfig(); // loading the gw2 xml config file from appdata and loading user settings

            Cinema_Accountlist.ItemsSource = listview_acc.ItemsSource = AccountManager.AccountCollection;
            arglistbox.ItemsSource = AccountArgumentManager.AccountArgumentCollection.Where(a => a.Argument.Active && a.Account == AccountManager.DefaultAccount);

            Thread checkver = new Thread(checkversion);
            checkver.IsBackground = true;
            checkver.Start();
            cinema_setup();
            LoadAddons();
            SettingsTabSetup();
            AddOnManager.SingletonAddon();
            if (Properties.Settings.Default.notifylbupdate)
            {
                Thread checklbver = new Thread(checklbversion);
                checklbver.Start();
            }
            CrashAnalyzer.ReadCrashLogs();
        }

        private void SettingsTabSetup()
        {
            cb_lbupdatescheck.IsChecked = Properties.Settings.Default.notifylbupdate;
            cb_useinstancegui.IsChecked = Properties.Settings.Default.useinstancegui;
        }

        private void donatepopup()
        {
            if ((Properties.Settings.Default.counter_launches % 100) == 5)
            {
                Popup popup = new Popup();
                popup.Show();
            }
        }

        private void checklbversion()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                bt_downloadrelease.Content = "Fetching Release List please wait";
            }));

            VersionSwitcher.CheckForUpdate();
            Dispatcher.Invoke(new Action(() =>
            {
                lv_lbversions.ItemsSource = VersionSwitcher.Releaselist;
                bt_downloadrelease.Content = "Download";
            }));
        }

        private static void UnhandledExceptionReport(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            CrashReporter.ReportCrashToAll(e);
        }

        private void slideshow_diashow(string imagespath)
        {
            bool isactive = false;
            string[] files = Directory.GetFiles(imagespath, "*.*", SearchOption.AllDirectories).Where(a => a.EndsWith(".png") || a.EndsWith(".jpg") || a.EndsWith(".jpeg") || a.EndsWith(".bmp")).ToArray<string>();
            Random rnd = new Random();
            BitmapSource startimg = LoadImage(files[rnd.Next(files.Length - 1)]);
            startimg.Freeze();

            Dispatcher.Invoke(new Action(() =>
            {
                img_slideshow.Source = startimg;
            }));

            while (true)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    isactive = myWindow.IsActive;
                }));

                while (isactive)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Storyboard ImgFadeIn = (Storyboard)FindResource("anim_imgfadein");
                        ImgFadeIn.Begin();
                    }));
                    Thread.Sleep(1000);
                    int nr = rnd.Next(files.Length - 1);
                    BitmapSource currentimg = LoadImage(files[nr]);
                    currentimg.Freeze();
                    Thread.Sleep(5000); //Time how long the picture is actually displayed
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Storyboard ImgFadeOut = (Storyboard)FindResource("anim_imgfadeout");
                        ImgFadeOut.Begin();
                    }));
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        img_slideshow.Source = currentimg;
                    }));

                    Thread.Sleep(1000);
                }
                Thread.Sleep(3000);
            }
        }

        private bool cinema_checksetup(bool checkslideshow, bool checkvideomode)
        {
            string imagespath = Properties.Settings.Default.cinema_imagepath;
            string musicpath = Properties.Settings.Default.cinema_musicpath;
            string maskpath = Properties.Settings.Default.cinema_maskpath;
            string videopath = Properties.Settings.Default.cinema_videopath;
            string loginwindowpath = Properties.Settings.Default.cinema_loginwindowpath;
            var backgroundcolor = Properties.Settings.Default.cinema_backgroundcolor;

            string[] musicext = {
                ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA",
                ".AVI", ".MP4", ".DIVX", ".WMV",
            };

            string[] videoext = {
                ".AVI", ".MP4", ".DIVX", ".WMV", ".M4A",
            };

            string[] picext = {
                ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF",
            };

            bool invalid = false;

            if (checkvideomode)
            {
                //Check all needed VideoMode resources here
                if (!videoext.Contains(Path.GetExtension(videopath), StringComparer.OrdinalIgnoreCase) || !System.IO.File.Exists(videopath))
                {
                    MessageBox.Show("Invalid video file detected! File could not be found / is not a video file.\n Filepath: " + videopath + "\n\nPlease choose another file in the cinemamode section!");
                    invalid = true;
                }
            }

            if (checkslideshow)
            {
                //Check all needed SlideshowMode resources here
                if (!musicext.Contains(Path.GetExtension(musicpath), StringComparer.OrdinalIgnoreCase) || !System.IO.File.Exists(musicpath))
                {
                    MessageBox.Show("Invalid music file detected! File could not be found / is not a music file.\n Filepath: " + musicpath + "\n\nPlease choose another file in the cinemamode section!");
                    invalid = true;
                }

                if (!picext.Contains(Path.GetExtension(maskpath), StringComparer.OrdinalIgnoreCase) || !System.IO.File.Exists(maskpath))
                {
                    MessageBox.Show("Invalid mask file detected! File could not be found / is not a picture file.\n Filepath: " + maskpath + "\n\nPlease choose another file in the cinemamode section!");
                    invalid = true;
                }

                if (!Directory.Exists(imagespath) || Directory.GetFiles(imagespath, "*.*", SearchOption.AllDirectories).Where(a => a.EndsWith(".png") || a.EndsWith(".jpg") || a.EndsWith(".jpeg") || a.EndsWith(".bmp")).ToArray<string>().Length <= 0)
                {
                    MessageBox.Show("Invalid image folder detected! No Images could be found at the choosen location! \n Filepath: " + imagespath + "\n\nPlease choose another folder in the cinemamode section!");
                    invalid = true;
                }
            }

            //General needed resources
            if ((!picext.Contains(Path.GetExtension(loginwindowpath), StringComparer.OrdinalIgnoreCase) || !System.IO.File.Exists(loginwindowpath)) && loginwindowpath != "")
            {
                MessageBox.Show("Invalid loginwindow file detected! File could not be found / is not a picture file.\n Filepath: " + loginwindowpath + "\n\nPlease choose another file in the cinemamode section!");
                invalid = true;
            }

            if (invalid) return false;

            return true;
        }

        private void cinema_setup()
        {
            bool videomode = Properties.Settings.Default.cinema_video;
            bool slideshowmode = Properties.Settings.Default.cinema_slideshow;
            cinemamode = Properties.Settings.Default.cinema_use;
            if (cinemamode) cinemamode = cinema_checksetup(slideshowmode, videomode);
            Properties.Settings.Default.cinema_use = cinemamode;
            Properties.Settings.Default.Save();
            LoadCinemaSettings();

            string musicpath = Properties.Settings.Default.cinema_musicpath;
            string imagespath = Properties.Settings.Default.cinema_imagepath;
            string maskpath = Properties.Settings.Default.cinema_maskpath;
            string videopath = Properties.Settings.Default.cinema_videopath;
            string loginwindowpath = Properties.Settings.Default.cinema_loginwindowpath;
            var backgroundcolor = Properties.Settings.Default.cinema_backgroundcolor;

            //Settings UI Setup
            try
            {
                cinema_videoplayback.Source = new Uri(videopath, UriKind.Relative);
            }
            catch (Exception err)
            {
                MessageBox.Show("Video File for Video Mode could not be found! \nPath: " + videopath + "\n" + err.Message);
            }
            if (videomode && !slideshowmode) rb_cinemavideomode.IsChecked = true;
            if (!videomode && slideshowmode) rb_cinemaslideshowmode.IsChecked = true;
            ClrPcker_Background.SelectedColor = backgroundcolor;
            lab_loginwindowpath.Content = "Current Login Window: " + Path.GetFileNameWithoutExtension(loginwindowpath);
            sl_logoendpos.Value = Properties.Settings.Default.cinema_slideshowendpos;
            sl_logoendscaleX.Value = Properties.Settings.Default.cinema_slideshowendscale;
            sl_volumecontrol.Value = Properties.Settings.Default.mediaplayer_volume;
            Cinema_MediaPlayer.Volume = sl_volumecontrol.Value;

            if (cinemamode)
            {
                //Cinema Mode

                reso_x = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                reso_y = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                myWindow.WindowState = WindowState.Maximized;

                //Notes: Login frame = 560x300
                //Test resolutions here!
                //Only edit width!
                /*
                myWindow.Width = 1600;

                myWindow.Height = (int)(myWindow.Width / 16 * 9);
                int reso_x = (int)myWindow.Width;
                int reso_y = (int)myWindow.Height;

                //Centering Window, only needed in Debug
                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                double windowWidth = this.Width;
                double windowHeight = this.Height;
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);
                */

                //Setting up the Login Window Location
                Canvas.SetTop(Canvas_login, reso_y - (reso_y / 2));
                Canvas.SetLeft(Canvas_login, reso_x / 10);
                //Setting up Endposition of Logo Animation
                var endpos = (System.Windows.Media.Animation.EasingDoubleKeyFrame)Resources["Mask_EndPos"];
                endpos.Value = Properties.Settings.Default.cinema_slideshowendpos * reso_x / 200;
                var endscale = (System.Windows.Media.Animation.EasingDoubleKeyFrame)Resources["Mask_EndScaleX"];
                endscale.Value = (double)Properties.Settings.Default.cinema_slideshowendscale;

                //General UI Hiding/Scaling
                SettingsGrid.Visibility = Visibility.Hidden;
                bt_ShowSettings.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(WindowOptionsColum, 2);
                Cinema_MediaPlayer.Visibility = Visibility.Hidden;
                Canvas_Custom_UI.Visibility = Visibility.Visible;
                VolumeControl.Visibility = Visibility.Visible;

                //Setting Custom mode independent Cinema Elements
                try
                {
                    if (backgroundcolor != null) myWindow.Background = new SolidColorBrush(backgroundcolor);
                    if (loginwindowpath != "") img_loginwindow.Source = LoadImage(loginwindowpath);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Login Window Image could not be found!\nPath: " + loginwindowpath + "\n" + err.Message);
                }

                if (videomode)
                {
                    try
                    {
                        //Load Backgroundvideo
                        Cinema_MediaPlayer.Visibility = Visibility.Visible;
                        Cinema_MediaPlayer.Source = new Uri(Properties.Settings.Default.cinema_videopath, UriKind.Relative);
                        Cinema_MediaPlayer.Play();
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show("The chosen video for cinemamode is not valid/ does not exist!\n" + err.Message);
                        Properties.Settings.Default.cinema_use = false;
                        Properties.Settings.Default.Save();
                    }
                }

                if (slideshowmode)
                {
                    try
                    {
                        Storyboard anim_slideshow = (Storyboard)FindResource("anim_slideshow_start");
                        anim_slideshow.Begin();

                        if (maskpath != null)
                        {
                            //Setting opacity mask (logo)
                            ImageBrush mask = new ImageBrush(LoadImage(maskpath));
                            mask.Stretch = Stretch.Uniform;
                            img_slideshow.OpacityMask = mask;
                        }

                        //Starting background slideshowthread
                        if (!slideshowthread_isrunning)
                        {
                            Thread th_slideshow = new Thread(() => slideshow_diashow(imagespath));
                            th_slideshow.Start();
                        }
                        slideshowthread_isrunning = true;

                        img_slideshow.Visibility = Visibility.Visible;
                        Cinema_MediaPlayer.Source = new Uri(musicpath);
                        Cinema_MediaPlayer.Play();
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show("One or more settings for slideshowmode are missing!\n" + err.Message);
                        Properties.Settings.Default.cinema_use = false;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            else
            {
                //Normal Mode
                VolumeControl.Visibility = Visibility.Collapsed;
                Cinema_MediaPlayer.Stop();
                Cinema_MediaPlayer.Visibility = Visibility.Hidden;
                SettingsGrid.Visibility = Visibility.Visible;
                myWindow.WindowState = WindowState.Normal;
                myWindow.Height = 680;
                myWindow.Width = 700;
                bt_ShowSettings.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(WindowOptionsColum, 1);
                Canvas_Custom_UI.Visibility = Visibility.Collapsed;
            }
        }

        private void checkversion()
        {
            try
            {
                if (!isclientuptodate() && Globals.version_api != null)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBoxResult win = MessageBox.Show("A new Build of Gw2 is available! Not updating can cause Gw2 Launchbuddy to not work! Update now?", "Client Build Info", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (win.ToString() == "Yes")
                        {
                            updateclient();
                            LoadConfig();
                            checkversion();
                        }
                    }));
                }
                string versioninfo = "Build Version: " + ClientManager.ClientInfo.Version;

                Dispatcher.Invoke(new Action(() =>
                {
                    if (Globals.version_api == ClientManager.ClientInfo.Version)
                    {
                        ClientManager.ClientInfo.IsUpToDate = true;
                        versioninfo += "\tStatus: up to date!";
                        lab_version.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        if (Globals.version_api != null)
                        {
                            versioninfo += "\tStatus: outdated!";
                            lab_version.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            ClientManager.ClientInfo.IsUpToDate = true;
                            versioninfo += "\tStatus: unknown!(API down)";
                            lab_version.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }

                    lab_version.Content = versioninfo;
                }));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private bool isclientuptodate()
        {
            WebClient downloader = new WebClient();
            Regex filter = new Regex(@"\d*\d");
            try
            {
                Globals.version_api = filter.Match(downloader.DownloadString("https://api.guildwars2.com/v2/build")).Value;
            }
            catch
            {
                ClientManager.ClientInfo.IsUpToDate = true;
                MessageBox.Show("The official Gw2 API is not reachable / down! Launchbuddy can't make sure that your gameclient is uptodate.\nPlease keep your game manually uptodate to avoid crashes!");
            }

            if (Globals.version_api == ClientManager.ClientInfo.Version) return true;
            return false;
        }

        private void updateclient()
        {
            Process progw2 = new Process();
            ProcessStartInfo infoprogw2 = new ProcessStartInfo { FileName = ClientManager.ClientInfo.InstallPath + ClientManager.ClientInfo.Executable, Arguments = "-image" };
            progw2.StartInfo = infoprogw2;
            progw2.Start();
            progw2.WaitForExit();
        }

        private void setupend()
        {
            try
            {
                myWindow.Visibility = Visibility.Visible;
                winsetupinfo.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void createlist()
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

        private void updateserverlist(ObservableCollection<Server> newauthlist, ObservableCollection<Server> newassetlist)
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
            bt_checkservers.Content = "Check Servers (Last update: " + DateTime.Now.ToString("h:mm:ss tt") + ")";

            // Sorting  servers (ping).
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listview_auth.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Descending));
            CollectionView sview = (CollectionView)CollectionViewSource.GetDefaultView(listview_assets.ItemsSource);
            sview.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Descending));
            sview.Refresh();
        }

        private void LoadConfig()
        {
            try
            {
                // Checking if saved priority is different then normal and if its enabled.
                if (Properties.Settings.Default.priority != 0) priority_type.SelectedValue = Properties.Settings.Default.priority;
                if (Properties.Settings.Default.use_priority == true) cb_priority.IsChecked = true;
                if (Properties.Settings.Default.use_autologin == true) cb_login.IsChecked = true;
                //if (Properties.Settings.Default.selected_acc != 0) listview_acc.SelectedIndex = Cinema_Accountlist.SelectedIndex = Properties.Settings.Default.selected_acc;
            }
            catch (Exception err)
            {
                MessageBox.Show("Error in UI setup. \n " + err.Message);
            }

            // Importing the XML file at AppData\Roaming\Guild Wars 2\
            // This file also contains infos about the graphic settings

            //Find the newest xml file in APPDATA (the xml files share the same name as their exe files -> multiple .xml files possible!)
            string[] configfiles = new string[] { };
            try
            {
                configfiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\", "*.exe.xml");
            }
            catch (Exception e)
            {
                //TODO: Handle corrupt/missing Guild Wars install
                MessageBox.Show("Guild Wars may not be installed. \n " + e.Message);
                return;
            }
            Globals.ClientXmlpath = "";
            long max = 0;

            foreach (string config in configfiles)
            {
                if (System.IO.File.GetLastWriteTime(config).Ticks > max)
                {
                    max = System.IO.File.GetLastWriteTime(config).Ticks;
                    Globals.ClientXmlpath = config;
                }
            }

            //Read the GFX Settings
            Globals.SelectedGFX = GFXManager.ReadFile(Globals.ClientXmlpath);
            lv_gfx.ItemsSource = Globals.SelectedGFX.Config;
            lv_gfx.Items.Refresh();

            // Read the xml file
            try
            {
                StreamReader stream = new System.IO.StreamReader(Globals.ClientXmlpath);
                XmlTextReader reader = null;
                reader = new XmlTextReader(stream);

                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "VERSIONNAME":
                            Regex filter = new Regex(@"\d*\d");
                            ClientManager.ClientInfo.Version = filter.Match(getvalue(reader)).Value;
                            lab_version.Content = "Client Version: " + ClientManager.ClientInfo.Version;
                            break;

                        case "INSTALLPATH":

                            ClientManager.ClientInfo.InstallPath = getvalue(reader);
                            lab_path.Content = "Install Path: " + ClientManager.ClientInfo.InstallPath;
                            break;

                        case "EXECUTABLE":

                            ClientManager.ClientInfo.Executable = getvalue(reader);
                            lab_path.Content += ClientManager.ClientInfo.Executable;
                            break;

                        case "EXECCMD":
                            //Filter arguments from path
                            lab_para.Content = "Latest Startparameters: ";
                            Regex regex = new Regex(@"(?<=^|\s)-(umbra.(\w)*|\w*)");
                            string input = getvalue(reader);
                            MatchCollection matchList = regex.Matches(input);

                            //ArgumentManager.ToList().Where(a => matchList.Cast<Match>().Select(b => b.Value).Contains(a.Flag) && !a.Blocker).ToList().ForEach(c => c.IsSelected());

                            foreach (Argument argument in ArgumentManager.ArgumentCollection)
                                foreach (Match parameter in matchList)
                                    if (argument.Flag == parameter.Value && !argument.Blocker)
                                        AccountArgumentManager.StopGap.IsSelected(parameter.Value, true);

                            //foreach (CheckBox entry in arglistbox.Items)
                            //{
                            //    foreach (Match parameter in matchList)
                            //    {
                            //        if (entry.Content.ToString().Equals(parameter.Value, StringComparison.OrdinalIgnoreCase) &&
                            //            !noKeep.Contains(parameter.Value, StringComparer.OrdinalIgnoreCase))
                            //            entry.IsChecked = true;
                            //    }
                            //}

                            RefreshUI();
                            break;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Gw2 info file not found! Please choose the Directory manualy!");
            }
        }

        private string getvalue(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                return reader.Value;
            }
            return null;
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

        private long getping(string ip)
        {
            // Get Ping in ms
            Ping pingsender = new Ping();
            return (int)pingsender.Send(ip).RoundtripTime;
        }

        private void bt_checkservers_Click(object sender, RoutedEventArgs e)
        {
            //Starting servercheck thread
            bt_checkservers.Content = "Loading Server List";
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
                Globals.selected_assetsv = (Server)listview_assets.SelectedItem;
                tb_assetsport.Text = Globals.selected_assetsv.Port;
                checkb_assets.Content = "Use Assets Server : " + Globals.selected_assetsv.IP;
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
                Globals.selected_authsv = (Server)listview_auth.SelectedItem;

                tb_authport.Text = Globals.selected_authsv.Port;

                checkb_auth.Content = "Use Authentication Server : " + Globals.selected_authsv.IP;
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
            tb_authport.Text = Globals.selected_authsv.Port;
            AccountArgumentManager.StopGap.IsSelected("-authsrv");
            AccountArgumentManager.StopGap.SetOptionString("-authsrv", Globals.selected_authsv.IP + ":" + tb_authport.Text);
            RefreshUI();
        }

        private void checkb_auth_Unchecked(object sender, RoutedEventArgs e)
        {
            lab_port_auth.IsEnabled = false;
            tb_authport.IsEnabled = false;
            AccountArgumentManager.StopGap.IsSelected("-authsrv", false);
            AccountArgumentManager.StopGap.SetOptionString("-authsrv", null);
            RefreshUI();
        }

        private void checkb_assets_Checked(object sender, RoutedEventArgs e)
        {
            lab_port_assets.IsEnabled = true;
            tb_assetsport.IsEnabled = true;
            tb_assetsport.Text = Globals.selected_assetsv.Port;
            AccountArgumentManager.StopGap.IsSelected("-assetsrv");
            AccountArgumentManager.StopGap.SetOptionString("-assetsrv", Globals.selected_assetsv.IP + ":" + tb_assetsport.Text);
            RefreshUI();
        }

        private void checkb_assets_Unchecked(object sender, RoutedEventArgs e)
        {
            lab_port_assets.IsEnabled = false;
            tb_assetsport.IsEnabled = false;
            AccountArgumentManager.StopGap.IsSelected("-assetsrv", false);
            AccountArgumentManager.StopGap.SetOptionString("-assetsrv", null);
            RefreshUI();
        }

        private void checkb_clientport_Checked(object sender, RoutedEventArgs e)
        {
            tb_clientport.IsEnabled = true;
            lab_port_client.IsEnabled = true;
            AccountArgumentManager.StopGap.IsSelected("-clientport");
            AccountArgumentManager.StopGap.SetOptionString("-clientport", tb_clientport.Text);
            RefreshUI();
        }

        private void checkb_clientport_Unchecked(object sender, RoutedEventArgs e)
        {
            tb_clientport.IsEnabled = false;
            lab_port_client.IsEnabled = false;
            AccountArgumentManager.StopGap.IsSelected("-clientport", false);
            AccountArgumentManager.StopGap.SetOptionString("-clientport", null);
            RefreshUI();
        }

        private void bt_launch_Click(object sender, RoutedEventArgs e)
        {
            GFXManager.OverwriteGFX();
            //UpdateServerArgs();
            LaunchManager.Launch();
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
                ClientManager.ClientInfo.InstallPath = Path.GetDirectoryName(filedialog.FileName) + @"\";
                ClientManager.ClientInfo.Executable = Path.GetFileName(filedialog.Fi‌​leName);
                lab_path.Content = ClientManager.ClientInfo.InstallPath + ClientManager.ClientInfo.Executable;
                //Gw2_Launchbuddy.Properties.Settings.Default.reshadepath = exename;
            }
        }

        public void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            // Needs rewrite
            // Modified Shortcut script by "CooLMinE" at http://www.fluxbytes.com/
            try
            {
                string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
                string arguments = AccountManager.Account(null).CommandLine();
                shortcut.IconLocation = Assembly.GetExecutingAssembly().Location;
                shortcut.Description = "Created with Gw2 Launchbuddy, © TheCheatsrichter";

                shortcut.Arguments = arguments;
                shortcut.TargetPath = targetFileLocation;
                shortcut.Save();

                string dynamicinfo = "";
                foreach (string arg in arguments.Split(' '))
                {
                    dynamicinfo += arg + "\n\t\t";
                }

                System.Windows.MessageBox.Show("Custom Launcher created at : " + ClientManager.ClientInfo.InstallPath + "\nUsed arguments:" + dynamicinfo);
            }
            catch (Exception err)
            {
                MessageBox.Show("Error when creating shortcut. Invalid Path?\n\n" + err.Message);
            }
        }

        private void arglistbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (arglistbox.SelectedItem != null)
            {
                var item = (Gw2_Launchbuddy.ObjectManagers.AccountArgument)arglistbox.SelectedItem;
                lab_descr.Content = "Description (" + item.Argument.Flag + "):";

                textblock_descr.Text = ArgumentManager.ArgumentCollection.Where(a => a.Flag == item.Argument.Flag).Select(a => a.Description).FirstOrDefault() ?? "Description missing! (PLEASE REPORT)";
            }
        }

        private void bt_shortcut_Click(object sender, RoutedEventArgs e)
        {
            if (cb_login.IsChecked == true)
            {
                try
                {
                    CreateShortcut("Gw2_Launcher_" + AccountManager.DefaultAccount.Nickname, ClientManager.ClientInfo.InstallPath, ClientManager.ClientInfo.InstallPath + ClientManager.ClientInfo.Executable);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
            else
            {
                CreateShortcut("Gw2_Custom_Launcher", ClientManager.ClientInfo.InstallPath, ClientManager.ClientInfo.InstallPath + ClientManager.ClientInfo.Executable);
            }
            try
            {
                Process.Start(ClientManager.ClientInfo.InstallPath);
            }
            catch (Exception err)
            {
                MessageBox.Show("Could not open file directory\n" + err.Message);
            }
        }

        private void SaveAddons()
        {
            AddOnManager.SaveAddons(Globals.AppDataPath + "Addons.xml");
        }

        private void LoadAddons()
        {
            lv_AddOns.ItemsSource = AddOnManager.LoadAddons(Globals.AppDataPath + "Addons.xml");
        }

        private void bt_addacc_Click(object sender, RoutedEventArgs e)
        {
            if (ObjectManagers.AccountManager.IsValidEmail(tb_email.Text))
            {
                if (tb_passw.Password.Length > 4)
                {
                    Account acc = new Account(tb_nick.Text, tb_email.Text, tb_passw.Password);
                    AccountManager.Add(acc);
                    listview_acc.ItemsSource = AccountManager.AccountCollection;
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
            CheckAutoLogin();
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
            CheckAutoLogin();
        }

        private void listview_acc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bt_accsortup.IsEnabled = true;
            bt_accsortdown.IsEnabled = true;
            bt_accedit.IsEnabled = true;
            bt_remacc.IsEnabled = true;

            var selectedAccounts = ((ListView)sender).Items.Cast<Account>();
            AccountManager.SetSelected(selectedAccounts);

            cb_login.Content = "Use Autologin: " + (AccountManager.SelectedAccountCollection.Count == 1 ? AccountManager.SelectedAccountCollection[0].Nickname : AccountManager.SelectedAccountCollection.Count + " Accounts Total");

            CheckAutoLogin();
        }

        private void CheckAutoLogin()
        {
            if (AccountManager.SelectedAccountCollection.Count == 0)
            {
                AccountArgumentManager.StopGap.IsSelected("-email", false);
                AccountArgumentManager.StopGap.IsSelected("-password", false);
                AccountArgumentManager.StopGap.IsSelected("-nopatchui", false);
            }
            else
            {
                AccountArgumentManager.StopGap.IsSelected("-email");
                AccountArgumentManager.StopGap.IsSelected("-password");
                AccountArgumentManager.StopGap.IsSelected("-nopatchui");
            }
            RefreshUI();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.instance_win_X = Globals.Appmanager.Left;
            Properties.Settings.Default.instance_win_Y = Globals.Appmanager.Top;
            Properties.Settings.Default.use_priority = (bool)cb_priority.IsChecked;
            Properties.Settings.Default.priority = (ProcessPriorityClass)priority_type.SelectedValue;
            Properties.Settings.Default.Save();
            AccountManager.ImportExport.SaveAccountInfo();
            SaveAddons();
            AddOnManager.CloseAll();
            Environment.Exit(Environment.ExitCode);
        }

        private void bt_remacc_Click(object sender, RoutedEventArgs e)
        {
            if (listview_acc.SelectedItem != null)
            {
                AccountManager.Remove(listview_acc.SelectedItem as Account);
                listview_acc.SelectedIndex = -1;
            }
        }

        private void bt_quaggan_Click(object sender, RoutedEventArgs e)
        {
            if (ClientManager.ClientInfo.InstallPath != "")
            {
                ClientFix clientfix = new ClientFix();
                clientfix.Show();
            }
            else
            {
                MessageBox.Show("Gw2.exe install path is empty!");
            }
        }

        private void tb_authport_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Globals.selected_authsv.Port = tb_authport.Text;
        }

        private void tb_assetsport_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Globals.selected_assetsv.Port = tb_assetsport.Text;
        }

        private void exp_server_Collapsed(object sender, RoutedEventArgs e)
        {
            //ServerUI.Height = new GridLength(30);
            Application.Current.MainWindow.Height = 585;
        }

        private void exp_server_Expanded(object sender, RoutedEventArgs e)
        {
            //ServerUI.Height = new GridLength(290);
            Application.Current.MainWindow.Height = 845;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listview_auth.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Ascending));
            CollectionView sview = (CollectionView)CollectionViewSource.GetDefaultView(listview_assets.ItemsSource);
            sview.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Ascending));
        }

        private void SortByColumn(ListView list, object sender)
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

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            Application.Current.Shutdown();
        }

        private void tab_options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                RefreshUI();
            }
        }

        private void tab_options_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            RefreshUI();
        }

        private void bt_minimize_Click(object sender, RoutedEventArgs e)
        {
            myWindow.WindowState = WindowState.Minimized;
            myWindow.Opacity = 0;
        }

        private void bt_cinema_setimagefolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderdialog = new System.Windows.Forms.FolderBrowserDialog();

            if (System.Windows.Forms.DialogResult.OK == folderdialog.ShowDialog())
            {
                lv_cinema_images.SelectedIndex = -1;
                lab_imagepreview.Content = "Current Image:";
                var files = Directory.GetFiles(folderdialog.SelectedPath, "*.*", SearchOption.AllDirectories).Where(a => a.EndsWith(".png") || a.EndsWith(".jpg") || a.EndsWith(".jpeg") || a.EndsWith(".bmp"));
                ObservableCollection<CinemaImage> images = new ObservableCollection<CinemaImage>();
                foreach (var file in files)
                {
                    images.Add(new CinemaImage(file));
                    lv_cinema_images.ItemsSource = images;
                    Properties.Settings.Default.cinema_imagepath = folderdialog.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void lv_cinema_images_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedimage = lv_cinema_images.SelectedItem as CinemaImage;
            if (selectedimage != null)
            {
                img_imagepreview.Source = LoadImage(selectedimage.Path);
                lab_imagepreview.Content = "Current Image: " + selectedimage.Name;
            }
        }

        private void bt_cinema_setmask_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.Multiselect = false;
            filedialog.Filter = "Png Files(*.png) | *.png";
            filedialog.ShowDialog();

            if (filedialog.FileName != "")
            {
                Properties.Settings.Default.cinema_maskpath = filedialog.FileName;
                Properties.Settings.Default.Save();
                lab_maskpreview.Content = "Current Mask: " + Path.GetFileName(filedialog.FileName);
                img_maskpreview.Source = LoadImage(filedialog.FileName);
                ImageBrush newmask = new ImageBrush(LoadImage(filedialog.FileName));
                newmask.Stretch = Stretch.Uniform;
                img_slideshow.OpacityMask = newmask;
            }
        }

        private void listview_auth_Click(object sender, RoutedEventArgs e)
        {
            SortByColumn(listview_auth, sender);
        }

        private void bt_setmusic_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.Multiselect = false;
            filedialog.Filter = "MP3 Files(*.mp3) | *.mp3|WAV Files (*.wav)|*.wav|AAC Files (*.aac)|*.aac|All Files(*.*)|*.*";
            filedialog.ShowDialog();

            if (filedialog.FileName != "")
            {
                Properties.Settings.Default.cinema_musicpath = filedialog.FileName;
                Properties.Settings.Default.Save();
                lab_musicpath.Content = "Current Music File: " + Path.GetFileName(filedialog.FileName);
                Cinema_MediaPlayer.Source = (new Uri(filedialog.FileName));
            }
        }

        private bool IsValidPath(string path)
        {
            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void LoadCinemaSettings()
        {
            string imagepath = Properties.Settings.Default.cinema_imagepath;
            string maskpath = Properties.Settings.Default.cinema_maskpath;
            string musicpath = Properties.Settings.Default.cinema_musicpath;
            string loginpath = Properties.Settings.Default.cinema_loginwindowpath;

            try
            {
                if (IsValidPath(imagepath) && Directory.Exists(imagepath))
                {
                    var files = Directory.GetFiles(imagepath, "*.*", SearchOption.AllDirectories).Where(a => a.EndsWith(".png") || a.EndsWith(".jpg") || a.EndsWith(".jpeg") || a.EndsWith(".bmp"));
                    ObservableCollection<CinemaImage> images = new ObservableCollection<CinemaImage>();
                    foreach (var file in files)
                    {
                        images.Add(new CinemaImage(file));
                        lv_cinema_images.ItemsSource = images;
                    }
                }
            }
            catch { }

            try
            {
                if (IsValidPath(maskpath) && Path.GetExtension(maskpath) == ".png")
                {
                    img_maskpreview.Source = LoadImage(maskpath);
                    lab_maskpreview.Content = "Current Mask: " + Path.GetFileName(maskpath);
                }
            }
            catch
            {
                lab_maskpreview.Content = "Current Mask: ERROR! " + Path.GetFileName(maskpath) + " file not found!";
            }

            try
            {
                if (IsValidPath(loginpath) && Path.GetExtension(maskpath) == ".png")
                {
                    lab_loginwindowpath.Content = "Current Login Window: " + Path.GetFileName(loginpath);
                }
            }
            catch
            {
                lab_loginwindowpath.Content = "Current Mask: ERROR! " + Path.GetFileName(loginpath) + " file not found!";
            }

            try
            {
                if (IsValidPath(musicpath))
                {
                    Cinema_MediaPlayer.Source = (new Uri(musicpath));
                    lab_musicpath.Content = "Current Music File: " + Path.GetFileName(musicpath);
                }
            }
            catch
            {
                lab_maskpreview.Content = "Current Music File: ERROR! " + Path.GetFileName(musicpath) + " file not found!";
            }
        }

        private void bt_musicstart_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.cinema_musicpath != null && Properties.Settings.Default.cinema_musicpath != "")
            {
                Cinema_MediaPlayer.Source = new Uri(Properties.Settings.Default.cinema_musicpath);
                Cinema_MediaPlayer.Play();
            }
            else
            {
                MessageBox.Show("Invalid Music Path");
            }
        }

        private void bt_musicstop_Click(object sender, RoutedEventArgs e)
        {
            Cinema_MediaPlayer.Stop();
        }

        private void bt_cinema_Click(object sender, RoutedEventArgs e)
        {
            cinemamode = !Properties.Settings.Default.cinema_use;
            Properties.Settings.Default.cinema_use = cinemamode;
            Properties.Settings.Default.Save();
            Cinema_MediaPlayer.Stop();
            cinema_setup();
        }

        private void bt_ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsGrid.Visibility == Visibility.Hidden)
            {
                SettingsGrid.Visibility = Visibility.Visible;
            }
            else
            {
                SettingsGrid.Visibility = Visibility.Hidden;
            }
        }

        private void rb_slideshowmode(object sender, RoutedEventArgs e)
        {
            try
            {
                Videomode.Visibility = Visibility.Collapsed;
                Slideshow.Visibility = Visibility.Visible;
                Properties.Settings.Default.cinema_video = false;
                Properties.Settings.Default.cinema_slideshow = true;
                Properties.Settings.Default.Save();
            }
            catch { }
        }

        private void rb_videomode(object sender, RoutedEventArgs e)
        {
            try
            {
                Slideshow.Visibility = Visibility.Collapsed;
                Videomode.Visibility = Visibility.Visible;
                Properties.Settings.Default.cinema_video = true;
                Properties.Settings.Default.cinema_slideshow = false;
                Properties.Settings.Default.Save();
            }
            catch { }
        }

        private void bt_cinema_setvideo_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.DefaultExt = "mp4";
            filedialog.Multiselect = false;
            filedialog.Filter = "Mp4 Files(*.mp4) | *.mp4|Raw Files(*.raw)|*.raw|Wmv Files(*.wmv)|*.wmv|Mpeg Files(*.mpeg)|*.mpeg|All Files(*.*)|*.*";
            System.Windows.Forms.DialogResult result = filedialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                cinema_videoplayback.Source = new Uri(filedialog.FileName, UriKind.Relative);
                Properties.Settings.Default.cinema_videopath = filedialog.FileName;
                SetVideoInfo();
                cinema_videoplayback.Play();
            }
        }

        private void SetVideoInfo()
        {
            try
            {
                string videopath = Properties.Settings.Default.cinema_videopath;

                lab_videoname.Content = "Name: " + Path.GetFileNameWithoutExtension(videopath);
                lab_videopath.Content = "Path: " + Path.GetFullPath(videopath);
                lab_videoformat.Content = "Format: " + Path.GetExtension(videopath);
                lab_videoresolution.Content = "Resolution: " + cinema_videoplayback.NaturalVideoWidth + " x " + cinema_videoplayback.NaturalVideoHeight;
                lab_videolength.Content = "Length: " + cinema_videoplayback.NaturalDuration.ToString();
            }
            catch { }
        }

        private void bt_cinema_videoplay_Click(object sender, RoutedEventArgs e)
        {
            if (cinema_videoplayback.Source != null)
            {
                cinema_videoplayback.Play();
            }
        }

        private void bt_cinema_videostop_Click(object sender, RoutedEventArgs e)
        {
            if (cinema_videoplayback.Source != null)
            {
                cinema_videoplayback.Stop();
            }
        }

        private void listview_assets_Click(object sender, RoutedEventArgs e)
        {
            SortByColumn(listview_assets, sender);
        }

        private static BitmapSource LoadImage(string path)
        {
            var bitmap = new BitmapImage();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            return bitmap;
        }

        private void cinema_videoplayback_Loaded(object sender, RoutedEventArgs e)
        {
            SetVideoInfo();
        }

        private void cinema_videoplayback_MediaOpened(object sender, RoutedEventArgs e)
        {
            SetVideoInfo();
        }

        private void Cinema_Launchaccount_Click(object sender, RoutedEventArgs e)
        {
            //UpdateServerArgs();
            LaunchManager.Launch();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = SettingsGrid.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        private void CheckBox_Checked(Object sender, RoutedEventArgs e)
        {
            AccountArgumentManager.StopGap.IsSelected(((CheckBox)sender).Content.ToString(), true);
            RefreshUI();
        }

        private void CheckBox_Unchecked(Object sender, RoutedEventArgs e)
        {
            AccountArgumentManager.StopGap.IsSelected(((CheckBox)sender).Content.ToString(), false);
            RefreshUI();
        }

        private void RefreshUI()
        {
            lab_currentsetup.Content = "Current Setup: " + AccountArgumentManager.StopGap.Print();
            lab_usedaddons.Content = "Used AddOns: " + AddOnManager.ListAddons();
        }

        private void UpdateServerArgs()
        {
            //Should really be bound to changing applicable UI elements
            if (checkb_assets.IsChecked == true)
                AccountArgumentManager.StopGap.SetOptionString("-assetsrv", Globals.selected_assetsv.IP + ":" + tb_assetsport.Text);
            if (checkb_auth.IsChecked == true)
                AccountArgumentManager.StopGap.SetOptionString("-authsrv ", Globals.selected_authsv.IP + ":" + tb_authport.Text);
            if (checkb_clientport.IsChecked == true)
                AccountArgumentManager.StopGap.SetOptionString("-clientport", tb_clientport.Text);
        }

        private void Window_LostKeyboardFocus(Object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (cinemamode || rb_cinemavideomode.IsChecked == true) { Cinema_MediaPlayer.Pause(); }
            BeginStoryboard(this.FindResource("anim_musicfadeout") as System.Windows.Media.Animation.Storyboard);
        }

        private void Window_GotKeyboardFocus(Object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (cinemamode || rb_cinemavideomode.IsChecked == true) { Cinema_MediaPlayer.Play(); }
            BeginStoryboard(this.FindResource("anim_musicfadein") as System.Windows.Media.Animation.Storyboard);
        }

        private void myWindow_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void bt_mute_Click(object sender, RoutedEventArgs e)
        {
            if (Cinema_MediaPlayer.IsMuted)
            {
                Cinema_MediaPlayer.IsMuted = false;
                img_mutebutton.Source = new BitmapImage(new Uri("/Resources/Icons/speaker_loud.png", UriKind.Relative));
            }
            else
            {
                Cinema_MediaPlayer.IsMuted = true;
                img_mutebutton.Source = new BitmapImage(new Uri("/Resources/Icons/speaker_mute.png", UriKind.Relative));
            }
        }

        private void bt_mute_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sl_volumecontrol.Visibility = Visibility.Visible;
        }

        private void sl_volumecontrol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Cinema_MediaPlayer.Volume = sl_volumecontrol.Value;
        }

        private void WrapPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sl_volumecontrol.Visibility = Visibility.Collapsed;
            Properties.Settings.Default.mediaplayer_volume = sl_volumecontrol.Value;
            Properties.Settings.Default.Save();
        }

        private void Cinema_MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Stop();
            (sender as MediaElement).Play();
        }

        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            myWindow.Background = new SolidColorBrush((Color)ClrPcker_Background.SelectedColor);
            if ((Color)ClrPcker_Background.SelectedColor != null)
            {
                Properties.Settings.Default.cinema_backgroundcolor = (Color)ClrPcker_Background.SelectedColor;
                Properties.Settings.Default.Save();
            }
        }

        private void bt_setloginwindow_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.DefaultExt = "png";
            filedialog.Multiselect = false;
            filedialog.Filter = "Png Files(*.png) | *.png";
            System.Windows.Forms.DialogResult result = filedialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                cinema_videoplayback.Source = new Uri(filedialog.FileName, UriKind.Relative);
                Properties.Settings.Default.cinema_loginwindowpath = filedialog.FileName;
                img_loginwindow.Source = LoadImage(filedialog.FileName);
                lab_loginwindowpath.Content = "Current Loginwindow: " + Path.GetFileNameWithoutExtension(filedialog.FileName);
                Properties.Settings.Default.Save();
            }
        }

        private void sl_logoendpos_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var anim_slideshow = (System.Windows.Media.Animation.Storyboard)Resources["anim_slideshow_start"];
            anim_slideshow.Begin();
        }

        private void sl_logoendpos_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var anim_slideshow = (System.Windows.Media.Animation.Storyboard)Resources["anim_slideshow_start"];
            anim_slideshow.Begin();
            Properties.Settings.Default.cinema_slideshowendpos = (int)sl_logoendpos.Value;
            Properties.Settings.Default.Save();
        }

        private void sl_logoendscaleX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var endscale = (System.Windows.Media.Animation.EasingDoubleKeyFrame)Resources["Mask_EndScaleX"];
            endscale.Value = sl_logoendscaleX.Value;
            lab_endscaleX.Content = "Image EndScale: " + Math.Round(sl_logoendscaleX.Value, 2) + " X Zoom";
        }

        private void sl_logoendscaleX_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var anim_slideshow = (System.Windows.Media.Animation.Storyboard)Resources["anim_slideshow_start"];
            anim_slideshow.Begin();
            Properties.Settings.Default.cinema_slideshowendscale = sl_logoendscaleX.Value;
            Properties.Settings.Default.Save();
        }

        private void bt_loadgfx_Click(object sender, RoutedEventArgs e)
        {
            var tmp = GFXManager.LoadFile();
            if (tmp != null)
            {
                Globals.SelectedGFX = tmp;
                lv_gfx.ItemsSource = Globals.SelectedGFX.Config;
                lv_gfx.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Invalid GFX Config File selected!");
            }
        }

        private void bt_resetgfx_Click(object sender, RoutedEventArgs e)
        {
            Globals.SelectedGFX = GFXManager.ReadFile(Globals.ClientXmlpath);
            lv_gfx.ItemsSource = Globals.SelectedGFX.Config;
            lv_gfx.Items.Refresh();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            ComboBox box = sender as ComboBox;
            GFXOption option = box.DataContext as GFXOption;

            if (option !=null)
            {
                lv_gfx.SelectedItem = option;
                if(option.Value!=option.OldValue)
                {
                    (lv_gfx.SelectedItem as ListViewItem).Background = Brushes.Gray;
                }
            }
            */
        }

        private void lv_gfx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (lv_gfx.SelectedItem != null) MessageBox.Show((lv_gfx.SelectedItem as GFXOption).ToXml());
        }

        private void bt_savegfx_Click(object sender, RoutedEventArgs e)
        {
            GFXManager.SaveFile();
        }

        private void bt_applygfx_Click(object sender, RoutedEventArgs e)
        {
            GFXManager.OverwriteGFX();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ClientManager.ClientInfo.InstallPath + ClientManager.ClientInfo.Executable;
            startInfo.Arguments = " -image -shareArchive";
            Process gw2pro = new Process { StartInfo = startInfo };

            gw2pro.Start();
            gw2pro.WaitForExit();

            Globals.SelectedGFX = GFXManager.ReadFile(Globals.ClientXmlpath);
            lv_gfx.ItemsSource = Globals.SelectedGFX.Config;
            lv_gfx.Items.Refresh();
        }

        private void bt_accsortup_Click(object sender, RoutedEventArgs e)
        {
            if (listview_acc.SelectedItem != null)
            {
                int index = listview_acc.SelectedIndex;
                Account selectedacc = AccountManager.SelectedAccountCollection[index];
                if (index - 1 >= 0)
                {
                    selectedacc.Move(-1);
                    listview_acc.SelectedIndex = index - 1;
                }
            }
        }

        private void bt_accsortdown_Click(object sender, RoutedEventArgs e)
        {
            if (listview_acc.SelectedItem != null)
            {
                int index = listview_acc.SelectedIndex;
                Account selectedacc = AccountManager.AccountCollection[index];
                if (index + 1 < AccountManager.AccountCollection.Count)
                {
                    selectedacc.Move(+1);
                    listview_acc.SelectedIndex = index + 1;
                }
            }
        }

        private void bt_accedit_Click(object sender, RoutedEventArgs e)
        {
            Account selectedacc = listview_acc.SelectedItem as Account;
            if (selectedacc != null)
            {
                string input = null;
                string message = "Please enter the password of the account.\n\nNickname:\t" + selectedacc.Nickname + "\nCreated at:\t" + selectedacc.CreateDate;
                TextBoxPopUp pw_win = new Gw2_Launchbuddy.TextBoxPopUp(message, "Editing Account", true);
                if (pw_win.ShowDialog().Value)
                {
                    input = pw_win.Input();
                }
                else
                {
                    return;
                }

                if (input == selectedacc.Password)
                {
                    tb_email.Text = selectedacc.Email;
                    tb_nick.Text = selectedacc.Nickname;
                    tb_passw.Password = selectedacc.Password;
                    AccountManager.Account(selectedacc.Nickname).Remove();
                    tb_nick.Focus();
                }
                else
                {
                    MessageBox.Show("Invalid password!\nMake sure the entered password is correct.(Case sensitive!)\n\nIf the account's password allready is saved incorrectly editing is not possible");
                }
            }
        }

        private void bt_selecticon_Click(object sender, RoutedEventArgs e)
        {
            Account acc = (sender as Button).DataContext as Account;
            acc = AccountManager.AccountCollection.Single(x => x.Email == acc.Email);

            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.DefaultExt = "png";
            filedialog.Multiselect = false;
            filedialog.Filter = "Png Files(*.png) | *.png";
            System.Windows.Forms.DialogResult result = filedialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                acc.SetIcon(filedialog.FileName);
            }
            listview_acc.Items.Refresh();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bt_downloadrelease.IsEnabled = true;
            bt_downloadrelease.Content = "Download and use Release V" + (lv_lbversions.SelectedItem as Release).Version;
            wb_releasedescr.NavigateToString((lv_lbversions.SelectedItem as Release).Description);
        }

        private void bt_downloadrelease_Click(object sender, RoutedEventArgs e)
        {
            if (lv_lbversions.SelectedItem != null)
            {
                Version rl_version = (lv_lbversions.SelectedItem as Release).Version;
                if (rl_version.CompareTo(Globals.LBVersion) < 0)
                {
                    MessageBoxResult win = MessageBox.Show("Usage of older versions of Launchbuddy can corrupt your Accountmanager data!\n\nAre you sure you want to download V" + rl_version, "Release Download", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (win.ToString() == "No")
                    {
                        return;
                    }
                }
                (sender as Button).Content = "Downloading LB V" + rl_version;
                VersionSwitcher.ApplyRelease(lv_lbversions.SelectedItem as Release);
                (sender as Button).Content = "Download";
            }
        }

        private void bt_selectaccgfx_Click(object sender, RoutedEventArgs e)
        {
            Account acc = (sender as Button).DataContext as Account;
            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.DefaultExt = "xml";
            filedialog.Multiselect = false;
            filedialog.Filter = "GFX Files(*.xml) | *.xml";
            filedialog.ShowDialog();

            if (filedialog.FileName != "")
            {
                acc.ConfigurationPath = filedialog.FileName;
                (sender as Button).Content = acc.ConfigurationName;
            }
        }

        private void bt_bugreport_Click(object sender, RoutedEventArgs e)
        {
            CrashReporter.ReportCrashToAll(new Exception("Bugreport"));
        }

        private void bt_fetchlbversions_Click(object sender, RoutedEventArgs e)
        {
            Thread checkforlbupdate = new Thread(checklbversion);
            checkforlbupdate.Start();
        }

        private void cb_lbupdatescheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.notifylbupdate = (bool)cb_lbupdatescheck.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void cb_priority_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.use_priority = false;
        }

        private void cb_priority_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.use_priority = true;
        }

        private void priority_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.priority = (ProcessPriorityClass)priority_type.SelectedItem;
        }

        private void cb_enable_Click(object sender, RoutedEventArgs e)
        {
            AddOn addon = (sender as CheckBox).DataContext as AddOn;
            addon.IsEnabled = (bool)(sender as CheckBox).IsChecked;
        }

        private void bt_EditAddon_Click(object sender, RoutedEventArgs e)
        {
            AddOn selectedaddon = lv_AddOns.SelectedItem as AddOn;
            if (selectedaddon != null)
            {
                cb_AddonMultiBefore.IsChecked = selectedaddon.IsMultibefore;
                cb_AddonMultiLaunch.IsChecked = selectedaddon.IsMultilaunch;
                cb_AddonSingle.IsChecked = selectedaddon.IsSinglelaunch;
                tb_AddonName.Text = selectedaddon.Name;
                tb_AddonArgs.Text = selectedaddon.args;
            }
            else
            {
                MessageBox.Show("Please select addon!");
            }

            AddOn item = lv_AddOns.SelectedItem as AddOn;
            AddOnManager.Remove(item.Name);
        }

        private void bt_AddAddon_Click(object sender, RoutedEventArgs e)
        {
            string[] args = Regex.Matches(tb_AddonArgs.Text, "--\\w* ?(\".*\")?|-\\w* ?(\".*\")?").Cast<Match>().Select(m => m.Value).ToArray(); //New Regex --\w* ?(\".*\")?|-\w* ?(\".*\")?
            AddOnManager.Add(tb_AddonName.Text, args, (bool)cb_AddonMultiLaunch.IsChecked, (bool)cb_AddonMultiBefore.IsChecked, (bool)cb_AddonSingle.IsChecked, true);
            tb_AddonName.Text = "";
            tb_AddonArgs.Text = "";
            cb_AddonMultiLaunch.IsChecked = false;
            cb_AddonMultiBefore.IsChecked = false;
            cb_AddonSingle.IsChecked = false;
            lv_AddOns.ItemsSource = AddOnManager.AddOns;
        }

        private void bt_RemAddon_Click(object sender, RoutedEventArgs e)
        {
            AddOn item = lv_AddOns.SelectedItem as AddOn;
            if (item != null)
            {
                AddOnManager.Remove(item.Name);
            }
        }

        private void cb_AddonMulti_Checked(object sender, RoutedEventArgs e)
        {
            cb_AddonMultiBefore.IsEnabled = true;
            cb_AddonSingle.IsEnabled = false;
        }

        private void cb_AddonMulti_Unchecked(object sender, RoutedEventArgs e)
        {
            cb_AddonMultiBefore.IsEnabled = false;
            cb_AddonMultiBefore.IsChecked = false;
            cb_AddonSingle.IsEnabled = true;
        }

        private void cb_AddonSingle_Unchecked(object sender, RoutedEventArgs e)
        {
            cb_AddonMultiLaunch.IsEnabled = true;
        }

        private void cb_AddonSingle_Checked(object sender, RoutedEventArgs e)
        {
            cb_AddonMultiLaunch.IsEnabled = false;
        }

        private void cb_useinstancegui_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.useinstancegui = (bool)cb_useinstancegui.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void sl_logoendpos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var endpos = (System.Windows.Media.Animation.EasingDoubleKeyFrame)Resources["Mask_EndPos"];
            endpos.Value = sl_logoendpos.Value * (reso_x / 200);
        }
    }

    public class SortAdorner : Adorner
    {
        private static Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir) : base(element)
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

    public class ListBoxHelper : DependencyObject
    {
        public static int GetAutoSizeItemCount(DependencyObject obj)
        {
            return (int)obj.GetValue(AutoSizeItemCountProperty);
        }

        public static void SetAutoSizeItemCount(DependencyObject obj, int value)
        {
            obj.SetValue(AutoSizeItemCountProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoSizeItemCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoSizeItemCountProperty =
            DependencyProperty.RegisterAttached("AutoSizeItemCount", typeof(int), typeof(ListBoxHelper), new PropertyMetadata(0, OnAutoSizeItemCountChanged));

        private static void OnAutoSizeItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListBox listBox = d as ListBox;
            listBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler((lb, arg) => UpdateSize(listBox)));
            listBox.ItemContainerGenerator.ItemsChanged += (ig, arg) => UpdateSize(listBox);
        }

        private static void UpdateSize(ListBox listBox)
        {
            ItemContainerGenerator gen = listBox.ItemContainerGenerator;
            FrameworkElement element = listBox.InputHitTest(new Point(listBox.Padding.Left + 5, listBox.Padding.Top + 5)) as FrameworkElement;
            if (element != null && gen != null)
            {
                object item = element.DataContext;
                if (item != null)
                {
                    FrameworkElement container = gen.ContainerFromItem(item) as FrameworkElement;
                    if (container == null)
                    {
                        container = element;
                    }
                    int maxCount = GetAutoSizeItemCount(listBox);
                    double newHeight = Math.Min(maxCount, gen.Items.Count) * container.ActualHeight;
                    newHeight += listBox.Padding.Top + listBox.Padding.Bottom + listBox.BorderThickness.Top + listBox.BorderThickness.Bottom + 2;
                    if (listBox.ActualHeight != newHeight)
                        listBox.Height = newHeight;
                }
            }
        }
    }
}