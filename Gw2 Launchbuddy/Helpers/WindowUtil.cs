using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.Helpers
{
    public static class WindowUtil
    {
        public static bool IsVisible(int x, int y)
        {
            foreach (System.Windows.Forms.Screen scrn in System.Windows.Forms.Screen.AllScreens)
            {
                // You may prefer Intersects(), rather than Contains()
                if (scrn.Bounds.Contains(x, y))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsVisible(double x, double y)
        {
            return IsVisible((int)x, (int)y);
        }
    }
}
