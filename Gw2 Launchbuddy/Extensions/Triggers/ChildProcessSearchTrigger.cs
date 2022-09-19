using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.Extensions
{
    public class ChildProcessSearchTrigger:IProcessTrigger
    {
        string proname;
        Process parent;

        public Process found_process;
        public ChildProcessSearchTrigger(string proname,Process parent)
        {
            this.proname = proname;
            this.parent = parent;
        }

        public bool IsActive
        {
            get
            {
                var childs = GetChildProcesses(parent);

                foreach(var pro in childs)
                {
                    if(pro.ProcessName==proname)
                    {
                        found_process = pro;
                        return true;
                    }
                }

                return false;
            }
        }

        private IEnumerable<Process> GetChildProcesses(Process process)
        {
            List<Process> children = new List<Process>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher(String.Format("Select * From Win32_Process Where ParentProcessID={0}", process.Id));

            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }

            return children;
        }
    }
}
