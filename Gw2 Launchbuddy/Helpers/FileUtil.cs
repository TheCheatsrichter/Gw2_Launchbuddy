using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

static public class FileUtil
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
        if (!CreateSymbolicLink(lpSymlinkFileName, lpTargetFileName, dwFlags))
        {
            MessageBox.Show("Error: Unable to create symbolic link. " + "(Error Code: " + Marshal.GetLastWin32Error() + ")");
        }
        return true;
    }


    [StructLayout(LayoutKind.Sequential)]
    struct RM_UNIQUE_PROCESS
    {
        public int dwProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    const int RmRebootReasonNone = 0;
    const int CCH_RM_MAX_APP_NAME = 255;
    const int CCH_RM_MAX_SVC_NAME = 63;

    enum RM_APP_TYPE
    {
        RmUnknownApp = 0,
        RmMainWindow = 1,
        RmOtherWindow = 2,
        RmService = 3,
        RmExplorer = 4,
        RmConsole = 5,
        RmCritical = 1000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct RM_PROCESS_INFO
    {
        public RM_UNIQUE_PROCESS Process;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
        public string strAppName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
        public string strServiceShortName;

        public RM_APP_TYPE ApplicationType;
        public uint AppStatus;
        public uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bRestartable;
    }

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    static extern int RmRegisterResources(uint pSessionHandle,
                                          UInt32 nFiles,
                                          string[] rgsFilenames,
                                          UInt32 nApplications,
                                          [In] RM_UNIQUE_PROCESS[] rgApplications,
                                          UInt32 nServices,
                                          string[] rgsServiceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
    static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

    [DllImport("rstrtmgr.dll")]
    static extern int RmEndSession(uint pSessionHandle);

    [DllImport("rstrtmgr.dll")]
    static extern int RmGetList(uint dwSessionHandle,
                                out uint pnProcInfoNeeded,
                                ref uint pnProcInfo,
                                [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
                                ref uint lpdwRebootReasons);

    /// <summary>
    /// Find out what process(es) have a lock on the specified file.
    /// </summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>Processes locking the file</returns>
    /// <remarks>See also:
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
    /// http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)
    /// 
    /// </remarks>
    static public List<Process> WhoIsLocking(string path)
    {
        uint handle;
        string key = Guid.NewGuid().ToString();
        List<Process> processes = new List<Process>();

        int res = RmStartSession(out handle, 0, key);
        if (res != 0) throw new Exception("Could not begin restart session.  Unable to determine file locker.");

        try
        {
            const int ERROR_MORE_DATA = 234;
            uint pnProcInfoNeeded = 0,
                 pnProcInfo = 0,
                 lpdwRebootReasons = RmRebootReasonNone;

            string[] resources = new string[] { path }; // Just checking on one resource.

            res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

            if (res != 0) throw new Exception("Could not register resource.");

            //Note: there's a race condition here -- the first call to RmGetList() returns
            //      the total number of process. However, when we call RmGetList() again to get
            //      the actual processes this number may have increased.
            res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

            if (res == ERROR_MORE_DATA)
            {
                // Create an array to store the process results
                RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                pnProcInfo = pnProcInfoNeeded;

                // Get the list
                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                if (res == 0)
                {
                    processes = new List<Process>((int)pnProcInfo);

                    // Enumerate all of the results and add them to the 
                    // list to be returned
                    for (int i = 0; i < pnProcInfo; i++)
                    {
                        try
                        {
                            processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                        }
                        // catch the error -- in case the process is no longer running
                        catch (ArgumentException) { }
                    }
                }
                else throw new Exception("Could not list processes locking resource.");
            }
            else if (res != 0) throw new Exception("Could not list processes locking resource. Failed to get size of result.");
        }
        finally
        {
            RmEndSession(handle);
        }

        return processes;
    }

    public static string WhoIsLockingAsString(string path)
    {
        List<Process> pros = WhoIsLocking(path);

        string outp = "";
        foreach(Process pro in pros)
        {
            outp += $"Path: {path}+ Locked by :{pro.ProcessName} + ID:{pro.Id} + Exited:{pro.HasExited}";
        }
        return outp;
    }

    public static string GetFileHashMD5(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
        }
        catch(Exception e)
        {
#if DEBUG
            Debug.Print(e.Message);
            string lockert = FileUtil.WhoIsLockingAsString(path);
#endif
            return null;
        }

        return null;
    }

    public static string FolderDialog()
    {
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                if (Directory.Exists(fbd.SelectedPath)) return fbd.SelectedPath;
            }
        }
        return null;
    }

    public static bool IsSymbolic(string path)
    {
        FileInfo pathInfo = new FileInfo(path);
        return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }
}