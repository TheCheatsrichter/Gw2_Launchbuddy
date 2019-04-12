using System.Windows;
using Gw2_Launchbuddy.ObjectManagers;

namespace Gw2_Launchbuddy.Helpers
{
    /// <summary>
    /// Interaktionslogik für WindowInfo_Grabber.xaml
    /// </summary>
    public partial class WindowInfo_Grabber : Window
    {
        public WindowInfo_Grabber()
        {
            InitializeComponent();
        }

        public WindowConfig GetInfo()
        {
            WindowConfig info = new WindowConfig();
            info.WinPos_X = (int)(this.Left);
            info.WinPos_Y = (int)(this.Top);
            info.Win_Height = (int)this.Height;
            info.Win_Width = (int)this.Width;
            info.WindowState = this.WindowState;
            return info;
        }
    }
}
