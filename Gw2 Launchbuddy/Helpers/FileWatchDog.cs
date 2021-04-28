using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gw2_Launchbuddy.Helpers
{
    public class FileWatchDog
    {
        string filepath;
        int triggercount = 0;
        float timeout;
        Thread WatchThread;
        bool count_negativeflank;
        bool done = false;

        public bool Done
        {
            get { return done; }
        }

        public FileWatchDog(string filepath, int triggercount = 1, float timeout = 10000, bool count_negativeflank = false)
        {
            this.filepath = filepath;
            this.timeout = timeout;
            this.triggercount = triggercount;
            this.count_negativeflank = count_negativeflank;
            this.WatchThread = new Thread(WatchFile);
        }

        public void StartWatching()
        {
            WatchThread.Start();
        }

        public void WaitForDone()
        {
            WatchThread.Join();
        }

        private void WatchFile()
        {
            bool oldstate = count_negativeflank;
            int counter = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Debug.WriteLine($"Watchdog STARTED watching {filepath}, timeout:{timeout}, triggercount:{triggercount}");
            while (counter < triggercount*1000 || sw.ElapsedMilliseconds > timeout)
            {
                bool state = IsFileLocked();
                if (oldstate != state)
                {
                    oldstate = state;
                    counter++;
                }
            }
            sw.Stop();
            Debug.WriteLine($"Watchdog FINISHED watching {filepath}, time:{sw.ElapsedMilliseconds / 1000}, triggercount:{counter}");
            if (sw.ElapsedMilliseconds < timeout)
            {
                while (IsFileLocked())
                {
                    Thread.Sleep(250);
                }
            }
            done = true;
        }

        private bool IsFileLocked()
        {
            try
            {
                using (FileStream stream = new FileInfo(filepath).Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

    }
}
