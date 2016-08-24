using System;
using System.Windows;
using System.IO;
using System.Diagnostics;


namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for Clientfix.xaml
    /// </summary>
    public partial class Clientfix : Window
    {
        public string exepath { get; set; }
        public string exename { get; set; }


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
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }


        private void bt_repair_Click(object sender, RoutedEventArgs e)
        {
            verifygame();
        }

        private void bt_patch_Click(object sender, RoutedEventArgs e)
        {
            patchgame();
        }

        void patchgame()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = exepath + exename;
            startInfo.Arguments = " -image";

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        private void bt_appdata_Click(object sender, RoutedEventArgs e)
        {
            cleanappdata();
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
            catch (Exception err) { MessageBox.Show(err.Message); }

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
            catch (Exception err) { MessageBox.Show(err.Message); }
            

        }

        private void bt_auto_Click(object sender, RoutedEventArgs e)
        {
            deletebinfolder();
            cleanappdata();

            MessageBoxResult win = MessageBox.Show("Should quaggan now search for errors in the Gw2 file? (can take up to 30 mins!)", "Update Bin folder Info", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (win.ToString() == "Yes")
            {
                verifygame();
            }
            patchgame();

        }

        private void bt_bin_Click(object sender, RoutedEventArgs e)
        {
            deletebinfolder();
        }
    }
}
