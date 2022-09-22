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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gw2_Launchbuddy.Helpers
{
    /// <summary>
    /// Interaktionslogik für UIPointer.xaml
    /// </summary>
    public partial class UIPointer : Window
    {
        public UIPointer(Point targetpoint)
        {
            InitializeComponent();
            Left = targetpoint.X;
            Top = targetpoint.Y - (Height/2);
        }

        private void OnFadeoutDone(object sender, EventArgs e)
        {
            Close();
        }
    }
}
