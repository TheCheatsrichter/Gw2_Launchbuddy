using Gw2_Launchbuddy.Helpers;
using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.Modifiers
{
    public static class DDOSDelays
    {
        public static int GetWaitTime(bool ipsensitive=true)
        {
            //Wait between clients

            int timetowait = 0;

            //Get all accounts that were active in the last 4 hours

            int active_accounts = 0;

            if (ipsensitive)
            {
                PublicIPFetcher.UpdateIP();
                active_accounts = AccountManager.Accounts.Count(x => x.Settings.AccountInformation.HadLoginInPastMinutes(180) && x.Settings.AccountInformation.LastLogin > PublicIPFetcher.Time_LastIpChange);
            }
            else
            {
                active_accounts = AccountManager.Accounts.Count(x => x.Settings.AccountInformation.HadLoginInPastMinutes(180));
            }

            switch (active_accounts)
            {
                case int _ when active_accounts >= 36:
                    timetowait = 120 * 1000;
                    break;

                case int _ when active_accounts >= 31:
                    timetowait = 60 * 1000;
                    break;

                case int _ when active_accounts >= 21:
                    timetowait = 40 * 1000;
                    break;

                case int _ when active_accounts >= 13:
                    timetowait = 20 * 1000;
                    break;

                default:
                    timetowait = 1800 + (active_accounts * active_accounts * 80);
                    break;
            }

            return timetowait;
        }
    }
}
