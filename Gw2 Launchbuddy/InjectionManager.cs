using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


//USED TO LAUNCH GW2 Suspended, might be needed in the future

namespace Gw2_Launchbuddy
{
    public static class InjectionManager
    {
        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(string lpApplicationName,
              string lpCommandLine, IntPtr lpProcessAttributes,
              IntPtr lpThreadAttributes,
              bool bInheritHandles, ProcessCreationFlags dwCreationFlags,
              IntPtr lpEnvironment, string lpCurrentDirectory,
              ref STARTUPINFO lpStartupInfo,
              out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);


        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }
        [Flags]
        public enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        public static Process CreateInjectedProcess(string ExePath,string Arguments, string Dlls)
        {
            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            bool success = CreateProcess(ExePath, Arguments,
                IntPtr.Zero, IntPtr.Zero, false,
                ProcessCreationFlags.ZERO_FLAG,
                IntPtr.Zero, null, ref si, out pi);
            DllInjector Injector = DllInjector.GetInstance;
            //MessageBox.Show("Click to inject");
            Injector.Inject(pi.dwProcessId, @"C:\Program Files\Guild Wars 2\bin64\d3d9.dll");
            //MessageBox.Show("Click to resume");
            IntPtr ThreadHandle = pi.hThread;
            ResumeThread(ThreadHandle);
            Process pro = new Process();
            pro = Process.GetProcessById((int)pi.dwProcessId);
            return pro;
        }
    }


}
