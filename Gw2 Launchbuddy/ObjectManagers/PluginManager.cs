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

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class PluginManager
    {
        private static EventHandler HandlerLBStart;
        private static EventHandler HandlerLBClose;
        private static EventHandler<PluginContracts.EventArguments.ClientStatusEventArgs> HandlerClientStatusChanged;

        public static ObservableCollection<IPlugin> InstalledPlugins { set; get; }

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
                                    pluginTypes.Add(type);
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

            InstalledPlugins = new ObservableCollection<IPlugin>();
            foreach (Type type in pluginTypes)
            {
                IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                InstalledPlugins.Add(plugin);
            }
        }

        public static void InitPlugins()
        {
            foreach(IPlugin plugin in InstalledPlugins)
            {
                InitPlugin(plugin);
            }
        }

        private static void InitPlugin(IPlugin plugin)
        {
            if(!plugin.Verify)
            {
                System.Windows.Forms.MessageBox.Show($"{plugin.PluginInfo.Name} verifycation failed. Please reinstall/update this plugin.");
                return;
            }

            if (!plugin.Init())
            {
                System.Windows.Forms.MessageBox.Show($"Could not initialize plugin {plugin.PluginInfo.Name}");
                return;
            }

            if (plugin is LBPlugin)
            {
                LBPlugin lbplugin = plugin as LBPlugin;

                HandlerLBStart += lbplugin.OnLBStart;
                HandlerLBClose += lbplugin.OnLBClose;

                HandlerClientStatusChanged += lbplugin.OnClientStatusChanged;

                EnviromentManager.MainWin.AddTabPlugin(lbplugin);

                lbplugin.Accounts = AccountManager.IAccs;
            }
        }

        public static void AddPluginWithDialog()
        {
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog();
            fd.DefaultExt = ".dll";
            fd.ShowDialog();

            if (fd.FileName != null)
            {
                if (System.IO.File.Exists(fd.FileName)) AddPlugin(fd.FileName);
            }
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
                                InstalledPlugins.Add(plugin);

                                if(!plugin.Install())  MessageBox.Show($"Plugin {assembly.FullName} could not be installed successfully. Proceed with cation.");

                                if(plugin.Verify)
                                {
                                    File.Copy(path, EnviromentManager.LBPluginsPath + plugin.PluginInfo.Name+".dll");
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

        public static void RemovePlugin(PluginInfo plugininfo)
        {
            foreach(IPlugin plugin in InstalledPlugins.Where(x=>x.PluginInfo==plugininfo))
            {
                if(plugin.Uninstall())
                {
                    File.Delete(EnviromentManager.LBPluginsPath + plugininfo.Name + ".dll");
                }
                else
                {
                    MessageBoxResult win = MessageBox.Show($"Plugin { plugin.PluginInfo.Name}could not be removed completly clean. Uninstall it anyway?", "Client Retry", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (win.ToString() == "Yes")
                    {
                        File.Delete(EnviromentManager.LBPluginsPath + plugininfo.Name + ".dll");
                    }
                    else
                    {
                        return;
                    }
                }

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
            MessageBox.Show($"{e.Source} crashed with error: \n{e.Message}");
        }
    }
}