using System;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Windows.Media.Animation;
using log4net;

namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for Clientfix.xaml
    /// </summary>
    public partial class Clientfix : Window
    {
        private static ILog Log { get; } = LogManager.GetLogger(typeof(Clientfix));

        public string exepath { get; set; }
        public string exename { get; set; }
        bool glasses = false;

        public Clientfix()
        {
            InitializeComponent();
        }

        void verifygame()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = exepath + exename;
            startInfo.Arguments = " -verify";

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception err) // logged
            {
                Log.Error($"Unable to verify game. (FileName:{startInfo.FileName}|Arguments:{startInfo.Arguments})", err);
                MessageBox.Show($"We couldn't verify the game.\nMore technical info:\n{err}");
            }
        }


        private void bt_repair_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            verifygame();
            setbusy(false);
        }

        private void bt_patch_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            patchgame();
            setbusy(false);
        }

        void waitforlauncher()
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


        void patchgame()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = exepath + exename;
            startInfo.Arguments = " -image";
            Process gw2pro = new Process { StartInfo = startInfo };

            try
            {
                gw2pro.Start();
                gw2pro.WaitForExit();
            }
            catch (Exception err) // logged
            {
                Log.Error($"Unable to patch the game.", err);
                MessageBox.Show($"Something went wrong trying to patch the game.\nMore technical info:\n{err}");
            }

        }

        private void bt_appdata_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            cleanappdata();
            setbusy(false);
        }

        void cleanappdata()
        {
            MessageBoxResult win = MessageBox.Show("When quaggan cleans the Appdata some gamesettings will get deleted!\nDeleting these files can however increase the overall fps of the game in some cases!\n\nClean Appdata?", "Clean Appdata Info", MessageBoxButton.YesNo, MessageBoxImage.Question);

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

                    MessageBox.Show("Quaggan cleaned " + datfiles.Length + " file(s) from Appdata");
                }
            }
            catch (Exception err) // logged
            {
                Log.Error("Unable to clean app data", err);
                MessageBox.Show($"Quaggan sad, quaggan could not clean folder.\nTechnical quaggan speech:{err}");
            }

        }

        void deletebinfolder()
        {
            MessageBoxResult win = MessageBox.Show("When quaggan updates the bin folder 3rd party shaders like Reshade,GemFX and SweetFX will get removed!\n\nThis can solve problems with non functioning 3rd party shaders and launcher crashes.\n\nUpdate bin folder?", "Update Bin folder Info", MessageBoxButton.YesNo, MessageBoxImage.Question);

            try
            {
                if (win.ToString() == "Yes")
                {
                    if (Directory.Exists(exepath + "bin"))
                    {
                        Directory.Delete(exepath + "bin", true);
                    }
                    if (Directory.Exists(exepath + "bin64"))
                    {
                        Directory.Delete(exepath + "bin64", true);
                    }
                }
            }
            catch (Exception err) // logged
            {
                Log.Error("Unable to update bin folder", err);
                MessageBox.Show($"Quaggan couldn't update the bin folder! Foo!!!\nTechnical quaggan speech:{err}");
            }


        }

        private void bt_auto_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            deletebinfolder();
            cleanappdata();

            MessageBoxResult win = MessageBox.Show("Should quaggan now search for errors in the Gw2 file? (can take up to 30 mins!)", "Update Bin folder Info", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (win.ToString() == "Yes")
            {
                verifygame();
            }
            patchgame();
            setbusy(false);

        }

        private void bt_bin_Click(object sender, RoutedEventArgs e)
        {
            setbusy(true);
            deletebinfolder();
            setbusy(false);
        }

        void setbusy(bool busy)
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

        void getglasses()
        {
            if (!glasses) BeginStoryboard(this.FindResource("anim_quaggan") as Storyboard);
            glasses = true;
        }

        private void bt_resetup_Click(object sender, RoutedEventArgs e)
        {
            string AppdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Gw2 Launchbuddy\\";
            File.Delete(AppdataPath + "handle.exe");
            File.Delete(AppdataPath + "handle64.exe");
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }
    }
}
