using System;
using System.Windows.Controls;
using PluginContracts.ObjectInterfaces;

namespace PluginContracts
{
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }

    public interface IPlugin
    {
        PluginInfo PluginInfo { get; }
        bool Init();
        bool Install();
        bool Uninstall();
        bool IsUpToDate { get; }
        bool IsInstalled { get; }
        bool Update();
    }

    public interface LBPlugin : IPlugin
    {
        TabItem UIContent { get; }
        void OnLBStart(object sender, EventArgs e);
        void OnLBClose(object sender, EventArgs e);
    }

    public interface AccountPlugin : IPlugin
    {
        void OnClientStart(object sender, EventArgs e);
        void OnClientClose(object sender, EventArgs e);
        void OnClientCrash(object sender, EventArgs e);
    }
}
