using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Gw2_Launchbuddy
{
    public static class GFXManager
    {
        //Options to skip
        private static List<string> SkippedOptions = new List<string>
        {
            "gamma",
        };

        static public GFXConfig LoadFile(string path)
        {
            return ReadFile(path);
        }

        static public bool IsValidGFX(string path)
        {
            if (!File.Exists(path)) return false;
            if (!(Path.GetExtension(path) == ".xml")) return false;
            // TODO: Check for formatting errors

            return true;
        }

        public static void SaveFile(GFXConfig Config)
        {
            SaveFileDialog savediag = new System.Windows.Forms.SaveFileDialog();
            savediag.DefaultExt = ".xml";
            savediag.Filter = "XML Files(*.xml)|*.xml";
            savediag.Title = "Saving GFX Settings";
            savediag.AddExtension = true;
            savediag.FileName = "GW2 Custom GFX";
            savediag.InitialDirectory = EnviromentManager.GwClientPath;
            savediag.ShowDialog();

            if (savediag.FileName != "") ToXml(savediag.FileName,Config);
        }

        private static List<string> GetHeader()
        {
            IORepeater.WaitForFileAvailability(EnviromentManager.GwClientXmlPath);
            List<string> header = new List<string>();
            string[] lines = System.IO.File.ReadAllLines(EnviromentManager.GwClientXmlPath);
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
            IORepeater.WaitForFileAvailability(EnviromentManager.GwClientXmlPath);
            List<string> foot = new List<string>();
            foot.Add("</GAMESETTINGS>");
            foot.Add("</GSA_SDK>");
            return foot;
        }

        public static void UseGFX(GFXConfig config)
        {
            try
            {
                IORepeater.WaitForFileAvailability(EnviromentManager.GwClientXmlPath);
                string path = EnviromentManager.TMP_GFXConfig;
                ToXml(path, config);

                IORepeater.WaitForFileAvailability(EnviromentManager.GwClientXmlPath);

                if (!File.Exists(path))
                {
                    MessageBox.Show("GFX Setting could not be created! Please check GFX settings!");
                }
                if (File.Exists(path))
                {
                    if (File.Exists(EnviromentManager.GwClientXmlPath))
                    {
                        IORepeater.FileDelete(EnviromentManager.TMP_BackupGFXConfig);
                        IORepeater.FileMove(EnviromentManager.GwClientXmlPath, EnviromentManager.TMP_BackupGFXConfig);
                    }
                    IORepeater.FileMove(path, EnviromentManager.GwClientXmlPath);
                }
            }
            catch (Exception e)
            {
                throw new Exception(EnviromentManager.Create_Environment_Report() + "\n" + e.Message);
            }

        }

        public static void RestoreDefault()
        {
            try
            {
                IORepeater.WaitForFileAvailability(EnviromentManager.GwClientXmlPath);
                if (File.Exists(EnviromentManager.TMP_BackupGFXConfig))
                {
                    if (File.Exists(EnviromentManager.GwClientXmlPath)) IORepeater.FileDelete(EnviromentManager.GwClientXmlPath);
                    IORepeater.FileMove(EnviromentManager.TMP_BackupGFXConfig, EnviromentManager.GwClientXmlPath);
                }
                if (File.Exists(EnviromentManager.TMP_GFXConfig)) IORepeater.FileDelete(EnviromentManager.TMP_GFXConfig);
            }

            catch (Exception e)
            {
                throw new Exception(EnviromentManager.Create_Environment_Report() + "\n" + e.Message);
            }
        }

        public static string[] ToXml(string dest,GFXConfig GFXConfig)
        {
            //ToDo: Add Resolution Option and Gamma Slider
            List<string> XmlFormat = new List<string>();
            XmlFormat.AddRange(GetHeader());
            foreach (GFXOption option in GFXConfig.Config)
            {
                XmlFormat.AddRange(option.ToXml());
            }

            XmlFormat.AddRange(GetFoot());
            System.IO.File.WriteAllLines(dest, XmlFormat);
            return XmlFormat.ToArray();
        }

        public static void OverwriteGFX(GFXConfig config)
        {
            ToXml(EnviromentManager.GwClientXmlPath,config);
        }

        static public GFXConfig ReadFile(string path)
        {

            GFXConfig tmp_conf = new GFXConfig();
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

                tmp_conf.Config.Add(gfxoption);
            }
            return tmp_conf;
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

        [XmlIgnore]
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
        public ObservableCollection<GFXOption> Config { get; set; }
        public GFXConfig()
        {
            Config = new ObservableCollection<GFXOption>();
        }
    }
}