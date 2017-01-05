using System;
using System.Collections.Generic;
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
        public string Name;
        public bool Registered = false;
        public string type;
        public string value;
        public List<string> Options= new List<string>();
    }

    public class GFXConfig
    {
        public string Configname;
        public string ConfigPath;
        public List<GFXOption> Config = new List<GFXOption>();
    }

}
