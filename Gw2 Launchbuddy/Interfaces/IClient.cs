using Gw2_Launchbuddy.ObjectManagers;

namespace Gw2_Launchbuddy.Interfaces
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
        Client.ClientStatus Status { set; get; }
    }
}
