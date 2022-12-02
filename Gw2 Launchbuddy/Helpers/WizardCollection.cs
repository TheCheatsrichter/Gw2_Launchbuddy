using Gw2_Launchbuddy;
using Gw2_Launchbuddy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WizardTest
{
    static class WizardCollection
    {
        static MainWindow win;
        static Dictionary<int, List<HelpWizardStep>> helpredirect;


        static List<HelpWizardStep> hp_launching;
        static List<HelpWizardStep> hp_basicaccountcreation;
        static List<HelpWizardStep> hp_music;
        static List<HelpWizardStep> hp_network;
        static List<HelpWizardStep> hp_plugins;
        static List<HelpWizardStep> hp_addons;
        static List<HelpWizardStep> hp_thirdpartydll;
        static List<HelpWizardStep> hp_steamsetup;
        public static void Init(MainWindow window)
        {
            win = window;
            SetupWizards();
            FillDictionary();
        }

        public static void LaunchHelp(FrameworkElement target)
        {
            List<HelpWizardStep> steps;
            if(helpredirect.TryGetValue(target.GetHashCode(),out steps))
            {
                new HelpWizard(steps).Show();
            }else
            {
                MessageBox.Show("No help for this page implemented yet");
            }
        }

        private static void FillDictionary()
        {
            helpredirect = new Dictionary<int, List<HelpWizardStep>>
        {
            {win.tab_accs.GetHashCode() , hp_basicaccountcreation },
            {win.tab_addons.GetHashCode() , hp_addons },
            {win.tab_home.GetHashCode() , hp_launching },
            {win.tab_music.GetHashCode() , hp_music },
            {win.tab_server.GetHashCode() , hp_network },
            {win.tab_plugins.GetHashCode() , hp_plugins },
            {win.bt_help_steamsetup.GetHashCode(),hp_steamsetup },
            {win.bt_help_dllinjection.GetHashCode(),hp_thirdpartydll }
        };
        }

        private static void SetupWizards()
        {

            hp_launching = new List<HelpWizardStep>
        {
            new HelpWizardStep(win.tab_home,"Home Screen","At the homescreen of Launchbuddy you get a general overview of your configured accounts and some general information"),
            new HelpWizardStep(win.sp_lbnews,"Home Screen","Starting with the newsletter which gets updated regulary to keep you informated about the latest Launchbuddy news and upcoming changes"),
            new HelpWizardStep(win.lv_accs,"Home Screen","Next up: launching accounts. To launch one or multiple accounts simply select them in the list (shift click is also supported). If you have not set up any accounts yet please switch over to the Account settings tab on the left."),
            new HelpWizardStep(win.bt_selectdailylogins,"Home Screen","If you feel lazy you can also use some of the Quick Selection options. Daily logins for example selects all accounts which have a login reward waiting for you!"),
            new HelpWizardStep(win.bt_launch,"Home Screen","Your accounts should now be selected. To start them simply press the Launch Guild Wars 2 button at the bottom")
        };

            hp_basicaccountcreation = new List<HelpWizardStep>
        {
            new HelpWizardStep(win.tab_accs,"Account Settings Overview","If you want to create or modify your gw2 accounts click on the Account Settings tab on the left"),
            new HelpWizardStep(win.bt_accadd,"Account Settings Overview","In the first colum you can see a list of your created accounts. In the second colum the settings for the currently selected accounts are shown. Lets add a new account for demonstration purposes by clicking on the Add button."),
            new HelpWizardStep(win.tb_accnickname,"Account Settings Overview","A new account has now been added and is ready to be configured! Let us first give it a proper nickname to better organize your accounts. After that we will go through each setting section to give you a overview of whats available"),
            new HelpWizardStep(win.exp_accloginfile,"Account Settings Overview","The Login Credentials section is used to set up your login information if you want Launchbuddy to automatically launch your accounts."),
            new HelpWizardStep(win.exp_accicon,"Account Settings Overview","The icon section is used to easily identify this account. You can also import your custom icons if you dont feel like using the default ones"),
            new HelpWizardStep(win.exp_accargs,"Account Settings Overview","Launch arguments are special settings which the Guild Wars 2 gameclient itself provides. These range from basic volume controls up to additional information when a map is loaded. To show a description of what each setting does simply hover over it."),
            new HelpWizardStep(win.exp_accgfx,"Account Settings Overview","What if you want to launch just this account with custom graphic settings than your others? This is what this section is all about!"),
            new HelpWizardStep(win.exp_accinjection,"Account Settings Overview","3rd party softwares like ArcDps and Reshade do use a process called DLL injection. To make this also work while multiboxing these softwares need to be added here. For better perfomance do NOT place the main dll in the Gw2 gamefolder, rather place it somewhere else and let Launchbuddy do the import. Please keep in mind that some third party softwares need a special setup for this to work. For that consult the according website of the creator"),
            new HelpWizardStep(win.exp_acchotkeys,"Account Settings Overview","Hotkeys can be used to set specific keyboard shortcuts to something like launching or focusing (bring window to foreground) this account"),
            new HelpWizardStep(win.exp_accwindow,"Account Settings Overview","Gamewindow settings are here to specify a place and size for this account's game window. Really usefull to minimize a cluster of gamewindows when many accounts are launched"),
            new HelpWizardStep(win.exp_accadvancedset,"Account Settings Overview","Spicy stuff is contained here. If you feel like you know what you are doing these settings are for you. CAUTION: Some settings can cause trouble!"),
            new HelpWizardStep(win.bt_accsave,"Account Settings Overview","All done! Let's save our changes.\nEnd of the account settings tour. I hope this tour gave you a small overview of what is available in the account specific settings.")
        };

            hp_music = new List<HelpWizardStep>
        {
            new HelpWizardStep(win.tab_music,"Custom Ingame Music","Launchbuddy can enable you to listen to your favorite music while ingame! But what does make this so special? These playlists are dynamically implemented into the game itself. This means that each playlist will react to your ingame actions."),
            new HelpWizardStep(win.lv_musicplaylists,"Custom Ingame Music","On the left you can see a list of game states in which each playlists gets played. Let's add your favorite catchy tune to the battle playlist by clicking on it."),
            new HelpWizardStep(win.bt_addsong,"Custom Ingame Music","To add a song to the playlist simply click on the Add Song button. Afterwards it will be imported and shown in the list. You can listen to tracks by clicking on the play icon right next to them"),
            new HelpWizardStep(win.cb_enableplaylist,"Custom Ingame Music","Last but not least click on the Enable this Playlist checkbox to enable this playlist when you game is launched. You have now successfully created your custom ingame playlist! These playlists do scale with the ingame music volume setting.")
        };

            hp_network = new List<HelpWizardStep>
        {
            new HelpWizardStep(win.tab_server,"Network Settings","Having trouble will loging in? Having trouble to download (no connection / slow download speed) the game? Or how about your network needing special ports to be used? Then you have come to the right place in the Network Settings tab. These settings will be applied globally to all launched accounts."),
            new HelpWizardStep(win.tab_server,"Network Settings","Here you can see two types of server lists. Authentication servers are used to log in your account. So if you are having trouble logging in try to select another one. Asset servers are used to download the games data. If your game downloads are slow or do not load at all try to pick a new one."),
            new HelpWizardStep(win.bt_checkservers,"Network Settings","Let's choose a custom authentication server for our gameclients to use. First click on the Refresh Serverlist Button"),
            new HelpWizardStep(win.lv_auth,"Network Settings","Now pick a server by clicking on it in the list. The selected server will now be shown in the form below"),
            new HelpWizardStep(win.checkb_auth,"Network Settings","To actually use this server now all that is left to do is checking the box Use Authentication Server: IP"),
            new HelpWizardStep(win.tab_server,"Network Settings","All your accounts which are now launched will use teh server picked by you!")
        };

            hp_plugins = new List<HelpWizardStep>
        {
            new HelpWizardStep(win.tab_plugins,"Plugins","Plugins are used to add functionality to Launchbuddy itself. Developers can use the templates provided at Github to create their own extension of Launchbuddy."),
            new HelpWizardStep(win.bt_addplugin,"Plugins","Download the newest version of the plugins of your choice and simply add them with the button."),
        };

            hp_addons = new List<HelpWizardStep>
        {
            new HelpWizardStep(win.tab_addons,"Addons","If you want to launch third party desktop applications with each account or whenever Launchbuddy itself gets launched add them here"),
            new HelpWizardStep(win.tb_AddonName,"Addons","Let's add a new addon! Starting with giving it an unique name"),
            new HelpWizardStep(win.tb_AddonArgs,"Addons","If additional launch parameters (arguments) are wished to be used type them in here"),
            new HelpWizardStep(win.cb_AddonOnLB,"Addons","If the program should be launched whenever Launchbuddy is started check Start with LB"),
            new HelpWizardStep(win.cb_AddonRunAsAdmin,"Addons","Should the addon be launched as normal current user or have admin privileges?"),
            new HelpWizardStep(win.bt_AddAddon,"Addons","And to finish it all press the Add button. This will open a filedialog for you to select the .exe file"),
            new HelpWizardStep(win.cb_AddonRunAsAdmin,"Addons","Tutorial for Addons has been completed"),
        };


            hp_steamsetup = new List<HelpWizardStep>
        {
            new HelpWizardStep(null,"Steam Account Setup","In order to make your steam account Launchbuddy ready a few steps have to be made. This is the guide on how to make this possible WITHOUT linking game files. This method has a few drawbacks however lets you keep two independent game installs"),
            new HelpWizardStep(null,"Steam Account Setup","Before we start a few things to consider: 1. You must not launch the game client from any other folder than the steam folder. 2. You must not update the game client from any other folder than the steam folder. If you cannot work with these requirements please use the Game Data Linker option in the Account settings. If you still want to proceed press next."),
            new HelpWizardStep(null,"Steam Account Setup","First of we have to make sure that your steam client is up to date. Please launch Gw2 manually with steam and let it update. Login until you see the charcter selection screen. Then close the game"),
            new HelpWizardStep(win.tab_lbsettings,"Steam Account Setup","With your steam version updated Launchbuddy should now update your gamefolder correctly. This either can be set manually in the settings tab or can automatically be achieved when Launchbuddy is restarted"),
            new HelpWizardStep(win.lab_path,"Steam Account Setup","Keep in mind that Launchbuddy will now use the steam gamedata to launch all your accounts! If you use other game installs the steam setup has to be made again!"),
        };

            hp_thirdpartydll = new List<HelpWizardStep>
        {
            new HelpWizardStep(null,"Injected Software Setup","This is the help wizard for setting up third party injected software (e.g arcdps,blishhud,...) which are not natively supported"),
            new HelpWizardStep(null,"Injected Software Setup","First make sure that you have a clean gw2 install WITHOUT any third party programs installed. To achieve this make sure that no .dll file (mainly named d3d11.dll) is in your gw2 game folder"),
            new HelpWizardStep(win.lv_accssettings,"Injected Software Setup","Select an account which should use the third party injected software"),
            new HelpWizardStep(win.bt_AddDll,"Injected Software Setup","Now you can add the dll files to the accounts which actually should use the specific software."),
        };
        }

    }
}
