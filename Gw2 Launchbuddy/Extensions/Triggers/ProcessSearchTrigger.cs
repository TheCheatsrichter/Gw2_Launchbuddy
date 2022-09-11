using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Gw2_Launchbuddy.Extensions
{
    class ProcessSearchTrigger:IProcessTrigger
    {
        string proname;

        ProcessExtension found_process;
        public ProcessSearchTrigger(string proname)
        {
            this.proname = proname;
        }

        public bool IsActive
        {
            get
            {
                var pros =Process.GetProcessesByName(proname);

                if(pros.Length!=0)
                {
                    found_process = pros[0] as ProcessExtension;
                    return true;
                }
                return false;
            }
        }

    }
}
