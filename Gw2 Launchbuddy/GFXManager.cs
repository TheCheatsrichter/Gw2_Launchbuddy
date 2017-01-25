using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace Gw2_Launchbuddy
{

    public static class GFXManager
    {
        static string xmlpath = "";
        static GFXConfig CurrentConfig = new GFXConfig();

        //Options to skip
        static List<string> SkippedOptions = new List<string>
        {
            "gamma",
        };
        

        static public void ChooseFile()
        {
            OpenFileDialog filediag = new OpenFileDialog { Multiselect = false,DefaultExt="xml" };
            xmlpath = "";

            //ToDo: No infinite loop when choosing wrong file
            while (!IsValidGFX(xmlpath))
            {
                filediag.ShowDialog();
                xmlpath = filediag.FileName;   
            }
            ReadFile(xmlpath);
        }

        static public bool IsValidGFX(string path)
        {
            if (!File.Exists(path)) return false;
            if (!(Path.GetExtension(path) == ".xml")) return false;
            // TODO: Check for formatting errors

            return true;
        }


        private static List<string> GetHeader()
        {
            List<string> header = new List<string>();
            string[] lines = System.IO.File.ReadAllLines(Globals.ClientXmlpath);
            foreach (string line in lines)
            {
                header.Add(line);
                string norm = Regex.Replace(line, @"\s", "");
                if (norm == "<GAMESETTINGS>") break;
            }
            return header;
        }

        private static List<string> GetFoot()
        {
            List<string> foot = new List<string>();
            foot.Add("</GAMESETTINGS>");
            foot.Add("</GSA_SDK>");
            return foot;
        }

        public static string[] ToXml()
        {
            //ToDo: Add Resolution Option and Gamma Slider
            List<string> XmlFormat = new List<string>();
            XmlFormat.AddRange(GetHeader());
            foreach (GFXOption option in CurrentConfig.Config)
            {
                XmlFormat.AddRange(option.ToXml());
            }

            XmlFormat.AddRange(GetFoot());
            System.IO.File.WriteAllLines("test.xml", XmlFormat);
            return XmlFormat.ToArray();
        }

        static public GFXConfig ReadFile(string path)
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
                if (SkippedOptions.Contains<string>(gfxoption.Name))
                {
                    continue;
                }
                gfxoption.type = node.Attributes["Type"].Value;
                gfxoption.Registered = node.Attributes["Registered"].Value == "True";
                gfxoption.Value= node.Attributes["Value"].Value;
                gfxoption.OldValue = gfxoption.Value;

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
            CurrentConfig = config;
            return config;
        }

    
    }

    public class GFXOption
    {
        public List<string> ToXml()
        {
            List<string>output = new List<string>();

            string head = "";
            head += "<OPTION ";
            head += "Name=\"" + Name + "\" ";
            head += "Registered=\"" + Registered.ToString() + "\" ";
            head += "Type=\"" + type + "\" ";
            head += "Value=\"" + Value + "\">";
            output.Add(head);

            foreach (string option in Options)
            {
                output.Add("\t<" + "ENUM EnumValue=\"" + option + "\"/>");
            }

            output.Add("</OPTION>");

            return output;
        }

        public string Name { set; get; }
        public bool Registered { set; get; }
        public string type { set; get; }
        public string Value { set; get; }
        public string OldValue { set; get; }
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
