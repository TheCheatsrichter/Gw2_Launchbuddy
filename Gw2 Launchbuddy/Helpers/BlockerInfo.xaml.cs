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

        private BlockerInfo()
        {
            InitializeComponent();
        }

        public static void Run(string Title,string Message, Action blockerfunction)
        {
            blockerinfo = new BlockerInfo();
            blockerinfo.Title = Title;
            blockerinfo.tb_message.Text = Message;
            function = blockerfunction;
            Done = false;
            Thread blocker_thread = new Thread(new ThreadStart(WaitForFunction));
            blocker_thread.Start();
            blockerinfo.ShowDialog();
        }

        private static void WaitForFunction()
        {
            function();
            try
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => blockerinfo.Close()));
                Done = true;
            }
            catch { } 
        }

        private void bt_cancel_Click(object sender, RoutedEventArgs e)
        {
            blockerinfo.Close();
        }
    }
}
