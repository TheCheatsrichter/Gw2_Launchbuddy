using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Gw2_Launchbuddy.Interfaces;
using System.Collections.ObjectModel;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class PluginManager
    {
        private static ObservableCollection<IPlugin> pluginCollection { get; set; }
        public static ReadOnlyObservableCollection<IPlugin> PluginCollection { get; set; }

        static PluginManager()
        {
            pluginCollection = new ObservableCollection<IPlugin>();
            PluginCollection = new ReadOnlyObservableCollection<IPlugin>(pluginCollection);

            var plugins = LoadPlugins<IPlugin>("Plugins\\");
            foreach (var plugin in plugins)
            {
                pluginCollection.Add(plugin);
            }
        }
        public static ICollection<T> LoadPlugins<T>(string path)
        {
            var plugins = new List<T>();

            if (Directory.Exists(path) && !Globals.options.Safe)
            {
                var dlls = Directory.GetFiles(path, "*.dll");

                var assemblies = new List<Assembly>();
                foreach(var dll in dlls)
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(dll);
                    Assembly assembly = Assembly.Load(assemblyName);

                    if(assembly != null)
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (!type.IsInterface && !type.IsAbstract && type.GetInterface(typeof(T).Name) != null)
                            {
                                plugins.Add((T)Activator.CreateInstance(type));
                            }
                        }
                    }
                }
            }
            return plugins;
        }

        public static void DoInits()
        {
            foreach (var plugin in PluginCollection)
            {
                plugin.Init();
            }
        }
    }
}