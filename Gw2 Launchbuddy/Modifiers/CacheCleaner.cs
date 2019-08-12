using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2_Launchbuddy.ObjectManagers;
using System.IO;

namespace Gw2_Launchbuddy.Modifiers
{
    public static class CacheCleaner
    {
        public static void Clean()
        {
            List<string> CacheFolders = new List<string>();
            CacheFolders.Clear();
            string path = EnviromentManager.GwCacheFoldersPath;
            if (Directory.Exists(path))
            {
                string[] entries = Directory.GetDirectories(path);
                foreach(string entry in entries.Where<string>(a=>a.Contains("gw2cache")))
                {
                    if(DateTime.Now.Subtract(File.GetLastAccessTime(entry))> new TimeSpan(7,0,0,0,0))
                    {
                        try
                        {
                            Directory.Delete(entry,true);
                        }
                        catch
                        {
                            
                        }
                    }
                }
            }
        }
    }
}
