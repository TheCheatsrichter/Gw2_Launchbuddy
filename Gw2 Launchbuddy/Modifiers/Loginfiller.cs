using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2_Launchbuddy.ObjectManagers;

namespace Gw2_Launchbuddy.Modifiers
{
    public static class Loginfiller
    {
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        public static void Login(Account acc, bool clearfields = false)
        {
            ModuleReader.WaitForModule("WINNSI.DLL",acc.Client.Process);

            Process pro = acc.Client.Process;

            //SetForegroundWindow(pro.MainWindowHandle);
            PressKeyDown(Keys.Control | Keys.A,pro);
            PressKeyDown(Keys.Return,pro);
            TypeString(acc.Settings.Email, pro);
            PressKeyDown(Keys.Tab, pro);
            PressKeyDown(Keys.Control | Keys.A, pro);
            PressKeyDown(Keys.Return, pro);
            TypeString(acc.Settings.Password, pro);
            PressKeyDown(Keys.Tab, pro);
            PressKeyDown(Keys.Tab, pro);
            PressKeyDown(Keys.Tab, pro);
            PressKeyDown(Keys.Enter, pro);
            Thread.Sleep(5000);
            PressKeyDown(Keys.Enter, pro);
            PressKeyUp(Keys.Enter, pro);
        }

        private static void PressKeyDown(Keys key, Process pro)
        {
            const uint WM_KEYDOWN = 0x0100;
            PostMessage(pro.MainWindowHandle, WM_KEYDOWN, (int)key, 0);
            Thread.Sleep(50);
        }

        private static void PressKeyUp(Keys key, Process pro)
        {
            const uint WM_KEYUP = 0x0101;
            PostMessage(pro.MainWindowHandle, WM_KEYUP, (int)key, 0);
            Thread.Sleep(50);
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

