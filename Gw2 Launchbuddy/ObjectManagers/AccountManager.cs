using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows;
using System.Collections;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.ComponentModel;
using System.Xml.Serialization;

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
            return new Account(GenerateName("New Account"));
        }

        public static string GenerateName(string name_pre)
        {
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
            return name_pre + name_suf;
        }

        public static void Clone(Account acc)
        {
            Account newacc = new Account(GenerateName(acc.Nickname+" Clone"),acc);
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
            if(Accounts.Contains(acc)&& Accounts.Count>1)
            {
                int index = Accounts.IndexOf(acc);
                int newindex = index + (steps*-1);
                if (newindex > Accounts.Count-1)
                    newindex -= Accounts.Count;
                if (newindex < 0)
                    newindex += Accounts.Count;
                Accounts.Move(index,newindex);
            }
        }

        public static void SaveAccounts()
        {

            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(Accounts.GetType());
            FileStream file = File.Create(EnviromentManager.LBAccPath);
            writer.Serialize(file, Accounts);
            file.Close();

        }
        public static void ImportAccounts()
        {
            Stream xmlInputStream = File.OpenRead(EnviromentManager.LBAccPath);
            XmlSerializer deserializer = new XmlSerializer(typeof(ObservableCollection<Account>));
            Accounts = (ObservableCollection<Account>)deserializer.Deserialize(xmlInputStream);
            xmlInputStream.Close();
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
        [XmlIgnore]
        public bool IsEnabled = false;
        public AccountSettings Settings { get; set; }

        private void CreateAccount(string nickname)
        {
            if (!AccountManager.Accounts.Any(a => a.Nickname == nickname))
            {
                Nickname = nickname;
                AccountManager.Accounts.Add(this);
                Client Client = new Client(this);
                if(Settings==null)
                {
                    Settings = new AccountSettings();
                    Settings.Arguments = new Arguments();
                }
            }
            else
            {
                MessageBox.Show("Account with Nickname"+nickname+"allready exists!");	
            }
        }

        private Account() { }

        public Account(string nickname)
        {
            CreateAccount(nickname);
        }
        public Account(string nickname, Account account)
        {
            this.Settings = account.Settings.GetClone();
            CreateAccount(nickname);
        }

        [XmlIgnore]
        public Client Client { get { return ClientManager.Clients.FirstOrDefault(c => c.account == this); } }
    }

    public class AccountSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Arguments Arguments { get; set; }
        public GFXConfig GFXFile { get; set; }
        public ObservableCollection<string> DLLs { get; set; }
        private Icon icon;


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public AccountSettings()
        {
            //Init Defaults if no safefile
            if(GFXFile==null)GFXFile = GFXManager.LoadFile(EnviromentManager.GwClientXmlPath);
            if(DLLs==null)DLLs = new ObservableCollection<string>();
        }

        public Icon Icon { get { return icon; } set { icon = value; OnPropertyChanged("Icon"); } }
        [XmlIgnore]
        public ObservableCollection<Icon> Icons { get { return IconManager.Icons; } }

        public string enc_email=null;
        public string enc_password=null;

        [XmlIgnore]
        private AES Cryptor = new AES();

        [XmlIgnore]
        public string Email { set { enc_email = Cryptor.Encrypt(value); } get { return Cryptor.Decrypt(enc_email); } }
        [XmlIgnore]
        public string Password { set { enc_password = Cryptor.Encrypt(value); } get { return Cryptor.Decrypt(enc_password); } }

        [XmlIgnore]
        public string UI_Email
        {
            get
            {
                if (Email == null) return "";
                return Email.Substring(0, 2) + "*****@****." + Email.Split('.')[1];
            }
            set { }
        }

        [XmlIgnore]
        public string UI_Password { get { return "***************"; } set{}}

        public AccountSettings GetClone()
        {
            AccountSettings settings = (AccountSettings)this.MemberwiseClone();
            settings.Email = null;
            settings.Password = null;
            return settings;
        }

        //Missing stuff (all nullable):
        /*
        localdat;
        Window Size,Startposition etc.
        */
    }
}