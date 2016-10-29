# Gw2_Launchbuddy V 1.0.0
# The following FAQ may not match with upcoming BETA releases!
Custom launcher generator for the video game Guild Wars 2.
TheCheatsrichter. All rights reserved.

First start:

On the first startup Gw2 Launchbuddy will show you a small Setup Info window with a quaggan. This setup window will stay open as long it takes to download all neccessary components from the internet!
So depending on your internet connection this should take up from a few milliseconds up to 1 minute. ( about 2MB are beeing downloaded)
After this Gw2 Launchbuddy will launch normaly. If your setup gets corrupted or the multiboxing feature stops working make sure to use the "Refresh Launchbuddy Setup" in the Clientfix section!


How to use it:

General usage:

Gw2 Launchbuddy automatically uses the path of the last Gw2.exe which was launched on your computer.  First make sure that your installation path is pointing to the right directory!
After checking your path you can choose all available options for launching Gw2 which are located mostly on the lefthand side of the window.
If your game stopped working or the multiboxing feature doens't work, use the clientfix button in the top tight corner.

Choosing servers:

Click the "Check Servers" button and wait for Gw2 Launchbuddy to load all available servers. How long it takes to load depends on your internet connection!
After the lists are loaded choose one server by selecting it in the list and checking the equivalent box. If needed change the used Server Port by editing the text next to it.

Multiboxing:

There are two ways to multibox. The first method envolves the usage of the account manager (autologin, see below). The second method is to simply press "Launch Gw2" twice. After both instances are launched you
can login normaly. Multiboxing can drastically decrease your games perfomance as to clients have to read from the same .dat file. This bottleneck can be reduced by using a SSD. It is also recommended to choose
the "-windowed" argument on low-mid range PCs.


Using autologin:

Enter your GW2 email and account password, optionaly also type in a Nickname, and press the add button. Your account is now added to the account manager and is stored in an
AES encrypted file for additional security. To use autologin select all accounts you want to launch by mutliselecting them in the list. Make sure that your account information is correct!
Invalid account information leads to a game freeze with white or blackscreen. Two step authentication can also cause this kind of crash! So make sure that your network is listed as a trusted network ip!


########################################

FAQ:

- Are these options banable? - No, all command line arguments are provided by arenanet themself. 

- Is multiboxing banable? - Multiboxing is legal as long the rule "1 Input = 1 Action" isn't broken. Compare it to people with multiple accounts and multiple computers, they basically run two instances of the game at once. 

- Why does it need admin privileges? - In order to achieve multiboxing a so called handle for a mutex has to be closed, which needs admin rights. All other functions just need "standard" privileges.

- Why does this programm need internet acces? - Internet acces is needed to set the Multiboxing feature up (only on first start) and to fetch the Gw2 serverlist with all informations.

- Where are my account details stored? - Your account information is stored at %Appdata%/Guild Wars2/Launchbuddy.bin as an AES encrypted file, so readable acess is only available via Gw2 Launchbuddy!

- Why don't you use the autlogin with the "local.dat" method? - Sadly this option isn't available because of multiboxing. I had to decide which of both features I would implement. -> Once gw2 is started local.dat can't be modified.

- My game freezes and shows a black/white screen when using autologin. - This error is caused by invalid Inputs in the account manager!

- Can I multibox without using Autologin? - Yes simply press "Launch Gw2" twice.

- After some time an errormessage "Error 502" pops up. What does it mean? - This error is caused by a downtime of the official Gw2 API. Gw2 Launchbuddy will still work as normaly, however launching the game without the newest Guild Wars 2 build can cause a crash! 

- Multiboxing does not work/work anymore. How can I fix this? - Open the Clientfix menu and select Refresh Launchbuddy Setup!


Want to help me out? use the donate button or support me ingame TheCheatsrichter.6547
