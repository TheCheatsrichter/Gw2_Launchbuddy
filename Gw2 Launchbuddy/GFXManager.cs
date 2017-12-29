using Gw2_Launchbuddy.ObjectManagers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Gw2_Launchbuddy
{
    public static class GFXManager
    {
        private static GFXConfig CurrentConfig = new GFXConfig();

        //Options to skip
        private static List<string> SkippedOptions = new List<string>
        {
            "gamma",
        };

        static public GFXConfig LoadFile(string path)
        {
            CurrentConfig.ConfigPath = path;
            return ReadFile(CurrentConfig.ConfigPath);
        }

        static public bool IsValidGFX(string path)
        {
            if (!File.Exists(path)) return false;
            if (!(Path.GetExtension(path) == ".xml")) return false;
            // TODO: Check for formatting errors

            return true;
        }

        public static void SaveFile()
        {
            SaveFileDialog savediag = new System.Windows.Forms.SaveFileDialog();
            savediag.DefaultExt = ".xml";
            savediag.Filter = "XML Files(*.xml)|*.xml";
            savediag.Title = "Saving GFX Settings";
            savediag.AddExtension = true;
            savediag.FileName = "GW2 Custom GFX";
            savediag.InitialDirectory = ClientManager.ClientInfo.InstallPath;
            savediag.ShowDialog();

            if (savediag.FileName != "") ToXml(savediag.FileName);
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

        public static void UseGFX(string path)
        {
            if (path == null || path == "")
                return;

            if (!File.Exists(path) && path != "Default")
            {
                MessageBox.Show("GFX Setting not found!\n\nPath:" + path + " \n\nLaunching with default settings!");
                path = "Default";
            }

            if (path == "Default" && File.Exists(Globals.AppDataPath + "GFX_tmp.xml"))
            {
                if (File.Exists(Globals.ClientXmlpath)) File.Delete(Globals.ClientXmlpath);
                File.Move(Globals.AppDataPath + "GFX_tmp.xml", Globals.ClientXmlpath);
            }

            if (File.Exists(path) && Path.GetExtension(path) == ".xml")
            {
                if (!File.Exists(Globals.AppDataPath + "GFX_tmp.xml"))
                {
                    File.Move(Globals.ClientXmlpath, Globals.AppDataPath + "GFX_tmp.xml");
                }
                if (File.Exists(Globals.ClientXmlpath))
                {
                    File.Delete(Globals.ClientXmlpath);
                }
                File.Copy(path, Globals.ClientXmlpath);
            }
        }

        public static void RestoreDefault()
        {
            if (File.Exists(Globals.AppDataPath + "GFX_tmp.xml"))
            {
                if (File.Exists(Globals.ClientXmlpath)) File.Delete(Globals.ClientXmlpath);
                File.Move(Globals.AppDataPath + "GFX_tmp.xml", Globals.ClientXmlpath);
            }
        }

        public static string[] ToXml(string dest)
        {
            //ToDo: Add Resolution Option and Gamma Slider
            List<string> XmlFormat = new List<string>();
            XmlFormat.AddRange(GetHeader());
            foreach (GFXOption option in CurrentConfig.Config)
            {
                XmlFormat.AddRange(option.ToXml());
            }

            XmlFormat.AddRange(GetFoot());
            System.IO.File.WriteAllLines(dest, XmlFormat);
            return XmlFormat.ToArray();
        }

        public static void OverwriteGFX()
        {
            ToXml(Globals.ClientXmlpath);
        }

        static public GFXConfig ReadFile(string path)
        {
            CurrentConfig.Config.Clear();
            CurrentConfig.Configname = Path.GetFileNameWithoutExtension(path);
            CurrentConfig.ConfigPath = path;
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
                gfxoption.Value = node.Attributes["Value"].Value;
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

                CurrentConfig.Config.Add(gfxoption);
            }
            return CurrentConfig;
        }
    }

    public class GFXOption
    {
        public List<string> ToXml()
        {
            List<string> output = new List<string>();

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
            get
            {
                IEnumerable<string> tmp = Options;
                return tmp;
            }
        }
    }

    public class GFXConfig
    {
        public bool issaved = false;
        public string Configname { set; get; }
        public string ConfigPath { set; get; }
        public ObservableCollection<GFXOption> Config = new ObservableCollection<GFXOption>();
    }
}