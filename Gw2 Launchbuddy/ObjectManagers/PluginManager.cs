using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class PluginManager
    {

    }

    public class Plugin
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string ReleaseDate { get; private set; }
    }
}
