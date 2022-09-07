using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.Extensions
{
    public class ProcessExtension : Process
    {

        bool started = false;
        int timeout_close = 5000;

        public ExitStatus exitstatus = ExitStatus.none;
        public enum ExitStatus
        {
            none = 0,
            manual_mainwindow = 1,
            manual_close = 2,
            self_close = 3,
            self_crash = 4,
            self_update = 5,
            manual_kill = 6
        }

        public bool IsRunning
        {
            get
            {
                if (this == null) return false;
                try
                {
                    return started && !HasExited;
                }
                catch
                {
                    return false;
                }

            }
        }
        public ExitStatus Exitstatus
        {
            get { return exitstatus; }
        }

        public TimeSpan Runtime
        {
            get { return ExitTime - StartTime; }
        }

        public virtual new bool Start()
        {
            this.EnableRaisingEvents = true;
            started = true;

            base.Exited += OnExitedSelf;
            return base.Start();
        }

        public virtual void OnExitedSelf(object sender, EventArgs e)
        {

            if (exitstatus == ExitStatus.self_update) return;
            if (!HasExited) return;

            switch (base.ExitCode)
            {
                case 0:
                    exitstatus = ExitStatus.self_close;
                    break;
                default:
                    exitstatus = ExitStatus.self_crash;
                    break;
            }
        }

        public virtual bool Stop()
        {
            if (base.HasExited)
            {
                return false;
            }
            base.Exited -= OnExitedSelf;
            exitstatus = ExitStatus.manual_mainwindow;
            if (!base.CloseMainWindow())
            {
                base.WaitForExit(timeout_close);
                if (!base.HasExited)
                {
                    base.Close();
                    base.WaitForExit(timeout_close);
                    exitstatus = ExitStatus.manual_close;
                    if (!base.HasExited)
                    {
                        base.Kill();
                        exitstatus = ExitStatus.manual_kill;
                        Refresh();
                        base.WaitForExit(timeout_close);
                        if (!base.HasExited)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public string CalculateProcessMD5()
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(this.StartTime.ToString() + this.Id.ToString());
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString();
        }

        public void WaitForProcessExit()
        {
            while(IsRunning)
            {
                Thread.Sleep(10);
            }
        }
    }

    public class GwGameProcess : ProcessExtension
    {
        GameStatus gamestatus = GameStatus.none;

        string exe_tmppath = @"D:\Guild Wars 2\Gw2-64.tmp";
        string exe_name = "Gw2-64.exe";
        string localdat_path = @"C:\Users\Adrian\AppData\Roaming\Guild Wars 2\Local.dat";
        public enum GameStatus
        {
            none = 0,
            self_updating = 1,
            loginwindow_prelogin = 2,
            loginwindow_authentication = 3,
            loginwindow_pressplay = 4,
            game_startup = 5,
            game_charscreen = 6
        }

        public void OnStateChange(object sender, EventArgs e)
        {
            base.Refresh();
            IntPtr hwndMain = base.MainWindowHandle;

            Enum.TryParse((sender as ProcessState).Name, out gamestatus);
            //Console.WriteLine("EXTERNAL: State changed to " + (sender as ProcessState).Name);
        }

        ProcessPipeline CreateStates()
        {

            List<IProcessTrigger> pt_selfupdate = new List<IProcessTrigger>
            {
                new ModuleTrigger("VERSION.dll",this),
                new ModuleTrigger("WINMM.dll",this),
                new FileLockTrigger(localdat_path,fileaccessmode:FileAccess.ReadWrite),
            };

            List<IProcessTrigger> pt_loginwindow_prelogin = new List<IProcessTrigger>
            {
                new ModuleTrigger("dwmapi.dll",this),
                new ModuleTrigger("CoherentUI64.dll",this),
                new SleepTrigger(3000),
                new FileSizeTrigger(@"D:\Guild Wars 2\Gw2-64.tmp",null,0),
                //new WindowDimensionsTrigger(this,0,0,100,100)
            };

            List<IProcessTrigger> pt_loginwindow_authentication = new List<IProcessTrigger>
            {
                new ModuleTrigger("rsaenh.dll",this),
                new ModuleTrigger("DPAPI.dll",this)
            };

            List<IProcessTrigger> pt_loginwindow_pressplay = new List<IProcessTrigger>
            {
                new ModuleTrigger("rsaenh.dll",this)
            };

            List<IProcessTrigger> pt_game_startup = new List<IProcessTrigger>
            {
                new ModuleTrigger("atiu9p64.dll",this)
            };
            List<IProcessTrigger> pt_game_charscreen = new List<IProcessTrigger>
            {
                new ModuleTrigger("icm32.dll",this),
                new FileLockTrigger(localdat_path,positiveflank:false,fileaccessmode:FileAccess.ReadWrite), //Why is there still a filelock, LB solved this O_o?!
            };

            ProcessPipeline pipeline = new ProcessPipeline
            {
                new ProcessState(((GameStatus)1).ToString(), this, pt_selfupdate),
                new ProcessState(((GameStatus)2).ToString(), this, pt_loginwindow_prelogin),
                new ProcessState(((GameStatus)3).ToString(), this, pt_loginwindow_authentication),
                new ProcessState(((GameStatus)4).ToString(), this, pt_loginwindow_pressplay),
                new ProcessState(((GameStatus)5).ToString(), this, pt_game_startup),
                new ProcessState(((GameStatus)6).ToString(), this, pt_game_charscreen)
            };

            pipeline.StateChanged += OnStateChange;
            return pipeline;
        }

        public override bool Start()
        {
            ProcessWatcher prow = new ProcessWatcher(this, CreateStates());
            prow.Run();
            base.Exited += OnExit;
            return base.Start();
        }

        public void OnExit(object sender, EventArgs e)
        {
            gamestatus = GameStatus.none;
            if (new FileSizeTrigger(exe_tmppath, 0, null).IsActive)
            {
                Console.WriteLine("Self updated detected");
                exitstatus = ExitStatus.self_update;
            }
        }

        public async Task<bool> WaitForStateAsynch(GameStatus status)
        {
            while (IsRunning)
            {
                if (gamestatus == status)
                {
                    return true;
                }
                await Task.Delay(10);
            }
            return false;
        }

        public bool WaitForState(GameStatus status)
        {
            while (IsRunning)
            {
                if ((int)gamestatus >= (int)status)
                {
                    return true;
                }
                Thread.Sleep(10);
            }
            return false;
        }
    }

}
