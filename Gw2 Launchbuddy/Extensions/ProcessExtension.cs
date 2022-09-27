using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gw2_Launchbuddy.Helpers;
using Gw2_Launchbuddy.ObjectManagers;

namespace Gw2_Launchbuddy.Extensions
{
    public class ProcessExtension
    {
        bool started = false;
        int timeout_close = 5000;
        private Process pro;

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

        public ProcessExtension(Process pro,bool allreadystarted=false)
        {
            this.pro = pro;
            started = allreadystarted;
        }

        public Process GetProcess() { return pro; }

        public double DPIFactor { get { return WindowUtil.GetWindowDPIFactor(MainWindowHandle); } }

        #region Facade

        //Redirect calls to Process. This enables "normal" usage + full control over accesses. Otherwise finding process via ID wont work.
        public int Id { get { return pro.Id; } }
        public bool HasExited { get { return pro.HasExited; } }
        public DateTime ExitTime { get { return pro.ExitTime; } }
        public DateTime StartTime { get { return pro.StartTime; } }
        public bool EnableRaisingEvents { set { pro.EnableRaisingEvents = value; } get { return pro.EnableRaisingEvents; } }
        public int ExitCode { get { return pro.ExitCode; } }
        public bool CloseMainWindow(){ return pro.CloseMainWindow();}
        public bool WaitForExit(int ms) { return pro.WaitForExit(ms); }
        public void WaitForExit() { pro.WaitForExit(); }
        public void Refresh() => pro.Refresh();
        public IntPtr MainWindowHandle { get {
                //Handling issue https://stackoverflow.com/questions/60342879/process-mainwindowhandle-is-non-zero-in-net-framework-but-zero-in-net-core-unl
                int i = 0;
                if (HasExited) return IntPtr.Zero;
                while (!HasExited)
                {
                    if (!(Process.GetProcessById(pro.Id).MainWindowHandle == IntPtr.Zero))
                    {
                        break;
                    }
                    Thread.Sleep(50);
                    if(i>=200)
                    {
                        throw new Exception("MainWindowhandle timeout was reached");
                    }
                    i++;
                }
                try
                {
                    pro.Refresh();
                    return pro.MainWindowHandle;
                }
                catch
                {
                    return IntPtr.Zero;
                }
            } }
        public EventHandler Exited;
        public ProcessThreadCollection Threads { get { return pro.Threads; } }
        public ProcessStartInfo StartInfo { get { return pro.StartInfo; } set { pro.StartInfo = value; } }
        public ProcessPriorityClass PriorityClass { get { return pro.PriorityClass; } set { pro.PriorityClass = value; } }
        public string ProcessName { get { return pro.ProcessName; } }

        #endregion


        public bool MigrateProcess(Process target)
        {
            started = true;
            pro = target;
            pro.EnableRaisingEvents = true;
            pro.Exited += OnExitedSelf;
            return true;
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

            pro.Exited += OnExitedSelf;
            return pro.Start();
        }

        public virtual void OnExitedSelf(object sender, EventArgs e)
        {
            if (exitstatus == ExitStatus.self_update) return;
            if (!HasExited) return;

            switch (ExitCode)
            {
                case 0:
                    exitstatus = ExitStatus.self_close;
                    break;
                default:
                    exitstatus = ExitStatus.self_crash;
                    break;
            }
            Exited?.Invoke(sender,e);
        }

        public virtual bool Stop()
        {
            if (HasExited)
            {
                return false;
            }
            pro.Exited -= OnExitedSelf;
            exitstatus = ExitStatus.manual_mainwindow;
            if (!CloseMainWindow())
            {
                WaitForExit(timeout_close);
                if (!HasExited)
                {
                    pro.Close();
                    WaitForExit(timeout_close);
                    exitstatus = ExitStatus.manual_close;
                    if (!HasExited)
                    {
                        pro.Kill();
                        exitstatus = ExitStatus.manual_kill;
                        Refresh();
                        WaitForExit(timeout_close);
                        if (!HasExited)
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
            var inputBytes = Encoding.ASCII.GetBytes(this.StartTime.ToString() + Id.ToString());
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
        public EventHandler GameStatusChanged;
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

        public GwGameProcess (Process pro):base(pro)
        {

        }

        public GwGameProcess(Process pro,bool started=false) : base(pro,started)
        {

        }

        public void OnStateChange(object sender, EventArgs e)
        {
            base.Refresh();
            if (base.HasExited) return;
            IntPtr hwndMain = base.MainWindowHandle;

            Enum.TryParse((sender as ProcessState).Name, out gamestatus);
            GameStatusChanged?.Invoke(sender,e);
            //Console.WriteLine("EXTERNAL: State changed to " + (sender as ProcessState).Name);
        }

        public GameStatus Status
        {
            get { return gamestatus; }
        }

        public bool ReachedState(GameStatus state)
        {
            return (int)gamestatus >= (int)state;
        }

        ProcessPipeline CreateStates()
        {

            List<IProcessTrigger> pt_selfupdate = new List<IProcessTrigger>
            {
                new ModuleTrigger("VERSION.dll",this),
                new ModuleTrigger("WINMM.dll",this),
                new FileLockTrigger(EnviromentManager.GwLocaldatPath,fileaccessmode:FileAccess.ReadWrite),
            };

            List<IProcessTrigger> pt_loginwindow_prelogin = new List<IProcessTrigger>
            {
                new ModuleTrigger("CoherentUI64.dll",this),
                new SleepTrigger(2500),
                new FileSizeTrigger(EnviromentManager.GwClientTmpPath,null,0),
                //new WindowDimensionsTrigger(this,0,0,100,100)
            };

            List<IProcessTrigger> pt_loginwindow_authentication = new List<IProcessTrigger>
            {
                new ModuleTrigger("DPAPI.dll",this),
                new SleepTrigger(700), //Needs better trigger in the future, currently unknown delta between pre login and authentication pending
            };

            List<IProcessTrigger> pt_loginwindow_pressplay = new List<IProcessTrigger>
            {
                new ModuleTrigger("DPAPI.dll",this)
            };

            List<IProcessTrigger> pt_game_startup = new List<IProcessTrigger>
            {
                new ModuleTrigger("WINSTA.dll",this)
            };
            List<IProcessTrigger> pt_game_charscreen = new List<IProcessTrigger>
            {
                new ModuleTrigger("icm32.dll",this),
                new FileLockTrigger(EnviromentManager.GwLocaldatPath,positiveflank:false,fileaccessmode:FileAccess.ReadWrite),
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
            Exited += OnExit;
            return base.Start();
        }

        public void OnExit(object sender, EventArgs e)
        {
            gamestatus = GameStatus.none;
            if (new FileSizeTrigger(EnviromentManager.GwClientTmpPath, 0, null).IsActive)
            {
                Console.WriteLine("Self updated detected");
                exitstatus = ExitStatus.self_update;
            }
        }

        public async Task<bool> WaitForStateAsynch(GameStatus status, int timeout = -1)
        {
            DateTime waitstart = DateTime.Now;
            while (IsRunning)
            {
                if (gamestatus == status)
                {
                    return true;
                }
                if (timeout > 0)
                {
                    if ((DateTime.Now - waitstart).TotalMilliseconds > timeout)
                    {
                        return false;
                    }
                }
                await Task.Delay(10);
            }
            return false;
        }

        public bool WaitForState(GameStatus status,int timeout=-1)
        {
            DateTime waitstart = DateTime.Now;

            while (IsRunning)
            {
                if (ReachedState(status))
                {
                    return true;
                }
                if(timeout>0)
                {
                    if((DateTime.Now- waitstart).TotalMilliseconds > timeout)
                    {
                        return false;
                    }
                }

                Thread.Sleep(10);
            }
            return false;
        }
    }

    public class SteamGwGameProcess:GwGameProcess
    {
        Process steamprocess;
        public SteamGwGameProcess(Process pro,Process steamprocess) :base(pro)
        {
            this.steamprocess = steamprocess;
        }

        public override bool Start()
        {
            steamprocess.Start();

            Action waitforgame = () =>
            {
                while (ClientManager.SearchForeignClients(true).Count<=0)
                {
                    Thread.Sleep(10);
                }
            };
            BlockerInfo.Run("Waiting for steam","Launchbuddy is waiting for steam to launch gw2",waitforgame);

            if(!BlockerInfo.Done)
            {
                return false;
            }

            return MigrateProcess(ClientManager.SearchForeignClients(true)[0]);
        }
    }

}
