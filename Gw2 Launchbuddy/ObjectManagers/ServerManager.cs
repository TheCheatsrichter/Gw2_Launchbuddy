using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.ObjectManagers
{
    class ServerManager
    {
    }

    public class Server
    {
        public string IP { get; set; }
        public string Port { get; set; }
        public string Ping { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
    }
}
