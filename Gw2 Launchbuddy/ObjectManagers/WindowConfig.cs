using System;
using System.Runtime.InteropServices;
using System.Windows;
using Gw2_Launchbuddy.Helpers;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public class WindowConfig
    {
        private int posx;
        private int posy;
        private int width;
        private int height;
        private WindowState state;

        public int WinPos_X { set { posx = value; } get { return posx; } }
        public int WinPos_Y { set { posy = value; } get { return posy; } }
        public int Win_Width { set { width = value; } get { return width; } }
        public int Win_Height { set { height = value; } get { return height; } }

        public WindowState WindowState { set { state = value; } get { return state; } }

        public void Configure_Window()
        {
            WindowInfo_Grabber grabber = new WindowInfo_Grabber();
            grabber.ShowDialog();
            WindowConfig tmp = grabber.GetInfo();
            this.WinPos_X = tmp.WinPos_X;
            this.WinPos_Y = tmp.WinPos_Y;
            this.Win_Width = tmp.Win_Width;
            this.Win_Height = tmp.Win_Height;
            this.WindowState = tmp.WindowState;
        }
    }
}
