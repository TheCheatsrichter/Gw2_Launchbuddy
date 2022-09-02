using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gw2_Launchbuddy.Extensions
{
    class FileSizeTrigger : IProcessTrigger
    {
        int? lowerlimit = null;
        int? upperlimit = null;
        string filepath;

        public FileSizeTrigger(string filepath, int? lowerlimit, int? upperlimit)
        {
            this.lowerlimit = lowerlimit;
            this.upperlimit = upperlimit;

            this.filepath = filepath;
        }

        public bool IsActive
        {
            get
            {
                if (!File.Exists(filepath)) return false;
                FileInfo info = new FileInfo(filepath);
                bool isinlimit = true;

                if(lowerlimit!=null)
                {
                    if (info.Length <= lowerlimit) isinlimit = false;
                }

                if(upperlimit!=null)
                {
                    if (info.Length > upperlimit) isinlimit = false;
                }
                //Console.WriteLine("Filesizetrigger size:" + info.Length + " Triggerstate:" + isinlimit);
                return isinlimit;
            }
        }

    }
}
