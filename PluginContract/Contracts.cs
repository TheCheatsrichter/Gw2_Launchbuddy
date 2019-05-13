using System;
using System.Windows.Controls;
using PluginContract.ObjectInterfaces;

namespace PluginContract
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
        void OnLBStart();
        void OnLBClose();
    }

    public interface AccountPlugin : IPlugin
    {
        void OnClientStart();
        void OnClientClose();
        void OnClientCrash();
        IAcc[] Accounts { set; get; }
    }
}
