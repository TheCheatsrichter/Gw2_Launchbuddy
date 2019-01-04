using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Gw2_Launchbuddy.Interfaces.Plugins.Injectable;
using Gw2_Launchbuddy.Interfaces.Plugins.Test;
using System.Collections.ObjectModel;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class PluginManager
    {
        private static ObservableCollection<IPluginTest> testPluginCollection { get; set; }
        public static ReadOnlyObservableCollection<IPluginTest> TestPluginCollection { get; set; }
        private static ObservableCollection<IPluginInjectable> overlayPluginCollection { get; set; }
        public static ReadOnlyObservableCollection<IPluginInjectable> OverlayPluginCollection { get; set; }

        static PluginManager()
        {
            testPluginCollection = new ObservableCollection<IPluginTest>();
            TestPluginCollection = new ReadOnlyObservableCollection<IPluginTest>(testPluginCollection);

            var testPlugins = LoadPlugins<IPluginTest>("Plugins\\");
            foreach (var plugin in testPlugins)
            {
                testPluginCollection.Add(plugin);
            }


            overlayPluginCollection = new ObservableCollection<IPluginInjectable>();
            OverlayPluginCollection = new ReadOnlyObservableCollection<IPluginInjectable>(overlayPluginCollection);

            var overlayPlugins = LoadPlugins<IPluginInjectable>("Plugins\\");
            foreach (var plugin in overlayPlugins)
            {
                overlayPluginCollection.Add(plugin);
            }
        }
        public static ICollection<T> LoadPlugins<T>(string path)
        {
            var plugins = new List<T>();

            if (Directory.Exists(path) && !EnviromentManager.LaunchOptions.Safe)
            {
                var dlls = Directory.GetFiles(path, "*.dll");

                var assemblies = new List<Assembly>();
                foreach (var dll in dlls)
                {
                    try
                    {
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(dll);
                        Assembly assembly = Assembly.Load(assemblyName);

                        if (assembly != null)
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
                    catch (Exception ex)
                    {
                        Console.WriteLine(dll + "is not an assemly of expected type");
                    }
                }
            }
            return plugins;
        }

        public static void DoInits()
        {
            foreach (var plugin in TestPluginCollection)
            {
                plugin.Init();
            }

            foreach (var plugin in OverlayPluginCollection)
            {
                plugin.Init();
            }
        }
    }
}