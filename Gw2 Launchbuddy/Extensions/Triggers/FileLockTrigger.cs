using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gw2_Launchbuddy.Extensions
{
    class FileLockTrigger : IProcessTrigger
    {
        string filepath;
        bool positiveflank=true;

        FileShare filesharemode;
        FileAccess fileaccessmode;
        public FileLockTrigger(string filepath,bool positiveflank=true,FileShare filesharemode=FileShare.ReadWrite,FileAccess fileaccessmode= FileAccess.Read)
        {
            this.filepath = filepath;
            this.positiveflank = positiveflank;
            this.filesharemode = filesharemode;
            this.fileaccessmode = fileaccessmode;
        }

        public bool IsActive
        {
            get
            {
                bool islocked = false;
                try
                {
                    var file = new FileInfo(filepath);
                    using (FileStream stream = file.Open(FileMode.Open, fileaccessmode, filesharemode))
                    {
                        stream.Close();
                    }
                }
                catch (IOException)
                {
                    islocked = true;
                }
                if (!positiveflank) return !islocked;
                return islocked;
            }
        }
    }
}
