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
using System.Xml.Serialization;

namespace Gw2_Launchbuddy.Modifiers
{
    public static class LocalDatManager
    {
        private static ObservableCollection<LocalDatFile> datacollection = new ObservableCollection<LocalDatFile>();
        public static ObservableCollection<LocalDatFile> DataCollection { get { return datacollection; } }
        private static LocalDatFile CurrentFile = null;

        public static void Add(LocalDatFile newfile)
        {
            if (!datacollection.Any<LocalDatFile>(d => d.Path == newfile.Path))
            {
                datacollection.Add(newfile);
            }
            else
            {
                MessageBox.Show("Local dat allready in usage by another account");
            }
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
                if (!CreateSymbolicLink(sourcefile, EnviromentManager.GwLocaldatPath, 0x0))
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

        public static void Apply(LocalDatFile file)
        {
            try
            {
                //Is Valid?
                if (!file.Valid) throw new Exception("Invalid Login file " + file.Name + " please recreate this file in the account Manager.");
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
            if (!file.IsUpToDate) MessageBox.Show("The used account login file seems outdated. Outdated files may cause login problems. Please recreate the file in the account settings");
        }
    }

    [Serializable]
    public class LocalDatFile
    {

        //EnviromentManager.GwClientExePath

        private string path;
        private string gw2build;

        public string Path { get { return path; } }
        public string Name { get { return System.IO.Path.GetFileNameWithoutExtension(path); } }
        public string Gw2Build { get { return gw2build; } }
        [XmlIgnore]
        public bool IsUpToDate { get { return Gw2Build == EnviromentManager.GwClientVersion; } }
        public bool Valid = false;

        public LocalDatFile(string filename)
        {
            gw2build = EnviromentManager.GwClientVersion;
            if (InitFile(filename)) LocalDatManager.Add(this);
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

        private bool InitFile(string filename)
        {
            string oldhash = CalculateMD5(EnviromentManager.GwLocaldatPath);
            Process pro = new Process { StartInfo = new ProcessStartInfo(EnviromentManager.GwClientExePath) };
            pro.Start();
            MessageBox.Show("Please check the remember email and password checkbox and login manually. Then press THIS button to continue!");
            try { pro.Kill(); } catch { }

            int ct = 0;
            bool exists = true;
            while(exists && ct < 25)
            {
                try
                {
                    Process.GetProcessById(pro.Id);
                    exists = true;
                    Thread.Sleep(500);
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
                Console.WriteLine("Login data Hashmatch: "+filename+": "+oldhash == CalculateMD5(EnviromentManager.GwLocaldatPath));
#endif
                if (oldhash == CalculateMD5(EnviromentManager.GwLocaldatPath) && LocalDatManager.DataCollection.Count > 0)
                {
                    throw new Exception("User did not press login before closing Gw2.");
                }
                string filepath = EnviromentManager.LBLocaldatsPath + filename + ".dat";
                if (File.Exists(filepath)) File.Delete(filepath);
                File.Copy(EnviromentManager.GwLocaldatPath, filepath);
                gw2build = EnviromentManager.GwClientVersion;
                path = filepath;
                Valid = true;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: The Gameclient did not create a valid Login data file. " + e.Message);
                Valid = false;
                return false;
            }
        }

        private LocalDatFile() { LocalDatManager.Add(this); }
    }
}
