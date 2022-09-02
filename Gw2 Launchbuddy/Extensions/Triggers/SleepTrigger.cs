using System;
using System.Collections.Generic;
using System.Text;

namespace Gw2_Launchbuddy.Extensions
{
    class SleepTrigger:IProcessTrigger
    {
        DateTime starttime;
        int ms_seconds = 0;

        public SleepTrigger(int waittimeinms)
        {
            this.ms_seconds = waittimeinms;
        }
        public bool IsActive
        {
            get
            {
                if (starttime == DateTime.MinValue)
                {
                    starttime = DateTime.Now;
                    return false;
                }
                return (DateTime.Now - starttime).TotalMilliseconds > ms_seconds;
            }
        }
    }
}
