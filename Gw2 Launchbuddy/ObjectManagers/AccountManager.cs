using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class AccountManager
    {
        private static ObservableCollection<Account> accountCollection { get; set; }
        public static ReadOnlyObservableCollection<Account> AccountCollection { get; private set; }

        public static Account DefaultAccount { get; private set; }

        public static ObservableCollection<Account> SelectedAccountCollection { get => new ObservableCollection<Account>(accountCollection.Where(a => a.Selected == true)); }

        static AccountManager()
        {
            accountCollection = new ObservableCollection<Account>();
            AccountCollection = new ReadOnlyObservableCollection<Account>(accountCollection);

            DefaultAccount = new ObjectManagers.Account("", "", "");
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
            private static string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Guild Wars 2\Launchbuddy.bin";
            public static void LoadAccountInfo()
            {
                try
                {
                    if (File.Exists(path) == true)
                    {
                        using (Stream stream = File.Open(path, FileMode.Open))
                        {
                            var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            ObservableCollection<Account> aes_accountlist = (ObservableCollection<Account>)bformatter.Deserialize(stream);

                            foreach (Account acc in aes_accountlist)
                            {
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
                try
                {
                    using (Stream stream = System.IO.File.Open(path, FileMode.Create))
                    {
                        var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        bformatter.Serialize(stream, accountCollection);
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
            get
            {
                try
                {
                    return new AES().Decrypt(email);
                }
                catch
                {
                    email = new AES().Encrypt(email);
                }
                return new AES().Decrypt(email);
            }
            set
            {
                email = new AES().Encrypt(value);
                AccountArgumentManager.Get(this, "-email")?.WithOptionString(value);
            }
        }

        public string ObscuredEmail
        {
            get => Email.Substring(0, 2) + "********" + Email.Substring(Email.LastIndexOf('.') - 2);
        }

        public string Password
        {
            get
            {
                try
                {
                    return new AES().Decrypt(password);
                }
                catch
                {
                    password = new AES().Encrypt(password);
                }
                return new AES().Decrypt(password);
            }
            set
            {
                password = new AES().Encrypt(value);
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
        private ImageSource icon;

        public ImageSource Icon
        {
            get
            {
                if (icon == null)
                {
                    var image = new BitmapImage();
                    using (var ms = new MemoryStream(IconBytes))
                    {
                        ms.Position = 0;
                        image.BeginInit();
                        image.StreamSource = ms;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        icon = image;
                    }
                }
                return icon;
            }
            private set => icon = value;
        }

        private byte[] iconBytes;

        public byte[] IconBytes
        {
            get
            {
                if (iconBytes == null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        System.Drawing.Bitmap bitmap = Gw2_Launchbuddy.Properties.Resources.user;
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Position = 0;
                        iconBytes = ms.GetBuffer();
                    }
                }
                return iconBytes;
            }
            set
            {
                iconBytes = value;
                Icon = null;
            }
        }

        public void SetIcon(string Path)
        {
            if (System.IO.File.Exists(Path))
            {
                using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var ms = new MemoryStream())
                    {
                        fs.Position = 0;
                        fs.CopyTo(ms);
                        IconBytes = ms.GetBuffer();
                    }
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
}