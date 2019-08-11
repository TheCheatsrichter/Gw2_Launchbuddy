using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            List<WindowSpacer> spacer_windows = new List<WindowSpacer>();
            foreach(Account acc in AccountManager.Accounts)
            {
                if(acc.Settings.HasWindowConfig)
                {
                    WindowSpacer window = new WindowSpacer(acc);
                    spacer_windows.Add(window);
                    window.Show();
                }
            }

            WindowInfo_Grabber grabber = new WindowInfo_Grabber();
            grabber.ShowDialog();


            foreach (WindowSpacer win in spacer_windows)
            {
                win.Close();
            }

            WindowConfig tmp = grabber.GetInfo();
            this.WinPos_X = tmp.WinPos_X;
            this.WinPos_Y = tmp.WinPos_Y;
            this.Win_Width = tmp.Win_Width;
            this.Win_Height = tmp.Win_Height;
            this.WindowState = tmp.WindowState;
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private RECT? GetWindowDimensions(Process pro)
        {
            RECT rct;
            pro.Refresh();
            bool test = GetWindowRect(new HandleRef(pro,pro.MainWindowHandle), out rct);
            if (!GetWindowRect(new HandleRef(this, pro.MainWindowHandle), out rct))
            {
                MessageBox.Show("ERROR");
                return null;
            }
            return rct;
        }

        public bool? IsConfigured(Process pro)
        {
            RECT? rct = GetWindowDimensions(pro);

            if(rct !=null)
            {
                bool success = true;
                success = rct.Value.Top == WinPos_Y;
                success = rct.Value.Left == WinPos_X;

                success = rct.Value.Right- rct.Value.Left == Win_Width;
                success = rct.Value.Bottom - rct.Value.Top == Win_Height;
                return success;
            }
            else
            {
                return null;
            }
        }
    }
}
