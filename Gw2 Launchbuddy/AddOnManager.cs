using System;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Gw2_Launchbuddy
{
    public class AddOn
    {
        public string Name { set; get; }
        public ProcessStartInfo Info{ set; get; }
        public bool IsMultilaunch { set; get; }
        public bool IsOverlay { set; get; }
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

        public AddOn(string Name,ProcessStartInfo Info, bool IsMultilaunch,bool IsOverlay)
        {
            this.Name = Name;
            this.Info= Info;
            this.IsMultilaunch= IsMultilaunch;
            this.IsOverlay=IsOverlay;
        }
    }

    class AddOnManager
    {
        public ObservableCollection<AddOn> AddOns = new ObservableCollection<AddOn>();

        public void Add(string name,string[] args,bool IsMultilaunch,bool IsOverlay)
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
                    AddOns.Add(new AddOn(name, ProInfo, IsMultilaunch, IsOverlay));
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
                Process addon_pro = new Process { StartInfo = addon.Info };
                addon.ChildProcess.Add(addon_pro);
                addon_pro.Start();
            }
        }

    }
}
