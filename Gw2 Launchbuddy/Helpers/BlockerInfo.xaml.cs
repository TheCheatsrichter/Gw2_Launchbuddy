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
    /// <summary>
    /// Interaktionslogik für BlockerInfo.xaml
    /// </summary>
    public partial class BlockerInfo : Window
    {
        public static bool Done = false;
        private static Action function = null;
        static BlockerInfo blockerinfo;
        static Thread blocker_thread;

        private BlockerInfo()
        {
            InitializeComponent();
        }

        public static void Run(string Title,string Message, Action blockerfunction,bool topmost=true)
        {
            blockerinfo = new BlockerInfo();
            blockerinfo.Title = Title;
            blockerinfo.tb_message.Text = Message;
            function = blockerfunction;
            blockerinfo.Topmost = topmost;
            Done = false;
            blocker_thread = new Thread(new ThreadStart(WaitForFunction));
            blocker_thread.Start();
            blockerinfo.ShowDialog();
            //blockerinfo.Focus();
        }

        private static void WaitForFunction()
        {
            try
            {
                function();
            }catch
            {
                Console.WriteLine("Blockerinfo crashed on function execution");
            }
            
            try
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => blockerinfo.Close()));
                Done = true;
            }
            catch (Exception e) {
            }
        }

        private void bt_cancel_Click(object sender, RoutedEventArgs e)
        {
            blockerinfo.Close();
        }

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
    }
}
