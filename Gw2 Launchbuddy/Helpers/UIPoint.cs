using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.Helpers
{
    public class UIPoint
    {
        int x = 0, y = 0;
        double dpiscale = 1;

        public int X { get { return (int)(x * dpiscale); } }
        public int Y { get { return (int)(y * dpiscale); } }

        public int UnscaledX { get { return x; } }
        public int UnscaledY { get { return y; } }
        public double DPIScale { get { return dpiscale; } }

        public UIPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public (int, int) DPIConverted(double DPIScale)
        {
            dpiscale = DPIScale;
            return (X, Y);
        }
    }
}
