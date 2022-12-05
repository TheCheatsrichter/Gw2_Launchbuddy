using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Gw2_Launchbuddy.Extensions
{
    class WindowDimensionsTrigger:IProcessTrigger
    {
        //ONLY CAN BE USED IF PROCESS HANDLE HAS BEEN REGISTERED BY WINDOWS. USUALLY TAKES PROCESS.START + 2 secs

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rectangle rect);

        Process pro;
        int width_lowerlimit, width_upperlimit, height_lowerlimit, height_upperlimit;
        public WindowDimensionsTrigger(Process pro,int width_lowerlimit, int width_upperlimit,int height_lowerlimit, int height_upperlimit)
        {
            this.pro = pro;
            this.width_lowerlimit = width_lowerlimit;
            this.width_upperlimit = width_upperlimit;
            this.height_lowerlimit = height_lowerlimit;
            this.height_upperlimit = height_upperlimit;
        }

        public bool IsActive
        {
            get
            {
                pro = Process.GetProcessById(pro.Id);
                pro.Refresh();
                Rectangle windowrect,real_windowrect;
                IntPtr hwndMain = pro.MainWindowHandle;
                GetWindowRect(hwndMain, out windowrect);

                //Gw2 Loginclient uses Width- Position as width
                Console.WriteLine($"Width:{windowrect.Width} height:{windowrect.Height} Posx:{windowrect.X}, Posy:{windowrect.Y} Width-Posx:{windowrect.Width-windowrect.X} Height-Posy:{windowrect.Height-windowrect.Y}");

                real_windowrect = new Rectangle(0,0, windowrect.Width - windowrect.X, windowrect.Height - windowrect.Y);

                bool isvalid = true;
                if(real_windowrect.Size.Width < width_lowerlimit) isvalid=false;
                if(real_windowrect.Size.Width > width_upperlimit) isvalid = false;
                if(real_windowrect.Size.Height > height_upperlimit) isvalid = false;
                if(real_windowrect.Size.Height < height_lowerlimit) isvalid = false;

                return isvalid;
            }
        }
    }
}
