using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gw2_Launchbuddy.Helpers
{
    public static class EmbeddedBlockerInfo
    {
        public static bool Done = false;
        private static Action function = null;
        static Thread blocker_thread;

        static Grid Blockergrid;
        static Button Cancelbutton;
        static TextBlock Messageblock;


        public static void ShowBlocker (Grid blockergrid,Button cancelbutton,TextBlock messageblock, string Message, Action blockerfunction, bool topmost = true)
        {
            Blockergrid = blockergrid;
            Cancelbutton = cancelbutton;
            Messageblock = messageblock;

            blockergrid.Visibility = Visibility.Visible;
            messageblock.Text = Message;
            function = blockerfunction;
            Done = false;
            blocker_thread = new Thread(new ThreadStart(WaitForFunction));
            blocker_thread.Start();
        }

        public static void CloseBlocker()
        {
            try
            {
                Console.WriteLine("Aborting wait thread.");
                blocker_thread.Suspend();
                blocker_thread.Abort();
                blocker_thread = null;
                function = null;
                Thread.Sleep(50);
                Console.WriteLine("Wait Thread aborted");
            }
            catch
            {

            }
            finally
            {
                Blockergrid.Visibility = Visibility.Collapsed;
            }
        }

        private static void WaitForFunction()
        {
            try
            {
                function();
            }
            catch
            {
                Console.WriteLine("Blockerinfo crashed on function execution");
            }

            try
            {
                Done = true;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => CloseBlocker()));
                
            }
            catch (Exception e)
            {
            }
        }

        private static void bt_cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseBlocker();
        }

        /*
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Console.WriteLine("Aborting wait thread.");
                blocker_thread.Suspend();
                blocker_thread.Abort();
                blocker_thread = null;
                function = null;
                Thread.Sleep(50);
                Console.WriteLine("Wait Thread aborted");
            }
            catch
            {

            }
            finally
            {
                GC.Collect();
            }
        }
        */

    }
}
