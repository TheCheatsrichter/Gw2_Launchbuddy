using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_Launchbuddy.Interfaces
{
    public interface IProcessWrapper<T>
    {
        event EventHandler<EventArgs> Started;
        string MD5 { get; }

        DateTime StartTime { get; }
        int Id { get; }

        //T SetWorkingDirectory(string value);
        bool Start();
        bool StartAndWait();
        void Stop();
    }
}
