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
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public Popup()
        {
            InitializeComponent();
            lb_donatelabel.Content = "Gw2 Launchbuddy V" + Globals.LBVersion.ToString();
            textb_Message.Text = "Guild Wars 2 Launchbuddy is a free, opensource software, which therefore is depending on the community. \nI personally spend many hours of my free time to create and maintain this application. So if you like it feel free to press the donate button to keep this project alive for future updates!\nEven the smallest donations would help me as a student out a lot! :)\n\nGreetings TheCheatsrichter";

        }

        private void bt_donate_Click(object sender, RoutedEventArgs e)
        {
            string url = "";

            string business = "thecheatsrichter@gmx.at";  // your paypal email
            string description = "Gw2 Launchbuddy Donation";            // '%20' represents a space. remember HTML!
            string currency = "EUR";                 // AUD, USD, etc.

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                //"&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
        }
    }
}
