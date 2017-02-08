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

namespace Gw2_Launchbuddy
{
    /// <summary>
    /// Interaction logic for GUI_ApplicationManager.xaml
    /// </summary>
    public partial class GUI_ApplicationManager : Window
    {
        public GUI_ApplicationManager()
        {
            InitializeComponent();
            lv_gfx.ItemsSource = Globals.LinkedAccs;
        }

        private void bt_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
