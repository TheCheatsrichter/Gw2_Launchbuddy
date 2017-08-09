using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gw2_Launchbuddy
{
    public static class AccountManager
    {
        private static List<Account> accountList = new List<Account>();
        public static int Count => accountList.Count;
        public static List<Account> ToList() => accountList;

        public static Account Account(string Nickname)
        {
            return accountList.Where(a => a.Nickname == Nickname).Single();
        }

        public static Account Add(Account Account)
        {
            accountList.Add(Account);
            return Account;
        }
        public static Account Add(string Nickname, string Email, string Password)
        {
            return Add(new Account(Nickname, Email, Password));
        }

        public static void Remove(this Account Account)
        {
            accountList.Remove(Account);
        }
    }

    [Serializable()]
    public class Account
    {
        private string email;
        private string password;

        public string Nickname { get; set; }
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
                AccountArgumentManager.ArgumentNoCreate(this, "-email")?.WithOptionString(value);
            }
        }
        public string Password {
            get
            {
                return password;
            }
            set
            {
                password = value;
                AccountArgumentManager.ArgumentNoCreate(this, "-password")?.WithOptionString(value);
            }
        }

        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public DateTime RunDate { get; set; }

        public Account IsSelected(bool Selected = true) { this.Selected = Selected; return this; }
        
        public bool Selected { get; set; }

        public Account(string Nickname, string Email, string Password)
        {
            this.Nickname = Nickname;
            this.Email = Email;
            this.Password = Password;
            this.CreateDate = DateTime.Now;
            this.ModifyDate = DateTime.Now;
        }

        [NonSerialized]
        private string configurationPath;

        public string ConfigurationPath
        {
            get
            {
                return String.IsNullOrWhiteSpace(configurationPath) ? "Default" : configurationPath;
            }
            set
            {
                configurationPath = value;
            }
        }

        public string ConfigurationName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(ConfigurationPath);
            }
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
            get
            {
                return icon ?? defaultIcon;
            }
            private set
            {
                icon = value;
            }
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

        public AccountArgument Argument(string Flag)
        {
            return AccountArgumentManager.Argument(this, Flag);
        }

        public List<AccountArgument> GetArgumentList()
        {
            return AccountArgumentManager.GetAccountArguments(this);
        }

        public string PrintArguments()
        {
            return String.Join(" ", GetArgumentList().Where(a => a.Selected == true).Select(a => a.Argument.Flag + (a.Argument.Sensitive ? null : " " + a.OptionString)));
        }
        public string CommandLine(Account account = null)
        {
            string output = String.Join(" ", GetArgumentList().Where(a => a.Selected == true).Select(a => a.Argument.Flag + " " + a.OptionString));
            return output;
        }
    }
    /*[Serializable()]
    public class AccountOld
    {
        [NonSerialized]
        private ImageSource icon;
        public ImageSource Icon
        {
            get
            {
                if (icon == null)
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

                        return bitmapimage;
                    }
                }
                return icon;
            }
            set
            {
                icon = value;
            }
        }
        public string Iconpath
        {
            get
            {
                return iconpath;
            }
            set
            {
                if (System.IO.File.Exists(value))
                {
                    //Icon = LoadImage(value);
                }
                iconpath = value;
            }
        }
        private string iconpath { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime Time { get; set; }
        public string Nick { get; set; }

        public string Configpath
        {
            set
            {
                configpath = value;
                Configname = Path.GetFileNameWithoutExtension(value);
            }
            get
            {
                if (configpath != "" && configpath != null)
                    return configpath;
                return "Default";
            }
        }

        private string configpath { set; get; }
        public string Configname { set; get; }
    }*/
}
