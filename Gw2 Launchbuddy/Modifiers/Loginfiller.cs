using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2_Launchbuddy.Extensions;
using Gw2_Launchbuddy.ObjectManagers;

namespace Gw2_Launchbuddy.Modifiers
{
    public static class Loginfiller
    {
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        /*
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        private static bool UpdateDPIScale(IntPtr mainwindowhandle)
        {
            try
            {
                //TODO: Add user 32 bit dll support when win version < 10
                dpiscale = GetDpiForWindow(mainwindowhandle) / 96;
            }catch
            {
                dpiscale = 1;
                return false;
            }
            
            return true;
        }
        */


        static (int,int) pos_email_tb = (480,430);
        static (int, int) pos_passw_tb = (480, 510);
        static (int, int) pos_login_bt = (100, 600);
        static (int, int) pos_authemail_bt = (290, 550);
        static (int, int) pos_play_bt = (825, 715);

        static float dpiscale = 1;


        public static async void Login(string email,string passwd,GwGameProcess pro, bool clearfields = false)
        {
            //email 480,430
            //paswd 480,510
            //login 100,600
            //2way auth email remind later 290,550
            //playbtn 825,715


            try
            {
                pro.Refresh();
                await pro.WaitForStateAsynch(GwGameProcess.GameStatus.loginwindow_prelogin);
                //SetForegroundWindow(pro.MainWindowHandle);
                Thread.Sleep(1000);
                MouseClickLeft(pro, pos_email_tb);
                for (int i = 0; i < 100; i++) PressKeyDown(Keys.Back, pro, false); //Very unclean method, but modifiers onyl work on focus
                Thread.Sleep(50);
                TypeString(email, pro);
                //PressKeyDown(Keys.Tab, pro);
                MouseClickLeft(pro, pos_passw_tb);
                TypeString(passwd, pro);
                /*
                PressKeyDown(Keys.Tab, pro);
                PressKeyDown(Keys.Tab, pro);
                PressKeyDown(Keys.Tab, pro);
                PressKeyDown(Keys.Enter, pro);
                */
                MouseClickLeft(pro, pos_login_bt);
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show("Could not perform automated login. Gameclient seems to have crashed / be closed before the login data could be filled in." + e.Message);
            }

        }

        public static async void PressLoginButton(Account acc)
        {
            //acc.Client.Process.WaitForExit();
            if(!acc.Client.Process.ReachedState(GwGameProcess.GameStatus.loginwindow_authentication))
            {
                acc.Client.Process.WaitForState(Extensions.GwGameProcess.GameStatus.loginwindow_prelogin);
                if (!acc.Client.Process.HasExited)
                {
                    MouseClickLeft(acc.Client.Process, pos_login_bt);
                }
            }
            Thread.Sleep(1800);

            int retries = 3;
            while(retries >0 && !acc.Client.Process.ReachedState(GwGameProcess.GameStatus.game_startup))
            {
                if (!acc.Client.Process.HasExited)
                {
                    MouseClickLeft(acc.Client.Process, pos_play_bt);

                    if(!acc.Client.Process.WaitForState(GwGameProcess.GameStatus.game_startup, 500))
                    {
                        MouseClickLeft(acc.Client.Process, pos_authemail_bt);
                        Thread.Sleep(100);
                        MouseClickLeft(acc.Client.Process, pos_play_bt);
                    }

                    acc.Client.Process.WaitForState(GwGameProcess.GameStatus.game_startup, 3000);
                    /*
                    PressKeyDown(Keys.Enter, acc.Client.Process);
                    PressKeyUp(Keys.Enter, acc.Client.Process);
                    */
                }
                retries--;
            }

        }

        private static int MakeLParam(int x, int y) => (y << 16) | (x & 0xFFFF);

        private static void PressKeyDown(Keys key, Process pro,bool delay=true)
        {
            const uint WM_KEYDOWN = 0x0100;
            PostMessage(pro.MainWindowHandle, WM_KEYDOWN, (int)key, 0);
            if (delay)Thread.Sleep(50);
        }

        private static void PressKeyUp(Keys key, Process pro, bool delay = true)
        {
            const uint WM_KEYUP = 0x0101;
            PostMessage(pro.MainWindowHandle, WM_KEYUP, (int)key, 0);
            if(delay)Thread.Sleep(50);
        }

        private static void MouseDownLeft(Process pro,int pos_x,int pos_y,bool delay=true)
        {
            const int WM_LBUTTONDOWN = 0x0201;

            PostMessage(pro.MainWindowHandle, WM_LBUTTONDOWN, 1, MakeLParam(pos_x,pos_y));
            if (delay) Thread.Sleep(50);
        }

        private static void MouseUpLeft(Process pro, int pos_x, int pos_y, bool delay = true)
        {
            const int WM_LBUTTONUP = 0x0202;

            PostMessage(pro.MainWindowHandle, WM_LBUTTONUP, 1, MakeLParam(pos_x, pos_y));
            if (delay) Thread.Sleep(50);
        }

        private static void MouseClickLeft(Process pro, int pos_x, int pos_y, bool delay = true)
        {
            pro.Refresh();
            MouseDownLeft(pro, pos_x , pos_y);
            MouseUpLeft(pro, pos_x, pos_y);
            if (delay) Thread.Sleep(50);
        }

        private static void MouseClickLeft(Process pro, (int,int) pos, bool delay = true)
        {
            MouseClickLeft(pro, pos.Item1, pos.Item2, delay);
            if (delay) Thread.Sleep(50);
        }

        private static void PressCMD(Keys key , Process pro)
        {
            const uint WM_COMMAND = 0x0111;
            PostMessage(pro.MainWindowHandle, WM_COMMAND, (int)key, 0);
        }

        private static void TypeString(string input, Process pro)
        {
            const uint WM_CHAR = 0x0102;
            foreach (char letter in input.ToCharArray())
            {
                PostMessage(pro.MainWindowHandle, WM_CHAR, letter, 0);
            }
            Thread.Sleep(50);
        }
    }
}

