using System.Windows;
using Gw2_Launchbuddy.ObjectManagers;

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
            lb_donatelabel.Content = "Gw2 Launchbuddy V" + EnviromentManager.LBVersion.ToString();
            textb_Message.Text = "Guild Wars 2 Launchbuddy is a free, opensource software, which therefore is depending on the community. \nI personally spend many hours of my free time to create and maintain this application. So if you like it, feel free to press the donation button to keep this project alive for future updates!\nEven the small amounts will help me out a lot as I am still a student! :)\n\nGreetings TheCheatsrichter";
        }

        private void bt_donate_Click(object sender, RoutedEventArgs e)
        {
            /*
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
            */
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WKHYFSBMK6TQE&source=url");
        }

        private void bt_patreon_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.patreon.com/gw2launchbuddy");
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("www.patreon.com/gw2launchbuddy");
        }
    }
}