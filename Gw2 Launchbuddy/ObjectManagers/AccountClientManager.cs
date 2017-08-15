using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class AccountClientManager
    {
        private static List<AccountClient> accountClientList = new List<AccountClient>();

        public static AccountClient Add(Account Account, Client Client) => Add(new AccountClient(Account, Client));
        public static AccountClient Add(AccountClient AccountClient)
        {
            accountClientList.Add(AccountClient);
            return AccountClient;
        }

        public static void Remove(Account Account, Client Client) => Remove(accountClientList.Where(a => a.Account == Account && a.Client == Client).Single());
        public static void Remove(AccountClient AccountClient)
        {
            accountClientList.Remove(AccountClient);
        }
    }

    public class AccountClient
    {
        public Client Client { set; get; }
        public Account Account { set; get; }

        public AccountClient(Account Account, Client Client)
        {
            this.Client = Client;
            this.Account = Account;
            Client.Process.StartInfo.Arguments = Account.CommandLine();
        }
    }
}
