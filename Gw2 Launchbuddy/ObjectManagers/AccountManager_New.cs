using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class AccountManager_New
    {  
        public static class AcccountCollector
        {
            static ObservableCollection<Account> AccountCollection = new ObservableCollection<Account>();
            public static void Add(Account account)
            {
                if (!AccountCollection.Any<Account>(a=>a.Nickname==account.Nickname))
                {
                    AccountCollection.Add(account);
                }
            }
        }


        public class Account
        {
            public bool IsSelected { get; set; }
            public string Nickname { get; set; }

            [XmlElement(IsNullable = true)]
            public LoginCredentials LoginCrendentials;

            [XmlElement(IsNullable = true)]
            public string GFXConfig { get; set; }

            [XmlElement(IsNullable = true)]
            public ObservableCollection<string> OptionalArguments = new ObservableCollection<string>();

            public Account(string nickname)
            {
                Nickname = nickname;
                IsSelected = false;
            }

            public void LaunchClient()
            {

            }

            public void CloseClient()
            {

            }

        }

        public class LoginCredentials
        {
            bool IsUsed = false;

            public string Email;
            public string Password;

            public string ObscuredEmail
            {
                get
                {
                    return Email.Split('@')[0].Substring(0, 2) + "***" + Email.Split('@')[1];
                }
            }
        }

        public class ClientOptions
        {
            ObservableCollection<string> Arguments = new ObservableCollection<string>();
        }
   
    }
}
