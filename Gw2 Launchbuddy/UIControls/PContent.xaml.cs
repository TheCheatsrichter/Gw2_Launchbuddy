using Gw2_Launchbuddy.Premium;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gw2_Launchbuddy.UIControls
{
    /// <summary>
    /// Interaktionslogik für PContent.xaml
    /// </summary>
    public partial class PContent : UserControl
    {
        public PContent()
        {
            InitializeComponent();

            if (!PProtection.IspVersion())
            {
                actualcontent.IsEnabled = false;
            }else
            {
                blockericon.Visibility = Visibility.Collapsed;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("www.patreon.com/gw2launchbuddy");
        }

        public object LockedContent
        {
            get { return (object)GetValue(LockedContentProperty); }
            set { SetValue(LockedContentProperty, value); }
        }
        public static readonly DependencyProperty LockedContentProperty =
            DependencyProperty.Register("LockedContent", typeof(object), typeof(PContent),
              new PropertyMetadata(null));
    }
}
