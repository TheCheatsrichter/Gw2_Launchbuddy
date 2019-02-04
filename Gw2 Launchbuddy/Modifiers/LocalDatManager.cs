using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;


namespace Gw2_Launchbuddy.Modifiers
{
    public static class LocalDatManager
    {
        private static ObservableCollection<LocalDatFile> datacollection = new ObservableCollection<LocalDatFile>();
        public static ObservableCollection<LocalDatFile> DataCollection { get { return datacollection; } }
        private static LocalDatFile CurrentFile = null;

        public static void Add(LocalDatFile newfile)
        {
            datacollection.Add(newfile);
        }

        public static void Remove(LocalDatFile file)
        {
            if (datacollection.Any<LocalDatFile>(d => d.Path == file.Path))
            {
                datacollection.Remove(file);
                if (File.Exists(file.Path)) File.Delete(file.Path);
            }
            else
            {
                MessageBox.Show("Local dat not registered");
            }
        }

        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        private enum SymbolicLink
        {
            File = 0x0,
            Directory = 0x1
        }

        private static void CreateSymbolLink(string sourcefile)
        {
            if (File.Exists(sourcefile) && !File.Exists(EnviromentManager.GwLocaldatPath))
            {
                if (!CreateSymbolicLink( EnviromentManager.GwLocaldatPath, sourcefile, 0x0))
                {
                    throw new Exception("Could not create Symbolic link. Not running as Admin?");
                }
            }
            else
            {
                throw new Exception("Provided Local.dat file does not exist or Linkfile is allready created.");
            }
        }

        private static bool IsSymbolic(string path)
        {
            FileInfo pathInfo = new FileInfo(path);
            return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        public static void CleanUp()
        {
            ObservableCollection<LocalDatFile> tmp_datfiles = new ObservableCollection<LocalDatFile>();
            foreach(Account acc in AccountManager.Accounts)
            {
                tmp_datfiles.Add(acc.Settings.Loginfile);
            }
            try
            {
                foreach (string file in Directory.GetFiles(EnviromentManager.LBLocaldatsPath))
                {
                    if (!tmp_datfiles.Any<LocalDatFile>(f => f.Name == Path.GetFileNameWithoutExtension(file)))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch { }
        }

        public static void Apply(LocalDatFile file)
        {
            try
            {
                Repair();

                //Is Valid?
                if (!file.Valid) MessageBox.Show("Invalid Login file " + file.Name + " please recreate this file in the account Manager.");

                //Check Version???
                CheckVersion(file);

                //Create Backup of Local dat
                if (!IsSymbolic(EnviromentManager.GwLocaldatPath))
                {
                    if (File.Exists(EnviromentManager.GwLocaldatBakPath)) File.Delete(EnviromentManager.GwLocaldatBakPath);
                    File.Copy(EnviromentManager.GwLocaldatPath, EnviromentManager.GwLocaldatBakPath);
                }
                //Delete Local.dat
                if (File.Exists(EnviromentManager.GwLocaldatPath)) File.Delete(EnviromentManager.GwLocaldatPath);
                //Create Symlink Replacer
                CreateSymbolLink(file.Path);
                //Remember last used file for ToDefault()
                CurrentFile = file;
            }

            catch (Exception e)
            {
                MessageBox.Show("An error occured while swaping the Login file." + e.Message);
                Repair();
            }

        }

        public static void ToDefault()
        {
            if (File.Exists(EnviromentManager.GwLocaldatPath)) File.Delete(EnviromentManager.GwLocaldatPath);
            if (File.Exists(EnviromentManager.GwLocaldatBakPath))
            {
                File.Copy(EnviromentManager.GwLocaldatBakPath, EnviromentManager.GwLocaldatPath);
                File.Delete(EnviromentManager.GwLocaldatBakPath);
            }
            else
            {
                File.Copy(CurrentFile.Path, EnviromentManager.GwLocaldatPath);
            }
        }

        private static void Repair()
        {
            if (!File.Exists(EnviromentManager.GwLocaldatPath) && File.Exists(EnviromentManager.GwLocaldatBakPath))
            {
                File.Move(EnviromentManager.GwLocaldatBakPath, EnviromentManager.GwLocaldatPath);
            }
            if (File.Exists(EnviromentManager.GwLocaldatBakPath)) File.Delete(EnviromentManager.GwLocaldatBakPath);
        }

        private static void CheckVersion(LocalDatFile file)
        {
            //if (!file.IsUpToDate) MessageBox.Show("The used account login file seems outdated. Outdated files may cause login problems. Please recreate the file in the account settings");
        }


        public static LocalDatFile CreateNewFile(string filename)
        {
            LocalDatFile datfile = new LocalDatFile();

            string filepath = EnviromentManager.LBLocaldatsPath + filename + ".dat";
            datfile.gw2build = EnviromentManager.GwClientVersion;
            datfile.Path = filepath;

            Process pro = new Process { StartInfo = new ProcessStartInfo(EnviromentManager.GwClientExePath) };
            pro.Start();
            Action blockefunc = () => ModuleReader.WaitForModule("icm32.dll", pro,null);
            Helpers.BlockerInfo.Run("Loginfile Creation","Please check remember email/password and login into the game.", blockefunc);
            if (!Helpers.BlockerInfo.Done) MessageBox.Show("No Clean Login. Loginfile might be not set correctly! Proceed with cation.");

            int ct = 0;
            bool exists = true;
            while (exists && ct < 100)
            {
                try
                {
                    pro.Kill();
                    Process.GetProcessById(pro.Id);
                    exists = true;
                    Thread.Sleep(100);
                    ct++;
                }
                catch
                {
                    exists = false;
                }
            }
            try
            {
#if DEBUG
                Console.WriteLine("Login data Hash: " + filename + ": " + datfile.MD5HASH);
#endif
                /*               if (LocalDatManager.DataCollection.Any(f=>f.MD5HASH==CalculateMD5(EnviromentManager.GwLocaldatPath) && f.Name!=Name))
                               {
                                   throw new Exception("Same logindata allready used by another account!");
                               }
                               */
                if (File.Exists(filepath)) File.Delete(filepath);
                File.Copy(EnviromentManager.GwLocaldatPath, filepath);
                datfile.Valid = true;
                return datfile;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: The Gameclient did not create a valid Login data file. " + e.Message);
                datfile.Valid = false;
                return datfile;
            }
        }
    }

    public class LocalDatFile
    {
        //EnviromentManager.GwClientExePath

        public string Path { set; get; }
        public string gw2build { set; get; }
        public string Name { get { return System.IO.Path.GetFileNameWithoutExtension(Path); } }
        public string Gw2Build { get { return gw2build; } }
        public bool IsUpToDate { get { return Gw2Build == EnviromentManager.GwClientVersion; } }
        public bool Valid = false;
        public string MD5HASH {  get { return CalculateMD5(Path); } }

        ~LocalDatFile()
        {
            try
            {
                LocalDatManager.DataCollection.Remove(this);
            }catch
            {
#if DEBUG
                Console.WriteLine("Tried to remove non registered Local.dat File");
#endif
            }
            
        }

        private string CalculateMD5(string filename)
        {
            if (File.Exists(filename))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            return null;
        }

        public LocalDatFile() { LocalDatManager.Add(this); }
    }
}
