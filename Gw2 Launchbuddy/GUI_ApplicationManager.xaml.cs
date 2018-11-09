using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for GUI_ApplicationManager.xaml
    /// </summary>
    public partial class GUI_ApplicationManager : Window
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public GUI_ApplicationManager()
        {
            InitializeComponent();
            this.Left = Properties.Settings.Default.instance_win_X;
            this.Top = Properties.Settings.Default.instance_win_Y;
        }

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lv_instances.ItemsSource = AccountClientManager.AccountClientCollection;
        }

        private void lv_gfx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lv_instances.SelectedIndex >= 0)
                {
                    AccountClient selinstance = lv_instances.SelectedItem as AccountClient;
                    IntPtr hwndMain = ((Client)selinstance.Client).MainWindowHandle;
                    SetForegroundWindow(hwndMain);
                }
            }
            catch
            {
                AccountClient selinstance = lv_instances.SelectedItem as AccountClient;
                AccountClientManager.Remove(selinstance);
            }

        }

        private void bt_closeinstance_Click(object sender, RoutedEventArgs e)
        {
            AccountClient selinstance = (sender as Button).DataContext as AccountClient;
            try
            {
                ((Client)selinstance.Client).Stop();
            }
            catch { }
            //Globals.LinkedAccs.Remove(selinstance);
        }

        private void bt_maxmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AccountClient selinstance = (sender as Button).DataContext as AccountClient;
                IntPtr hwndMain = ((Client)selinstance.Client).MainWindowHandle;
                SetForegroundWindow(hwndMain);
            }
            catch
            {

            }

        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }
    }
}