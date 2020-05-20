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

        public static void Login(string email,string passwd,Process pro, bool clearfields = false)
        {
            pro.Refresh();
            ModuleReader.WaitForModule("WINNSI.DLL",pro);
            //SetForegroundWindow(pro.MainWindowHandle);
            Thread.Sleep(1000);
            for(int i =0;i<100;i++)PressKeyDown(Keys.Back,pro,false); //Very unclean method, but modifiers onyl work on focus
            Thread.Sleep(50);
            TypeString(email, pro);
            PressKeyDown(Keys.Tab, pro);
            TypeString(passwd, pro);
            PressKeyDown(Keys.Tab, pro);
            PressKeyDown(Keys.Tab, pro);
            PressKeyDown(Keys.Tab, pro);
            PressKeyDown(Keys.Enter, pro);
        }

        public static void PressLoginButton(Account acc)
        {
            ModuleReader.WaitForModule("WINNSI.DLL", acc.Client.Process);
            Thread.Sleep(1500);
            PressKeyDown(Keys.Enter,acc.Client.Process);
            PressKeyUp(Keys.Enter, acc.Client.Process);
        }

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

