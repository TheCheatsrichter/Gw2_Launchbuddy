using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        static extern int GetDpiForWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("user32.dll")]
        static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, PrintWindowFlags flags);

        enum PrintWindowFlags : uint
        {
            PW_RENDERFULLCONTENT = 0x00000002
        }

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

        public static double GetWindowDPIFactor(IntPtr winhandle)
        {
            if (System.Environment.OSVersion.Version < new Version(10,0, 1607)) return 1;

            if (winhandle == IntPtr.Zero)
            {
                return 1;
            }
            return (double)GetDpiForWindow(winhandle) / 96;
        }

        public static bool IsVisible(double x, double y)
        {
            return IsVisible((int)x, (int)y);
        }


        public static bool Focus(Process pro)
        {
            pro.Refresh();
            if (pro.HasExited) return false;
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
            } catch
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

        public static void ScaleTo(Process pro, int width, int height)
        {
            if (pro.HasExited) return;
            if (pro.MainWindowHandle == IntPtr.Zero) return;
            pro.Refresh();
            ScaleTo(pro.MainWindowHandle, width, height);
        }

        public static void ScaleTo(IntPtr handle, int width, int height)
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
#if DEBUG
            //Console.WriteLine($"{winhandle} \t{GetForegroundWindow()}");
#endif
            return winhandle == GetForegroundWindow();
        }

        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            RECT rc = new RECT();
            GetWindowRect(hwnd, ref rc);

            Bitmap bmp = new Bitmap(rc.right - rc.left, rc.bottom - rc.top, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = gfxBmp.GetHdc();
            bool succeeded = PrintWindow(hwnd, hdcBitmap, PrintWindowFlags.PW_RENDERFULLCONTENT);
            gfxBmp.ReleaseHdc(hdcBitmap);
            if (!succeeded)
            {
                gfxBmp.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(Point.Empty, bmp.Size));
            }
            IntPtr hRgn = CreateRectRgn(0, 0, 0, 0);
            GetWindowRgn(hwnd, hRgn);
            Region region = Region.FromHrgn(hRgn);
            if (!region.IsEmpty(gfxBmp))
            {
                gfxBmp.ExcludeClip(region);
                gfxBmp.Clear(Color.Transparent);
            }
            gfxBmp.Dispose();
            return bmp;
        }
    }

}
