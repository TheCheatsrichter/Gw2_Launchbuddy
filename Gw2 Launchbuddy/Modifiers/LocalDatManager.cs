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
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        public enum SymbolicLink
        {
            File = 0x0,
            Directory = 0x1,
            Unprivileged = 0x2
        }

        private static void CreateSymbolLink(string sourcefile)
        {
            if (File.Exists(sourcefile) && !File.Exists(EnviromentManager.GwLocaldatPath))
            {
                if (!CreateSymbolicLink( EnviromentManager.GwLocaldatPath, sourcefile, SymbolicLink.Unprivileged))
                {
                    System.Diagnostics.Process.Start("https://docs.microsoft.com/en-us/windows/uwp/get-started/enable-your-device-for-development");
                    throw new Exception("Could not create Symbolic link. Please activate Windows Developer Options or run Launchbuddy as Admin!");
                }
            }
            else
            {
                throw new Exception("Provided Local.dat file does not exist or Linkfile is already created.");
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

                WaitForFileAccess();

                //Create Backup of Local dat
                if (!IsSymbolic(EnviromentManager.GwLocaldatPath))
                {
                    if (File.Exists(EnviromentManager.GwLocaldatBakPath)) File.Delete(EnviromentManager.GwLocaldatBakPath);
                    File.Copy(EnviromentManager.GwLocaldatPath, EnviromentManager.GwLocaldatBakPath);
                    WaitForFileAccess();
                }
                //Delete Local.dat
                if (File.Exists(EnviromentManager.GwLocaldatPath)) File.Delete(EnviromentManager.GwLocaldatPath);
                //Create Symlink Replacer
                WaitForFileAccess();
                CreateSymbolLink(file.Path);
                //Remember last used file for ToDefault()
                CurrentFile = file;
            }

            catch (Exception e)
            {
                Repair();
                throw new Exception("An error occured while swaping the Login file." + e.Message);
            }

        }

        private static void WaitForFileAccess()
        {
            int i = 0;
            while(i<=100)
            {
                try
                {
                    if (!File.Exists(EnviromentManager.GwLocaldatPath)) return;

                    using (Stream stream = new FileStream(EnviromentManager.GwLocaldatPath, FileMode.Open))
                    {
                        // File/Stream manipulating code here
                        break;
                    }
                }
                catch
                {
                    //check here why it failed and ask user to retry if the file is in use.
                }
                i++;
                Thread.Sleep(100);
            }

            if (i == 100) throw new Exception("Could not acces Localdat file. Make sure no other Gw2 instance is running.");
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

        public static void UpdateLocalDat(LocalDatFile file)
        {
            try
            {
                if (!file.IsUpToDate)
                {
                    Apply(file);
                    Process pro = new Process { StartInfo = new ProcessStartInfo { FileName = EnviromentManager.GwClientExePath } };
                    pro.Start();
                    pro.WaitForInputIdle();
                    Action waitforlaunch = () => ModuleReader.WaitForModule("WINNSI.DLL", pro);
                    Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is updating an outdated Loginfile", waitforlaunch);
                    pro.Kill();
                    Action waitforlock = () => WaitForLoginfileRelease(file);
                    Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to save the updated loginfile.", waitforlock);
                    WaitForFileAccess();
                    ToDefault();
                    file.gw2build = Api.ClientBuild;
                }
            }
            catch(Exception e)
            {
                throw new Exception("An error occured when Updating the Login file. " +e.Message);
            }
        }

        private static bool LoginFileIsLocked(LocalDatFile file)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(file.Path,FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        private static void WaitForLoginfileRelease(LocalDatFile file)
        {
            int i = 0;
            while (LoginFileIsLocked(file)&& i < 5)
            {
                i++;
                Thread.Sleep(1000);
            }
        }

        public static LocalDatFile CreateNewFileAutomated(string filename,string email,string passwd)
        {
            LocalDatFile datfile = new LocalDatFile();

            string filepath = EnviromentManager.LBLocaldatsPath + filename + ".dat";
            datfile.gw2build = Api.ClientBuild;
            datfile.Path = filepath;

            Process pro = new Process { StartInfo = new ProcessStartInfo(EnviromentManager.GwClientExePath) };
            pro.Start();
            Action blockefunc = () => ModuleReader.WaitForModule("WINNSI.DLL", pro, null);
            Helpers.BlockerInfo.Run("Loginfile Creation", "LB is recreating your loginfile", blockefunc);
            if (!Helpers.BlockerInfo.Done) MessageBox.Show("No Clean Login. Loginfile might be not set correctly! Proceed with caution.");
            Thread.Sleep(100);
            Loginfiller.Login(email,passwd,pro,true);
            Thread.Sleep(250);

            blockefunc = () => ModuleReader.WaitForModule("DPAPI.dll", pro, null);
            Helpers.BlockerInfo.Run("Loginfile Creation", "Please add additional Information if needed.", blockefunc);
            if (!Helpers.BlockerInfo.Done) MessageBox.Show("No Clean Login. Loginfile might be not set correctly! Proceed with caution.");

            int ct = 0;
            bool exists = true;
            while (exists && ct < 100)
            {
                try
                {
                    pro.CloseMainWindow();
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
            try { pro.Kill(); } catch { }

            try
            {
                Console.WriteLine("Login data Hash: " + filename + ": " + datfile.MD5HASH);

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

        public static LocalDatFile CreateNewFile(string filename)
        {
            LocalDatFile datfile = new LocalDatFile();

            string filepath = EnviromentManager.LBLocaldatsPath + filename + ".dat";
            datfile.gw2build = Api.ClientBuild;
            datfile.Path = filepath;

            Process pro = new Process { StartInfo = new ProcessStartInfo(EnviromentManager.GwClientExePath) };
            pro.Start();
            Action blockefunc = () => ModuleReader.WaitForModule("DPAPI.dll", pro,null);
            Helpers.BlockerInfo.Run("Loginfile Creation","Please check remember email/password and press the login and play button. This window will be closed automatically on success.", blockefunc);
            if (!Helpers.BlockerInfo.Done) MessageBox.Show("No Clean Login. Loginfile might be not set correctly! Proceed with caution.");
            Thread.Sleep(100);
          
            int ct = 0;
            bool exists = true;
            while (exists && ct < 100)
            {
                try
                {
                    pro.CloseMainWindow();
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
            try { pro.Kill(); } catch { }

            try
            {
#if DEBUG
                Console.WriteLine("Login data Hash: " + filename + ": " + datfile.MD5HASH);
#endif
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
        public bool IsOutdated { get { return !(IsUpToDate && Valid); } }
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
