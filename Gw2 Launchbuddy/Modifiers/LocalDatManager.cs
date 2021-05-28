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
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.I1)]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        public enum SymbolicLink
        {
            File = 0x0,
            Directory = 0x1
        }

        public static bool CreateSymbolicLinkExtended(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags)
        {
            if (!CreateSymbolicLink(lpSymlinkFileName, lpTargetFileName, SymbolicLink.File))
            {
                MessageBox.Show("Error: Unable to create symbolic link. " +"(Error Code: " + Marshal.GetLastWin32Error() + ")");
            }
            return true;
        }

        public static void CleanUp()
        {
            ObservableCollection<LocalDatFile> tmp_datfiles = new ObservableCollection<LocalDatFile>();
            foreach (Account acc in AccountManager.Accounts)
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
            if (!file.Valid)
            {
                MessageBox.Show("Invalid Login file " + file.Name + " please recreate this file in the account Manager.");
                return;
            }
            try
            {
                WaitForLoginfileRelease(EnviromentManager.GwLocaldatPath);
                WaitForLoginfileRelease(file);

                ToDefault(file);

                if (File.Exists(EnviromentManager.GwLocaldatPath)) File.Move(EnviromentManager.GwLocaldatPath, EnviromentManager.GwLocaldatBakPath);

                if (!CreateSymbolicLinkExtended(EnviromentManager.GwLocaldatPath, file.Path, SymbolicLink.File))
                {
                    ToDefault(file);
                    throw new Exception("Could not create symbolic link");
                }

                WaitForLoginfileRelease(EnviromentManager.GwLocaldatPath);
                WaitForLoginfileRelease(file);
            }
            catch (Exception e)
            {
                ToDefault(file);
                throw new Exception("An error occured while swaping the Login file." + e.Message);
            }
        }

        public static void UpdateLocalDat(LocalDatFile file)
        {
            bool success = false;

            while (!success)
            {
                Apply(file);

                string OldHash = String.Copy(file.MD5HASH);

                try
                {
                    Process pro = new Process { StartInfo = new ProcessStartInfo { FileName = EnviromentManager.GwClientExePath } };
                    pro.Start();
                    Thread.Sleep(500);
                    pro.Refresh();
                    Action waitforlaunch = () => ModuleReader.WaitForModule("WINNSI.DLL", pro);
                    Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is updating a Loginfile. This window should automatically close when the launcher login is ready.", waitforlaunch);

                    try
                    {
                        if (!pro.CloseMainWindow())
                        {
                            try { pro.Close(); } catch { }
                            try { pro.Kill(); } catch { }
                        }
                    }
                    catch
                    {

                    }

                    Action waitAction = () => WaitForProcessClose(pro);
                    Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to be closed.", waitAction);

                    waitAction = () => WaitForLoginfileRelease(file);
                    Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to save the updated loginfile.", waitAction);

                    waitAction = () => WaitForLoginfileRelease(EnviromentManager.GwLocaldatPath);
                    Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to release the loginfile.", waitAction);
                }
                catch (Exception e)
                {
                    throw new Exception("An error occured while updating the loginfile.\n"+EnviromentManager.Create_Environment_Report()+e.Message);
                }

                ToDefault(file);

                success = file.ValidateUpdate(OldHash);
            }
        }

        public static LocalDatFile CreateNewFile(string filename, string email = null, string password = null)
        {
            ToDefault(null);

            //Copy Loginfile to Backup Location because Localdat will be overwritten
            File.Copy(EnviromentManager.GwLocaldatPath, EnviromentManager.GwLocaldatBakPath);

            LocalDatFile datfile = new LocalDatFile();

            string filepath = EnviromentManager.LBLocaldatsPath + filename + ".dat";
            datfile.gw2build = Api.ClientBuild;
            datfile.Path = filepath;

            Process pro = new Process { StartInfo = new ProcessStartInfo(EnviromentManager.GwClientExePath) };
            pro.Start();
            Thread.Sleep(500);
            pro.Refresh();
#if DEBUG
            if(email ==null || password == null) Console.WriteLine("Loginfile Creation: No Email or Password given proceeding with manual creation.");
#endif
            if (email != null && password != null)
            {
                Action blockefunc = () => ModuleReader.WaitForModule("WINNSI.DLL", pro, null);
                Helpers.BlockerInfo.Run("Loginfile Creation", "LB is recreating your loginfile", blockefunc);
                if (!Helpers.BlockerInfo.Done) MessageBox.Show("No Clean Login. Loginfile might be not set correctly! Proceed with caution.");

                Thread.Sleep(100);
                Loginfiller.Login(email, password, pro, true);
                Thread.Sleep(250);

                blockefunc = () => ModuleReader.WaitForModule("DPAPI.dll", pro, null);
                Helpers.BlockerInfo.Run("Loginfile Creation", "Please add additional Information if needed.", blockefunc);
                if (!Helpers.BlockerInfo.Done) MessageBox.Show("No Clean Login. Loginfile might be not set correctly! Proceed with caution.");

            }
            else
            {
                Action blockerfunc = () => ModuleReader.WaitForModule("DPAPI.dll", pro, null);
                Helpers.BlockerInfo.Run("Loginfile Creation", "Please check remember email/password and press the login and play button. This window will be closed automatically on success.", blockerfunc);
                if (!Helpers.BlockerInfo.Done) MessageBox.Show("No Clean Login. Loginfile might be not set correctly! Proceed with caution.");
                Thread.Sleep(100);
            }

            try
            {
                if (!pro.CloseMainWindow())
                {
                    try { pro.Close(); } catch { }
                    try { pro.Kill(); } catch { }
                }
            }
            catch
            {

            }

            Action waitAction = () => WaitForProcessClose(pro);
            Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to be closed.", waitAction);

            waitAction = () => WaitForLoginfileRelease(EnviromentManager.GwLocaldatPath);
            Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to release the loginfile.", waitAction);

            if (File.Exists(filepath)) File.Delete(filepath);
            File.Move(EnviromentManager.GwLocaldatPath, filepath);

            datfile.ValidateUpdate(null);

            ToDefault(null);

            return datfile;
        }

        private static bool WaitForProcessClose(Process pro)
        {
            int i = 0;
            if (pro == null) return true;
            while (i < 10 && !pro.HasExited)
            {
                Thread.Sleep(100);
                pro.Refresh();
                i++;
            }
            return i < 10;
        }

        private static bool LoginFileIsLocked(LocalDatFile file)
        {
            return LoginFileIsLocked(file.Path);
        }

        private static bool LoginFileIsLocked(string filepath)
        {
            FileStream stream = null;
            if (!File.Exists(filepath)) return false;
            try
            {
                stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
            WaitForLoginfileRelease(file.Path);
        }

        private static void WaitForLoginfileRelease(string filepath)
        {
            int i = 0;
            while (LoginFileIsLocked(filepath) && i < 30)
            {
                Debug.WriteLine($"Waiting for Loginfile.dat to be unlocked retry: {i}");
                i++;
                Thread.Sleep(250);
            }
        }

        public static void ToDefault(LocalDatFile defaultfile)
        {
            try
            {
                //Backup And Localdat exists
                if (File.Exists(EnviromentManager.GwLocaldatBakPath) && File.Exists(EnviromentManager.GwLocaldatPath))
                {
                    File.Delete(EnviromentManager.GwLocaldatPath);
                    File.Move(EnviromentManager.GwLocaldatBakPath, EnviromentManager.GwLocaldatPath);
                }

                //Backup exists, Localdat does not
                if (File.Exists(EnviromentManager.GwLocaldatBakPath) && !File.Exists(EnviromentManager.GwLocaldatPath))
                {
                    File.Move(EnviromentManager.GwLocaldatBakPath, EnviromentManager.GwLocaldatPath);
                }

                //Backup and Localdat do not exists
                if (!File.Exists(EnviromentManager.GwLocaldatBakPath) && !File.Exists(EnviromentManager.GwLocaldatPath))
                {
                    if (defaultfile != null)
                    {
                        File.Copy(defaultfile.Path, EnviromentManager.GwLocaldatPath);
                    }
                }
            }
            catch
            {
                MessageBox.Show("An error araised when the loginfile was restored to its original form. Loginfile might not be set correctly now.");
            }
        }


    }

    public class LocalDatFile
    {
        public string Path { set; get; }
        public string gw2build { set; get; }
        public string Name { get { return System.IO.Path.GetFileNameWithoutExtension(Path); } }
        public string Gw2Build { get { return gw2build; } }
        public bool IsUpToDate { get { return Gw2Build == EnviromentManager.GwClientVersion; } }
        public bool IsOutdated { get { return !(IsUpToDate) & Valid; } }
        public bool Valid = false;
        public string MD5HASH { get { return CalculateMD5(Path); } }

        private string CalculateMD5(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var hash = md5.ComputeHash(stream);
                            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public bool ValidateUpdate(string OldHash)
        {
            Console.WriteLine($"OldHash= {OldHash} NewHash= {MD5HASH}");
            //Should be called after every Update
            if (OldHash == MD5HASH)
            {
                DialogResult dialogResult = MessageBox.Show(($"The loginfile for {AccountManager.GetAccountByID(int.Parse(this.Name)).Nickname} did not change when updating from {Gw2Build} to {EnviromentManager.GwClientVersion}. Proceed anyway?"), "Loginfile Update", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    Valid = false;
                    return false;
                }
            }
            gw2build = EnviromentManager.GwClientVersion;
            Valid = true;
            return true;
        }

        public LocalDatFile() { }
    }
}

/*
public static class LocalDatManager
{
    private static LocalDatFile CurrentFile = null;


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
            if (!CreateSymbolicLink(EnviromentManager.GwLocaldatPath, sourcefile, SymbolicLink.File))
            {
                throw new Exception("Could not create Symbolic link. Not running as Admin?");
            }
        }
        else
        {
            throw new Exception($"Provided Local.dat file does not exist or Linkfile is already created. Source:{sourcefile} Exists:{File.Exists(sourcefile)}, Target:{EnviromentManager.GwLocaldatPath} Exists:{File.Exists(EnviromentManager.GwLocaldatPath)}");
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
        foreach (Account acc in AccountManager.Accounts)
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
            WaitForLoginfileRelease(EnviromentManager.GwLocaldatPath);
            if (LoginFileIsLocked(EnviromentManager.GwLocaldatPath))
            {
                try
                {
                    HandleManager.ClearFileLock(EnviromentManager.GwClientExeName, EnviromentManager.GwLocaldatPath);
                }
                catch { }
            }

            Repair();
            //Is Valid?
            if (!file.Valid) MessageBox.Show("Invalid Login file " + file.Name + " please recreate this file in the account Manager.");

            //Create Backup of Local dat
            if (!IsSymbolic(EnviromentManager.GwLocaldatPath))
            {
                if (File.Exists(EnviromentManager.GwLocaldatBakPath)) File.Delete(EnviromentManager.GwLocaldatBakPath);
                File.Move(EnviromentManager.GwLocaldatPath, EnviromentManager.GwLocaldatBakPath);
            }
            //Delete Local.dat
            if (File.Exists(EnviromentManager.GwLocaldatPath)) File.Delete(EnviromentManager.GwLocaldatPath);

            //Create Symlink Replacer
            CreateSymbolLink(file.Path);
            //Remember last used file for ToDefault()
            CurrentFile = file;

            WaitForLoginfileRelease(EnviromentManager.GwLocaldatPath);
        }

        catch (Exception e)
        {
            try
            {
                Repair();
            }
            catch { throw new Exception("An error occured while swaping the Login file." + EnviromentManager.Create_Environment_Report() + "\n\n" + e.Message); }

            throw new Exception("An error occured while swaping the Login file." +EnviromentManager.Create_Environment_Report()+"\n\n" + e.Message);
        }

    }

    public static void ToDefault()
    {
        if (File.Exists(EnviromentManager.GwLocaldatPath)) File.Delete(EnviromentManager.GwLocaldatPath);
        if (File.Exists(EnviromentManager.GwLocaldatBakPath))
        {
            File.Move(EnviromentManager.GwLocaldatBakPath, EnviromentManager.GwLocaldatPath);
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
        //if (File.Exists(EnviromentManager.GwLocaldatBakPath)) File.Delete(EnviromentManager.GwLocaldatBakPath);
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
                //pro.WaitForInputIdle();
                Thread.Sleep(500);
                pro.Refresh();
                Action waitforlaunch = () => ModuleReader.WaitForModule("WINNSI.DLL", pro);
                Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is updating an outdated Loginfile", waitforlaunch);
                pro.Close();
                try
                {
                    pro.Kill();
                }
                catch
                { }

                Action waitforlock = () => WaitForLoginfileRelease(file);
                Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to save the updated loginfile.", waitforlock);
                file.gw2build = Api.ClientBuild;
                ToDefault();
            }
        }
        catch (Exception e)
        {
            throw new Exception("An error occured when Updating the Login file. " + e.Message);
        }
    }

    private static bool LoginFileIsLocked(LocalDatFile file)
    {
        return LoginFileIsLocked(file.Path);
    }

    private static bool LoginFileIsLocked(string filepath)
    {
        FileStream stream = null;
        try
        {
            stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
        WaitForLoginfileRelease(file.Path);
    }

    private static void WaitForLoginfileRelease(string filepath)
    {
        int i = 0;
        while (LoginFileIsLocked(filepath) && i < 30)
        {
            Debug.WriteLine($"Waiting for Loginfile.dat to be unlocked retry: {i}");
            i++;
            Thread.Sleep(250);
        }
    }

    public static LocalDatFile CreateNewFileAutomated(string filename, string email, string passwd)
    {
        Repair();
        LocalDatFile datfile = new LocalDatFile();

        string filepath = EnviromentManager.LBLocaldatsPath + filename + ".dat";
        datfile.gw2build = Api.ClientBuild;
        datfile.Path = filepath;

        Process pro = new Process { StartInfo = new ProcessStartInfo(EnviromentManager.GwClientExePath) };
        pro.Start();
        Thread.Sleep(500);
        pro.Refresh();
        Action blockefunc = () => ModuleReader.WaitForModule("WINNSI.DLL", pro, null);
        Helpers.BlockerInfo.Run("Loginfile Creation", "LB is recreating your loginfile", blockefunc);
        if (!Helpers.BlockerInfo.Done) MessageBox.Show("No Clean Login. Loginfile might be not set correctly! Proceed with caution.");
        Thread.Sleep(100);
        Loginfiller.Login(email, passwd, pro, true);
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
        Repair();
        LocalDatFile datfile = new LocalDatFile();

        string filepath = EnviromentManager.LBLocaldatsPath + filename + ".dat";
        datfile.gw2build = Api.ClientBuild;
        datfile.Path = filepath;

        Process pro = new Process { StartInfo = new ProcessStartInfo(EnviromentManager.GwClientExePath) };
        pro.Start();
        Thread.Sleep(250);
        pro.Refresh();
        Action blockefunc = () => ModuleReader.WaitForModule("DPAPI.dll", pro, null);
        Helpers.BlockerInfo.Run("Loginfile Creation", "Please check remember email/password and press the login and play button. This window will be closed automatically on success.", blockefunc);
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

    private static bool CheckForDuplicateLoginfile(LocalDatFile file)
    {
        return AccountManager.Accounts.Any(x => x.Settings.Loginfile.MD5HASH == file.MD5HASH);
    }
}
*/

