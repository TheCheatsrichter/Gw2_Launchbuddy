using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginContracts.ObjectInterfaces;

namespace PluginContracts.EventArguments
{
    public class ClientStatusEventArgs : System.EventArgs
    {
        private int id;
        private ClientStatus status;

        public ClientStatusEventArgs(int id,ClientStatus status)
        {
            this.id = id;
            this.status = status;
        } // eo ctor

        public int ID { get { return id; } }
        public ClientStatus Status { get { return status; } }
    }
}
