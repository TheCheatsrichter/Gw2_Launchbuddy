namespace Gw2_Launchbuddy.Interfaces
{
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }

        void Init();
        void Exit();
        void Client_PreLaunch();
        void Client_PostLaunch();
        void Client_Exit();
    }
}