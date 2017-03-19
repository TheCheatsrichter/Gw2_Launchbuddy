﻿using System;
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
    /// Interaction logic for TextBoxPopUp.xaml
    /// </summary>
    public partial class TextBoxPopUp : Window
    {
        public bool securemode = false;
        public string Output = null;

        public TextBoxPopUp(string message,string title, bool ispassword=false)
        {
            InitializeComponent();
            Title = title;
            textblock_info.Text = message;
            securemode = ispassword;

            if (ispassword)
            {
                tb_input.Visibility = Visibility.Hidden;
                pwbox_input.Visibility = Visibility.Visible;
                pwbox_input.Focus();
            }else
            {
                tb_input.Focus();
            }

            
        }

        private void bt_ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        public string Input()
        {
            if (securemode)
            {
                return pwbox_input.Password;
            } else
            {
                return tb_input.Text;
            }
        }

        private void bt_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
