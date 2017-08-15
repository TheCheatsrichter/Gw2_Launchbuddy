using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class AccountArgumentManager
    {
        private static List<AccountArgument> accountArgumentList = new List<AccountArgument>();

        public static List<AccountArgument> GetAccountArguments(this Account Account)
        {
            return accountArgumentList.Where(a => a.Account == Account).ToList();
        }

        public static AccountArgument Add(this Account Account, string Flag) => Add(new AccountArgument(Account, ArgumentManager.Argument(Flag)));
        public static AccountArgument Add(Account Account, Argument Argument) => Add(new AccountArgument(Account, Argument));
        public static AccountArgument Add(AccountArgument AccountArgument)
        {
            if (AccountArgument.Argument.Flag == "-email") AccountArgument.OptionString = AccountArgument.Account.Email;
            if (AccountArgument.Argument.Flag == "-password") AccountArgument.OptionString = AccountArgument.Account.Password;
            accountArgumentList.Add(AccountArgument);
            return AccountArgument;
        }

        public static AccountArgument GetOrCreate(this Account Account, string Flag)
        {
            return accountArgumentList.Where(a => a.Argument.Flag == Flag && a.Account == Account).SingleOrDefault() ?? Add(Account, Flag);
        }

        public static AccountArgument Get(this Account Account, string Flag)
        {
            return accountArgumentList.Where(a => a.Argument.Flag == Flag && a.Account == Account).SingleOrDefault();
        }

        public static class StopGap
        {
            public static List<AccountArgument> GetAll()
            {
                return accountArgumentList;
            }

            public static List<AccountArgument> GetAllFromDefault()
            {
                return accountArgumentList.Where(a => a.Account.Default).ToList();
            }

            public static Dictionary<string, AccountArgument> ToDictionary()
            {
                return accountArgumentList.Where(a => a.Argument.Active && a.Account.Default).ToDictionary(a => a.Argument.Flag, a => a);
            }

            public static void SetOptionString(string Flag, string OptionString)
            {
                foreach (Account account in AccountManager.ToList())
                {
                    GetOrCreate(account, Flag).WithOptionString(OptionString);
                }
            }
            public static string GetOptionString(string Flag)
            {
                return GetOrCreate(AccountManager.DefaultAccount, Flag).OptionString;
            }

            public static void IsSelected(string Flag, bool Selected = true)
            {
                foreach (Account account in AccountManager.ToList(true))
                {
                    GetOrCreate(account, Flag).IsSelected(Selected);
                }
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

        public AccountArgument WithOptionString(string OptionString) { this.OptionString = OptionString; return this; }
        public AccountArgument IsSelected(bool Selected = true) { this.Selected = Selected; return this; }

        public bool Selected { get; set; }
        public string OptionString { get; set; }
        public SecureString SecureOptionString { get; set; }
    }
}
