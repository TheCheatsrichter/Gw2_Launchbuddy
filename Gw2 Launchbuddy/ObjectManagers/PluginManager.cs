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
        private static ObservableCollection<ITestPlugin> testPluginCollection { get; set; }
        public static ReadOnlyObservableCollection<ITestPlugin> TestPluginCollection { get; set; }
        private static ObservableCollection<IOverlay> overlayPluginCollection { get; set; }
        public static ReadOnlyObservableCollection<IOverlay> OverlayPluginCollection { get; set; }

        static PluginManager()
        {
            testPluginCollection = new ObservableCollection<ITestPlugin>();
            TestPluginCollection = new ReadOnlyObservableCollection<ITestPlugin>(testPluginCollection);

            var testPlugins = LoadPlugins<ITestPlugin>("Plugins\\");
            foreach (var plugin in testPlugins)
            {
                testPluginCollection.Add(plugin);
            }


            overlayPluginCollection = new ObservableCollection<IOverlay>();
            OverlayPluginCollection = new ReadOnlyObservableCollection<IOverlay>(overlayPluginCollection);

            var overlayPlugins = LoadPlugins<IOverlay>("Plugins\\");
            foreach (var plugin in overlayPlugins)
            {
                overlayPluginCollection.Add(plugin);
            }
        }
        public static ICollection<T> LoadPlugins<T>(string path)
        {
            var plugins = new List<T>();

            if (Directory.Exists(path) && !Globals.options.Safe)
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
                    catch
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