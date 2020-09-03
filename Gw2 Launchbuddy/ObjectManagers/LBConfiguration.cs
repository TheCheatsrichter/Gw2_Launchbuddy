using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class LBConfiguration
    {
        public static LBConfigDataSet Config = new LBConfigDataSet();


        public static void Load()
        {
            Config = GetDataFromFile();
        }

        public static void Save()
        {
            SaveDataToFile(Config);
        }

        public static void Reset()
        {
            Config = new LBConfigDataSet();
        }

        private static LBConfigDataSet GetDataFromFile()
        {
            if (File.Exists(EnviromentManager.LBConfigPath))
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(LBConfigDataSet));

                    using (StringReader sr = new StringReader(File.ReadAllText(EnviromentManager.LBConfigPath)))
                    {
                        return (LBConfigDataSet)ser.Deserialize(sr);
                    }
                }
                catch
                {
                    MessageBox.Show("LB Configuration file could not be imported. Returning to default settings");
                    return new LBConfigDataSet();
                }
            }
            return new LBConfigDataSet();
        }


        private static void SaveDataToFile(LBConfigDataSet ObjectToSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                File.WriteAllText(EnviromentManager.LBConfigPath, textWriter.ToString());
            }
        }

    }

    [Serializable]
    public class LBConfigDataSet
    {
        //This is an ugly solution, but a fast one as it made a minimal compatibility impact
        //Help is wanted if you would like to create a proper Configfile and Manager

        public string cinema_imagepath;
        public string cinema_maskpath;
        public string cinema_musicpath;

        public bool cinema_use =false;
        public bool cinema_video =false;
        public bool cinema_slideshow =true;

        public string cinema_videopath="";
        public double mediaplayer_volume =90;

        public System.Windows.Media.Color cinema_backgroundcolor = System.Windows.Media.Color.FromRgb(0,0,0);
        public string cinema_loginwindowpath;
        public int cinema_slideshowendpos =30;
        public double cinema_slideshowendscale =1.205;
        public int counter_launches =0;
        public bool notifylbupdate =true;
        public bool useinstancegui =true;
        public int ui_selectedtab =0;
        public bool useloadingui =true;
        public double mainwin_pos_x =-1;
        public double mainwin_pos_y =-1;
        public double mainwin_size_x =-1;
        public double mainwin_size_y =-1;
        public List<string> plugins_toremove;
        public List<string> plugins_todelete;
        public List<string> plugins_toinstall;
        public List<string> plugins_toupdate;
        public bool plugins_autoupdate =false;
        public string instancegui_windowsettings ="0,0,150,300"; // should be Truple, kept as string for minimal compatibility impact
        public bool instancegui_ispinned =false;
        public bool autoupdatedatfiles =true;
        public string taco_path;
    }
}
