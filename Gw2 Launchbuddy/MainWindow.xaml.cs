using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Gw2_Launchbuddy.Modifiers;
using System.Windows.Navigation;

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
                //Reference this as Mainwin for external use
                EnviromentManager.MainWin = this;
            }
            catch
            {
                LBConfiguration.Reset();
            }
            Init();
        }

        private void UIInit()
        {
            //Account Setups
            lv_accs.ItemsSource = lv_accssettings.ItemsSource = AccountManager.Accounts;

            //CrashAnalyser
            lv_crashlogs.ItemsSource = CrashAnalyzer.Crashlogs;

            //Plugin List
            lv_plugins.ItemsSource = PluginManager.InstalledPlugins;
        }

        public void Init()
        {
#if !DEBUG

            //LB statistics
            LBConfiguration.Config.counter_launches += 1;
            LBConfiguration.Save();
#else
            System.Diagnostics.Debug.WriteLine("Compiled without crash handler, and always first run.");
            LBConfiguration.Config.counter_launches = 1;
#endif


            EnviromentManager.AfterUI_Inits();

            //Setup
            DonatePopup();
            UIInit();

            /*Thread checkver = new Thread(checkversion);
            checkver.IsBackground = true;
            checkver.Start();*/

            LoadEnviromentUI();
            TacoUISetup();
            cinema_setup();

            Mainwin_LoadSetup(); //do this after cinema setup!

            Cinema_Accountlist.ItemsSource = AccountManager.Accounts;
            lv_accs.ItemsSource = AccountManager.Accounts;
            lv_accssettings.ItemsSource = AccountManager.Accounts;
            SettingsTabSetup();
            if (LBConfiguration.Config.notifylbupdate)
            {
                Thread checklbver = new Thread(checklbversion);
                checklbver.Start();
            }

        }

        private void TacoUISetup()
        {
            tb_tacopath.Text = LBTacO.TacoPath;
        }

        private void LoadEnviromentUI()
        {
            lab_version.Content = "Version: " + EnviromentManager.GwClientVersion;
            lab_path.Content = "Path: " + EnviromentManager.GwClientPath;
        }

        private void Mainwin_LoadSetup()
        {

            this.Top = LBConfiguration.Config.mainwin_pos_y;
            this.Left = LBConfiguration.Config.mainwin_pos_x;

            if (LBConfiguration.Config.mainwin_size_x >= 100 && LBConfiguration.Config.mainwin_size_y >= 100)
            {
                this.Height = LBConfiguration.Config.mainwin_size_y;
                this.Width = LBConfiguration.Config.mainwin_size_x;
            }
        }

        private void Mainwin_SaveSetup()
        {
            LBConfiguration.Config.mainwin_pos_x = this.Left;
            LBConfiguration.Config.mainwin_pos_y = this.Top;
            LBConfiguration.Config.mainwin_size_y = this.Height;
            LBConfiguration.Config.mainwin_size_x = this.Width;
        }

        private void SettingsTabSetup()
        {
            cb_lbupdatescheck.IsChecked = LBConfiguration.Config.notifylbupdate;
            cb_useinstancegui.IsChecked = LBConfiguration.Config.useinstancegui;
            cb_autoupdatedatfiles.IsChecked = LBConfiguration.Config.autoupdatedatfiles;
        }

        private void DonatePopup()
        {
            if ((LBConfiguration.Config.counter_launches % 100) == 5)
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
            string imagespath = LBConfiguration.Config.cinema_imagepath;
            string musicpath = LBConfiguration.Config.cinema_musicpath;
            string maskpath = LBConfiguration.Config.cinema_maskpath;
            string videopath = LBConfiguration.Config.cinema_videopath;
            string loginwindowpath = LBConfiguration.Config.cinema_loginwindowpath;
            var backgroundcolor = LBConfiguration.Config.cinema_backgroundcolor;

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
                    MessageBox.Show("Invalid video file detected! File could not be found / is not a video file.\n File path: " + videopath + "\n\nPlease choose another file in the cinema mode section!");
                    invalid = true;
                }
            }

            if (checkslideshow)
            {
                //Check all needed SlideshowMode resources here
                if (!musicext.Contains(Path.GetExtension(musicpath), StringComparer.OrdinalIgnoreCase) || !System.IO.File.Exists(musicpath))
                {
                    MessageBox.Show("Invalid music file detected! File could not be found / is not a music file.\n File path: " + musicpath + "\n\nPlease choose another file in the cinema mode section!");
                    invalid = true;
                }

                if (!picext.Contains(Path.GetExtension(maskpath), StringComparer.OrdinalIgnoreCase) || !System.IO.File.Exists(maskpath))
                {
                    MessageBox.Show("Invalid mask file detected! File could not be found / is not a picture file.\n File path: " + maskpath + "\n\nPlease choose another file in the cinema mode section!");
                    invalid = true;
                }

                if (!Directory.Exists(imagespath) || Directory.GetFiles(imagespath, "*.*", SearchOption.AllDirectories).Where(a => a.EndsWith(".png") || a.EndsWith(".jpg") || a.EndsWith(".jpeg") || a.EndsWith(".bmp")).ToArray<string>().Length <= 0)
                {
                    MessageBox.Show("Invalid image folder detected! No Images could be found at the chosen location! \n File path: " + imagespath + "\n\nPlease choose another folder in the cinema mode section!");
                    invalid = true;
                }
            }

            //General needed resources
            if ((!picext.Contains(Path.GetExtension(loginwindowpath), StringComparer.OrdinalIgnoreCase) || !System.IO.File.Exists(loginwindowpath)) && loginwindowpath != "")
            {
                MessageBox.Show("Invalid login window file detected! File could not be found / is not a picture file.\n File path: " + loginwindowpath + "\n\nPlease choose another file in the cinema mode section!");
                invalid = true;
            }

            if (invalid) return false;

            return true;
        }

        private void cinema_setup()
        {
            bool videomode = LBConfiguration.Config.cinema_video;
            bool slideshowmode = LBConfiguration.Config.cinema_slideshow;
            cinemamode = LBConfiguration.Config.cinema_use;
            if (cinemamode) cinemamode = cinema_checksetup(slideshowmode, videomode);
            LBConfiguration.Config.cinema_use = cinemamode;
            LBConfiguration.Save();
            LoadCinemaSettings();

            string musicpath = LBConfiguration.Config.cinema_musicpath;
            string imagespath = LBConfiguration.Config.cinema_imagepath;
            string maskpath = LBConfiguration.Config.cinema_maskpath;
            string videopath = LBConfiguration.Config.cinema_videopath;
            string loginwindowpath = LBConfiguration.Config.cinema_loginwindowpath;
            var backgroundcolor = LBConfiguration.Config.cinema_backgroundcolor;

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
            sl_logoendpos.Value = LBConfiguration.Config.cinema_slideshowendpos;
            sl_logoendscaleX.Value = LBConfiguration.Config.cinema_slideshowendscale;
            sl_volumecontrol.Value = LBConfiguration.Config.mediaplayer_volume;
            Cinema_MediaPlayer.Volume = sl_volumecontrol.Value;

            if (cinemamode)
            {
                //Cinema Mode

                
                WindowHeaderGrid.Visibility = Visibility.Hidden;
                myWindow.WindowState = WindowState.Maximized;
                

                //Notes: Login frame = 560x300
                //Test resolutions here!
                //Only edit width!

                int reso_x = (int)myWindow.Width;
                int reso_y = (int)myWindow.Height;

                //Centering Window, only needed in Debug
                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                double windowWidth = this.Width;
                double windowHeight = this.Height;
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);


                //Setting up the Login Window Location
                Canvas.SetTop(Canvas_login, screenHeight - (screenHeight / 2.5));
                Canvas.SetLeft(Canvas_login, screenWidth / 10);
                //Setting up End Position of Logo Animation
                var endpos = (System.Windows.Media.Animation.EasingDoubleKeyFrame)Resources["Mask_EndPos"];
                endpos.Value = LBConfiguration.Config.cinema_slideshowendpos * reso_x / 200;
                var endscale = (System.Windows.Media.Animation.EasingDoubleKeyFrame)Resources["Mask_EndScaleX"];
                endscale.Value = (double)LBConfiguration.Config.cinema_slideshowendscale;

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
                        img_slideshow.Visibility = Visibility.Hidden;
                        //Load background video
                        Cinema_MediaPlayer.Visibility = Visibility.Visible;
                        Cinema_MediaPlayer.Source = new Uri(LBConfiguration.Config.cinema_videopath, UriKind.Relative);
                        Cinema_MediaPlayer.Play();
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show("The chosen video for cinema mode is not valid or does not exist!\n" + err.Message);
                        LBConfiguration.Config.cinema_use = false;
                        LBConfiguration.Save();
                    }
                }

                if (slideshowmode)
                {
                    try
                    {
                        img_slideshow.Visibility = Visibility.Visible;
                        Storyboard anim_slideshow = (Storyboard)FindResource("anim_slideshow_start");
                        anim_slideshow.Begin();

                        if (maskpath != null)
                        {
                            //Setting opacity mask (logo)
                            ImageBrush mask = new ImageBrush(LoadImage(maskpath));
                            mask.Stretch = Stretch.Uniform;
                            img_slideshow.OpacityMask = mask;
                        }

                        //Starting background slide show thread
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
                        MessageBox.Show("One or more settings for slide show mode are missing!\n" + err.Message);
                        LBConfiguration.Config.cinema_use = false;
                        LBConfiguration.Save();
                    }
                }
            }
            else
            {
                //Normal Mode
                WindowHeaderGrid.Visibility = Visibility.Visible;
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
            if (EnviromentManager.GwClientUpToDate == false)
            {
                lab_version.Content = "Build: " + EnviromentManager.GwClientVersion + " (outdated)";
                lab_version.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                lab_version.Content = "Build: " + EnviromentManager.GwClientVersion;
                lab_version.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void setupend()
        {
            try
            {
                myWindow.Visibility = Visibility.Visible;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void UpdateServerUI()
        {
            bt_checkservers.IsEnabled = true;

            lv_auth.ItemsSource = ServerManager.authservers;
            lv_assets.ItemsSource = ServerManager.assetservers;
            lab_authserverlist.Content = "Authentication Servers (" + ServerManager.authservers.Count + " servers found):";
            lab_assetserverlist.Content = "Asset Servers (" + ServerManager.assetservers.Count + " servers found):";
            bt_checkservers.Content = "Check Servers (Last update: " + $"{DateTime.Now:HH:mm:ss tt}" + ")";

            // Sorting  servers (ping).
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lv_auth.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Descending));
            CollectionView sview = (CollectionView)CollectionViewSource.GetDefaultView(lv_assets.ItemsSource);
            sview.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Descending));
            sview.Refresh();
        }

        private void LoadConfig()
        {
            //Read the GFX Settings
            lab_path.Content = "Install Path: " + EnviromentManager.GwClientPath;
        }

        private void bt_checkservers_Click(object sender, RoutedEventArgs e)
        {
            //Starting server check thread
            bt_checkservers.Content = "Loading Server List";
            bt_checkservers.IsEnabled = false;

            Thread serverthread = new Thread(UpdateServerList);
            serverthread.IsBackground = true;
            serverthread.Start();

        }

        private void UpdateServerList()
        {
            ServerManager.UpdateServerlists();
            try
            {
                Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => UpdateServerUI()));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void lv_assets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UI Handling for selected Asset Server
            if (lv_assets.SelectedItem != null)
            {
                ServerManager.SelectedAssetserver = (Server)lv_assets.SelectedItem;
                tb_assetsport.Text = ServerManager.SelectedAssetserver.Port;
                checkb_assets.Content = "Use Assets Server : " + ServerManager.SelectedAssetserver.IP;
                checkb_assets.IsEnabled = true;

                tb_assetsport.IsEnabled = true;
            }
            else
            {
                checkb_assets.IsChecked = false;
            }
        }

        private void lv_auth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UI Handling for selected Authentication Server
            if (lv_auth.SelectedItem != null)
            {
                ServerManager.SelectedAuthserver = (Server)lv_auth.SelectedItem;

                tb_authport.Text = ServerManager.SelectedAuthserver.Port;
                tb_authport.IsEnabled = true;

                checkb_auth.Content = "Use Authentication Server : " + ServerManager.SelectedAuthserver.IP;
                checkb_auth.IsEnabled = true;
            }
            else
            {
                checkb_auth.IsChecked = false;
            }
        }
        private void bt_launch_Click(object sender, RoutedEventArgs e)
        {
            if (lv_accs.SelectedItem == null)
            {
                tab_home.Focus();
                MessageBox.Show("Please select Accounts to launch!");
            }
            else
            {
                EnviromentManager.Show_LBInstanceGUI();
                AccountManager.LaunchAccounts();
                if (addonflag)
                {
                    addonflag = false;
                    AddOnManager.LaunchAll();
                }
            }
        }

        private void bt_installpath_Click(object sender, RoutedEventArgs e)
        {
            //Alternative Path selection (when XML import fails)
            Builders.FileDialog.DefaultExt(".exe")
                .Filter("EXE Files(*.exe)|*.exe")
                .EnforceExt(".exe")
                .ShowDialog((Helpers.FileDialog fileDialog) =>
                {
                    if (fileDialog.FileName != "")
                    {
                        EnviromentManager.GwClientPath = Path.GetDirectoryName(fileDialog.FileName) + @"\";
                        EnviromentManager.GwClientExeName = Path.GetFileName(fileDialog.Fi‌​leName);
                        lab_path.Content = EnviromentManager.GwClientPath + EnviromentManager.GwClientExePath;
                    }
                });
        }

        private void bt_addacc_Click(object sender, RoutedEventArgs e)
        {
            bt_accsave.IsEnabled = true;

            if (lv_accssettings.SelectedItem as Account != null)
            {
                Account acc = new Account("New Account", lv_accssettings.SelectedItem as Account);
            }
            else
            {
                Account acc = new Account("New Account", lv_accssettings.SelectedItem as Account);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LBConfiguration.Save();
            Environment.Exit(Environment.ExitCode);
        }

        private void bt_remacc_Click(object sender, RoutedEventArgs e)
        {
            bt_accsave.IsEnabled = true;
            if (lv_accssettings.SelectedItem != null)
            {
                AccountManager.Remove(lv_accssettings.SelectedItem as Account);
                lv_accssettings.SelectedIndex = -1;
            }
        }
        private void tb_authport_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            try
            {
                ServerManager.SelectedAuthserver.Port = tb_authport.Text;
            } catch
            {
                MessageBox.Show("Invalid Authserver Port");
                tb_authport.Focus();
            }
            
        }

        private void tb_assetsport_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            try
            {
                ServerManager.SelectedAssetserver.Port = tb_assetsport.Text;
            }
            catch
            {
                MessageBox.Show("Invalid Assetserver Port");
                tb_assetsport.Focus();
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lv_auth.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Ascending));
            CollectionView sview = (CollectionView)CollectionViewSource.GetDefaultView(lv_assets.ItemsSource);
            sview.SortDescriptions.Add(new SortDescription("Ping", ListSortDirection.Ascending));
        }

        /*
        private void bt_donate_Click(object sender, RoutedEventArgs e)
        {
            string url = "";

            string business = "thecheatsrichter@gmx.at";  // your PayPal email
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

        private void bt_patreon_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"www.patreon.com/gw2launchbuddy");
        }
        */

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            Mainwin_SaveSetup();
            LBConfiguration.Save();
            EnviromentManager.Close();
            Application.Current.Shutdown();
        }
        private void bt_minimize_Click(object sender, RoutedEventArgs e)
        {
            myWindow.WindowState = WindowState.Normal;
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
                    LBConfiguration.Config.cinema_imagepath = folderdialog.SelectedPath;
                    LBConfiguration.Save();
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

            Builders.FileDialog.DefaultExt(".png")
                .Filter("PNG Files(*.png)|*.png")
                .ShowDialog((Helpers.FileDialog fileDialog) =>
                {
                    if (fileDialog.FileName != "")
                    {
                        LBConfiguration.Config.cinema_maskpath = fileDialog.FileName;
                        LBConfiguration.Save();
                        lab_maskpreview.Content = "Current Mask: " + Path.GetFileName(fileDialog.FileName);
                        img_maskpreview.Source = LoadImage(fileDialog.FileName);
                        ImageBrush newmask = new ImageBrush(LoadImage(fileDialog.FileName));
                        newmask.Stretch = Stretch.Uniform;
                        img_slideshow.OpacityMask = newmask;
                    }
                });
        }

        private void bt_setmusic_Click(object sender, RoutedEventArgs e)
        {
            Builders.FileDialog.Filter("MP3 Files(*.mp3)|*.mp3|WAV Files (*.wav)|*.wav|AAC Files (*.aac)|*.aac|All Files(*.*)|*.*")
                .ShowDialog((Helpers.FileDialog fileDialog) =>
                {
                    if (fileDialog.FileName != "")
                    {
                        LBConfiguration.Config.cinema_musicpath = fileDialog.FileName;
                        LBConfiguration.Save();
                        lab_musicpath.Content = "Current Music File: " + Path.GetFileName(fileDialog.FileName);
                        Cinema_MediaPlayer.Source = (new Uri(fileDialog.FileName));
                    }
                });
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
            string imagepath = LBConfiguration.Config.cinema_imagepath;
            string maskpath = LBConfiguration.Config.cinema_maskpath;
            string musicpath = LBConfiguration.Config.cinema_musicpath;
            string loginpath = LBConfiguration.Config.cinema_loginwindowpath;

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
            if (LBConfiguration.Config.cinema_musicpath != null && LBConfiguration.Config.cinema_musicpath != "")
            {
                Cinema_MediaPlayer.Source = new Uri(LBConfiguration.Config.cinema_musicpath);
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
            cinemamode = !LBConfiguration.Config.cinema_use;
            LBConfiguration.Config.cinema_use = cinemamode;
            LBConfiguration.Save();
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
                LBConfiguration.Config.cinema_video = false;
                LBConfiguration.Config.cinema_slideshow = true;
                LBConfiguration.Save();
            }
            catch { }
        }

        private void rb_videomode(object sender, RoutedEventArgs e)
        {
            try
            {
                Slideshow.Visibility = Visibility.Collapsed;
                Videomode.Visibility = Visibility.Visible;
                LBConfiguration.Config.cinema_video = true;
                LBConfiguration.Config.cinema_slideshow = false;
                LBConfiguration.Save();
            }
            catch { }
        }

        private void bt_cinema_setvideo_Click(object sender, RoutedEventArgs e)
        {
            cinema_videoplayback.Stop();
            Builders.FileDialog.DefaultExt(".mp4")
                .Filter("Mp4 Files(*.mp4)|*.mp4|Raw Files(*.raw)|*.raw|WMV Files(*.wmv)|*.wmv|MPEG Files(*.mpeg)|*.mpeg|All Files(*.*)|*.*")
                .CheckForMedia(Helpers.FileDialog.MediaTypes.Video, cinema_videoplayback)
                .ShowDialog((Helpers.FileDialog fileDialog) =>
                {
                    if (fileDialog.Result == System.Windows.Forms.DialogResult.OK)
                    {
                        cinema_videoplayback.Source = new Uri(fileDialog.FileName, UriKind.Relative);
                        LBConfiguration.Config.cinema_videopath = fileDialog.FileName;
                        SetVideoInfo();
                        cinema_videoplayback.Play();
                    }
                });
        }

        private void SetVideoInfo()
        {
            try
            {
                string videopath = LBConfiguration.Config.cinema_videopath;

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
            AccountManager.SwitchSelectionAll(false);

            foreach (object acc in Cinema_Accountlist.SelectedItems)
            {
                (acc as Account).IsEnabled = true;
            }

            AccountManager.LaunchAccounts();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = SettingsGrid.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        private void Window_LostKeyboardFocus(Object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (cinemamode || rb_cinemavideomode.IsChecked == true) { Cinema_MediaPlayer.Pause(); }
        }

        private void Window_GotKeyboardFocus(Object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (cinemamode || rb_cinemavideomode.IsChecked == true) { Cinema_MediaPlayer.Play(); }
        }

        private void myWindow_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { this.DragMove(); } catch { }
        }

        private void bt_mute_Click(object sender, RoutedEventArgs e)
        {
            if (Cinema_MediaPlayer.IsMuted)
            {
                Cinema_MediaPlayer.IsMuted = false;
                img_mutebutton = new ImageBrush(new BitmapImage(new Uri("/Resources/Icons/speaker_loud.png", UriKind.Relative)));
            }
            else
            {
                Cinema_MediaPlayer.IsMuted = true;
                img_mutebutton = new ImageBrush(new BitmapImage(new Uri("/Resources/Icons/speaker_mute.png", UriKind.Relative)));

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
            LBConfiguration.Config.mediaplayer_volume = sl_volumecontrol.Value;
            LBConfiguration.Save();
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
                LBConfiguration.Config.cinema_backgroundcolor = (Color)ClrPcker_Background.SelectedColor;
                LBConfiguration.Save();
            }
        }

        private void bt_setloginwindow_Click(object sender, RoutedEventArgs e)
        {
            Builders.FileDialog.DefaultExt(".png")
                .Filter("PNG Files(*.png) | *.png")
                .ShowDialog((Helpers.FileDialog fileDialog) =>
                {
                    if (fileDialog.Result == System.Windows.Forms.DialogResult.OK)
                    {
                        cinema_videoplayback.Source = new Uri(fileDialog.FileName, UriKind.Relative);
                        LBConfiguration.Config.cinema_loginwindowpath = fileDialog.FileName;
                        img_loginwindow.Source = LoadImage(fileDialog.FileName);
                        lab_loginwindowpath.Content = "Current Login Window: " + Path.GetFileNameWithoutExtension(fileDialog.FileName);
                        LBConfiguration.Save();
                    }
                });
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
            LBConfiguration.Config.cinema_slideshowendpos = (int)sl_logoendpos.Value;
            LBConfiguration.Save();
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
            LBConfiguration.Config.cinema_slideshowendscale = sl_logoendscaleX.Value;
            LBConfiguration.Save();
        }

        private void bt_loadgfx_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings accsettings = (sender as Button).DataContext as AccountSettings;
            Builders.FileDialog.DefaultExt(".xml")
                .Filter("XML Files(*.xml)|*.xml")
                .InitialDirectory(EnviromentManager.GwClientPath)
                .ShowDialog((Helpers.FileDialog fileDialog) =>
                {
                    if (GFXManager.IsValidGFX(fileDialog.FileName))
                    {
                        var tmp = GFXManager.LoadFile(fileDialog.FileName);
                        if (tmp != null)
                        {
                            accsettings.GFXFile = tmp;
                            lv_gfx.ItemsSource = accsettings.GFXFile.Config;
                        }
                        else
                        {
                            MessageBox.Show("Invalid GFX Config File selected!");
                        }
                    }
                });
        }

        private void bt_resetgfx_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings accsettings = (sender as Button).DataContext as AccountSettings;
            accsettings.GFXFile = GFXManager.ReadFile(EnviromentManager.GwClientXmlPath);
            lv_gfx.Items.Refresh();
        }

        private void bt_savegfx_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings accsettings = (sender as Button).DataContext as AccountSettings;
            GFXManager.SaveFile(accsettings.GFXFile);
        }

        private void bt_accsortup_Click(object sender, RoutedEventArgs e)
        {
            bt_accsave.IsEnabled = true;
            if (lv_accssettings.SelectedItem != null)
            {
                AccountManager.MoveAccount(lv_accssettings.SelectedItem as Account, +1);
            }
        }

        private void bt_accsortdown_Click(object sender, RoutedEventArgs e)
        {
            bt_accsave.IsEnabled = true;
            if (lv_accssettings.SelectedItem != null)
            {
                AccountManager.MoveAccount(lv_accssettings.SelectedItem as Account, -1);
            }
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
                if (rl_version.CompareTo(EnviromentManager.LBVersion) < 0)
                {
                    MessageBoxResult win = MessageBox.Show("Usage of older versions of Launchbuddy can corrupt your Launchbuddy data!\n\nAre you sure you want to download V" + rl_version, "Release Download", MessageBoxButton.YesNo, MessageBoxImage.Warning);
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

        private void bt_bugreport_Click(object sender, RoutedEventArgs e)
        {
            CrashReporter.ReportCrashToAll(new Exception("BugReport"));
        }

        private void bt_fetchlbversions_Click(object sender, RoutedEventArgs e)
        {
            Thread checkforlbupdate = new Thread(checklbversion);
            checkforlbupdate.Start();
        }

        private void cb_lbupdatescheck_Click(object sender, RoutedEventArgs e)
        {
            LBConfiguration.Config.notifylbupdate = (bool)cb_lbupdatescheck.IsChecked;
            LBConfiguration.Save();
        }

        private void cb_useinstancegui_Click(object sender, RoutedEventArgs e)
        {
            LBConfiguration.Config.useinstancegui = (bool)cb_useinstancegui.IsChecked;
            LBConfiguration.Save();
        }

        private void cb_autoupdatedatfiles_Click(object sender, RoutedEventArgs e)
        {
            LBConfiguration.Config.autoupdatedatfiles = (bool)cb_autoupdatedatfiles.IsChecked;
            LBConfiguration.Save();
        }

        private void bt_manualauthserver_Click(object sender, RoutedEventArgs e)
        {
            ServerManager.AddAuthServer(tb_manualauthserver.Text);
        }

        private void bt_manualassetserver_Click(object sender, RoutedEventArgs e)
        {
            ServerManager.AddAssetServer(tb_manualassetserver.Text);
        }

        private void bt_accsave_Click(object sender, RoutedEventArgs e)
        {
            AccountManager.SaveAccounts();
        }

        private void lv_crashlogs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Crashlog crashlog = lv_crashlogs.SelectedItem as Crashlog;
            tblock_crashinfo.Text = crashlog.Quickinfo;
            tblock_crashsolutioninfo.Text = crashlog.Solutioninfo;
            bt_fixcrash.IsEnabled = crashlog.IsSolveable;
        }

        private void bt_fixcrash_Click(object sender, RoutedEventArgs e)
        {
            Crashlog crashlog = lv_crashlogs.SelectedItem as Crashlog;
            crashlog.Solve();
            bt_fixcrash.IsEnabled = false;
        }

        private void bt_AddDll_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings accsettings = (sender as Button).DataContext as AccountSettings;
            Builders.FileDialog.DefaultExt(".dll")
                .Filter("DLL Files(*.dll)|*.dll")
                .ShowDialog((Helpers.FileDialog fileDialog) =>
                {
                    if (fileDialog.FileName != "" && !accsettings.DLLs.Contains(fileDialog.FileName))
                    {
                        accsettings.DLLs.Add(fileDialog.FileName);
                    }
                });
        }

        private void bt_accadd_Click(object sender, RoutedEventArgs e)
        {
            Account newacc = AccountManager.CreateEmptyAccount();
            lv_accssettings.SelectedItem = newacc;
            tb_accnickname.Focus();
            tb_accnickname.SelectAll();
        }

        private void lv_accs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var accs = (sender as ListView).SelectedItems;
            AccountManager.SwitchSelectionAll(false);
            foreach (Account acc in accs)
            {
                acc.IsEnabled = true;
            }
            if(accs.Count>0)
            {
                lb_acccountinfo.Content = $"Accounts ({accs.Count} selected):";
            }else
            {
                lb_acccountinfo.Content = "Accounts:";
            }
        }

        private void lv_accssettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var acc = (sender as ListView).SelectedItem as Account;
            if (acc != null)
            {
                AccountManager.EditAccount = acc;
                gr_acceditor.DataContext = AccountManager.EditAccount;
                gr_acceditor.IsEnabled = true;
                gr_acceditor.Visibility = Visibility.Visible;
                sp_acclistbuttons.IsEnabled = true;
                tb_passw.Clear();
            }
            else
            {
                gr_acceditor.Visibility = Visibility.Collapsed;
                gr_acceditor.IsEnabled = false;
                sp_acclistbuttons.IsEnabled = false;
            }


        }

        private void bt_accrem_Click(object sender, RoutedEventArgs e)
        {
            AccountManager.Remove(lv_accssettings.SelectedItem as Account);
        }

        private void lvitem_argument_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            textblock_descr.Text = ((sender as ListViewItem).DataContext as Argument).Description;
        }

        private void bt_RemDll_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings accsett = (sender as Button).DataContext as AccountSettings;
            accsett.DLLs.Remove(lv_InjectDlls.SelectedItem as string);
        }

        private void lv_accicon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountSettings accsett = (sender as ListView).DataContext as AccountSettings;
            accsett.Icon = (sender as ListView).SelectedItem as Icon;
        }

        private void bt_addicon_Click(object sender, RoutedEventArgs e)
        {
            IconManager.AddIcon();
        }

        private void bt_remicon_Click(object sender, RoutedEventArgs e)
        {
            if (lv_accicon.SelectedItem != null)
                IconManager.RemIcon(lv_accicon.SelectedItem as Icon);
        }

        private void tb_accnickname_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                sv_accsettings.Focus();
            }
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

        
        private void tb_email_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            string email = tb_email.Text;
            if(!email.Contains('*'))
            {
                if (!IsValidEmail(email) && email != "" || email.Contains('*'))
                {
                    MessageBox.Show("Invalid email " + tb_email.Text + ". Leave blank to deactivate.");
                    tb_email.SelectAll();
                    tb_email.Focus();
                }
                else
                {
                    AccountSettings sett = (sender as TextBox).DataContext as AccountSettings;
                    sett.Email = (sender as TextBox).Text;
                }
            }
        }

        private void tb_passw_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (tb_passw.Password.Length >= 8 || tb_passw.Password == "" || tb_passw.Password == null)
            {
                AccountSettings sett = (sender as PasswordBox).DataContext as AccountSettings;
                sett.Password = (sender as PasswordBox).Password;
            }
            else
            {
                MessageBox.Show("Invalid Password! Leave blank to deactivate.");
                tb_passw.SelectAll();
                tb_passw.Focus();
            }
        }
        

        private void checkb_clientport_Checked(object sender, RoutedEventArgs e)
        {
            if (ServerManager.IsPort(tb_clientport.Text))
            {
                ServerManager.clientport = tb_clientport.Text;
            }
            else
            {
                MessageBox.Show("Invalid clientport");
                tb_clientport.Focus();
            }
        }

        private void checkb_assets_Checked(object sender, RoutedEventArgs e)
        {
            Server assetserv = new Server();
            assetserv = lv_assets.SelectedItem as Server;

            assetserv.Port = tb_assetsport.Text;

            if (ServerManager.IsPort(tb_assetsport.Text))
            {
                assetserv.Port = tb_assetsport.Text;
                ServerManager.SelectedAssetserver.Enabled = true;
            }
            else
            {
                MessageBox.Show("Invalid assetserver port");
                tb_assetsport.Focus();
            }
        }

        private void checkb_auth_Checked(object sender, RoutedEventArgs e)
        {
            Server authserv = new Server();
            authserv = lv_auth.SelectedItem as Server;

            authserv.Port = tb_authport.Text;

            if (ServerManager.IsPort(tb_authport.Text))
            {
                authserv.Port = tb_assetsport.Text;
                ServerManager.SelectedAuthserver.Enabled = true;
            }
            else
            {
                MessageBox.Show("Invalid authentication server port");
                tb_authport.Focus();
            }
        }

        private void checkb_assets_Unchecked(object sender, RoutedEventArgs e)
        {
            ServerManager.SelectedAssetserver.Enabled = false;
        }

        private void checkb_auth_Unchecked(object sender, RoutedEventArgs e)
        {

            ServerManager.SelectedAuthserver.Enabled = false;
        }

        private void bt_noicon_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings accsett = (sender as Button).DataContext as AccountSettings;
            accsett.Icon = null;
        }

        private void bt_accclone_Click(object sender, RoutedEventArgs e)
        {
            if (AccountManager.EditAccount != null)
            {
                AccountManager.Clone(AccountManager.EditAccount);
            }
        }

        private void bt_accload_Click(object sender, RoutedEventArgs e)
        {
            AccountManager.ImportAccounts();
            lv_accssettings.ItemsSource = AccountManager.Accounts;
        }

        private void tab_server_Initialized(object sender, EventArgs e)
        {
            bt_checkservers.Content = "Loading Server List";
            bt_checkservers.IsEnabled = false;

            Thread serverthread = new Thread(UpdateServerList);
            serverthread.IsBackground = true;
            serverthread.Start();
        }

        private void tab_home_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lv_accs.ItemsSource = null;
            lv_accs.ItemsSource = AccountManager.Accounts;
        }

        private void bt_hotkeyrem_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as AccountSettings).RemoveHotkey(lv_hotkeys.SelectedItem as AccountHotkey);
        }

        private void bt_hotkeyadd_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as AccountSettings).AddHotkey();
        }

        private void tb_hotkey_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Hotkey hotkey = ((sender as TextBox).DataContext as Hotkey);
            hotkey.Modifiers = Keyboard.Modifiers;
            hotkey.KeyValue = e.Key;
            (sender as TextBox).Text = hotkey.KeyAsString;
        }

        private void tb_hotkey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            (sender as TextBox).Text = ((sender as TextBox).DataContext as Hotkey).KeyAsString;
        }

        private void lv_hotkeys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object test=((sender as ListView).SelectedItem)as Hotkey;
        }

        private void tb_relaunches_LostFocus(object sender, RoutedEventArgs e)
        {
            textinput_onlyint(sender as TextBox);
            ((sender as TextBox).DataContext as AccountSettings).SetRelaunched(UInt32.Parse((sender as TextBox).Text));
        }

        private void textinput_onlyint(TextBox tb)
        {
            if (!Regex.IsMatch(tb.Text, @"[0-9]+"))
            {
                tb.Text = "0";
            }
        }

        private void cb_useinstancegui_Checked(object sender, RoutedEventArgs e)
        {
            LBConfiguration.Config.useinstancegui = (bool)(sender as CheckBox).IsChecked;
        }

        private void cb_autoupdatedatfiles_Checked(object sender, RoutedEventArgs e)
        {
            LBConfiguration.Config.autoupdatedatfiles = (bool)(sender as CheckBox).IsChecked;
        }


        #region DELETE ON 1.9

        public bool addonflag = true;
        private void bt_AddAddon_Click(object sender, RoutedEventArgs e)
        {
            AddOnManager.Add(tb_AddonName.Text,tb_AddonArgs.Text.Split('-'), false,(bool)cb_AddonOnLB.IsChecked);
        }

        private void bt_RemAddon_Click(object sender, RoutedEventArgs e)
        {
            AddOnManager.Remove((lv_AddOns.SelectedItem as AddOn).Name);
        }

        private void tab_addons_GotFocus(object sender, RoutedEventArgs e)
        {
            lv_AddOns.ItemsSource = AddOnManager.addOnCollection;
        }
        #endregion DELETE ON 1.9



        private void bt_loginfile_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings settings = ((sender as Button).DataContext as AccountSettings);
            settings.SetLoginFile();
        }

        private void bt_loginfileupdate_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings settings = ((sender as Button).DataContext as AccountSettings);
            settings.Loginfile.gw2build = "0";
            try
            {
                LocalDatManager.UpdateLocalDat(settings.Loginfile);
            }
            catch (Exception err)
            {
                MessageBox.Show("Could not repair Loginfile. Please recreate a loginfile with Set Loginfile. \n"+err.Message);
            }
        }

        private void bt_recerateloginfile_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings settings = ((sender as Button).DataContext as AccountSettings);
            settings.RecreateLoginFile();
        }

        private void bt_windowinfo_setup_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings settings = ((sender as Button).DataContext as AccountSettings);
            settings.CreateWinConfig();
        }

        private void bt_windowinfo_delete_Click(object sender, RoutedEventArgs e)
        {
            AccountSettings settings = ((sender as Button).DataContext as AccountSettings);
            settings.WinConfig = null;
        }

        private void sl_logoendpos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var endpos = (System.Windows.Media.Animation.EasingDoubleKeyFrame)Resources["Mask_EndPos"];
            endpos.Value = sl_logoendpos.Value * (reso_x / 200);
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)

        {

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;

        }

        private void bt_patreon_Click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("www.patreon.com/gw2launchbuddy");
        }

        private void bt_donate_Click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WKHYFSBMK6TQE&source=url");
        }

        #region Plugins

        public void AddTabPlugin(TabItem UIContent)
        {
            if(UIContent !=null)
            {
                tab_options.Items.Add(UIContent);
            }
        }

        public void RemTabPlugin(PluginContracts.ILBPlugin plugin)
        {
            if (plugin.UIContent != null)
            {
                tab_options.Items.Remove(plugin.UIContent);
            }
        }

        public void AddHomePlugin(StackPanel UIContent)
        {
            if (UIContent != null)
            {
                
            }
        }

        private void AddPlugin()
        {
            PluginManager.AddPluginWithDialog();
        }

        private void bt_addplugin_Click(object sender, RoutedEventArgs e)
        {
            AddPlugin();
        }

        private void bt_remplugin_Click(object sender, RoutedEventArgs e)
        {
            PluginManager.RemovePlugin(((sender as Button).DataContext as Plugin_Wrapper).Plugin);
        }

        private void cb_autoupdateplugins_Click(object sender, RoutedEventArgs e)
        {
            LBConfiguration.Save();
        }

        private void Bt_resetpropertysettings_Click(object sender, RoutedEventArgs e)
        {
            EnviromentManager.ResetPropertySettings();
        }

        private void Bt_resetinstanceguisettings_Click(object sender, RoutedEventArgs e)
        {
            if(EnviromentManager.LBInstanceGUI !=null)
            {
                EnviromentManager.LBInstanceGUI.ResetWindowSettings();
            }
        }

        private void bt_settacopath_Click(object sender, RoutedEventArgs e)
        {
            LBTacO.SetPath();
            tb_tacopath.Text = LBTacO.TacoPath;
        }

        private void bt_launchtaco_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var acc = (sender as Image).DataContext as Account;
            LBTacO.LaunchTacoInstance(acc);
        }

        private void bt_updateplugin_Click(object sender, RoutedEventArgs e)
        {
            PluginManager.UpdatePlugin(((sender as Button).DataContext as Plugin_Wrapper).Plugin,true);
           
        }


        #endregion Plugins
    }
}
