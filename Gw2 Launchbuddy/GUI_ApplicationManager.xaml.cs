using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading;
using System.Data;
using System.Runtime.InteropServices;


namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for GUI_ApplicationManager.xaml
    /// </summary>
    public partial class GUI_ApplicationManager : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
[return: MarshalAs(UnmanagedType.Bool)]
private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_MAXIMIZE = 3;

        public GUI_ApplicationManager()
        {
            InitializeComponent();
            Thread updatelist = new Thread(UpdateProAccs);
            updatelist.IsBackground = true;
            updatelist.Start();
        }

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lv_instances.ItemsSource = Globals.LinkedAccs;
        }

        private void lv_gfx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Globals.LinkedAccs.ToString();
            ProAccBinding bind = sender as ProAccBinding;
        }

        public void UpdateProAccs()
        {
            while(true)
            {
                Process[] pros = Process.GetProcessesByName(Globals.exename);
                foreach (ProAccBinding proacc in Globals.LinkedAccs)
                {
                    if (pros.Where(x => x.Id == proacc.pro.Id) == null)
                    {
                        Globals.LinkedAccs.Remove(proacc);
                    }
                }
                Thread.Sleep(5000);
            }
        }

        private void bt_closeinstance_Click(object sender, RoutedEventArgs e)
        {
            ProAccBinding selinstance = (sender as Button).DataContext as ProAccBinding;
            try{
                selinstance.pro.Kill();
            }
            catch { }
            Globals.LinkedAccs.Remove(selinstance);
        }

        private void bt_maxmin_Click(object sender, RoutedEventArgs e)
        {
            ProAccBinding selinstance = (sender as Button).DataContext as ProAccBinding;
            IntPtr hwndMain = selinstance.pro.MainWindowHandle;
            if (IsIconic(hwndMain))
            {
                ShowWindow(hwndMain, SW_MAXIMIZE);
            }
        }
    }
}
