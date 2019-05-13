using PluginContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LBPlugin
{
    public class Plugin : PluginContracts.LBPlugin
    {
        //Your Plugin Information
        PluginInfo plugininfo = new PluginInfo
        {
            Name = "Examplename",                                   //Name of your plugin
            Version = "1.0.0",                                      //Version of your plugin
            Author = "TheCheatsrichter",                            //Your name ;)
            Url = "www.google.com",                                 //The URL associated with your plugin-> website/ githubpage etc.
            Description = "I like turtles",
        };

        public PluginInfo PluginInfo { get { return plugininfo; } }

        //Your Plugin Handling

        public bool Init()
        {
            UI_Init();
            System.Windows.Forms.MessageBox.Show("LB PLugin initiated");
            return true;
        }                         //Initialize your Plugin

        public bool IsUpToDate { get { return true; } }             //Return if your plugin is on the newest Version
        public bool IsInstalled { get { return true; } }            //Return if your plugin has set up everything successfully to work
        public bool Update() { return true; }                       //Update your plugin to the newest version and return if the update was successfull
        public bool Install() { return true; }                      //Initial set up for your plugin
        public bool Uninstall() { return true; }                    //Cleanup all files/settings associated with your plugin

        //Account specific client events, will be called by Launchbuddy
        public void OnLBStart(object sender, EventArgs e)
        {
            // Do Stuff when LB Starts
            System.Windows.Forms.MessageBox.Show("On LB Start :D");
        }
        public void OnLBClose(object sender, EventArgs e)
        {
            //Do Stuff when the LB Closes
            System.Windows.Forms.MessageBox.Show("On LB Close :O");
        }




        //#############################################
        //UI Stuff

        //TabItem Instance
        private TabItem UI = new TabItem();

        //Return UI for Interface
        public TabItem UIContent { get { return UI; } }

        //Set Up UI
        private void UI_Init()
        {
            UI.Header = "LBTestPlugin";
            Grid content = new Grid();
            Button bt = new Button { Content = "Useless Button" };
            content.Children.Add(bt);
            UI.Content = content;
        }
    }
}
