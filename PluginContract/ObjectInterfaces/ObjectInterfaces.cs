using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginContract.ObjectInterfaces
{
    public interface IClient
    {
        void Launch();
        void Suspend();
        void Resume();
        void Maximize();
        void Minimize();
        void Focus();
        void Inject(string dllpath);
        void Window_Move(int posx, int posy);
        void Window_Scale(int width, int height);
        void Close();
        ClientStatus Status { set; get; }
    }

    public interface IAcc
    {
        string Nickname { get; }
        int ID { get; }
        IClient Client { get; }
    }

    [Flags]
    public enum ClientStatus
    {
        None = 0x00,
        Configured = 0x01,
        Created = 0x01 << 1,
        Injected = 0x01 << 2,
        MutexClosed = 0x01 << 3,
        Login = 0x01 << 4,
        Running = 0x01 << 5,
        Closed = 0x01 << 6,
        Crash = 0x01 << 7
    };
}
