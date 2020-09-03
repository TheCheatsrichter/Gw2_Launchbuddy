using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Linq;
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

        double drag_diff_x = 0, drag_diff_y = 0;
        bool ispinned = LBConfiguration.Config.instancegui_ispinned;

        public GUI_ApplicationManager()
        {
            InitializeComponent();
            string loaded_windowsettings = LBConfiguration.Config.instancegui_windowsettings;
            double[] windowsettings = { 0, 0, 160, 300 };
            try
            {
                windowsettings = Array.ConvertAll(loaded_windowsettings.Split(';'), Double.Parse);
            }
            catch (Exception e)
            {
                windowsettings = new double[] { 0, 0, 160, 300 };
                ResetWindowSettings();
            }

            try
            {
                if (Enumerable.SequenceEqual(windowsettings, new double[] { 0, 0, 0, 0 }))
                {
                    ResetWindowSettings();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error by defaulting GUI Instance dimension occured. If this is not the first time please report this bug under the Settings Tab.\n" +e.Message);
            }

            try
            {
                Left = windowsettings[0];
                Top = windowsettings[1];
                MaxWidth = windowsettings[2];
                MaxHeight = windowsettings[3];
                Width = MaxWidth;
                Height = MaxHeight;
            }
            catch(Exception e)
            {
                Left = 0;
                Top = 0;
                MaxWidth = 160;
                MaxHeight = 300;
                Width = MaxWidth;
                Height = MaxHeight;
                ResetWindowSettings();
                MessageBox.Show("Could not set Instange Manager Postion and Size. Reverting back to defaults.\n" + e.Message);
            }

            UpdateUIButtons();
        }

        public void ResetWindowSettings()
        {
            LBConfiguration.Config.instancegui_windowsettings = "0;0;160;300";
            LBConfiguration.Save();
        }

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SaveWindowSettings()
        {
            LBConfiguration.Config.instancegui_windowsettings = String.Join(";", new string[] { Left.ToString(), Top.ToString(), MaxWidth.ToString(), MaxHeight.ToString() });
            LBConfiguration.Config.instancegui_ispinned = ispinned;
            LBConfiguration.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lv_instances.ItemsSource = ClientManager.ActiveClients;
            if (!ispinned) BeginStoryboard((Storyboard)Resources["anim_collapse"]);
        }

        private void lv_gfx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var client = lv_instances.SelectedItem as Client;
            if (client != null) client.Focus();
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
            SaveWindowSettings();
            UpdateUIButtons();
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (ActualWidth > MinWidth)
            {
                MaxWidth += (e.HorizontalChange - drag_diff_x);
                Width = MaxWidth;
            }
            else
            {
                MaxWidth = MinWidth + 4;
                Width = MaxWidth;
                (sender as Thumb).ReleaseMouseCapture();
            }

            if (ActualHeight > MinHeight)
            {
                MaxHeight += (e.VerticalChange - drag_diff_y);
                Height = MaxHeight;
            }
            else
            {
                MaxHeight = MinHeight + 4;
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
            if (!ispinned) BeginStoryboard((Storyboard)Resources["anim_show"]);
        }

        private void myWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!ispinned) BeginStoryboard((Storyboard)Resources["anim_collapse"]);
        }

        private void bt_settings_Click(object sender, RoutedEventArgs e)
        {
            if (row_settings.Height.Value == 0)
            {
                row_settings.Height = new GridLength(150);
            }
            else
            {
                row_settings.Height = new GridLength(0);
            }

        }

        private void UpdateUIButtons()
        {
            try
            {
                if (ispinned)
                {
                    bt_pin.OpacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/pinned.png")));
                }
                else
                {
                    bt_pin.OpacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/pin.png")));
                }
            }
            catch
            {
                MessageBox.Show("ERROR: GUI Instance Manager pinned.png could not be loaded");
            }
        }
    }
}