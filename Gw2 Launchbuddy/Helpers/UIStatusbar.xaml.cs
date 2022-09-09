using Gw2_Launchbuddy.Extensions;
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
        double dpifactor_x = 1, dpifactor_y = 1;
        public UIStatusbar(Client client,Func<bool> defofdone,Rect? ui_offset=null)
        {
            InitializeComponent();
            this.client = client;
            th_trace = new Thread(()=>Th_TraceProcess(client,defofdone,ui_offset));
            client.Process.Exited += OnClientClose;
            th_trace.Start();
        }

        void OnClientClose(object sender,EventArgs e)
        {
            
            isdone = true;
            this.Dispatcher.Invoke(() => Close());
        }

        void CalcDpi(ref double x, ref double y)
        {
            dpifactor_x = System.Windows.PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            dpifactor_y = System.Windows.PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M22;
        }

        void Th_TraceProcess(Client client, Func<bool> defofdone, Rect? ui_offset)
        {
            while (!isdone)
            {
                try
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        CalcDpi(ref dpifactor_x, ref dpifactor_y);
                        RevertScaleDPI();
                        TraceProcess(client.Process, ui_offset);
                        SetVisibility(client.Process);
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
            }
            catch
            {
                tb_infotext.Text = infoprefix;
            }
        }

        public void SetVisibility(Process pro)
        {
            if(WindowUtil.HasFocus(pro))
            {
                Visibility = Visibility.Visible;
            }
            else
            {
                if (!WindowUtil.HasFocus(new WindowInteropHelper(this).Handle))
                {
                    Visibility = Visibility.Collapsed;
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

        public void RevertScaleDPI()
        {
            Width = MaxWidth/dpifactor_x;
            Height = MaxHeight/dpifactor_y;
        }

        public void TraceProcess(Process pro,Rect? ui_offset)
        {
            if (pro.HasExited) return;
            var winsize = WindowUtil.GetDimensions(pro);
            //Console.WriteLine($"X:{winsize.left} Y:{winsize.top} Width:{winsize.right - winsize.left} Height:{winsize.bottom - winsize.top}");

            if(ui_offset!=null)
            {
                var ui_off = (Rect)ui_offset;
                Left = (winsize.left) / (dpifactor_x) + (ui_off.Left/dpifactor_x );
                Top = (winsize.top) / (dpifactor_y) + (ui_off.Top / dpifactor_y);
            }else
            {
                Left = (winsize.left) / (dpifactor_x);
                Top = (winsize.top) / (dpifactor_y);
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
