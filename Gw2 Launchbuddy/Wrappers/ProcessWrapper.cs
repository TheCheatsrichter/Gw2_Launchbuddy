using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Gw2_Launchbuddy.Interfaces;
using System.Windows;
using System.Collections.ObjectModel;
using Gw2_Launchbuddy.ObjectManagers;
using System.Reflection;
using Gw2_Launchbuddy.Extensions;
using System.IO;
using System.Threading;

namespace Gw2_Launchbuddy.Wrappers
{
    public static class ProcessManager<T> where T : IProcessWrapper<ProcessWrapper>
    {
        public static string CalculateProcessMD5(Process Process)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(Process.StartTime.ToString() + Process.Id.ToString());
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString();
        }
    }

    public class ProcessWrapper : Process, IProcessWrapper<ProcessWrapper>
    {
        public string MD5 { get => ProcessManager<ProcessWrapper>.CalculateProcessMD5(this); }
        public event EventHandler<EventArgs> Started;

        public void Stop()
        {
            try
            {
                if (!CloseMainWindow())
                    Kill();
                if (!WaitForExit(1000))
                    Kill();
            }
            catch { }
        }

        public bool StartAndWait()
        {
            var success = Start();
            WaitForExit();
            return success;
        }

        new public bool Start()
        {
            bool success = base.Start();
            DllInjector Injector =DllInjector.GetInstance;
            Injector.InjectAll((uint)base.Id);
            //InjectionManager.CreateInjectedProcess(ClientManager.ClientInfo.FullPath, StartInfo.Arguments, ClientManager.ClientInfo.InstallPath + @"\bin64\d3d9.dll");
            if (EnableRaisingEvents == true) Started?.Invoke(this, new EventArgs());
            for (var i = 0; i < 10; i++)
            {
#if DEBUG
                System.Diagnostics.Debug.Print("Mutex Kill Attempt Nr" + i);
#endif
                try
                {
                    //if (HandleManager.ClearMutex(ClientManager.ClientInfo.ProcessName, "AN-Mutex-Window-Guild Wars 2", ref nomutexpros)) i = 10;
                    if (HandleManager.KillHandle(this, "AN-Mutex-Window-Guild Wars 2", false)) i = 10;
                }
                catch (Exception err)
                {
                    MessageBox.Show("Mutex release failed, will try again. Please provide the following if you want to help fix this problem: \r\n" + err.GetType().ToString() + "\r\n" + err.Message + "\r\n" + err.StackTrace);
                }

                //Maxtime 10 secs
                Thread.Sleep((int)(Math.Pow(i, 2) * 25 + 50));

            }
            return true;
        }


        public dynamic SetStartInfo(ProcessStartInfo value)
        {
            this.StartInfo = value;
            return this;
        }

        public dynamic SetWorkingDirectory(string value)
        {
            this.StartInfo.WorkingDirectory = value;
            return this;
        }

        public string Arguments { get => StartInfo.Arguments; set => StartInfo.Arguments = value; }
    }
}
