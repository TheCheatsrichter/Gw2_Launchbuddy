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

namespace LBPlugin
{
    public class Plugin : ILBPlugin
    {
        public PluginInfo PluginInfo { get; } = new PluginInfo
        {
            Name = "Plugin UI Limits",                                                 //Name of your plugin
            Version = new Version(1, 0, 0),                                           //Version of your plugin
            Author = "TheCheatsrichter",                                         //Your name ;)
            Url = new Uri("https://google.com"),                                 //The URL associated with your plugin-> website/ githubpage etc.
            Description = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla aliquam dapibus imperdiet. Aliquam erat volutpat. Nullam non dolor non odio commodo posuere. Vestibulum viverra ligula odio, quis tincidunt augue molestie ac. Fusce est ipsum, tristique vitae ipsum ultrices, dictum scelerisque risus. Aliquam feugiat erat non est interdum pulvinar. Donec id semper sapien. Proin consequat tortor vitae nunc tincidunt, vitae ullamcorper dui mollis. Morbi at laoreet magna. Sed varius sed tellus vel facilisis. Nam sagittis rhoncus est eget suscipit. Mauris sit amet bibendum diam."
        };

        private IEnvironment environment;

        public ObservableCollection<IAcc> Accounts { set; get; }
        public IEnvironment Environment { set { environment = value; } get { return environment; } }

        //Your Plugin Handling

        public bool Init()
        {
            UI_Init();
            return true;
        }                         //Initialize your Plugin

        public bool IsUpToDate { get { return true; } }             //Return if your plugin is on the newest Version
        public bool Verify { get { return true; } }                 //Return if your plugin has set up everything successfully to work
        public string Update() { return "null"; }                       //Update your plugin to the newest version and return if the update was successfull
        public bool Install() { return true; }                      //Initial set up for your plugin
        public bool Uninstall() { return true; }                    //Cleanup all files/settings associated with your plugin

        //Account specific client events, will be called by Launchbuddy

        public void OnLBStart(object sender, EventArgs e)
        {
            // Do Stuff when LB Starts
        }

        public void OnLBClose(object sender, EventArgs e)
        {
            //Do Stuff when the LB Closes
            System.Windows.Forms.MessageBox.Show("On LB Close :O");
        }

        public void OnClientStatusChanged(object sender, ClientStatusEventArgs e)
        {
            //Do Stuff when the LB Closes
            System.Windows.Forms.MessageBox.Show($"{e.ID} status changed to: {e.Status}");
        }

        //Return UI for Interface, return if none is needed
        public TabItem UIContent { get; } = new TabItem();

        //Set Up UI
        private void UI_Init()
        {
            UIContent.Header = "LBTestPlugin";
            Grid content = new Grid();
            Button bt = new Button { Content = "Useless Button" };
            content.Children.Add(bt);
            UIContent.Content = content;
        }
    }
}
