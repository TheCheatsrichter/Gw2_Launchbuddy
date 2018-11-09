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

namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>
    /// 
    static public class LoadingWindow
    {
        private static string message = null;

        static private Thread loadingui;

        public static void Start(string msg="")
        {
            try
            {


                if (loadingui != null) loadingui.Abort();
                if (msg != "") message = msg;

                loadingui = new Thread(new ParameterizedThreadStart((message) =>
                 {
                     LoadingScreen tmpWindow = new LoadingScreen();
                     tmpWindow.lb_msg.Content = message;
                     tmpWindow.ShowDialog();
                     System.Windows.Threading.Dispatcher.Run();
                 }));
                loadingui.SetApartmentState(ApartmentState.STA);
                loadingui.IsBackground = true;
                loadingui.Start(message);
            }
            catch
            {

            }
        }

        public static void Stop ()
        {
            try
            {
                loadingui.Abort();
            }
            catch
            {

            }
        }

    }


    public partial class LoadingScreen : Window
    {
        

        public LoadingScreen(string msg = null)
        {
            try
            {
                InitializeComponent();
                lb_msg.Content = msg;
            }
            catch { }
        }

    }
}
