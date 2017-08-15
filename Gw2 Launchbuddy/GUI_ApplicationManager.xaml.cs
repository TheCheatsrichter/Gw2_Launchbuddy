using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading;
using System.Data;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using Gw2_Launchbuddy.ObjectManagers;


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
            Thread updatelist = new Thread(UpdateProAccs);
            updatelist.IsBackground = true;
            updatelist.Start();
        }

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lv_instances.ItemsSource = Globals.LinkedAccs.Where(a => a.Client.Arguments == "");
        }

        private void lv_gfx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lv_instances.SelectedIndex >=0)
            {
                CheckPros();
                AccountClient selinstance = lv_instances.SelectedItem as AccountClient;
                IntPtr hwndMain = selinstance.Client.Process.MainWindowHandle;
                SetForegroundWindow(hwndMain);
            }
        }

        public void UpdateProAccs()
        {
            while (true)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                   CheckPros();
                }));
                Thread.Sleep(1000);
            }
        }

        public void CheckPros()
        {
            ObservableCollection<AccountClient> ToRemove = new ObservableCollection<AccountClient>();
            foreach(AccountClient proacc in Globals.LinkedAccs)
            {
                try
                {
                    Process.GetProcessById(proacc.Client.Process.Id);   
                }
                catch
                {
                    ToRemove.Add(proacc);
                }
            }
            foreach (AccountClient proacc in ToRemove)
            {
                Globals.LinkedAccs.Remove(proacc);
            }
        }

        private void bt_closeinstance_Click(object sender, RoutedEventArgs e)
        {
            CheckPros();
            AccountClient selinstance = (sender as Button).DataContext as AccountClient;
            try
            {
                if(!selinstance.Client.Process.CloseMainWindow())
                    selinstance.Client.Process.Kill();
                if (!selinstance.Client.Process.WaitForExit(1000))
                    selinstance.Client.Process.Kill();
            }
            catch { }
            Globals.LinkedAccs.Remove(selinstance);
        }

        private void bt_maxmin_Click(object sender, RoutedEventArgs e)
        {
            CheckPros();
            AccountClient selinstance = (sender as Button).DataContext as AccountClient;
            IntPtr hwndMain = selinstance.Client.Process.MainWindowHandle;
            SetForegroundWindow(hwndMain);
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
