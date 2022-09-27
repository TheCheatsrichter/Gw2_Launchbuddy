using Gw2_Launchbuddy.Extensions;
using Gw2_Launchbuddy.Modifiers;
using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gw2_Launchbuddy.Helpers
{
    /// <summary>
    /// Interaktionslogik für UITraceBlocker.xaml
    /// </summary>
    public partial class UIStatusbar : Window
    {
        Thread th_trace;
        bool isdone = false;
        string infoprefix = "Powered by Launchbuddy";
        Client client;
        public UIStatusbar(Client client,Func<bool> defofdone,UIPoint anchorpoint)
        {
            InitializeComponent();
            this.client = client;
            Visibility = Visibility.Collapsed;

            th_trace = new Thread(()=>Th_TraceProcess(client,defofdone,anchorpoint));
            client.Process.Exited += OnClientClose;
            th_trace.Start();
        }

        void OnClientClose(object sender,EventArgs e)
        {
            
            isdone = true;
            th_trace.Abort();
            this.Dispatcher.Invoke(() => Close());
        }


        void Th_TraceProcess(Client client, Func<bool> defofdone, UIPoint anchor)
        {

            while (!isdone)
            {
                try
                {
                    
                    this.Dispatcher.Invoke(() =>
                    {
                        //RevertScaleDPI();
                        anchor.DPIConverted(WindowUtil.GetWindowDPIFactor(client.Process.MainWindowHandle));
                        TraceProcess(client.Process.MainWindowHandle, anchor);
                        SetVisibility(client.Process.MainWindowHandle);
                        RefreshHeader(client);
                    });
                    isdone = defofdone();
                    Thread.Sleep(5);
                }
                catch
                {
                    isdone = true;
                }

            }
            try
            {
                this.Dispatcher.Invoke(() => Close());
            }catch
            {

            }
            
        }

        public void RefreshHeader(Client client)
        {
            try
            {
                tb_infotext.Text = infoprefix + "\t " + client.account.Nickname;

                lb_gameprogress.Content = client.LaunchProgress + "%";
                pb_gameprogress.Value = client.LaunchProgress;
            }
            catch
            {
                tb_infotext.Text = infoprefix;
            }
        }

        public void SetVisibility(IntPtr winhandle)
        {
            if (WindowUtil.HasFocus(winhandle))
            {
                Visibility = Visibility.Visible;
                Topmost = true;
            }
            else
            {
                if (!WindowUtil.HasFocus(new WindowInteropHelper(this).Handle))
                {
                    Visibility = Visibility.Hidden;
                    Topmost = false;
                    Console.WriteLine("Hidden");
                }
            }
        }

        /*
        public void ScaleToProcess(Process pro)
        {
            if (pro.HasExited) return;
            var winsize = WindowUtil.GetDimensions(pro);
            Width = (winsize.right - winsize.left)/dpifactor_x;
            Height = (winsize.bottom - winsize.top -150)/dpifactor_y ; //Cutoff some of the invisible stuff 
        }
        */

        /*
        public void RevertScaleDPI()
        {
            Width = MaxWidth/dpifactor_x;
            Height = MaxHeight/dpifactor_y;
        }
        */

        public void TraceProcess(IntPtr winhandle,UIPoint ui_offset)
        {
            try
            {
                var winsize = WindowUtil.GetDimensions(winhandle);
                //Console.WriteLine($"X:{winsize.left} Y:{winsize.top} Width:{winsize.right - winsize.left} Height:{winsize.bottom - winsize.top}");

                //GetDimensions returns position * dpi scale
                if (ui_offset != null)
                {
                    Left = (winsize.left / ui_offset.DPIScale) + ui_offset.UnscaledX;
                    Top = (winsize.top / ui_offset.DPIScale) + ui_offset.UnscaledY;
                }
            }catch
            {
                this.Close();
            }


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isdone = true;
            client.Process.Exited -= OnClientClose;
        }

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
