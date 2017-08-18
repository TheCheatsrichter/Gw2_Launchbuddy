using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Gw2_Launchbuddy
{
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
                ProcessStartInfo tmp = new ProcessStartInfo();
                tmp.Arguments = args;
                tmp.FileName = Path;
                tmp.WorkingDirectory = new FileInfo(tmp.FileName).Directory.FullName;
                return tmp;
            }
        }

        [System.Xml.Serialization.XmlElement("Multilaunch")]
        public bool IsMultilaunch { set; get; }

        [System.Xml.Serialization.XmlElement("Multibefore")]
        public bool IsMultibefore { set; get; }

        [System.Xml.Serialization.XmlElement("Singlelauch")]
        public bool IsSinglelaunch { set; get; }

        [System.Xml.Serialization.XmlElement("Enabled")]
        public bool IsEnabled { set; get; }

        [System.Xml.Serialization.XmlElement("Arguments")]
        public string args { set; get; }

        [System.Xml.Serialization.XmlElement("Path")]
        public string Path { set; get; }

        [XmlIgnore]
        public List<Process> ChildProcess = new List<Process>();

        public AddOn(string Name, ProcessStartInfo Info, bool IsMultilaunch, bool IsMultibefore, bool IsSinglelaunch, bool IsEnabled)
        {
            this.Name = Name;
            this.Info = Info;
            this.IsMultilaunch = IsMultilaunch;
            this.IsMultibefore = IsMultibefore;
            this.IsSinglelaunch = IsSinglelaunch;
            this.IsEnabled = IsEnabled;
            args = Info.Arguments;
            Path = Info.FileName;
        }

        public AddOn()
        {
        }
    }

    [XmlRootAttribute("AddonList")]
    public class AddonList
    {
        public ObservableCollection<AddOn> Addonlist { set; get; }

        private AddonList(ObservableCollection<AddOn> list)
        {
            Addonlist = list;
        }
    }

    public static class AddOnManager
    {
        public static ObservableCollection<AddOn> AddOns = new ObservableCollection<AddOn>();

        public static void Add(string name, string[] args, bool IsMultilaunch, bool IsMultibefore, bool IsSinglelaunch, bool IsEnabled)
        {
            if (name != "")
            {
                System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
                filedialog.Multiselect = false;
                filedialog.DefaultExt = "exe";
                filedialog.Filter = "Exe Files (*.exe)|*.exe|All files (*.*)|*.*";//"Exe Files(*.exe) | *.exe | XPR64 Files(*.xpr64)| *.xpr64";
                filedialog.ShowDialog();

                if (filedialog.FileName != "" && !AddOns.Any(a => a.Name == name))
                {
                    ProcessStartInfo ProInfo = new ProcessStartInfo();
                    ProInfo.FileName = filedialog.FileName;
                    ProInfo.Arguments = String.Join(" ", args);
                    ProInfo.WorkingDirectory = Path.GetDirectoryName(filedialog.FileName);
                    AddOns.Add(new AddOn(name, ProInfo, IsMultilaunch, IsMultibefore, IsSinglelaunch, IsEnabled));
                }
                else
                {
                    MessageBox.Show("Name already exist!");
                }
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
                foreach (AddOn addon in AddOns.Where(a => a.Name == pro.ProcessName))
                {
                    addon.ChildProcess.Add(pro);
                }
            }
        }

        public static void UpdateList()
        {
            CheckExisting();
            Process[] processes = Process.GetProcesses();

            foreach (AddOn addon in AddOns)
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

        public static void CloseAll()
        {
            foreach (AddOn addon in AddOns)
            {
                foreach (Process childpro in addon.ChildProcess.ToList())
                {
                    childpro.Kill();
                }
            }
        }

        public static string ListAddons(string seperator = ", ")
        {
            return String.Join(seperator, AddOns.Select(a => a.Name));
        }

        public static void Remove(string name)
        {
            try
            {
                foreach (AddOn addon in AddOns.Where(a => a.Name == name))
                {
                    AddOns.Remove(addon);
                }
            }
            catch { }
        }

        public static void LaunchSingle(string name)
        {
            AddOn addon = AddOns.FirstOrDefault(a => a.Name == name);
            if (addon.IsEnabled)
            {
                Process addon_pro = new Process { StartInfo = addon.Info };
                addon.ChildProcess.Add(addon_pro);
                addon_pro.Start();
            }
        }

        public static void LaunchAllBefore()
        {
            UpdateList();
            foreach (AddOn addon in AddOns)
            {
                if (addon.IsEnabled)
                {
                    if ((addon.IsMultilaunch && addon.IsMultibefore == true) && !addon.IsSinglelaunch)
                    {
                        try
                        {
                            Process addon_pro = new Process { StartInfo = addon.Info };
                            addon.ChildProcess.Add(addon_pro);
                            addon_pro.Start();
                            addon_pro.WaitForInputIdle(1000);
                        }
                        catch
                        {
                            MessageBox.Show(addon.Name + " could not be started!\nMake sure that the entered path is correct!");
                        }
                    }
                }
            }
        }

        public static void LaunchAllAfter()
        {
            UpdateList();
            foreach (AddOn addon in AddOns)
            {
                if (addon.IsEnabled)
                {
                    if ((addon.IsMultilaunch && addon.IsMultibefore == false) && !addon.IsSinglelaunch)
                    {
                        try
                        {
                            Process addon_pro = new Process { StartInfo = addon.Info };
                            addon.ChildProcess.Add(addon_pro);
                            addon_pro.Start();
                            addon_pro.WaitForInputIdle(1000);
                        }
                        catch
                        {
                            MessageBox.Show(addon.Name + " could not be started!\nMake sure that the entered path is correct!");
                        }
                    }
                }
            }
        }

        public static void SingletonAddon()
        {
            CheckExisting();
            foreach (AddOn addon in AddOns)
            {
                if (addon.IsEnabled)
                {
                    if ((addon.ChildProcess.Count < 1 && addon.IsSinglelaunch) && !(addon.IsMultibefore || addon.IsMultilaunch))
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
        }

        public static void SaveAddons(string path)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(ObservableCollection<AddOn>));
                TextWriter writer = new StreamWriter(path);
                x.Serialize(writer, AddOns);

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
                    AddOns = addons;
                    return addons;

                    /*
                    using (Stream stream = System.IO.File.Open(path, FileMode.Open))
                    {
                        var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        ObservableCollection<AddOn> addonlist = (ObservableCollection<AddOn>)bformatter.Deserialize(stream);

                        return addonlist;
                    }
                    */
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
}