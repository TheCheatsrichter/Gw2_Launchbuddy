using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class AccountArgumentManager
    {
        private static ObservableCollection<AccountArgument> accountArgumentCollection { get; set; }
        public static ReadOnlyObservableCollection<AccountArgument> AccountArgumentCollection { get; set; }

        static AccountArgumentManager()
        {
            accountArgumentCollection = new ObservableCollection<AccountArgument>();
            AccountArgumentCollection = new ReadOnlyObservableCollection<AccountArgument>(accountArgumentCollection);
        }

        public static List<AccountArgument> GetAccountArguments(this Account Account)
        {
            return accountArgumentCollection.Where(a => a.Account == Account).ToList();
        }

        public static AccountArgument Add(this Account Account, string Flag) => Add(new AccountArgument(Account, ArgumentManager.Argument(Flag)));

        public static AccountArgument Add(Account Account, Argument Argument) => Add(new AccountArgument(Account, Argument));

        public static AccountArgument Add(AccountArgument AccountArgument)
        {
            var testForExists = Get(AccountArgument.Account, AccountArgument.Argument.Flag);
            if (testForExists != null) return testForExists;
            if (AccountArgument.Argument.Flag == "-email") AccountArgument.OptionString = "\"" + AccountArgument.Account.Email + "\"";
            if (AccountArgument.Argument.Flag == "-password") AccountArgument.OptionString = "\"" + AccountArgument.Account.Password + "\"";
            accountArgumentCollection.Add(AccountArgument);
            return AccountArgument;
        }

        public static AccountArgument GetOrCreate(this Account Account, string Flag)
        {
            return accountArgumentCollection.Where(a => a.Argument.Flag == Flag && a.Account == Account).SingleOrDefault() ?? Add(Account, Flag);
        }

        public static AccountArgument Get(this Account Account, string Flag) => accountArgumentCollection.Where(a => a.Argument.Flag == Flag && a.Account == Account).SingleOrDefault();

        public static class StopGap
        {
            public static Dictionary<string, AccountArgument> ToDictionary()
            {
                return accountArgumentCollection.Where(a => a.Argument.Active && a.Account == AccountManager.DefaultAccount).ToDictionary(a => a.Argument.Flag, a => a);
            }

            public static void SetOptionString(string Flag, string OptionString)
            {
                GetOrCreate(AccountManager.DefaultAccount, Flag).WithOptionString(OptionString);
                foreach (Account account in AccountManager.AccountCollection)
                    GetOrCreate(account, Flag).WithOptionString(OptionString);
            }

            public static string GetOptionString(string Flag)
            {
                return GetOrCreate(AccountManager.DefaultAccount, Flag).OptionString;
            }

            public static void IsSelected(string Flag, bool Selected = true)
            {
                GetOrCreate(AccountManager.DefaultAccount, Flag).IsSelected(Selected);
                foreach (Account account in AccountManager.AccountCollection)
                    GetOrCreate(account, Flag).IsSelected(Selected);
            }

            public static bool SelectedCheck(string Flag)
            {
                return GetOrCreate(AccountManager.DefaultAccount, Flag).Selected;
            }

            public static string Print()
            {
                return AccountManager.DefaultAccount.PrintArguments();
            }
        }
    }

    public class AccountArgument
    {
        public AccountArgument(Account Account, Argument Argument)
        {
            this.Account = Account;
            this.Argument = Argument;
            this.Selected = Argument.Default;
        }

        public Account Account { get; set; }
        public Argument Argument { get; set; }

        public AccountArgument WithOptionString(string OptionString)
        {
            this.OptionString = OptionString; return this;
        }

        public AccountArgument IsSelected(bool Selected = true)
        {
            this.Selected = Selected; return this;
        }

        public bool Selected { get; set; }
        public string OptionString { get; set; }
        public SecureString SecureOptionString { get; set; }
    }
}