using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class AccountClientManager
    {
        //public static List<AccountClient> AccountClientList { get; private set; }

        public static ObservableCollection<AccountClient> AccountClientCollection { get; private set; }

        static AccountClientManager()
        {
            AccountClientCollection = new ObservableCollection<AccountClient>();
            //AccountClientList = new List<AccountClient>();
        }

        public static AccountClient Add(Account Account, Client Client) => Add(new AccountClient(Account, Client));
        public static AccountClient Add(AccountClient AccountClient)
        {
            //AccountClientList.Add(AccountClient);
            AccountClientCollection.Add(AccountClient);
            return AccountClient;
        }

        public static void Remove(Account Account, Client Client) => Remove(AccountClientCollection.Where(a => a.Account == Account && a.Client == Client).Single());
        public static void Remove(AccountClient AccountClient)
        {
            //AccountClientList.Remove(AccountClient);
            AccountClientCollection.Remove(AccountClient);
        }
        public static void Remove(Client Client)
        {
            AccountClientCollection.Remove(AccountClientCollection.Where(a => a.Client == Client).SingleOrDefault());
        }

        public static List<AccountClient> ToList() => AccountClientCollection.ToList();

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
