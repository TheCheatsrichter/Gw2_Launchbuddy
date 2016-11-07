# Gw2_Launchbuddy V 1.#.#
# The following FAQ ony applies to the above version number!
Custom launcher generator for the video game Guild Wars 2.
TheCheatsrichter. All rights reserved.

Credits:

Project Management: TheCheatsrichter

Programming: KairuByte, TheCheatsrichter

Graphics: Arenanet, TheCheatsrichter

Additonal Thanks to : WoodenPotatoes, Wonders Frostfire and all the guys on reddit who spent their time giving constructive feedback.


How to use it:

General usage:

Gw2 Launchbuddy automatically uses the path of the last Gw2.exe which was launched on your computer.  First make sure that your installation path is pointing to the right directory!
After checking your path you can choose all available options for launching Gw2 which are located mostly on the lefthand side of the General tab.
If your game stopped working, use the clientfix button in the top tight corner.

Choosing servers:

Click the "Check Servers" button and wait for Gw2 Launchbuddy to load all available servers. How long it takes to load depends on your internet connection!
After the lists are loaded choose one server by selecting it in the list and checking the equivalent box. If needed change the used Server Port by editing the text next to it.

Multiboxing:

There are two ways to multibox. The first method envolves the usage of the account manager (autologin, see below). The second method is to simply press "Launch Gw2" twice. After both instances are launched you
can login normaly. Multiboxing can drastically decrease your games perfomance as two clients have to read from the same .dat file. This bottleneck can be reduced by using a SSD. It is also recommended to choose
the "-windowed" argument on low-mid range PCs.


Using autologin:

Enter your GW2 email and account password, optionaly also type in a Nickname, and press the add button. Your account is now added to the account manager and is stored in an
AES encrypted file for additional security. To use autologin select all accounts you want to launch by mutliselecting them in the list. Make sure that your account information is correct!
Invalid account information leads to a game freeze with white or blackscreen. Two step authentication can also cause this kind of crash! So make sure that your network is listed as a trusted network ip!


########################################

FAQ:

- Are these options banable? - No, all command line arguments are provided by arenanet themself. 

- Is multiboxing banable? - Multiboxing is legal as long the rule "1 Input = 1 Action" isn't broken. Compare it to people with multiple accounts and multiple computers, they basically run two instances of the game at once. 

- Does it need admin privileges? - The current version of Launchbuddy should not require admin privledges, however if problems are experienced with multiboxing, running as admin may fix the problem.

- Why does this programm need internet acces? - Internet access is needed to report a crash and to fetch the Gw2 server lists.

- Where are my account details stored? - Your account information is stored at %Appdata%/Guild Wars2/Launchbuddy.bin as an AES encrypted file, so readable acess is only available via Gw2 Launchbuddy!

- Why don't you use the autlogin with the "local.dat" method? - Sadly this option isn't available because of multiboxing. I had to decide which of both features I would implement. -> Once gw2 is started local.dat can't be modified.

- My game freezes and shows a black/white screen when using autologin. - This error is usually caused by invalid inputs in the account manager, such as incorrect login info.

- Can I multibox without using Autologin? - Yes simply press "Launch Gw2" twice.

- After some time an errormessage "Error 502" pops up. What does it mean? - This error is caused by a downtime of the official Gw2 API. Gw2 Launchbuddy will still work as normaly, however launching the game without the newest Guild Wars 2 build can cause a crash! 


Want to help me out? Use the donate button or support us ingame TheCheatsrichter.6547, KairuByte.2703.
