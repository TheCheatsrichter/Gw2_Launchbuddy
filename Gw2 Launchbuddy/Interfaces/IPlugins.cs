using Gw2_Launchbuddy.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

namespace Gw2_Launchbuddy.Interfaces
{
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }

    public interface IPlugin
    {
        public PluginInfo PluginInfo {get;}
        bool Install();
        bool Uninstall();
        bool IsUpToDate { get; }
        bool IsInstalled { get; }
        bool Update();
    }


    public interface LBPlugin : IPlugin
    {
        TabItem UIContent { get; }
        void OnLBStart();
        void OnLBClose();
    }

    public interface AccountPlugin: IPlugin
    {
        IClient Client { set; get; }
        IAcc Acc { set; get; }
        void OnClientStart();
        void OnClientClose();
        void OnClientCrash();
    }

    #region Templates
    class AccPluginTest : AccountPlugin
    {
        //Your Plugin Information
        public string Name { get { return "Examplename"; } }        //Name of your plugin
        public string Version { get { return "1.0.0"; } }           //Version of your plugin
        public string Author { get { return "TheCheatsrichter"; } } //Your name ;)
        public string URL { get { return "www.google.com"; } }      //The URL associated with your plugin-> website/ githubpage etc.
        public Image Icon { get { return null; } }                  //Icon displayed in Launchbuddy, return null for no icon

        //Your Plugin Handling
        public bool IsUpToDate { get { return true; } }             //Return if your plugin is on the newest Version
        public bool IsInstalled { get { return true; } }            //Return if your plugin has set up everything successfully to work
        public bool Update() { return true; }                       //Update your plugin to the newest version and return if the update was successfull
        public bool Install() { return true; }                      //Initial set up for your plugin
        public bool Uninstall() { return true; }                    //Cleanup all files/settings associated with your plugin

        //Account specific Info
        public IClient Client { set; get; }                         //All the controls and information regarding the client bound to this instance
        public IAcc Acc { set; get; }                               //Information about the account bound to this instance

        //Account specific client events, will be called by Launchbuddy
        public void OnClientStart()
        {
            // Do Stuff when the Client Launches 
        }
        public void OnClientClose()
        {
            //Do Stuff when the Client Closes
        }
        public void OnClientCrash()
        {
            //Do Stuff when the Client Crashed
        }
    }

    class LBPluginTest : LBPlugin
    {
        //Your Plugin Information
        public string Name { get { return "Examplename"; } }        //Name of your plugin
        public string Version { get { return "1.0.0"; } }           //Version of your plugin
        public string Author { get { return "TheCheatsrichter"; } } //Your name ;)
        public string URL { get { return "www.google.com"; } }      //The URL associated with your plugin-> website/ githubpage etc.
        public Image Icon { get { return null; } }                  //Icon displayed in Launchbuddy, return null for no icon

        //Your Plugin Handling
        public bool IsUpToDate { get { return true; } }             //Return if your plugin is on the newest Version
        public bool IsInstalled { get { return true; } }            //Return if your plugin has set up everything successfully to work
        public bool Update() { return true; }                       //Update your plugin to the newest version and return if the update was successfull
        public bool Install() { return true; }                      //Initial set up for your plugin
        public bool Uninstall() { return true; }                    //Cleanup all files/settings associated with your plugin

        public TabItem UIContent
        {
            get
            {
                //Configure your WPF TabItem UI here
                TabItem UI = new TabItem();
                //Creating Wrappanel Container
                WrapPanel wp_content = new WrapPanel();
                //Adding a "hello world" Label
                wp_content.Children.Add(new Label { Content = "Hello World", FontSize = 24 });
                //Set TabItem Content to Wrappanel Container
                UI.Content = wp_content;
                return UI;
            }
        }

        //Launchbuddy events, will be called by Launchbuddy
        public void OnLBStart()
        {
            // Do Stuff when the Launchbuddy Launches 
        }
        public void OnLBClose()
        {
            //Do Stuff when the Launchbuddy Closes
        }

    }

    #endregion Templates
}
