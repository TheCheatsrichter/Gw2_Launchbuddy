using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy
{
    public static class AccountArgumentManager
    {
        private static List<AccountArgument> accountArgumentList;

        public static List<AccountArgument> GetAccountArguments(this Account Account)
        {
            return accountArgumentList.Where(a => a.Account == Account).ToList();
        }

        public static AccountArgument Add(this Account Account, string Flag) => Add(new AccountArgument(Account, ArgumentManager.Argument(Flag)));
        public static AccountArgument Add(AccountArgument AccountArgument)
        {
            if (AccountArgument.Argument.Flag == "-email") AccountArgument.OptionString = AccountArgument.Account.Email;
            if (AccountArgument.Argument.Flag == "-password") AccountArgument.OptionString = AccountArgument.Account.Password;
            accountArgumentList.Add(AccountArgument);
            return AccountArgument;
        }

        public static AccountArgument Argument(this Account Account, string Flag)
        {
            return accountArgumentList.Where(a => a.Argument.Flag == Flag && a.Account == Account).SingleOrDefault() ?? Add(Account, Flag);
        }

        public static AccountArgument ArgumentNoCreate(this Account Account, string Flag)
        {
            return accountArgumentList.Where(a => a.Argument.Flag == Flag && a.Account == Account).SingleOrDefault();
        }
    }

    public class AccountArgument
    {
        public AccountArgument(Account Account, Argument Argument)
        {
            this.Account = Account;
            this.Argument = Argument;
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
