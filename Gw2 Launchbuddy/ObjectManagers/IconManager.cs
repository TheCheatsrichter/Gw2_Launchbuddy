using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public class Icon
    {
        public string Name { set; get; }
        [XmlIgnore]
        public BitmapImage Image { set { } get { if (Name != null)return new BitmapImage(new Uri(Name)); return null; } }

        public Icon(string name, BitmapImage image)
        {
            Name = name;
            Image = image;
        }

        private Icon() { if(Name!=null)Image = new BitmapImage(new Uri(Name)); }
    }

    public static class IconManager
    {
        public static ObservableCollection<Icon> Icons { get; set; }

        public static void Init()
        {
            Icons = new ObservableCollection<Icon>();
            if (!Directory.Exists(EnviromentManager.LBIconsPath))
                Directory.CreateDirectory(EnviromentManager.LBIconsPath);

            //Write Resource icons to appdata
            Assembly assembly = Assembly.GetExecutingAssembly();
            var resnames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            resnames = resnames.Where<string>(a => a.EndsWith("Resources.resources")).ToArray<string>();
            foreach (string resname in resnames)
            {
                ResourceSet set = new ResourceSet(assembly.GetManifestResourceStream(resname));
                foreach (DictionaryEntry resource in set)
                {
                    if (resource.Key.ToString().StartsWith("c_"))
                    {
                        if(!File.Exists(EnviromentManager.LBIconsPath+ resource.Key.ToString()+".png"))
                        {
                            Bitmap icon = Properties.Resources.ResourceManager.GetObject(resource.Key.ToString()) as Bitmap;
                            File.WriteAllBytes(EnviromentManager.LBIconsPath + resource.Key.ToString() + ".png", ImageToByte2(icon));
                        }
                    }
                }
            }

            foreach(string file in Directory.GetFiles(EnviromentManager.LBIconsPath))
            {
                Icons.Add(new Icon(file,new BitmapImage(new Uri(file,UriKind.Absolute))));
            }

        }

        public static void AddIcon()
        {
            Builders.FileDialog.DefaultExt(".png")
                   .Filter("PNG Files(*.png)|*.png")
                   .ShowDialog((Helpers.FileDialog fileDialog) =>
                   {
                       string name = Path.GetFileName(fileDialog.FileName);
                       if (!File.Exists(EnviromentManager.LBIconsPath+name))
                       {
                           File.Copy(fileDialog.FileName, EnviromentManager.LBIconsPath + name);
                           Icons.Add(new Icon(fileDialog.FileName,new BitmapImage(new Uri(fileDialog.FileName,UriKind.Absolute))));
                       }
                   });
        }
        public static void RemIcon(Icon icon)
        {
            if (Icons.Contains(icon))
            {
                Icons.Remove(icon);
                try
                {
                    File.Delete(icon.Name);
                }
                catch
                {

                }
            }

        }

        private static byte[] ImageToByte2(Bitmap img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

    }
}
