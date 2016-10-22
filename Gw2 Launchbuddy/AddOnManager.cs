using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.NetworkInformation;
using System.Xml;
using System.IO;
using System.Diagnostics;
using IWshRuntimeLibrary;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.Text;


namespace Gw2_Launchbuddy
{
    public class AddOn
    {
        public string Name { set; get; }
        public ProcessStartInfo Info{ set; get; }
        public bool IsMultilaunch { set; get; }
        public bool IsOverlay { set; get; }
        public List<Process> ChildProcess = new List<Process>();

        public AddOn(string Name,ProcessStartInfo Info, bool IsMultilaunch,bool IsOverlay)
        {
            Name = this.Name;
            Info = this.Info;
            IsMultilaunch = this.IsMultilaunch;
            IsOverlay = this.IsOverlay;
        }
    }

    class AddOnManager
    {
        public List<AddOn> AddOns = new List<AddOn>();

        void Add(string name,string[] args,bool IsMultilaunch,bool IsOverlay)
        {
            System.Windows.Forms.OpenFileDialog filedialog = new System.Windows.Forms.OpenFileDialog();
            filedialog.Multiselect = false;
            filedialog.DefaultExt = "exe";
            filedialog.Filter = "Exe Files(*.exe) | *.exe";
            filedialog.ShowDialog();

            if (filedialog.FileName != "" && AddOns.Any(a => a.Name == name))
            {
                ProcessStartInfo ProInfo = new ProcessStartInfo();
                ProInfo.FileName = filedialog.FileName;
                ProInfo.Arguments = String.Join(" ", args);
                ProInfo.WorkingDirectory = Path.GetDirectoryName(filedialog.FileName);

                AddOns.Add(new AddOn(name, ProInfo, IsMultilaunch, IsOverlay));     
            }
        }


        void CheckExisting()
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

        void UpdateList()
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


        void Remove(string name)
        {
            foreach (AddOn addon in AddOns.Where(a => a.Name == name))
            {
                AddOns.Remove(addon);
            }
        }

        void LaunchSingle(string name)
        {
            AddOn addon = AddOns.FirstOrDefault(a => a.Name == name);
            Process addon_pro = new Process { StartInfo = addon.Info };
            addon.ChildProcess.Add(addon_pro);
            addon_pro.Start();
        }

        void LaunchAll()
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
