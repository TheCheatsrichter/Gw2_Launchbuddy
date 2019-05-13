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
        public static EventHandler OnLBStart;
        public static EventHandler OnLBClose;

        static ObservableCollection<IPlugin> InstalledPlugins { set; get; }

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
                AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                Assembly assembly = Assembly.Load(an);
                assemblies.Add(assembly);
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
                if (!plugin.Init())
                {
                    System.Windows.Forms.MessageBox.Show($"Could not initialize plugin {plugin.PluginInfo.Name}");
                    continue;
                }

                if (plugin is LBPlugin)
                {
                    OnLBStart += (plugin as LBPlugin).OnLBStart;
                    OnLBClose += (plugin as LBPlugin).OnLBClose;

                    EnviromentManager.MainWin.AddTabPlugin(plugin as LBPlugin);
                }
            }
        }
    }
}