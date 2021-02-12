using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gw2_Launchbuddy.Modifiers
{

    public class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInformation
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        internal enum ModuleFilter
        {
            ListModulesDefault = 0x0,
            ListModules32Bit = 0x01,
            ListModules64Bit = 0x02,
            ListModulesAll = 0x03,
        }

        [DllImport("psapi.dll")]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInformation lpmodinfo, uint cb);
    }

    public class Module
    {
        public Module(string moduleName, IntPtr baseAddress, uint size)
        {
            this.ModuleName = moduleName;
            this.BaseAddress = baseAddress;
            this.Size = size;
        }

        public string ModuleName { get; set; }
        public IntPtr BaseAddress { get; set; }
        public uint Size { get; set; }
    }

    public static class ModuleReader
    {

        public static void WaitForModule(string name, Process pro, int? timeout=1000)
        {
            int ct = 0;
            if(timeout!=null)
            {
                while (!CollectModules(pro).Any<Module>(m => m.ModuleName == name) && ct < timeout)
                {
                    Thread.Sleep(10);
                    ct++;
                }
            }else
            {
                while (!CollectModules(pro).Any<Module>(m => m.ModuleName == name) )
                {
                    Thread.Sleep(10);
                }
            }

            Console.WriteLine("DONE");
        }

        public static void ListAllModules(Process pro)
        {
            object time_old = DateTime.Now;
            Console.WriteLine(DateTime.Now);
            ProcessModuleCollection col_old = null;

            List<Module> Modules = new List<Module>();
            for (int i = 0; i < 100; i++)
            {
                foreach (Module module in CollectModules(pro))
                {
                    if (Modules.Any<Module>(m => m.ModuleName == module.ModuleName))
                    {
                        continue;
                    }
                    else
                    {
                        Modules.Add(module);
                        Console.WriteLine(DateTime.Now.Millisecond.ToString() + " " + module.Size + " " + module.ModuleName);
                    }

                }
                col_old = pro.Modules;

                Thread.Sleep(50);
            }
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(Modules.Count);
            Console.ReadKey();
        }

        public static List<Module> CollectModules(Process process)
        {
            List<Module> collectedModules = new List<Module>();

            IntPtr[] modulePointers = new IntPtr[0];
            int bytesNeeded = 0;

            try
            {
                // Determine number of modules
                if (!Native.EnumProcessModulesEx(process.Handle, modulePointers, 0, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
                {
                    return collectedModules;
                }
            }
            catch
            {
                //MessageBox.Show("Gameclient got closed unexpectedly. Could not collect Modules");
            }


            int totalNumberofModules = bytesNeeded / IntPtr.Size;
            modulePointers = new IntPtr[totalNumberofModules];

            try
            {
                // Collect modules from the process
                if (Native.EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
                {
                    for (int index = 0; index < totalNumberofModules; index++)
                    {
                        StringBuilder moduleFilePath = new StringBuilder(1024);
                        Native.GetModuleFileNameEx(process.Handle, modulePointers[index], moduleFilePath, (uint)(moduleFilePath.Capacity));

                        string moduleName = Path.GetFileName(moduleFilePath.ToString());
                        Native.ModuleInformation moduleInformation = new Native.ModuleInformation();
                        Native.GetModuleInformation(process.Handle, modulePointers[index], out moduleInformation, (uint)(IntPtr.Size * (modulePointers.Length)));

                        // Convert to a normalized module and add it to our list
                        Module module = new Module(moduleName, moduleInformation.lpBaseOfDll, moduleInformation.SizeOfImage);
                        collectedModules.Add(module);
                    }
                }
            }
            catch
            {

            }

            return collectedModules;
        }
    }
}


