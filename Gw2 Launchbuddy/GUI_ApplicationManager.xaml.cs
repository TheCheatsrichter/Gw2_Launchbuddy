using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for GUI_ApplicationManager.xaml
    /// </summary>
    public partial class GUI_ApplicationManager : Window
    {

        (double,double) drag_diff = (0,0);
        bool ispinned = false;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public GUI_ApplicationManager()
        {
            InitializeComponent();
            var windowsettings = Properties.Settings.Default.instancegui_windowsettings;
            /*
            if (windowsettings.Equals((0, 0, 0, 0)))
            {
                Properties.Settings.Default.instancegui_windowsettings = (0, 0, 160, 300);
                Properties.Settings.Default.Save();
            }
            Left = windowsettings.Item1;
            Top = windowsettings.Item2;
            Width = windowsettings.Item3;
            Height = windowsettings.Item4;
            */
        }

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SaveWindowSettings()
        {
            /*
            Properties.Settings.Default.instancegui_windowsettings = (Left, Top, ActualWidth, ActualHeight);
            Properties.Settings.Default.Save();
            */
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lv_instances.ItemsSource = ClientManager.ActiveClients;
            if (!ispinned) BeginStoryboard((Storyboard)Resources["anim_collapse"]);
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
            myWindow.ResizeMode = ResizeMode.NoResize;
            this.DragMove();
            myWindow.ResizeMode = ResizeMode.CanResize;
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

        private void bt_pin_Click(object sender, RoutedEventArgs e)
        {
            ispinned = !ispinned;
            if(ispinned)
            {
                bt_pin.OpacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/pinned.png")));
            }
            else
            {
                bt_pin.OpacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/pin.png")));
            }
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (ActualWidth > MinWidth)
            {
                MaxWidth += (e.HorizontalChange-drag_diff.Item1);
                Width = MaxWidth;
            }
            else
            {
                MaxWidth = MinWidth+4;
                Width = MaxWidth;
                (sender as Thumb).ReleaseMouseCapture();
            }

            if (ActualHeight> MinHeight)
            {
                MaxHeight += (e.VerticalChange-drag_diff.Item2);
                Height = MaxHeight;
            }
            else
            {
                MaxHeight = MinHeight+4;
                Height = MaxHeight;
                (sender as Thumb).ReleaseMouseCapture();
            }
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SaveWindowSettings();
        }

        private void myWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(!ispinned) BeginStoryboard((Storyboard)Resources["anim_show"]);
        }

        private void myWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!ispinned) BeginStoryboard((Storyboard)Resources["anim_collapse"]);
        }

        private void myWindow_StateChanged(object sender, EventArgs e)
        {
            Console.WriteLine(myWindow.WindowState);
        }

        private void bt_settings_Click(object sender, RoutedEventArgs e)
        {
            if (row_settings.Height.Value == 0)
            {
                row_settings.Height = new GridLength(150);
            } else
            {
                row_settings.Height = new GridLength(0);
            }

        }
    }
}