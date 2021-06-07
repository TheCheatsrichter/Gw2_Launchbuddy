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
                        IORepeater.FileDelete(file);
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
                IORepeater.WaitForFileAvailability(EnviromentManager.GwLocaldatPath);
                IORepeater.WaitForFileAvailability(file.Path);

                ToDefault(file);


                if (File.Exists(EnviromentManager.GwLocaldatPath)) IORepeater.FileMove(EnviromentManager.GwLocaldatPath, EnviromentManager.GwLocaldatBakPath);

                if (!CreateSymbolicLinkExtended(EnviromentManager.GwLocaldatPath, file.Path, SymbolicLink.File))
                {
                    ToDefault(file);
                    throw new Exception("Could not create symbolic link");
                }



                IORepeater.WaitForFileAvailability(EnviromentManager.GwLocaldatPath);
                IORepeater.WaitForFileAvailability(file.Path);
                GC.Collect();
            }
            catch (Exception e)
            {
                ToDefault(file);
                GC.Collect();
                throw new Exception("An error occured while swaping the Login file.\n" + EnviromentManager.Create_Environment_Report() + e.Message);
            }
        }

        public static void UpdateLocalDat(LocalDatFile file)
        {
            bool success = false;

            while (!success)
            {
                Apply(file);

                string OldHash;
                
                try
                {
                    OldHash = String.Copy(file.MD5HASH);
                }catch
                {
                    OldHash = "";
                }
                
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

                    waitAction = () => IORepeater.WaitForFileAvailability(file.Path);
                    Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to save the updated loginfile.", waitAction);

                    waitAction = () => IORepeater.WaitForFileAvailability(EnviromentManager.GwLocaldatPath);
                    Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to release the loginfile.", waitAction);
                }
                catch (Exception e)
                {
                    throw new Exception("An error occured while updating the loginfile.\n"+EnviromentManager.Create_Environment_Report()+e.Message);
                }

                ToDefault(file);

                success = file.ValidateUpdate(OldHash);
                GC.Collect();
            }
        }

        public static LocalDatFile CreateNewFile(string filename, string email = null, string password = null)
        {
            ToDefault(null);

            //Copy Loginfile to Backup Location because Localdat will be overwritten
            IORepeater.FileCopy(EnviromentManager.GwLocaldatPath, EnviromentManager.GwLocaldatBakPath,true);

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

            waitAction = () => IORepeater.WaitForFileAvailability(EnviromentManager.GwLocaldatPath);
            Helpers.BlockerInfo.Run("Loginfile Update", "Launchbuddy is waiting for Gw2 to release the loginfile.", waitAction);

            if (File.Exists(filepath)) IORepeater.FileDelete(filepath);
            IORepeater.FileMove(EnviromentManager.GwLocaldatPath, filepath);

            datfile.ValidateUpdate(null);

            ToDefault(null);
            GC.Collect();
            return datfile;
        }

        private static bool WaitForProcessClose(Process pro)
        {
            try
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
            catch
            {
                return true;
            }
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


        public static void ToDefault(LocalDatFile defaultfile)
        {
            try
            {
                //Backup And Localdat exists
                if (File.Exists(EnviromentManager.GwLocaldatBakPath) && File.Exists(EnviromentManager.GwLocaldatPath))
                {
                    IORepeater.FileDelete(EnviromentManager.GwLocaldatPath);
                    IORepeater.FileMove(EnviromentManager.GwLocaldatBakPath, EnviromentManager.GwLocaldatPath);
                }

                //Backup exists, Localdat does not
                if (File.Exists(EnviromentManager.GwLocaldatBakPath) && !File.Exists(EnviromentManager.GwLocaldatPath))
                {
                    IORepeater.FileMove(EnviromentManager.GwLocaldatBakPath, EnviromentManager.GwLocaldatPath);
                }

                //Backup and Localdat do not exists
                if (!File.Exists(EnviromentManager.GwLocaldatBakPath) && !File.Exists(EnviromentManager.GwLocaldatPath))
                {
                    if (defaultfile != null)
                    {
                        IORepeater.FileCopy(defaultfile.Path, EnviromentManager.GwLocaldatPath);
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
            Valid = File.Exists(EnviromentManager.LBLocaldatsPath+Name+".dat");
            return Valid;
        }

        public LocalDatFile() { }
    }
}
