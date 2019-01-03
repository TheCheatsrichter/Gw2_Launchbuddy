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
            lv_instances.ItemsSource = ClientManager.ActiveClients;
        }

        private void lv_gfx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var client=lv_instances.SelectedItem as Client;
            if(client!=null)client.Focus();
        }

        private void bt_closeinstance_Click(object sender, RoutedEventArgs e)
        {
            var client = (sender as Button).DataContext as Client;
            client.Close();
            lv_instances.ItemsSource = ClientManager.ActiveClients;
        }

        private void bt_maxmin_Click(object sender, RoutedEventArgs e)
        {
            var client = (sender as Button).DataContext as Client;
            client.Focus();

        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        private void bt_suspend_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as Client).Suspend();
        }

        private void bt_resume_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as Client).Resume();
        }
    }
}