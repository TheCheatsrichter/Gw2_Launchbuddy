using System;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Gw2_Launchbuddy
{
    [Serializable]
    public class AddOn
    {
        public string Name { set; get; }
        public ProcessStartInfo Info{ set; get; }
        public bool IsMultilaunch { set; get; }
        public bool IsLbAddon { set; get; }
        public string args {
            set { }
            get { return Info.Arguments; }
        }
        public string Path {
            set { }
            get {
                return Info.FileName;
            }
        }
        public List<Process> ChildProcess = new List<Process>();

        public AddOn(string Name,ProcessStartInfo Info, bool IsMultilaunch,bool IsLbAddon)
        {
            this.Name = Name;
            this.Info= Info;
            this.IsMultilaunch= IsMultilaunch;
            this.IsLbAddon=IsLbAddon;
        }
    }

    class AddOnManager
    {
        
        public ObservableCollection<AddOn> AddOns = new ObservableCollection<AddOn>();

        public void Add(string name,string[] args,bool IsMultilaunch,bool IsLbAddon)
        {
            if(name != "")
            {
                System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
                filedialog.Multiselect = false;
                filedialog.DefaultExt = "exe";
                filedialog.Filter = "Exe Files(*.exe) | *.exe";
                filedialog.ShowDialog();

                if (filedialog.FileName != "" && !AddOns.Any(a => a.Name == name))
                {
                    ProcessStartInfo ProInfo = new ProcessStartInfo();
                    ProInfo.FileName = filedialog.FileName;
                    ProInfo.Arguments = String.Join(" ", args);
                    ProInfo.WorkingDirectory = Path.GetDirectoryName(filedialog.FileName);
                    AddOns.Add(new AddOn(name, ProInfo, IsMultilaunch, IsLbAddon));
                }
            } else
            {
                MessageBox.Show("Please enter a name!");
            }

            
        }


        public void CheckExisting()
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

        public void UpdateList()
        {
            CheckExisting();

            Process[] processes = Process.GetProcesses();

            foreach(AddOn addon in AddOns)
            {
                foreach(Process childpro in addon.ChildProcess)
                {
                    if (!processes.Contains(childpro))
                    {
                        addon.ChildProcess.Remove(childpro);
                    }
                }  
            }

        }


        public void Remove(string name)
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

        public void LaunchSingle(string name)
        {
            AddOn addon = AddOns.FirstOrDefault(a => a.Name == name);
            Process addon_pro = new Process { StartInfo = addon.Info };
            addon.ChildProcess.Add(addon_pro);
            addon_pro.Start();
        }

        public void LaunchAll()
        {
            foreach (AddOn addon in AddOns)
            {
                if (addon.IsMultilaunch || addon.ChildProcess.Count <= 0)
                {
                    Process addon_pro = new Process { StartInfo = addon.Info };
                    addon.ChildProcess.Add(addon_pro);
                    addon_pro.Start();
                }
            }
        }

        public void LaunchLbAddons()
        {
            foreach (AddOn addon in AddOns)
            {
                if((addon.IsMultilaunch ||  addon.ChildProcess.Count <= 0) && addon.IsLbAddon)
                {
                    Process addon_pro = new Process { StartInfo = addon.Info };
                    addon.ChildProcess.Add(addon_pro);
                    addon_pro.Start();
                }
            }
        }


        public void SaveAddons(string path)
        {
            try
            {
                using (Stream stream = System.IO.File.Open(path, FileMode.Create))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bformatter.Serialize(stream, AddOns);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        public ObservableCollection<AddOn> LoadAddons(string path)
        {
            try
            {

                if (System.IO.File.Exists(path) == true)
                {
                    using (Stream stream = System.IO.File.Open(path, FileMode.Open))
                    {
                        var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        ObservableCollection<AddOn> addonlist = (ObservableCollection<AddOn>)bformatter.Deserialize(stream);

                        return addonlist;
                    }
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
