using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gw2_Launchbuddy.Helpers
{
    /// <summary>
    /// Interaktionslogik für WindowSpacer.xaml
    /// </summary>
    public partial class WindowSpacer : Window
    {
        public WindowSpacer(Account acc)
        {
            InitializeComponent();
            lb_charname.Content = acc.Nickname;


            if (acc.Settings.Icon != null) img_icon.Source = acc.Settings.Icon.Image;
            

            WindowConfig winconfig = acc.Settings.WinConfig;

            Left = winconfig.WinPos_X;
            Top = winconfig.WinPos_Y;

            Width = winconfig.Win_Width;
            Height = winconfig.Win_Height;
        }
    }
}
