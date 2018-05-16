using Gw2_Launchbuddy.Interfaces;
using System;
using System.Windows;

namespace TestPlugin
{
    public class TestPlugin : IPlugin
    {
        public string Name { get { return "Test Plugin"; } }

        public string Version { get { return "0.0.1"; } }

        /*
        public void Init()
        {
            MessageBox.Show("Plugin \"Initialized\"!");
        }

        public void Exit()
        {
            MessageBox.Show("Plugin \"Exited\"!");
        }

        public void Client_Exit()
        {
            MessageBox.Show("Client Exited!");
        }

        public void Client_PostLaunch()
        {
            MessageBox.Show("Client Launched!");
        }

        public void Client_PreLaunch()
        {
            MessageBox.Show("Client Pre Launch!");
        }
        */
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
