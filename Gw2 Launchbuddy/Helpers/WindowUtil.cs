﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.Helpers
{
    public static class WindowUtil
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

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


        public static bool Focus(Process pro)
        {
            pro.Refresh();
            return Focus(pro.MainWindowHandle);
        }
        public static bool Focus(IntPtr winhandle)
        {
            if (winhandle == IntPtr.Zero) return false;

            try
            {
                return SetForegroundWindow(winhandle);
            }
            catch
            {
                return false;
            }
        }

        public static bool Maximize(IntPtr winhandle)
        {
            if (winhandle == IntPtr.Zero) return false;
            try
            {
                return ShowWindow(winhandle, 0x03);
            }
            catch
            {
                return false;
            }
        }

        public static bool Maximize(Process pro)
        {
            pro.Refresh();
            return Maximize(pro.MainWindowHandle);
        }

        public static bool Minimize(IntPtr winhandle)
        {
            if (winhandle == IntPtr.Zero) return false;
            try
            {
                return ShowWindow(winhandle, 0x06);
            }
            catch
            {
                return false;
            }
        }

        public static bool Minimize(Process pro)
        {
            pro.Refresh();
            return Minimize(pro.MainWindowHandle);
        }

        public static bool MoveTo(Process pro, int posx, int posy)
        {
            if (pro.HasExited || pro.MainWindowHandle == IntPtr.Zero) return false;
            pro.Refresh();
            try
            {
                MoveTo(pro.MainWindowHandle, posx, posy);
                return true;
            }catch
            {
                return false;
            }
            
        }


        public static void MoveTo(IntPtr handle, int posx, int posy)
        {
            RECT Rect = new RECT();
            if (GetWindowRect(handle, ref Rect))
                MoveWindow(handle, posx, posy, Rect.right - Rect.left, Rect.bottom - Rect.top, true);
        }

        public static void ScaleTo(Process pro,int width, int height)
        {
            if (pro.HasExited || pro.MainWindowHandle == IntPtr.Zero) return;
            pro.Refresh();
            ScaleTo(pro.MainWindowHandle,width,height);
        }

        public static void ScaleTo(IntPtr handle, int width,int height)
        {
            RECT Rect = new RECT();
            if (GetWindowRect(handle, ref Rect))
                MoveWindow(handle, Rect.left, Rect.top, width, height, true);
        }

        public static RECT GetDimensions(IntPtr handle)
        {
            RECT Rect = new RECT();
            GetWindowRect(handle, ref Rect);
            return Rect;
        }

        public static RECT GetDimensions(Process pro)
        {
            pro.Refresh();
            return GetDimensions(pro.MainWindowHandle);
        }

        public static bool HasFocus(Process pro)
        {
            pro.Refresh();
            try
            {
                return HasFocus(pro.MainWindowHandle);
            }
            catch
            {
                return false;
            }
            
        }

        public static bool HasFocus(IntPtr winhandle)
        {
            return winhandle == GetForegroundWindow();
        }
    }
}