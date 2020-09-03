![Preview Image](https://i.imgur.com/IANRST5.png)

# Gw2_Launchbuddy V 2.#.#
# The following FAQ ony applies to the above version number!
Custom launcher generator for the video game Guild Wars 2.
TheCheatsrichter. All rights reserved.

**Credits:**

Project Management: TheCheatsrichter

Programming: KairuByte, TheCheatsrichter

Graphics: Arenanet, TheCheatsrichter

Additonal Thanks to : WoodenPotatoes, Wonders Frostfire and all the guys on reddit who spent their time giving constructive feedback.


**General usage:**

Gw2 Launchbuddy automatically uses the path of the last GW2 executable which was launched on your computer.  First make sure that your installation path is pointing to the right directory!
After checking your path you can choose all available options for launching Gw2 which are located mostly on the lefthand side of the General tab.
If your game stopped working, use the Crashes tab on the left to help diagnose any issues.

**Choosing servers:**

Network Settings **>** Wait for Gw2 Launchbuddy to load all available servers. How long it takes to load depends on your internet connection!
After the lists are loaded choose one server by selecting it in the list and checking the "Use Authentication Server: x.x.x.x" box. If needed change the Server Port by editing the text next to it.

**Multiboxing:**

There are two ways to multibox. The first method envolves the usage of the account manager (autologin, see below). The second method is to simply press "Launch Gw2" twice. After both instances are launched you
can login normaly. Multiboxing can drastically decrease your games perfomance as two clients have to read from the same .dat file. This bottleneck can be greatly reduced by using a SSD. It is also recommended to choose the "-windowed" argument on low-mid range PCs.


**Using autologin:**

Account Settings **>** Add **>** Type in a name for the account

**With dat files:**
Login Credentials **>** Set Loginfile **>** Allow Launchbuddy to create a .dat file **>** Login to your desired account and click login.
Launchbuddy should detect that you clicked login. Repeat as desired.

**With Account Details:**
Login Credentials **>** Enter your email and password. Your details are stored in an AES encrypted file for additional security. 
Make sure that your account information is correct!
Invalid account information leads to a game freeze with white or blackscreen. Two step authentication can also cause this kind of crash! So make sure that your network is listed as a trusted network ip!

**FAQ:**

- Are these options bannable? - No, all command line arguments are provided by Arenanet themself. 

- Is multiboxing bannable? - Multiboxing is legal as long the rule "1 Input = 1 Action" isn't broken. Compare it to people with multiple accounts and multiple computers, they basically run two instances of the game at once. 


- Why does this program need internet access? - Internet access is needed to report a crash and to fetch the Gw2 server lists.

- If I choose to use my account details as a login method, where are they stored? - Your account information is stored at %Appdata%\Gw2 Launchbuddy\Accs.xml" as AES encrypted text, so readable access is only available via Gw2 Launchbuddy!

- My game freezes and shows a black/white screen when using account details to autologin. - This error is usually caused by invalid inputs in the account manager, such as incorrect login info. If you use an authenticator and haven't authented your current IP, this is also a cause.

- Can I multibox without using Autologin? - Yes simply press "Launch Guild Wars 2" twice.

- After some time an error message "Error 502" pops up. What does it mean? - This error is caused by a downtime of the official Gw2 API. Gw2 Launchbuddy will still work as normally, however launching the game without the newest Guild Wars 2 build can cause a crash! 


Want to help me out? Use the donate button or support us ingame TheCheatsrichter.6547, KairuByte.2703.
