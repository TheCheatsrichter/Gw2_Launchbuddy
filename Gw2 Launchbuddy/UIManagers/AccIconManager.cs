using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Media.Imaging;
using Gw2_Launchbuddy.ObjectManagers;
using System.Linq;
using System.Drawing;

namespace Gw2_Launchbuddy.UI_Managers
{
    public static class AccIconManager
    {
        public static ObservableCollection<Bitmap> Icons { get; set; }

        private static void AddBaseIcons()
        {
            string[] embeddedResources = Assembly.GetAssembly(typeof(Image)).GetManifestResourceNames();
            foreach (var icon in embeddedResources.Where(a=>a.Substring(0,2)=="c_"))
            {
                Icons.Add(new Bitmap(icon));
            }
        }

        public static void LoadCustomIcons()
        {
            foreach(Account acc in AccountManager.Accounts)
            {
                Icons.Add(acc.Settings.Icon);
            }
        }

        public static void Init()
        {
            AddBaseIcons();
            LoadCustomIcons();
        }
    }
}
