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
using Gw2_Launchbuddy.Modifiers;
using System.Threading;
using PluginContracts;
using static Gw2_Launchbuddy.ObjectManagers.AccountSettings;

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
            Account newacc = new Account(GenerateName(acc.Nickname + " Clone"), acc);
        }

        public static void LaunchAccounts()
        {
            UpdateAccountFiles();
            foreach (Account acc in SelectedAccounts)
            {
                acc.Client.Launch();
                acc.Settings.RelaunchesLeft = acc.Settings.RelaunchesMax;
            }
        }
        public static void LaunchAccounts(ObservableCollection<Account> accs)
        {
            foreach (Account acc in accs)
            {
                acc.Client.Launch();
            }
        }

        public static void UpdateAccountFiles()
        {
            foreach(Account acc in Accounts.Where<Account>(x=>x.Settings.Loginfile!=null))
            {
                if(!acc.Settings.Loginfile.IsUpToDate)LocalDatManager.UpdateLocalDat(acc.Settings.Loginfile);
            }
        }

        public static bool HasEntry => Accounts.Count == 0 ? false : true;

        public static void MoveAccount(Account acc, int steps)
        {
            if (Accounts.Contains(acc) && Accounts.Count > 1)
            {
                int index = Accounts.IndexOf(acc);
                int newindex = index + (steps * -1);
                if (newindex > Accounts.Count - 1)
                    newindex -= Accounts.Count;
                if (newindex < 0)
                    newindex += Accounts.Count;
                Accounts.Move(index, newindex);
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
            try
            {
                if (File.Exists(EnviromentManager.LBAccPath))
                {
                    Stream xmlInputStream = File.OpenRead(EnviromentManager.LBAccPath);
                    XmlSerializer deserializer = new XmlSerializer(typeof(ObservableCollection<Account>));
                    Accounts = (ObservableCollection<Account>)deserializer.Deserialize(xmlInputStream);
                    xmlInputStream.Close();
                }
                AddIDToOlderVersions();
            }
            catch(Exception e)
            {
                MessageBox.Show("Could not load Accountdata.\n"+e.Message);
         }
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

        private static void AddIDToOlderVersions()
        {
            foreach (Account acc in Accounts)
            {
                if (acc.ID == 0 ||acc.Settings.AccountID==0)
                {
                    acc.ID = GenerateID();
                    acc.Settings.AccountID = acc.ID;
                }
            }
        }

            public static Account GetAccountByName(string nickname)
        {
            if (Accounts.Any<Account>(a => a.Nickname == nickname)) return Accounts.First<Account>(a => a.Nickname == nickname);
            return null;
        }

        public static Account GetAccountByID(int id)
        {
            if (Accounts.Any<Account>(a => a.ID == id)) return Accounts.First<Account>(a => a.ID == id);
            return null;
        }

        public static int GenerateID()
        {
            int i = 1;
            while(Accounts.Any<Account>(a=>a.ID==i))
            {
                i++;
            }
            return i;
        }

        static public ObservableCollection<PluginContracts.ObjectInterfaces.IAcc> IAccs {
            get {return new ObservableCollection<PluginContracts.ObjectInterfaces.IAcc>(Accounts.Select(a=>a.PluginCalls));}
        }
    }

    public class Account
    {
        public int ID { get; set; }
        public string Nickname { get; set; }
        [XmlIgnore]
        public bool IsEnabled = false;
        public AccountSettings Settings { get; set; }

        private void CreateAccount(string nickname = null)
        {
            if (!AccountManager.Accounts.Any(a => a.Nickname == nickname))
            {
                Client Client = new Client(this);
                PluginCalls = new AccPluginCalls(this);
                Nickname = nickname;
                AccountManager.Accounts.Add(this);
                if (Settings == null)
                {
                    Settings = new AccountSettings(this);
                    Settings.Arguments = new Arguments();
                }
            }
            else
            {
                MessageBox.Show("Account with Nickname" + nickname + "already exists!");
            }
        }

        private Account() {
            CreateAccount();
        }

        public Account(string nickname)
        {
            ID = AccountManager.GenerateID();
            CreateAccount(nickname);
        }
        public Account(string nickname, Account account)
        {
            this.Settings = account.Settings.GetClone();
            ID = AccountManager.GenerateID();
            CreateAccount(nickname);
        }

        [XmlIgnore]
        public Client Client { get { return ClientManager.Clients.FirstOrDefault(c => c.account == this); } }
        [XmlIgnore]
        public AccPluginCalls PluginCalls;


    }

    public class AccountSettings : INotifyPropertyChanged
    {
        public int AccountID { set; get; }
        [XmlIgnore]
        private Account account { get { return AccountManager.GetAccountByID(AccountID); } }
        [XmlIgnore]
        private WindowConfig winconfig;

        public event PropertyChangedEventHandler PropertyChanged;
        public Arguments Arguments { get; set; }
        public GFXConfig GFXFile { get; set; }
        public ObservableCollection<string> DLLs { get; set; }
        private Icon icon;
        public ObservableCollection<AccountHotkey> AccHotkeys { set; get; }
        public LocalDatFile Loginfile { set; get; }
        public WindowConfig WinConfig { set { winconfig = value; OnPropertyChanged("HasWindowConfig"); } get { return winconfig; } }
        public AccountInformation AccountInformation { set; get; }

        //Adavanced Settings
        [XmlIgnore]
        private uint relaunchesmax;
        public uint RelaunchesMax { set { relaunchesmax = value; RelaunchesLeft = value; } get { return relaunchesmax; } }
        [XmlIgnore]
        private uint relaunchesleft;
        [XmlIgnore]
        public uint RelaunchesLeft { set { relaunchesleft = value; } get { return relaunchesleft; } }

        private ProcessPriorityClass processpriority = ProcessPriorityClass.Normal;
        public ProcessPriorityClass ProcessPriority { set { processpriority = value; } get { return processpriority; } }
        [XmlIgnore]
        private ObservableCollection<ProcessPriorityClass> processpriorities = new ObservableCollection<ProcessPriorityClass>(Enum.GetValues(typeof(ProcessPriorityClass)).Cast<ProcessPriorityClass>());
        [XmlIgnore]
        public ObservableCollection<ProcessPriorityClass> ProcessPriorities { get { return processpriorities; } }

        private void Init()
        {
            if (GFXFile == null) GFXFile = GFXManager.LoadFile(EnviromentManager.GwClientXmlPath);
            if (DLLs == null) DLLs = new ObservableCollection<string>();
            if (AccHotkeys == null) AccHotkeys = new ObservableCollection<AccountHotkey>();
            if (RelaunchesMax == null) RelaunchesMax = 0;
            if (AccountInformation == null) this.AccountInformation = new AccountInformation();
            RelaunchesLeft = RelaunchesMax;
        }

        public void AddHotkey()
        {
            AccHotkeys.Add(new AccountHotkey(AccountID));
        }

        public void RemoveHotkey(AccountHotkey key)
        {
            Hotkeys.Remove(key);
            if (AccHotkeys.Contains(key))
                AccHotkeys.Remove(key);
        }

        public void CreateWinConfig()
        {
            if (WinConfig == null) WinConfig = new WindowConfig();
            WinConfig.Configure_Window();
        }

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
            Init();
        }

        public AccountSettings(Account acc)
        {
            //Init Defaults if no safefile
            Init();
            AccountID = acc.ID;
        }

        public Icon Icon { get { return icon; } set { icon = value; OnPropertyChanged("Icon"); } }
        [XmlIgnore]
        public ObservableCollection<Icon> Icons { get { return IconManager.Icons; } }

        public string enc_email = null;
        public string enc_password = null;

        [XmlIgnore]
        private AES Cryptor = new AES();

        [XmlIgnore]
        public string Email { set { enc_email = Cryptor.Encrypt(value); if (value == "") enc_email = null; OnPropertyChanged("HasLoginBackupData"); } get { return Cryptor.Decrypt(enc_email); } }
        [XmlIgnore]
        public string Password { set { enc_password = Cryptor.Encrypt(value); if (value == "") enc_password = null; OnPropertyChanged("HasLoginBackupData"); } get { return Cryptor.Decrypt(enc_password); } }


        [XmlIgnore]
        public string UI_Email
        {
            get
            {
                try
                {
                    return Email.Substring(0, 2) + "*****@****" + Email.Substring(Email.Length - 3);
                }
                catch
                {
                    return "";
                }
            }
            set { }
        }

        [XmlIgnore]
        public string UI_Password { get { return "***************"; } set { } }

        public AccountSettings GetClone()
        {
            AccountSettings settings = (AccountSettings)this.MemberwiseClone();
            settings.Email = null;
            settings.Password = null;
            return settings;
        }

        public void SetRelaunched(uint MaxRelaunches)
        {
            RelaunchesMax = MaxRelaunches;
            RelaunchesLeft = RelaunchesMax;
        }

        [XmlIgnore]
        public bool HasLoginBackupData { get { return Email != null && Password != null && Email != "" && Password != ""; } set { } }
        [XmlIgnore]
        public bool HasLoginCredentials { get { return Loginfile != null; } set { } }
        [XmlIgnore]
        public bool HasArguments { get { if (Arguments.Contains(Arguments.FirstOrDefault<Argument>(a => a.IsActive == true))) { return true; } return false; } set { } }
        [XmlIgnore]
        public bool HasDlls { get { if (DLLs.Count > 0) { return true; } return false; } set { } }
        [XmlIgnore]
        public bool HasAdvancedSettings { get { if (ProcessPriority != ProcessPriorityClass.Normal || RelaunchesMax > 0) { return true; } return false; } set { } }
        [XmlIgnore]
        public bool HasWindowConfig { get { return WinConfig != null; } }
        [XmlIgnore]
        public bool LoginfileOutdated {get { if(Loginfile != null)return Loginfile.IsOutdated; return false;}}


        public void SetLoginFile()
        {
            Loginfile = LocalDatManager.CreateNewFile(AccountID.ToString());
        }

        public void RecreateLoginFile()
        {
            if (HasLoginBackupData)
                Loginfile = LocalDatManager.CreateNewFileAutomated(AccountID.ToString(), Email, Password);
        }


        #region Plugininterface

        public class AccPluginCalls : PluginContracts.ObjectInterfaces.IAcc
        {
            private Account acc { set; get; }

            public AccPluginCalls(Account acc)
            {
                this.acc = acc;
            }

            public string Nickname { get { return Nickname; } }
            public int ID { get { return ID; } }

            public void Launch() => acc.Client.Launch();
            public void Suspend() => acc.Client.Suspend();
            public void Resume() => acc.Client.Resume();
            public void Maximize() => acc.Client.Maximize();
            public void Minimize() => acc.Client.Minimize();
            public void Focus() => acc.Client.Focus();
            public void Inject(string dllpath) => acc.Client.Inject(dllpath);
            public void Window_Move(int posx, int posy) => acc.Client.Window_Move(posx,posy);
            public void Window_Scale(int width, int height) => acc.Client.Window_Scale(width, height);
            public void Close() => acc.Client.Close();
            public PluginContracts.ObjectInterfaces.ClientStatus Status { get { return ((PluginContracts.ObjectInterfaces.ClientStatus)acc.Client.Status ); } }
        }



        #endregion Plugininterface
    }

    public class AccountInformation
    {
        public DateTime LastLogin { get; private set; }
        public DateTime LastClose { get; private set; }
        public TimeSpan Playtime { get; private set; }

        public AccountInformation()
        {
            LastLogin = DateTime.MinValue;
            LastClose = DateTime.MinValue;
        }

        public void SetLastLogin() { LastLogin = DateTime.Now; }
        public void SetLastClose()
        {
            LastClose = DateTime.Now;
            if (LastLogin != DateTime.MinValue) Playtime += LastClose - LastLogin;
        }

    }
}