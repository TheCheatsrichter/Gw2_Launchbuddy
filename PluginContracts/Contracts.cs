using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using PluginContracts.ObjectInterfaces;
using PluginContracts.EventArguments;

namespace PluginContracts
{
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }
        public Uri Url { get; set; }
        public string Description { get; set; }
    }

    public interface IPlugin
    {
        PluginInfo PluginInfo { get; }
        bool Init();
        bool Uninstall();
        bool IsUpToDate { get; }
        bool Verify { get; }
        string Update();
    }

    public interface ILBPlugin : IPlugin
    {
        TabItem UIContent { get; }

        ObservableCollection<IAcc> Accounts { get; set; }
        IEnviroment Enviroment { get; set; }

        void OnLBStart(object sender, System.EventArgs e);
        void OnLBClose(object sender, System.EventArgs e);

        void OnClientStatusChanged(object sender, ClientStatusEventArgs e); //NEED CHANGE
    }

}
