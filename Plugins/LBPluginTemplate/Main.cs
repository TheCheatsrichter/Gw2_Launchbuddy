using PluginContracts;
using PluginContracts.ObjectInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using PluginContracts.EventArguments;

namespace LBPluginTemplate
{
    public class Plugin : ILBPlugin
    {
        //Your Plugin Information
        PluginInfo plugininfo = new PluginInfo
        {
            Name = "Examplename",                                   //Name of your plugin
            Version = new Version("1.0.0"),                                      //Version of your plugin
            Author = "TheCheatsrichter",                            //Your name ;)
            Url = new Uri("https://google.com"),                                 //The URL associated with your plugin-> website/ githubpage etc.
            Description = "I like turtles",
        };

        public PluginInfo PluginInfo { get { return plugininfo; } }

        //General LB Data
        private ObservableCollection<IAcc> accs;
        private IEnvironment environment;

        public ObservableCollection<IAcc> Accounts { set { accs = value; } get { return accs; } }   //A collection of all Accounts in LB with various functions
        public IEnvironment Environment { set { environment = value; } get { return environment; } }    //Important LB folder,filepaths and client information

        //Your Plugin Handling

        public bool Init()                                          //Initialize your Plugin, will be called on each LB start
        {
            UI_Init();
            return true;
        }                         

        public bool IsUpToDate { get { return true; } }             //Return if your plugin is on the newest Version
        public bool Verify { get { return true; } }                 //Return if your plugin has set up everything successfully to work, will be used to check if the plugin should be run
        public string Update() { return ""; }                       //Update your plugin to the newest version and return if the update was successfull, make sure that your plugininfo.name is the name of the .dll file
        public bool Install() { return true; }                      //Install your plugin and set every thing up, will only be called when the plugin got added
        public bool Uninstall() { return true; }                    //Cleanup all files/settings associated with your plugin

        //Account specific client events, will be called by Launchbuddy

        public void OnLBStart(object sender, EventArgs e)
        {
            // Do Stuff when LB Starts
        }

        public void OnLBClose(object sender, EventArgs e)
        {
            //Do Stuff when the LB Closes
        }

        public void OnClientStatusChanged(object sender, ClientStatusEventArgs e)
        {
            //Do Stuff when a client changes its Status
        }


        //#############################################
        //UI Stuff
        //#############################################


        //TabItem Instance
        private TabItem UI = new TabItem(); //Your Tab in the LB UI, set to null if you don't want to show any UI

        //Return UI for Interface, return if none is needed
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
