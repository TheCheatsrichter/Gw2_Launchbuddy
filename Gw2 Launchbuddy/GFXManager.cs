using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace Gw2_Launchbuddy
{
    public class GFXManager
    {
        string xmlpath = "";
        GFXConfig CurrentConfig = new GFXConfig();

        public GFXManager(string xmlpath)
        {
            this.xmlpath = xmlpath;
        }

        public void ChooseFile()
        {
            OpenFileDialog filediag = new OpenFileDialog { Multiselect = false,DefaultExt="xml" };
            while (!File.Exists(xmlpath))
            {
                filediag.ShowDialog();
                xmlpath = filediag.FileName;
            }
        }

        public List<ListViewItem> ConfigToListview(GFXConfig config)
        {
            ReadFile(xmlpath);
            List<ListViewItem> items = new List<ListViewItem>() ;
            foreach (GFXOption option in config.Config)
            {
                items.Add(new ListViewItem(option.Name));
                //items.Add(new ComboBox());
            }
            return items;
            
        }

        public GFXConfig ReadFile(string path)
        {
            GFXConfig config = new GFXConfig();
            config.Configname = Path.GetFileNameWithoutExtension(path);
            config.ConfigPath = path;
            var xmlfile = new XmlDocument();
            xmlfile.Load(path);
            foreach (XmlNode node in xmlfile.SelectNodes("//OPTION"))
            {
                GFXOption gfxoption = new GFXOption();
                gfxoption.Name = node.Attributes["Name"].Value;
                gfxoption.type = node.Attributes["Type"].Value;
                gfxoption.Registered = node.Attributes["Registered"].Value == "True";
                gfxoption.value= node.Attributes["Value"].Value;

                if (node.ChildNodes.Count == 0 && gfxoption.type == "Bool") 
                {
                    gfxoption.Options.Add("true");
                    gfxoption.Options.Add("false");
                }


                foreach (XmlNode childnode in node.ChildNodes)
                {
                    switch (childnode.Name)
                    {
                        default: break;

                        case "ENUM":
                            gfxoption.Options.Add(childnode.Attributes["EnumValue"].Value);
                            break;

                    }  
                }

                config.Config.Add(gfxoption);
            }
            return config;
        }

    
    }

    public class GFXOption
    {
        public string Name { set; get; }
        public bool Registered { set; get; }
        public string type { set; get; }
        public string value { set; get; }
        public List<string> Options = new List<string>();
        public IEnumerable<string> IEOptions
        {
            set { IEOptions = value; }
            get {IEnumerable<string> tmp = Options;
                return tmp;
            }
        }
    }

    public class GFXConfig
    {
        public string Configname { set; get; }
        public string ConfigPath { set; get; }
        public ObservableCollection<GFXOption> Config= new ObservableCollection<GFXOption>();
    }


}
