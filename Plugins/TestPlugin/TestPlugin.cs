using Gw2_Launchbuddy.Interfaces.Plugins;
using Gw2_Launchbuddy.Interfaces.Plugins.Test;


namespace TestPlugin
{
    public class TestPlugin : IPluginTest
    {
        public PluginInfo Plugin => new PluginInfo()
        {
            Name = "Test Plugin",
            Version = "0.0.1",
            Author = "KairuByte",
            Url = null
        };

        public void Init()
        {

        }

        public void Exit()
        {

        }

        public void Client_Exit()
        {

        }

        public void Client_PostLaunch()
        {

        }

        public void Client_PreLaunch()
        {

        }
    }
}
