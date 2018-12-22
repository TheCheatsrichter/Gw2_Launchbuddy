using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Gw2_Launchbuddy.Interfaces
{
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        
        void Init();
    }

    public interface IOverlay : IPlugin
    {
        string ProjectName { get; }
        string ProjectURL { get; }
        string OverlayDll { get; }
    }

    public interface ITestPlugin : IPlugin
    {
        void Exit();
        void Client_PreLaunch();
        void Client_PostLaunch();
        void Client_Exit();

    }

    public interface IAccount
    {
        string Nickname { get; }
        //string Email { get; }
        //string Password { get; }

        DateTime CreateDate { get; }
        DateTime ModifyDate { get; }
        DateTime RunDate { get; }

        bool Selected { get; }

        ImageSource Icon { get; }
        List<IArgument> GetArgumentList();
    }
    public interface IArgument
    {
        string Flag { get; }
        string Description { get; }
        string OptionString { get; set; }
        bool Active { get; }
    }
}