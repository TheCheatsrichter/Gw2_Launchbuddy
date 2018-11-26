using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.ObjectModel;
using Gw2_Launchbuddy.ObjectManagers;
using System.Windows;

public enum DllInjectionResult
{
    DllNotFound,
    GameProcessNotFound,
    InjectionFailed,
    Success
}


namespace Gw2_Launchbuddy
{
    public sealed class DllInjector
    {
        static readonly IntPtr INTPTR_ZERO = (IntPtr)0;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        static DllInjector _instance;

        public static ObservableCollection<string> DllCollection= new ObservableCollection<string>();
        public static string DllConfigPath = Globals.AppDataPath + "DllCollection.txt";


        public static void AddDLL()
        {
            Builders.FileDialog.DefaultExt(".dll")
                .Filter("DLL Files(*.dll)|*.dll")
                .ShowDialog((Helpers.FileDialog fileDialog) =>
                {
                    if (fileDialog.FileName != "" && !DllCollection.Contains(fileDialog.FileName))
                    {
                        DllCollection.Add(fileDialog.FileName);
                    }
                });
        }
        public static void RemDLL(string Dll)
        {
            if(DllCollection.Contains(Dll))
            DllCollection.Remove(Dll);
        }

        public static ObservableCollection<string> LoadDlls()
        {
            string path = Globals.AppDataPath + "DllCollection.txt";
            if (File.Exists(DllConfigPath))
            {
                foreach(string dll in File.ReadAllLines(DllConfigPath))
                {
                    if(!DllCollection.Contains(dll) && File.Exists(dll))
                    DllCollection.Add(dll);
                    if (!File.Exists(dll))
                        MessageBox.Show(dll+" was moved or deleted! Please check your AddOn Injected Software");
                }
            }
            string basicdll=ClientManager.ClientInfo.InstallPath + @"bin64\d3d9.dll";
            if (File.Exists(basicdll) && !DllCollection.Contains(basicdll))
            {
                    MessageBox.Show("Found existing d3d9.dll! Automatically added to AddOns.");
                    DllCollection.Add(basicdll);
            }
            
            return DllCollection;
        }

        public static void SaveDlls()
        {
            if (File.Exists(DllConfigPath))
                File.Delete(DllConfigPath);
            File.WriteAllLines(DllConfigPath,DllCollection);
        }

        public static DllInjector GetInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DllInjector();
                }
                return _instance;
            }
        }

        DllInjector() { }

        public DllInjectionResult Inject(uint _procId, string sDllPath)
        {
            if (!File.Exists(sDllPath))
            {
                return DllInjectionResult.DllNotFound;
            }

            if (_procId == 0)
            {
                return DllInjectionResult.GameProcessNotFound;
            }

            if (!bInject(_procId, sDllPath))
            {
                return DllInjectionResult.InjectionFailed;
            }

            return DllInjectionResult.Success;
        }

        public DllInjectionResult InjectAll(uint _procId)
        {
            foreach (string sDllPath in DllCollection)
            {
                if (!File.Exists(sDllPath))
                {
                    return DllInjectionResult.DllNotFound;
                }

                if (_procId == 0)
                {
                    return DllInjectionResult.GameProcessNotFound;
                }

                if (!bInject(_procId, sDllPath))
                {
                    return DllInjectionResult.InjectionFailed;
                }
            }
            return DllInjectionResult.Success;
        }

        bool bInject(uint pToBeInjected, string sDllPath)
        {
            IntPtr hndProc = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, pToBeInjected);

            if (hndProc == INTPTR_ZERO)
            {
                return false;
            }

            IntPtr lpLLAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (lpLLAddress == INTPTR_ZERO)
            {
                return false;
            }

            IntPtr lpAddress = VirtualAllocEx(hndProc, (IntPtr)null, (IntPtr)sDllPath.Length, (0x1000 | 0x2000), 0X40);

            if (lpAddress == INTPTR_ZERO)
            {
                return false;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(sDllPath);

            if (WriteProcessMemory(hndProc, lpAddress, bytes, (uint)bytes.Length, 0) == 0)
            {
                return false;
            }
            if (CreateRemoteThread(hndProc, IntPtr.Zero, INTPTR_ZERO, lpLLAddress, lpAddress, 0, (IntPtr)null) == INTPTR_ZERO)
            {
                return false;
            }

            CloseHandle(hndProc);

            return true;
        }
    }

}
