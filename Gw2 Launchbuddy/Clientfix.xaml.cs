using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;

namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for ClientFix.xaml
    /// </summary>
    public partial class ClientFix : Window
    {
        private static Client fixClient = new Client();
        private bool glasses = false;

        public ClientFix()
        {
            InitializeComponent();
        }

        private void VerifyGame()
        {
            fixClient.StartInfo.Arguments = " -verify";

            try
            {
                fixClient.Start();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void bt_repair_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            VerifyGame();
            setbusy(false);
        }

        private void bt_patch_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            PatchGame();
            setbusy(false);
        }

        private void waitforlauncher()
        {
            setbusy(true);
            Process[] processlist = Process.GetProcesses();
            Process Gw2Process = null;
            int i = 0;
            bool islaunched = false;
            int id = 0;
            while (Gw2Process == null && ++i <= 10)
            {
                foreach (Process theprocess in processlist)
                {
                    if (theprocess.ProcessName.Contains("Gw2") && theprocess.ProcessName.Contains("Gw2 Launchbuddy") == false)
                    {
                        Gw2Process = theprocess;
                        id = Gw2Process.Id;
                        islaunched = true;
                    }
                }
                Thread.Sleep(1000);
            }

            if (!islaunched) MessageBox.Show("Gw2.exe did not launch!");

            processlist = Process.GetProcesses();
            while (islaunched && id != 0)
            {
                if (!Process.GetProcesses().Any(x => x.Id == id))
                {
                    islaunched = false;
                    setbusy(false);
                }
                Thread.Sleep(1000);
            }
        }

        private void PatchGame()
        {
            fixClient.Arguments = " -image";

            try
            {
                fixClient.StartAndWait();

                //Not needed waiting time
                /*
                Thread patcher = new Thread(waitforlauncher);
                patcher.Start();
                */
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void bt_appdata_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            cleanappdata();
            setbusy(false);
        }

        private void cleanappdata()
        {
            MessageBoxResult win = MessageBox.Show("When quaggan cleans the AppData some game settings will get deleted!\nDeleting these files can however increase the overall fps of the game in some cases!\n\nClean AppData?", "Clean AppData Info", MessageBoxButton.YesNo, MessageBoxImage.Question);

            try
            {
                if (win.ToString() == "Yes")
                {
                    var appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\";
                    string[] datfiles = Directory.GetFiles(appdatapath.ToString(), "*.dat");

                    foreach (string file in datfiles)
                    {
                        File.Delete(file);
                    }

                    MessageBox.Show("Quaggan cleaned " + datfiles.Length + " file(s) from AppData");
                }
            }
            catch (Exception err) { MessageBox.Show(err.Message); }
        }

        private void DeleteBinFolder()
        {
            MessageBoxResult win = MessageBox.Show("When quaggan updates the bin folder 3rd party shaders like ReShade, GemFX and SweetFX will get removed!\n\nThis can solve problems with non functioning 3rd party shaders and launcher crashes.\n\nUpdate bin folder?", "Update Bin Folder Info", MessageBoxButton.YesNo, MessageBoxImage.Question);

            try
            {
                if (win.ToString() == "Yes")
                {
                    Directory.Delete(ClientManager.ClientInfo.InstallPath + "\\bin", true);
                    Directory.Delete(ClientManager.ClientInfo.InstallPath + "\\bin64", true);
                }
            }
            catch (Exception err) { MessageBox.Show(err.Message); }
        }

        private void bt_auto_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            DeleteBinFolder();
            cleanappdata();

            MessageBoxResult win = MessageBox.Show("Should quaggan now search for errors in the Gw2 file? (can take up to 30 mins!)", "Update Bin folder Info", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (win.ToString() == "Yes")
            {
                VerifyGame();
            }
            PatchGame();
            setbusy(false);
        }

        private void bt_bin_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            DeleteBinFolder();
            setbusy(false);
        }

        private void setbusy(bool busy)
        {
            bt_appdata.IsEnabled = !busy;
            bt_auto.IsEnabled = !busy;
            bt_bin.IsEnabled = !busy;
            bt_patch.IsEnabled = !busy;
            bt_repair.IsEnabled = !busy;

            if (busy) tbl_quaggan.Text = "Quaggan is busy please wait";
            if (!busy)
            {
                tbl_quaggan.Text = "What else can quaggan do for youuu?";
                getglasses();
            }
        }

        private void getglasses()
        {
            if (!glasses) BeginStoryboard(this.FindResource("anim_quaggan") as Storyboard);
            glasses = true;
        }

        private void bt_resetup_Click(object sender, RoutedEventArgs e)
        {
            string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Gw2 Launchbuddy\\";
            File.Delete(AppDataPath + "handle.exe");
            File.Delete(AppDataPath + "handle64.exe");
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }
    }
}