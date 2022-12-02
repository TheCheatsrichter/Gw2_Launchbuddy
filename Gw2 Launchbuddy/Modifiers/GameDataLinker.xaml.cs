using Gw2_Launchbuddy.ObjectManagers;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace Gw2_Launchbuddy.Modifiers
{
    /// <summary>
    /// Interaktionslogik für GameDataLinker.xaml
    /// </summary>
    public partial class GameDataLinker : Window
    {
        private static string sourcePath = EnviromentManager.GwClientPath;
        private static string targetPath ="";

        public GameDataLinker()
        {
            InitializeComponent();
            tb_source.Text = sourcePath;
            tb_target.Text = targetPath;
        }

        private string SetFolder()
        {
            string path = FileUtil.FolderDialog();

            if(path!=null)
            {
                var files = Directory.GetFiles(path);
                if (!files.Any(a => Path.GetFileName(a) == "Gw2.dat"))
                {
                    System.Windows.MessageBox.Show($"Could not find Guild Wars 2 install in folder: \n{path}\nPlease  make sure that the folderpath is set correctly to the Guild Wars 2 gamefolder");
                    return null;
                }
            }
            return path;
        }

        private void bt_sourcefolderset_Click(object sender, RoutedEventArgs e)
        {
            sourcePath = SetFolder();
            tb_source.Text = sourcePath;
        }

        private void bt_targetfolderset_Click(object sender, RoutedEventArgs e)
        {
            targetPath = SetFolder();
            tb_target.Text = targetPath;
        }

        public bool ValidateGameFolder(string folderpath)
        {
            if (!Directory.Exists(folderpath))
            {
                MessageBox.Show($"Gamefolder does not exists. Please make sure that your gamefolder is set correctly.\n{folderpath}");
                return false;
            }
            if(FileUtil.IsSymbolic(folderpath))
            {
                MessageBox.Show($"{folderpath}\nIs allready linked to an existing game installation. To resolve this please manually delete the link files.");
                return false;
            }
            return true;
        }

        private void bt_createlink_Click(object sender, RoutedEventArgs e)
        {
            if(ValidateGameFolder(sourcePath) && ValidateGameFolder(targetPath))
            {
                if (sourcePath == targetPath)
                {
                    MessageBox.Show("Source folder cannot be the same as target folder!");
                    return;
                }

                Directory.Move(targetPath, targetPath + "_backup");
                FileUtil.CreateSymbolicLinkExtended(targetPath,sourcePath,FileUtil.SymbolicLink.Directory);
                MessageBox.Show("Gamefolders successfully linked. The source will now automatically synch its game data with the target");

                DirectoryInfo dirInfo = new DirectoryInfo(targetPath + "_backup");
                long dirSize = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                var result= MessageBox.Show($"Would you like to delete the gamedata of {targetPath + "_backup"}. This will free up:{dirSize/1000/1000}MB of space. This game data is no longer needed, could however be used as a backup.","Delete old data?",MessageBoxButton.YesNo);
                if(result== MessageBoxResult.Yes)
                {
                    Directory.Delete(targetPath + "_backup",true);
                }
            }
        }

        private void tb_source_TextChanged(object sender, TextChangedEventArgs e)
        {
            lb_sourcedisplay.Content = sourcePath;
        }

        private void tb_target_TextChanged(object sender, TextChangedEventArgs e)
        {
            lb_targetdisplay.Content = targetPath;
        }
    }
}
