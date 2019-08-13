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
            var windowsettings = Properties.Settings.Default.instancegui_windowsettings;
            if (windowsettings.Equals((0, 0, 0, 0)))
            {
                Properties.Settings.Default.instancegui_windowsettings = (0, 0, 160, 300);
                Properties.Settings.Default.Save();
            }
            Left = windowsettings.Item1;
            Top = windowsettings.Item2;
            Width = windowsettings.Item3;
            Height = windowsettings.Item4;
        }

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SaveWindowSettings()
        {
            Properties.Settings.Default.instancegui_windowsettings = (Left, Top, ActualWidth, ActualHeight);
            Properties.Settings.Default.Save();
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
            SaveWindowSettings();
        }

        private void bt_suspend_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as Client).Suspend();
        }

        private void bt_resume_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as Client).Resume();
        }

        private void myWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowSettings();
        }
    }
}