using Gw2_Launchbuddy.Interfaces.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

namespace Gw2_Launchbuddy.Interfaces.Plugins
{
    public interface IPlugin
    {
        PluginInfo Plugin { get; }
        void Init();
    }
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }

    public interface IPluginDerived : IPlugin
    {
        ProjectInfo Project { get; }
    }
    
    public class ProjectInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
    }
}

namespace Gw2_Launchbuddy.Interfaces.Plugins.Injectable
{
    public interface IPluginInjectable : IPlugin
    {
        PluginSettings Settings { get; }
    }
    
    public class PluginSettings
    {
        private string subdirectory = null;
        private string target = null;
        public string Subdirectory
        {
            get => subdirectory;
            set => subdirectory = System.IO.Directory.CreateDirectory("Plugins/" + value).FullName + "\\";
        }
        public string Target
        {
            get => target;
            set => target = System.IO.Path.GetFullPath(subdirectory + value);
        }
    }
}

namespace Gw2_Launchbuddy.Interfaces.Plugins.Test
{
    public interface IPluginTest : IPlugin
    {
        void Exit();
        void Client_PreLaunch();
        void Client_PostLaunch();
        void Client_Exit();
    }
}