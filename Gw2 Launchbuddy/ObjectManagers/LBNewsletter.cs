using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class LBNewsletter
    {
        public static string Data;

        public static Uri News_Url = new Uri(@"https://raw.githubusercontent.com/TheCheatsrichter/Gw2_Launchbuddy/master/Newsletter.md");

        public async static Task<string> FetchNews()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    Data = client.DownloadString(News_Url);
                }
            }
            catch
            {
                Data = "No news available";
            }

            return await Task.Run(() => Data);
        }
    }
}
