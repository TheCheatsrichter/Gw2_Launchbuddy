using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class AccountClientManager
    {
        private static ObservableCollection<AccountClient> accountClientCollection { get; set; }
        public static ReadOnlyObservableCollection<AccountClient> AccountClientCollection { get; set; }

        static AccountClientManager()
        {
            accountClientCollection = new ObservableCollection<AccountClient>();
            AccountClientCollection = new ReadOnlyObservableCollection<AccountClient>(accountClientCollection);
        }

        public static AccountClient Add(Account Account, Client Client) => Add(new AccountClient(Account, Client));

        public static AccountClient Add(AccountClient AccountClient)
        {
            accountClientCollection.Add(AccountClient);
            return AccountClient;
        }

        public static void Remove(Account Account, Client Client) => Remove(accountClientCollection.Where(a => a.Account == Account && a.Client == Client).Single());

        public static void Remove(AccountClient AccountClient)
        {
            accountClientCollection.Remove(AccountClient);
        }

        public static void Remove(Client Client)
        {
            accountClientCollection.Remove(accountClientCollection.Where(a => a.Client == Client).SingleOrDefault());
        }

        public static List<AccountClient> ToList() => accountClientCollection.ToList();
    }

    public class AccountClient
    {
        public Client Client { set; get; }
        public Account Account { set; get; }

        public AccountClient(Account Account, Client Client)
        {
            this.Client = Client;
            this.Account = Account;
            Client.StartInfo.Arguments = Account.CommandLine();
        }
    }
}