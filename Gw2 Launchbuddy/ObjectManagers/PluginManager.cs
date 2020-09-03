using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using PluginContracts;
using System.Windows;
using System.Security.Cryptography;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using PluginContracts.ObjectInterfaces;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public class Plugin_Wrapper:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public Assembly assembly { set; get; }

        private IPlugin plugin;
        public IPlugin Plugin { get { return plugin; } }

        public Plugin_Wrapper(IPlugin plugin,Assembly ass)
        {
            this.plugin = plugin;
            filehash = CalcFileHash();
            this.assembly = ass;
        }

        private string filehash;
        public string FileHash { get { return filehash; } }

        public Uri VirusTotalLink { get { return new Uri($"https://www.virustotal.com/en/file/{FileHash}/analysis/"); } }

        private bool willbeuninstalled=false;
        public bool WillBeUninstalled { set { willbeuninstalled = value; OnPropertyChanged("WillBeUninstalled"); } get { return willbeuninstalled; } }

        private string CalcFileHash()
        {
            SHA256 Sha256 = SHA256.Create();
            string filename = EnviromentManager.LBPluginsPath + plugin.PluginInfo.Name + ".dll";
            byte[] bytes = null;
            using (FileStream stream = File.OpenRead(filename))
            {
                bytes= Sha256.ComputeHash(stream);
            }
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }

        public bool UpdateNeeded { get { return !plugin.IsUpToDate; } }
    }

    public static class PluginManager
    {
        private static EventHandler HandlerLBStart;
        private static EventHandler HandlerLBClose;
        private static EventHandler<PluginContracts.EventArguments.ClientStatusEventArgs> HandlerClientStatusChanged;


        public static ObservableCollection<Plugin_Wrapper> InstalledPlugins { set; get; }

        public static void LoadPlugins()
        {
            string path = EnviromentManager.LBPluginsPath;
            string[] dllFileNames = null;
            if (Directory.Exists(path))
            {
                dllFileNames = Directory.GetFiles(path, "*.dll");
            }
            ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (string dllFile in dllFileNames)
            {
                try
                {
                    AppDomain dom = AppDomain.CreateDomain("Plugins");
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }
                catch
                {
                    continue;
                }

            }

            Type pluginType = typeof(IPlugin);
            ICollection<Type> pluginTypes = new List<Type>();
            InstalledPlugins = new ObservableCollection<Plugin_Wrapper>();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    try
                    {
                        Type[] types = assembly.GetTypes();
                        foreach (Type type in types)
                        {
                            if (type.IsInterface || type.IsAbstract)
                            {
                                continue;
                            }
                            else
                            {
                                if (type.GetInterface(pluginType.FullName) != null)
                                {
                                    IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                                    InstalledPlugins.Add(new Plugin_Wrapper(plugin,assembly));
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show($"Plugin {assembly.FullName} could not be loaded.\n{e.Message}");
                    }

                }
            }
        }

        public static void InitPlugins()
        {
            foreach(Plugin_Wrapper plugin in InstalledPlugins)
            {
                InitPlugin(plugin.Plugin);
            }
        }

        private static void InitPlugin(IPlugin plugin)
        {
            if(!plugin.Verify)
            {
                System.Windows.Forms.MessageBox.Show($"{plugin.PluginInfo.Name} verifycation failed. Please reinstall/update this plugin.");
                return;
            }
            if (plugin is ILBPlugin)
            {
                ILBPlugin lbplugin = plugin as ILBPlugin;

                HandlerLBStart += lbplugin.OnLBStart;
                HandlerLBClose += lbplugin.OnLBClose;

                HandlerClientStatusChanged += lbplugin.OnClientStatusChanged;
                lbplugin.Accounts = AccountManager.IAccs;
                lbplugin.Environment = new EnvironmentInfo
                {
                    LB_PluginsPath = EnviromentManager.LBPluginsPath,
                    GwClient_Version = EnviromentManager.GwClientVersion,
                    GwClient_UpToDate = EnviromentManager.GwClientUpToDate ?? false,
                    LB_LocaldatsPath = EnviromentManager.LBLocaldatsPath
                };
                lbplugin.Init();

                if(lbplugin.UIContent != null)EnviromentManager.MainWin.AddTabPlugin(lbplugin.UIContent);
            }
        }

        private class EnvironmentInfo : IEnvironment
        {
            public string LB_PluginsPath { get; set; }
            public string GwClient_Version { get; set; }
            public bool GwClient_UpToDate { get; set; }
            public string LB_LocaldatsPath { get; set; }
        }
        
        public static void AddPluginWithDialog()
        {
            Builders.FileDialog.DefaultExt(".dll")
               .Filter("dll Files(*.dll)|*.dll")
               .EnforceExt(".dll")
               .ShowDialog((Helpers.FileDialog fileDialog) =>
               {
                   if (fileDialog.FileName != null)
                   {
                       if (System.IO.File.Exists(fileDialog.FileName)) AddPlugin(fileDialog.FileName);
                   }
               });
        }

        public static void AddPlugin(string path)
        {
            if (File.Exists(path))
            {
                if(Path.GetExtension(path)==".dll")
                {
                    Assembly assembly=null;
                    try
                    {
                        AssemblyName an = AssemblyName.GetAssemblyName(path);
                        assembly = Assembly.Load(an);
                    }
                    catch
                    {
                        MessageBox.Show($"Could not load plugin from {path}");
                    }

                    if (assembly !=null)
                    {
                        try
                        {
                            ICollection<Type> pluginTypes = new List<Type>();
                            Type pluginType = typeof(IPlugin);
                            Type[] types = assembly.GetTypes();
                            foreach (Type type in types)
                            {
                                if (type.IsInterface || type.IsAbstract)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (type.GetInterface(pluginType.FullName) != null)
                                    {
                                        pluginTypes.Add(type);
                                    }
                                }
                            }

                            foreach (Type type in pluginTypes)
                            {
                                IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                                

                                //if(!plugin.Install())  MessageBox.Show($"Plugin {assembly.FullName} could not be installed successfully. Proceed with cation.");

                                if(plugin.Verify)
                                {
                                    File.Copy(path, EnviromentManager.LBPluginsPath + plugin.PluginInfo.Name+".dll");
                                    InstalledPlugins.Add(new Plugin_Wrapper(plugin,assembly));
                                    InitPlugin(plugin);
                                    return;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"Plugin {assembly.FullName} could not be loaded.\n{e.Message}");
                        }

                    }


                }
            }
        }

        public static void RemovePlugin(IPlugin pluginentry)
        {
            PluginContracts.PluginInfo plugininfo = pluginentry.PluginInfo;
            foreach(Plugin_Wrapper plugin in InstalledPlugins.Where(x=>x.Plugin.PluginInfo==plugininfo))
            {
                if(plugin.Plugin.Uninstall())
                {
                    UninstallPlugin(pluginentry);
                }
                else
                {
                    MessageBoxResult win = MessageBox.Show($"Plugin { plugin.Plugin.PluginInfo.Name}could not be removed completly clean. Uninstall it anyway?", "Client Retry", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (win.ToString() == "Yes")
                    {
                        UninstallPlugin(pluginentry);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            LBConfiguration.Save();
        }

        private static void UninstallPlugin(IPlugin pluginentry)
        {
            PluginInfo plugininfo = pluginentry.PluginInfo;
            (LBConfiguration.Config.plugins_toremove as List<string>).Add(EnviromentManager.LBPluginsPath + plugininfo.Name + ".dll");
            foreach (Plugin_Wrapper plug in InstalledPlugins.Where(a => a.Plugin == pluginentry))
            {
                plug.WillBeUninstalled = true;
            }
        }

        public static void UpdatePlugin(IPlugin plugin,bool reboot=false)
        {
            string location=plugin.Update();
            if(File.Exists(location))
            {
                if(Path.GetExtension(location)!=".dll")
                {
                    MessageBox.Show($"{plugin.PluginInfo.Name} did not download a .dll file. Updatefunction of the plugin might is broken.\nPlease contact {plugin.PluginInfo.Author} on {plugin.PluginInfo.Url} for further information.");
                    return;
                }

                string path = EnviromentManager.LBPluginsPath + plugin.PluginInfo.Name + ".dll";
                if (LBConfiguration.Config.plugins_toinstall == null)
                {
                    LBConfiguration.Config.plugins_toinstall = new List<string>();
                }
                LBConfiguration.Config.plugins_toinstall.Add(path+";"+location);
                LBConfiguration.Save();
            }

            if (reboot) EnviromentManager.Reboot();
        }

        public static void RemoveUninstalledPlugins()
        {
            if (LBConfiguration.Config.plugins_toremove ==null)
            {
                LBConfiguration.Config.plugins_toremove = new List<string>();
            }
            List<string> removed_plugins = new List<string>();

            foreach(string path in LBConfiguration.Config.plugins_toremove as List<string>)
            {
                if (File.Exists(path)) File.Delete(path);
                removed_plugins.Add(path);
            }
            foreach(string plugin in removed_plugins)
            {
                LBConfiguration.Config.plugins_toremove.Remove(plugin);
            }

            LBConfiguration.Save();
        }

        public static void AddToInstallPlugins()
        {
            if (LBConfiguration.Config.plugins_toinstall == null)
            {
                LBConfiguration.Config.plugins_toinstall = new List<string>();
            }
            List<string> added_plugins = new List<string>();

            foreach (string paths in LBConfiguration.Config.plugins_toinstall as List<string>)
            {
                string[] paths_splited = paths.Split(';');

                if (File.Exists(paths_splited[0])) File.Delete(paths_splited[0]);
                if (File.Exists(paths_splited[1])) File.Copy(paths_splited[1], paths_splited[0]);
                added_plugins.Add(paths);
            }
            foreach (string plugin in added_plugins)
            {
                LBConfiguration.Config.plugins_toinstall.Remove(plugin);
            }

            LBConfiguration.Save();
        }


        public static void AutoUpdatePlugins()
        {
            if(LBConfiguration.Config.plugins_autoupdate)
            {
                UpdateAllPlugins();
            }
        }

        public static void UpdateAllPlugins()
        {
            bool updated = false;
            foreach(Plugin_Wrapper plugin in InstalledPlugins.Where(a=>!a.Plugin.IsUpToDate))
            {            
                updated = true;
                Action waitforlaunch = () => UpdatePlugin(plugin.Plugin);
                Helpers.BlockerInfo.Run("Plugin Update", $"Launchbuddy is automatically updating {plugin.Plugin.PluginInfo.Name}. To disable this feature visit the plugin section.", waitforlaunch);
            }
            if (updated)
            {
                EnviromentManager.Reboot();
            }
        }

        public static void OnLBStart(EventArgs args)
        {
            if(HandlerLBStart != null)
            {
                try
                {
                    HandlerLBStart.Invoke(null, args);
                }
                catch (Exception e)
                {
                    PluginExceptionReport(e);
                }
            }
        }

        public static void OnLBClose(EventArgs args)
        {
            if (HandlerLBClose != null)
            {
                try
                {
                    HandlerLBClose.Invoke(null, args);
                }
                catch (Exception e)
                {
                    PluginExceptionReport(e);
                }
            }
        }

        public static void OnClientStatusChanged(PluginContracts.EventArguments.ClientStatusEventArgs args)
        {
            if (HandlerClientStatusChanged != null)
            {
                try
                {
                    HandlerClientStatusChanged.Invoke(null, args);
                }
                catch (Exception e)
                {
                    PluginExceptionReport(e);
                }
            }
        }

        private static void PluginExceptionReport(Exception e)
        {
            MessageBox.Show($"Plugin {e.Source} crashed with error: \n{e.Message}");
        }
    }
}