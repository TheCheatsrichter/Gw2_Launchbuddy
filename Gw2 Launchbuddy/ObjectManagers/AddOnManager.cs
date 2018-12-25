using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class AddOnManager
    {
        public static ObservableCollection<AddOn> addOnCollection { get; set; }
        public static ObservableCollection<AddOn> AddOnCollection { get; set; }

        static AddOnManager()
        {
            addOnCollection = new ObservableCollection<AddOn>();
            AddOnCollection = new ObservableCollection<AddOn>(addOnCollection);
        }

        public static void Add(string name, string[] args, bool IsMultilaunch, bool IsLbAddon)
        {
            if (name != "")
            {
                Builders.FileDialog.DefaultExt(".exe")
                    .Filter("EXE Files(*.exe)|*.exe")
                    .ShowDialog((Helpers.FileDialog fileDialog) =>
                    {
                        if (fileDialog.FileName != "" && !addOnCollection.Any(a => a.Name == name))
                        {
                            ProcessStartInfo ProInfo = new ProcessStartInfo();
                            ProInfo.FileName = fileDialog.FileName;
                            ProInfo.Arguments = String.Join(" ", args);
                            ProInfo.WorkingDirectory = Path.GetDirectoryName(fileDialog.FileName);
                            addOnCollection.Add(new AddOn(name, ProInfo, IsMultilaunch, IsLbAddon));
                        }
                    });
            }
            else
            {
                MessageBox.Show("Please enter a name!");
            }
        }

        public static void CheckExisting()
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process pro in processes)
            {
                foreach (AddOn addon in addOnCollection.Where(a => a.Name == pro.ProcessName))
                {
                    addon.ChildProcess.Add(pro);
                }
            }
        }

        public static void UpdateList()
        {
            CheckExisting();
            Process[] processes = Process.GetProcesses();

            foreach (AddOn addon in addOnCollection)
            {
                foreach (Process childpro in addon.ChildProcess.ToList())
                {
                    if (!processes.Contains(childpro))
                    {
                        addon.ChildProcess.Remove(childpro);
                    }
                }
            }
        }

        public static string ListAddons(string seperator = ", ")
        {
            return String.Join(seperator, addOnCollection.Select(a => a.Name));
        }

        public static void Remove(string name)
        {
            try
            {
                foreach (AddOn addon in addOnCollection.Where(a => a.Name == name))
                {
                    addOnCollection.Remove(addon);
                }
            }
            catch { }
        }

        public static void LaunchSingle(string name)
        {
            AddOn addon = addOnCollection.FirstOrDefault(a => a.Name == name);
            Process addon_pro = new Process { StartInfo = addon.Info };
            addon.ChildProcess.Add(addon_pro);
            addon_pro.Start();
        }

        public static void LaunchAll()
        {
            UpdateList();
            foreach (AddOn addon in addOnCollection)
            {
                if ((addon.IsMultilaunch || addon.ChildProcess.Count <= 0) && !addon.IsLbAddon)
                {
                    try
                    {
                        Process addon_pro = new Process { StartInfo = addon.Info };
                        addon.ChildProcess.Add(addon_pro);
                        addon_pro.Start();
                    }
                    catch
                    {
                        MessageBox.Show(addon.Name + " could not be started!\nMake sure that the entered path is correct!");
                    }
                }
            }
        }

        public static void LaunchLbAddons()
        {
            UpdateList();
            foreach (AddOn addon in addOnCollection)
            {
                if ((addon.IsMultilaunch || addon.ChildProcess.Count <= 0) && addon.IsLbAddon)
                {
                    try
                    {
                        Process addon_pro = new Process { StartInfo = addon.Info };
                        addon.ChildProcess.Add(addon_pro);
                        addon_pro.Start();
                    }
                    catch (Exception e)
                    {
                        CrashReporter.ReportCrashToAll(e);
                    }
                }
            }
        }

        public static void SaveAddons(string path)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(ObservableCollection<AddOn>));
                TextWriter writer = new StreamWriter(path);
                x.Serialize(writer, addOnCollection);

                /*
                using (Stream stream = System.IO.File.Open(path, FileMode.Create))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bformatter.Serialize(stream, AddOns);
                }
                */
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static ObservableCollection<AddOn> LoadAddons(string path)
        {
            try
            {
                if (System.IO.File.Exists(path) == true)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<AddOn>));

                    StreamReader reader = new StreamReader(path);
                    var addons = (ObservableCollection<AddOn>)serializer.Deserialize(reader);
                    reader.Close();
                    addOnCollection = addons;
                    return addons;
                }
                return null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
    }

    [Serializable]
    public class AddOn
    {
        [System.Xml.Serialization.XmlElement("Name")]
        public string Name { set; get; }

        [XmlIgnore]
        public ProcessStartInfo Info
        {
            set { }
            get
            {
                return new ProcessStartInfo
                {
                    Arguments = Args,
                    FileName = Path,
                    WorkingDirectory = new FileInfo(Path).Directory.FullName
                };
            }
        }

        [XmlElement("Multilaunch")]
        public bool IsMultilaunch { set; get; }

        [XmlElement("LbAddon")]
        public bool IsLbAddon { set; get; }

        [XmlElement("Arguments")]
        public string Args { set; get; }

        [XmlElement("Path")]
        public string Path { set; get; }

        [XmlIgnore]
        public List<Process> ChildProcess = new List<Process>();

        public AddOn(string Name, ProcessStartInfo Info, bool IsMultilaunch, bool IsLbAddon)
        {
            this.Name = Name;
            this.Info = Info;
            this.IsMultilaunch = IsMultilaunch;
            this.IsLbAddon = IsLbAddon;
            Args = Info.Arguments;
            Path = Info.FileName;
        }

        public AddOn()
        {
        }
    }

    [XmlRoot("AddonList")]
    public class AddonList
    {
        public ObservableCollection<AddOn> Addonlist { set; get; }

        private AddonList(ObservableCollection<AddOn> list)
        {
            Addonlist = list;
        }
    }

    
}