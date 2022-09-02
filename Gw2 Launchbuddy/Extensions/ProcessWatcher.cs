using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Gw2_Launchbuddy.Extensions
{
    class ProcessWatcher
    {
        private static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);

        ProcessExtension pro;
        Thread th_watcher;
        ProcessPipeline pipeline;
        bool Exited = false;
        public ProcessWatcher(ProcessExtension pro,ProcessPipeline pipeline)
        {
            this.pro = pro;
            this.pipeline = pipeline;
            pro.Exited += OnProcessExit;
            th_watcher = new Thread(Th_Watcher);
        }

        void OnProcessExit(object sender,EventArgs e)
        {
            Suspend();
        }

        public void Run()
        {
            th_watcher.Start();
            Resume(); // Resume same pro if it was closed before
        }

        public void Suspend()
        {
            waitHandle.Reset();
        }

        public void Resume()
        {
            waitHandle.Set();
        }

        public void Th_Watcher()
        {
            while(!pro.IsRunning)
            {
                Thread.Sleep(10);
            }

            while (!pipeline.Done())
            {
                waitHandle.WaitOne();
                var state = pipeline.CurrentState();

                if (state.GetStatus())
                {
                    Console.WriteLine($"Process reached state: {pipeline.CurrentState().Name}");
                    pipeline.Next();
                }
            }

        }
    }

    class ProcessPipeline : List<ProcessState>
    {
        int _index = 0;

        int index
        {
            get
            {
                return _index;
            }
            set
            {
                StateChanged?.Invoke(CurrentState(), null);
                _index = value;
                
            }
        }
        public int StateIndex
        {
            get { return index; }
        }

        public ProcessState CurrentState()
        {
            if (index < base.Count)
            {
                return base[index];
            }
            return null;
        }

        public ProcessState Next()
        {
            if (index == Count - 1)
            {
                index++;
            }
            if (index < base.Count)
            {
                index += 1;
                return base[index];
            }
            return null;
        }

        public void Reset()
        {
            index = 0;
        }

        public bool Done()
        {
            return CurrentState() == null;
        }

        public event EventHandler StateChanged;
    }

    class ProcessState
    {
        ProcessExtension pro;
        List<IProcessTrigger> triggers;
        string name;

        public string Name
        {
            get
            {
                return name;
            }
        }
        public ProcessState(string name, ProcessExtension pro,List<IProcessTrigger> triggers)
        {
            this.name = name;
            this.pro = pro;
            this.triggers = triggers;
        }

        public bool GetStatus()
        {
            foreach(var trigger in triggers)
            {
                if (!trigger.IsActive) return false;
            }
            return true;
        }

    }
    interface IProcessTrigger
    {
        bool IsActive { get; }
    }
}
