using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows;
using System.Collections;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace Gw2_Launchbuddy.ObjectManagers
{

    public static class AccountManager
    {
        static public Account EditAccount = null;
        static public ObservableCollection<Account> Accounts = new ObservableCollection<Account>();
        static public void Remove(Account Account) { Accounts.Remove(Accounts.First(a => a.Nickname == Account.Nickname)); }
        static public void Remove(string Nickname) { Accounts.Remove(Accounts.First(a => a.Nickname == Nickname)); }

        public static ObservableCollection<Account> SelectedAccounts { get { return new ObservableCollection<Account>(Accounts.Where<Account>(a => a.IsEnabled == true)); } }

        public static void SwitchSelectionAll(bool active)
        {
            foreach (Account acc in Accounts)
            {
                acc.IsEnabled = active;
            }
        }

        public static Account CreateEmptyAccount()
        {
            string name_pre = "New Account";
            string name_suf = "";
            bool isnew = false;
            int i = 0;
            while (!isnew)
            {
                if (i != 0)
                {
                    name_suf = "(" + i.ToString() + ")";
                }
                isnew = !AccountManager.Accounts.Any<Account>(a => a.Nickname == name_pre + name_suf);
                i++;
            }

            return new Account(name_pre + name_suf);
        }

        public static void LaunchAccounts()
        {
            foreach(Account acc in SelectedAccounts)
            {
                acc.Client.Launch();
            }
        }
        public static void LaunchAccounts(ObservableCollection<Account> accs)
        {
            foreach (Account acc in accs)
            {
                acc.Client.Launch();
            }
        }

        public static bool HasEntry => Accounts.Count == 0 ? false : true;

        public static void MoveAccount(Account acc,int steps)
        {
            if(Accounts.Contains(acc))
            {
                int index = Accounts.IndexOf(acc);
                int newindex = index + steps;
                if (newindex > Accounts.Count)
                    newindex -= Accounts.Count;
                if (newindex < 0)
                    newindex += Accounts.Count;
                Accounts.Insert(newindex,acc);
                Accounts.RemoveAt(index);

            }
        }

        public static void ImportAccounts()
        {
            //Import accounts
        }

        public static void SaveAccounts()
        {
            //Import accounts
        }

        public static bool IsValidEmail(string inp)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(inp);
                return addr.Address == inp;
            }
            catch
            {
                return false;
            }
        }
    }

    public class Account
    {
        public string Nickname { get; set; }
        public bool IsEnabled = false;
        public AccountSettings Settings { get; set; }

        private void CreateAccount(string nickname)
        {
            if (!AccountManager.Accounts.Any(a => a.Nickname == nickname))
            {
                Nickname = nickname;
                AccountManager.Accounts.Add(this);
                Client Client = new Client(this);
                Settings = new AccountSettings();
                Settings.Arguments = new Arguments();
            }
            else
            {
                MessageBox.Show("Account with Nickname"+nickname+"allready exists!");	
            }
        }

        public Account(string nickname)
        {
            CreateAccount(nickname);
        }
        public Account(string nickname, Account account)
        {
            this.Settings = account.Settings;//inherit settings
            CreateAccount(nickname);
        }

        public Client Client { get { return ClientManager.Clients.FirstOrDefault(c => c.account == this); } }
    }

    public class AccountSettings
    {
        public Arguments Arguments { get; set; }
        public GFXConfig GFXFile { get; set; }

        public AccountSettings()
        {
            //Init Defaults if no safefile
            GFXFile = GFXManager.LoadFile(EnviromentManager.GwClientXmlPath);
        }

        public ObservableCollection<Bitmap> Icons { get { return UI_Managers.AccIconManager.Icons; } }

        public Bitmap Icon
        {
            get
            {
                if (iconpath == null)
                {
                    return null;
                }
                return new Bitmap(iconpath);
            }
            set
            {
                iconpath = value.ToString();
            }
        }

        public string iconpath;
        //Missing stuff (all nullable):
        /*
        email;
        password;
        localdat;
        Icon;
        Arguments;
        GFX Profile;
        Plugins/Dlls;
        Window Size,Startposition etc.
        */
    }


    /*
    public static class AccountManager
    {
        private static ObservableCollection<Account> accountCollection { get; set; }
        public static ReadOnlyObservableCollection<Account> AccountCollection { get; private set; }

        public static Account DefaultAccount { get; private set; }

        public static ObservableCollection<Account> SelectedAccountCollection { get => new ObservableCollection<Account>(accountCollection.Where(a => a.Selected==true)); }

        static AccountManager()
        {
            accountCollection = new ObservableCollection<Account>();
            AccountCollection = new ReadOnlyObservableCollection<Account>(accountCollection);

            DefaultAccount = new ObjectManagers.Account(null, null, null);
            AccountArgumentManager.StopGap.IsSelected("-shareArchive", true);
        }

        public static Account Account(string Nickname) => accountCollection.Where(a => a.Nickname == Nickname).Single();

        public static Account Add(string Nickname, string Email, string Password) => Add(new Account(Nickname, Email, Password));

        public static Account Add(Account Account)
        {
            accountCollection.Add(Account);
            foreach (Argument Argument in ArgumentManager.ArgumentCollection)
            {
                AccountArgumentManager.Add(Account, Argument);
            }
            foreach (AccountArgument AccountArgument in AccountArgumentManager.GetAccountArguments(DefaultAccount))
            {
                var temp = AccountArgumentManager.Get(Account, AccountArgument.Argument.Flag).IsSelected(AccountArgument.Selected);
                if (AccountArgument.Argument.Flag != "-email" && AccountArgument.Argument.Flag != "-password") temp.OptionString = AccountArgument.OptionString;
            }
            return Account;
        }

        public static void Remove(this Account Account) => accountCollection.Remove(Account);

        public static void Move(Account Account, int Incriment)
        {
            var index = accountCollection.IndexOf(Account);
            accountCollection.RemoveAt(index);
            index = (1 <= index ? index : 1);
            if (index < accountCollection.Count() || Incriment <= 0)
                accountCollection.Insert(index + Incriment, Account);
            else accountCollection.Add(Account);
        }

        public static void SetSelected(IEnumerable<Account> accounts)
        {
            foreach (var account in accountCollection)
            {
                account.Selected = false;
            }

            foreach (var account in accounts)
            {
                account.Selected = true;
            }             
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static class ImportExport
        {
            public static void LoadAccountInfo()
            {
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\Launchbuddy.bin";

                    if (File.Exists(path) == true)
                    {
                        using (Stream stream = File.Open(path, FileMode.Open))
                        {
                            AES aes = new AES();
                            var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            ObservableCollection<Account> aes_accountlist = (ObservableCollection<Account>)bformatter.Deserialize(stream);

                            foreach (Account acc in aes_accountlist)
                            {
                                acc.Email = aes.Decrypt(acc.Email);
                                acc.Password = aes.Decrypt(acc.Password);
                                Add(acc);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            public static void SaveAccountInfo()
            {
                ObservableCollection<Account> aes_accountlist = new ObservableCollection<Account>();
                try
                {
                    AES aes = new AES();
                    aes_accountlist.Clear();
                    foreach (Account acc in AccountManager.accountCollection)
                    {
                        acc.Email = aes.Encrypt(acc.Email);
                        acc.Password = aes.Encrypt(acc.Password);
                        aes_accountlist.Add(acc);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show("Could not encrypt passwords\n" + err.Message);
                }

                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\Launchbuddy.bin";
                    using (Stream stream = System.IO.File.Open(path, FileMode.Create))
                    {
                        var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        bformatter.Serialize(stream, aes_accountlist);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }
    }

    [Serializable()]
    public class Account : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private string email;
        private string password;

        public string Nickname { get; set; }

        public string Email
        {
            get => email;
            set
            {
                email = value;
                AccountArgumentManager.Get(this, "-email")?.WithOptionString(value);
            }
        }

        public string ObscuredEmail
        {
            get
            {
                return email.Split('@')[0].Substring(0,2) + "***" + email.Split('@')[1];
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                AccountArgumentManager.Get(this, "-password")?.WithOptionString(value);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public DateTime RunDate { get; set; }

        public Account IsSelected(bool Selected = true)
        {
            this.Selected = Selected; return this;
        }

        [NonSerialized]
        public bool selected;

        public bool Selected { get => selected; set { selected = value; NotifyPropertyChanged(); } }

        public Account(string Nickname, string Email, string Password)
        {
            this.Nickname = Nickname;
            this.Email = Email;
            this.Password = Password;
            this.CreateDate = DateTime.Now;
            this.ModifyDate = DateTime.Now;
        }

        private string configurationPath;

        public string ConfigurationPath
        {
            get => String.IsNullOrWhiteSpace(configurationPath) ? "Default" : configurationPath;
            set => configurationPath = value;
        }

        public string ConfigurationName
        {
            get => Path.GetFileNameWithoutExtension(ConfigurationPath);
        }

        [NonSerialized]
        private static ImageSource defaultIcon;

        public static ImageSource DefaultIcon
        {
            get
            {
                if (defaultIcon == null)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        System.Drawing.Bitmap bitmap = Gw2_Launchbuddy.Properties.Resources.user;
                        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        memory.Position = 0;
                        BitmapImage bitmapimage = new BitmapImage();
                        bitmapimage.BeginInit();
                        bitmapimage.StreamSource = memory;
                        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapimage.EndInit();

                        defaultIcon = bitmapimage;
                    }
                }
                return defaultIcon;
            }
        }

        [NonSerialized]
        private ImageSource icon;

        public ImageSource Icon
        {
            get => icon ?? DefaultIcon;
            private set => icon = value;
        }

        public void SetIcon(string Path)
        {
            if (System.IO.File.Exists(Path))
            {
                var bitmap = new BitmapImage();
                using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();

                    icon = bitmap;
                }
            }
        }

        public AccountArgument Argument(string Flag) => AccountArgumentManager.GetOrCreate(this, Flag);

        public List<AccountArgument> GetArgumentList() => AccountArgumentManager.GetAccountArguments(this);

        public string PrintArguments() => String.Join(" ", GetArgumentList().Where(a => a.Selected == true).Select(a => a.Argument.Flag + (a.Argument.Sensitive ? null : " " + a.OptionString)));

        public string CommandLine() => String.Join(" ", GetArgumentList().Where(a => a.Selected == true).Select(a => a.Argument.Flag + (!String.IsNullOrWhiteSpace(a.OptionString) ? " " + a.OptionString : null)));

        public void Move(int Incriment) => AccountManager.Move(this, Incriment);

        public Client CreateClient()
        {
            var Client = ClientManager.CreateClient();
            AccountClientManager.Add(this, Client); //Not sure this is the best place for this create/assign
            return Client;
        }
    }
    */
}