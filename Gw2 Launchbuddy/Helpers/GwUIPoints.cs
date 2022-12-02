using Gw2_Launchbuddy.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.Helpers
{
    public static class GwUIPoints
    {
        public static UIPoint pos_email_tb = new UIPoint(480, 430);
        public static UIPoint pos_passw_tb = new UIPoint(480, 510);
        public static UIPoint pos_login_bt = new UIPoint(100, 600);
        public static UIPoint pos_authemail_bt = new UIPoint(400, 550);
        public static UIPoint pos_play_bt = new UIPoint(825, 715);
        public static UIPoint pos_authcode = new UIPoint(170, 500);
        public static UIPoint pos_authcode_remnetwork = new UIPoint(65, 545);
        public static UIPoint pos_loginwindow_offset = new UIPoint(20, 195);
        //400
        public static void UpdateDPIFactor(double factor)
        {
            pos_email_tb.DPIConverted(factor);
            pos_passw_tb.DPIConverted(factor);
            pos_login_bt.DPIConverted(factor);
            pos_authemail_bt.DPIConverted(factor);
            pos_play_bt.DPIConverted(factor);
            pos_authcode.DPIConverted(factor);
            pos_authcode_remnetwork.DPIConverted(factor);
            pos_loginwindow_offset.DPIConverted(factor);
        }

    }
}
