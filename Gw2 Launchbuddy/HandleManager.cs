//Guild Wars MultiLaunch - Safe and efficient way to launch multiple GWs.
//The Guild Wars executable is never modified, keeping you inline with the tos.
//
//Copyright (C) 2010  IMKey@GuildWarsGuru

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

//The original code was modified between 2010 and 2016 multiple times
//by KairuByte. Adaptations made for GW2 use.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Gw2_Launchbuddy
{
    public static class HandleManager
    {
        #region Native Method Signatures

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
            uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, UInt32 dwProcessID);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern NTSTATUS NtQueryInformationFile(IntPtr FileHandle,
            ref IO_STATUS_BLOCK IoStatusBlock, IntPtr FileInformation, int FileInformationLength,
            FILE_INFORMATION_CLASS FileInformationClass);

        [DllImport("ntdll.dll")]
        private static extern NTSTATUS NtQueryObject(IntPtr ObjectHandle, OBJECT_INFORMATION_CLASS ObjectInformationClass,
            IntPtr ObjectInformation, int ObjectInformationLength, out int ReturnLength);

        [DllImport("ntdll.dll")]
        private static extern NTSTATUS NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS SystemInformationClass,
            IntPtr SystemInformation, int SystemInformationLength, out int ReturnLength);

        #endregion Native Method Signatures

        #region Structures

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SYSTEM_HANDLE_INFORMATION
        {
            public UInt32 OwnerPID;
            public Byte ObjectType;
            public Byte HandleFlags;
            public UInt16 HandleValue;
            public UIntPtr ObjectPointer;
            public IntPtr AccessMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_BASIC_INFORMATION
        {
            public UInt32 Attributes;
            public UInt32 GrantedAccess;
            public UInt32 HandleCount;
            public UInt32 PointerCount;
            public UInt32 PagedPoolUsage;
            public UInt32 NonPagedPoolUsage;
            public UInt32 Reserved1;
            public UInt32 Reserved2;
            public UInt32 Reserved3;
            public UInt32 NameInformationLength;
            public UInt32 TypeInformationLength;
            public UInt32 SecurityDescriptorLength;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_STATUS_BLOCK
        {
            public UInt32 Status;
            public UInt64 Information;
        }

        #endregion Structures

        #region Enumerations

        //DuplicateHandle
        [Flags]
        private enum DuplicateOptions : uint
        {
            DUPLICATE_CLOSE_SOURCE = 0x00000001,
            DUPLICATE_SAME_ACCESS = 0x00000002
        }

        //OpenProcess
        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        //NtQueryObject and NtQuerySystemInformation
        [Flags]
        private enum NTSTATUS : uint
        {
            STATUS_SUCCESS = 0x00000000,
            STATUS_INFO_LENGTH_MISMATCH = 0xC0000004
        } //partial enum, actual set is huge, google ntstatus.h

        //NtQueryObject
        [Flags]
        private enum OBJECT_INFORMATION_CLASS : uint
        {
            ObjectBasicInformation = 0,
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2,
            ObjectAllTypesInformation = 3,
            ObjectHandleInformation = 4
        }

        //NtQuerySystemInformation
        [Flags]
        private enum SYSTEM_INFORMATION_CLASS : uint
        {
            SystemHandleInformation = 16
        } //partial enum, actual set is huge, google SYSTEM_INFORMATION_CLASS

        //NtQueryInformationFile
        [Flags]
        private enum FILE_INFORMATION_CLASS
        {
            FileNameInformation = 9
        } //partial enum, actual set is huge, google SYSTEM_INFORMATION_CLASS

        #endregion Enumerations

        #region Functions

        /// <summary>
        /// Clears file locks on the specified file, from specified process.
        /// </summary>
        /// <param name="Proc">Full path to process file to unlock from</param>
        /// <param name="File">Full path to file to be unlocked</param>
        /// <returns></returns>
        public static bool ClearFileLock(string Proc, string File)
        {
            var temp = new List<int>();
            return ClearFileLock(Proc, File, ref temp);
        }

        /// <summary>
        /// Clears file locks on the specified file, from specified process.
        /// </summary>
        /// <param name="Proc">Full path to process file to unlock from</param>
        /// <param name="File">Full path to file to be unlocked</param>
        /// <param name="excProcIDs">Process ID's to exclude</param>
        /// <returns></returns>
        public static bool ClearFileLock(string ProcName, string File, ref List<int> excProcIDs)
        {
            bool success = false;
            var excList = excProcIDs == null ? new List<int>() : excProcIDs.ToList();

            //take off the drive portion due to limitation in how killhandle works for file name
            File = File.Substring(2);

            //get list of currently running system processes
            Process[] processList = Process.GetProcesses();

            foreach (Process i in processList.Where(a => !excList.Contains(a.Id)).Where(a => a.ProcessName.Equals(ProcName, StringComparison.OrdinalIgnoreCase)))
            {
                if (HandleManager.KillHandle(i, File, true))
                {
                    success = true;
                    excProcIDs.Add(i.Id);
                }
            }

            return success;
        }

        /// <summary>
        /// Kills named mutex in active process.
        /// </summary>
        /// <param name="Proc">Full path to process file to unlock from</param>
        /// <param name="MutexName">Full name of mutex to be removed</param>
        /// <returns></returns>
        /// <returns></returns>
        public static bool ClearMutex(string Proc, string MutexName)
        {
            var temp = new List<int>();
            return ClearMutex(Proc, MutexName, ref temp);
        }

        /// <summary>
        /// Kills named mutex in active process.
        /// </summary>
        /// <param name="Proc">Full path to process file to unlock from</param>
        /// <param name="MutexName">Full name of mutex to be removed</param>
        /// <param name="excProcIDs">Process ID's to exclude</param>
        /// <returns></returns>
        public static bool ClearMutex(string ProcName, string MutexName, ref List<int> excProcIDs)
        {
            bool success = false;
            var excList = excProcIDs == null ? new List<int>() : excProcIDs.ToList();

            //get list of currently running system processes
            Process[] processList = Process.GetProcesses();

            foreach (Process i in processList.Where(a => !excList.Contains(a.Id)).Where(a => a.ProcessName.Equals(ProcName, StringComparison.OrdinalIgnoreCase)))
            {
                if (HandleManager.KillHandle(i, MutexName, false))
                {
                    success = true;
                    excProcIDs.Add(i.Id);
                }
            }

            return success;
        }

        /// <summary>
        /// Kills the handle whose name contains the nameFragment.
        /// </summary>
        /// <param name="targetProcess"></param>
        /// <param name="handleName"></param>
        public static bool KillHandle(Process targetProcess, string handleName, bool isFile)
        {
            bool success = false;

            //pSysInfoBuffer is a pointer to unmanaged memory
            IntPtr pSysHandles = GetAllHandles();

            //sanity check
            if (pSysHandles == IntPtr.Zero) return success;

            //Assemble list of SYSTEM_HANDLE_INFORMATION for the specified target process
            List<SYSTEM_HANDLE_INFORMATION> processHandles = GetHandles(targetProcess, pSysHandles);

            //free pSysInfoBuffer buffer
            Marshal.FreeHGlobal(pSysHandles);

            //Iterate through handles which belong to target process and kill
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.DupHandle, false, (UInt32)targetProcess.Id);

            foreach (SYSTEM_HANDLE_INFORMATION handleInfo in processHandles)
            {
                string name;

                if (isFile)
                {
                    name = GetFileName(handleInfo, hProcess);
                }
                else
                {
                    name = GetHandleName(handleInfo, hProcess);
                }

                //if (Regex.IsMatch(name, @"local\.dat$", RegexOptions.IgnoreCase))
                if (name.ToLower().Contains(handleName.ToLower()))
                {
                    if (CloseHandleEx(handleInfo.OwnerPID, new IntPtr(handleInfo.HandleValue)))
                    {
                        success = true;
                    }
                }
            }
            CloseHandle(hProcess);

            return success;
        }

        /// <summary>
        /// Closes a handle that is owned by another process.
        /// </summary>
        /// <param name="processID"></param>
        /// <param name="handleToClose"></param>
        private static bool CloseHandleEx(UInt32 processID, IntPtr handleToClose)
        {
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, processID);

            //Kills handle by DUPLICATE_CLOSE_SOURCE option, source is killed while destinationHandle goes to null
            IntPtr x;
            bool success = DuplicateHandle(hProcess, handleToClose, IntPtr.Zero,
                out x, 0, false, DuplicateOptions.DUPLICATE_CLOSE_SOURCE);

            CloseHandle(hProcess);

            return success;
        }

        /// <summary>
        /// Convert UNICODE_STRING located at pStringBuffer to a managed string.
        /// </summary>
        /// <param name="pStringBuffer">Pointer to start of UNICODE_STRING struct.</param>
        /// <returns>Managed string.</returns>
        private static string ConvertToString(IntPtr pStringBuffer)
        {
            long baseAddress = pStringBuffer.ToInt64();

            //don't know why, 8 bytes for 32 bit platforms and 16 bytes for 64 bit
            int offset = IntPtr.Size * 2;

            string handleName = Marshal.PtrToStringUni(new IntPtr(baseAddress + offset));

            return handleName;
        }

        /// <summary>
        /// Retrieves all currently active handles for all system processes.
        /// There currently isn't a way to only get it for a specific process.
        /// This relies on NtQuerySystemInformation which exists in ntdll.dll.
        /// </summary>
        /// <returns>Unmanaged IntPtr to the handles (raw data, must be processed)</returns>
        private static IntPtr GetAllHandles()
        {
            int bufferSize = 0x10000;   //initial buffer size of 65536 bytes (initial estimate)
            int actualSize;             //will store size of actual data written to buffer

            //initial allocation
            IntPtr pSysInfoBuffer = Marshal.AllocHGlobal(bufferSize);

            //query for handles (priming call, since initial buffer size will probably not be big enough)
            NTSTATUS queryResult = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemHandleInformation,
                pSysInfoBuffer, bufferSize, out actualSize);

            // Keep calling until buffer is large enough to fit all handles
            while (queryResult == NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
            {
                //deallocate space since we couldn't fit all the handles in
                Marshal.FreeHGlobal(pSysInfoBuffer);

                //double buffer size (we can't just use actualSize from last call since # of handles vary in time)
                bufferSize = bufferSize * 2;

                //allocate memory with increase buffer size
                pSysInfoBuffer = Marshal.AllocHGlobal(bufferSize);

                //query for handles
                queryResult = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemHandleInformation,
                    pSysInfoBuffer, bufferSize, out actualSize);
            }

            if (queryResult == NTSTATUS.STATUS_SUCCESS)
            {
                return pSysInfoBuffer; //pSystInfoBuffer will be freed later
            }
            else
            {
                //other NTSTATUS, shouldn't happen
                Marshal.FreeHGlobal(pSysInfoBuffer);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Filter out handles which belong to targetProcess.
        /// </summary>
        /// <param name="targetProcess">The process whose handles you want.</param>
        /// <param name="pAllHandles">Pointer to all the system handles.</param>
        /// <returns>List of handles owned by the targetProcess</returns>
        private static List<SYSTEM_HANDLE_INFORMATION> GetHandles(Process targetProcess, IntPtr pSysHandles)
        {
            List<SYSTEM_HANDLE_INFORMATION> processHandles = new List<SYSTEM_HANDLE_INFORMATION>();

            Int64 pBaseLocation = pSysHandles.ToInt64();    //base address
            Int64 currentOffset;                            //offset from pBaseLocation
            IntPtr pLocation;                               //current address

            SYSTEM_HANDLE_INFORMATION currentHandleInfo;

            //number of total system handles (should be okay for 64bit version too)
            int nHandles = Marshal.ReadInt32(pSysHandles);

            // Iterate through all system handles
            for (int i = 0; i < nHandles; i++)
            {
                //first (IntPtr.Size) bytes stores number of handles
                //data follows, each set is size of SYSTEM_HANDLE_INFORMATION
                currentOffset = IntPtr.Size + i * Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION));

                //calculate intptr to new location
                pLocation = new IntPtr(pBaseLocation + currentOffset);

                // Create structure out of the memory block
                currentHandleInfo = (SYSTEM_HANDLE_INFORMATION)
                    Marshal.PtrToStructure(pLocation, typeof(SYSTEM_HANDLE_INFORMATION));

                // Add only handles which match the target process id
                if (currentHandleInfo.OwnerPID == (UInt32)targetProcess.Id)
                {
                    processHandles.Add(currentHandleInfo);
                }
            }

            return processHandles;
        }

        /// <summary>
        /// Queries for file name associated with handle.
        /// </summary>
        /// <param name="handleInfo">The handle info.</param>
        /// <param name="hProcess">Open handle to the process which owns that handle.</param>
        /// <returns></returns>
        private static string GetFileName(SYSTEM_HANDLE_INFORMATION handleInfo, IntPtr hProcess)
        {
            try
            {
                IntPtr thisProcess = Process.GetCurrentProcess().Handle;
                IntPtr handle;

                // Need to duplicate handle in this process to be able to access name
                DuplicateHandle(hProcess, new IntPtr(handleInfo.HandleValue), thisProcess,
                    out handle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS);

                // Setup buffer to store unicode string
                int bufferSize = 0x200; //512 bytes

                // Allocate unmanaged memory to store name
                IntPtr pFileNameBuffer = Marshal.AllocHGlobal(bufferSize);
                IO_STATUS_BLOCK ioStat = new IO_STATUS_BLOCK();

                NtQueryInformationFile(handle, ref ioStat, pFileNameBuffer, bufferSize, FILE_INFORMATION_CLASS.FileNameInformation);

                // Close this handle
                CloseHandle(handle);    //super important... almost missed this

                // offset=4 seems to work...
                int offset = 4;
                long pBaseAddress = pFileNameBuffer.ToInt64();

                // Do the conversion to managed type
                string fileName = Marshal.PtrToStringUni(new IntPtr(pBaseAddress + offset));

                // Release
                Marshal.FreeHGlobal(pFileNameBuffer);

                return fileName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Queries for name of handle.
        /// </summary>
        /// <param name="targetHandleInfo">The handle info.</param>
        /// <param name="hProcess">Open handle to the process which owns that handle.</param>
        /// <returns></returns>
        private static string GetHandleName(SYSTEM_HANDLE_INFORMATION targetHandleInfo, IntPtr hProcess)
        {
            //skip special NamedPipe handle (this may cause hang up with NtQueryObject function)
            if (targetHandleInfo.AccessMask.ToInt64() == 0x0012019F)
            {
                return String.Empty;
            }

            IntPtr thisProcess = Process.GetCurrentProcess().Handle;
            IntPtr handle;

            // Need to duplicate handle in this process to be able to access name
            DuplicateHandle(hProcess, new IntPtr(targetHandleInfo.HandleValue), thisProcess,
                out handle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS);

            // Setup buffer to store unicode string
            int bufferSize = GetHandleNameLength(handle);

            // Allocate unmanaged memory to store name
            IntPtr pStringBuffer = Marshal.AllocHGlobal(bufferSize);

            // Query to fill string buffer with name
            NtQueryObject(handle, OBJECT_INFORMATION_CLASS.ObjectNameInformation, pStringBuffer, bufferSize, out bufferSize);

            // Close this handle
            CloseHandle(handle);    //super important... almost missed this

            // Do the conversion to managed type
            string handleName = ConvertToString(pStringBuffer);

            // Release
            Marshal.FreeHGlobal(pStringBuffer);

            return handleName;
        }

        /// <summary>
        /// Get size of the name info block for that handle.
        /// </summary>
        /// <param name="handle">Handle to process.</param>
        /// <returns></returns>
        private static int GetHandleNameLength(IntPtr handle)
        {
            int infoBufferSize = Marshal.SizeOf(typeof(OBJECT_BASIC_INFORMATION));  //size of OBJECT_BASIC_INFORMATION struct
            IntPtr pInfoBuffer = Marshal.AllocHGlobal(infoBufferSize);              //allocate

            // Query for handle's OBJECT_BASIC_INFORMATION
            NtQueryObject(handle, OBJECT_INFORMATION_CLASS.ObjectBasicInformation, pInfoBuffer, infoBufferSize, out infoBufferSize);

            // Map memory to structure
            OBJECT_BASIC_INFORMATION objInfo =
                (OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(pInfoBuffer, typeof(OBJECT_BASIC_INFORMATION));

            Marshal.FreeHGlobal(pInfoBuffer);   //release

            // If the handle has an empty name, we still need to give the buffer a size to map the UNICODE_STRING struct to.
            if (objInfo.NameInformationLength == 0)
            {
                return 0x100;    //reserve 256 bytes, since nameinfolength = 0 for filenames
            }
            else
            {
                return (int)objInfo.NameInformationLength;
            }
        }

        #endregion Functions
    }
}