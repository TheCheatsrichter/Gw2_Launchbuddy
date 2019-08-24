using PluginContracts;
using PluginContracts.ObjectInterfaces;
using System;

namespace AccountPlugin
{
    class AccPluginTest : PluginContracts.AccountPlugin
    {
        //Your Plugin Information
        PluginInfo plugininfo = new PluginInfo {
            Name = "Examplename",                                   //Name of your plugin
            Version = "1.0.0",                                      //Version of your plugin
            Author = "TheCheatsrichter",                            //Your name ;)
            Url = "www.google.com",                                 //The URL associated with your plugin-> website/ githubpage etc.
            Description = "I like turtles",
        };

        public PluginInfo PluginInfo { get{return plugininfo;} }

        //Your Plugin Handling

        public bool Init() {
            System.Windows.Forms.MessageBox.Show("LB Acc Plugin initiated");
            return true;
        }                         //Initialize your Plugin

        public bool IsUpToDate { get { return true; } }             //Return if your plugin is on the newest Version
        public bool IsInstalled { get { return true; } }            //Return if your plugin has set up everything successfully to work
        public bool Update() { return true; }                       //Update your plugin to the newest version and return if the update was successfull
        public bool Install() { return true; }                      //Initial set up for your plugin
        public bool Uninstall() { return true; }                    //Cleanup all files/settings associated with your plugin

        //Account specific Info
        public IAcc[] Accounts { set; get; }                               //Information about the account bound to this instance

        //Account specific client events, will be called by Launchbuddy
        public void OnClientStatusChanged(object sender, EventArgs e)
        {
            // Do Stuff when the Client Launches
            System.Windows.Forms.MessageBox.Show("Client Status changed :OO");
        }
    }
}
