using Gw2_Launchbuddy.ObjectManagers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Gw2_Launchbuddy
{
    internal static class LaunchManager
    {
        private static List<int> nomutexpros = new List<int>();

        public static void Launch()
        {
            //Checking for existing Gw2 instances. Do not continue until closed.
            //if (Process.GetProcesses().ToList().Where(a => !nomutexpros.Contains(a.Id) && a.ProcessName == Regex.Replace(exename, @"\.exe(?=[^.]*$)", "", RegexOptions.IgnoreCase)).Any())
            if (!ClientManager.ClientReg.CheckRegClients())
            {
                MessageBox.Show("At least one instance of Guild Wars is running that was not opened by LaunchBuddy. That instance will need to be closed.");
                return;
                //HandleManager.ClearMutex(exename, "AN-Mutex-Window-Guild Wars 2", ref nomutexpros);
            }

            if (Gw2_Launchbuddy.Properties.Settings.Default.useinstancegui)
            {
                if (Globals.Appmanager != null && Globals.Appmanager.Visibility == Visibility.Visible)
                {
                    Globals.Appmanager.Topmost = true;
                    Globals.Appmanager.Focus();
                    Globals.Appmanager.WindowState = WindowState.Normal;
                }
                else
                {
                    Globals.Appmanager.Show();
                }
            }

            //Launching the application with arguments
            if (AccountManager.SelectedAccountCollection.Count > 0)
            {
                foreach (var Account in AccountManager.SelectedAccountCollection)
                    LaunchGW2(Account);
            }
            else
            {
                LaunchGW2(AccountManager.DefaultAccount);
            }

            GFXManager.RestoreDefault();

            //Launching Singleton AddOns
            try
            {
                AddOnManager.SingletonAddon();
            }
            catch (Exception err)
            {
                MessageBox.Show("One or more AddOns could not be launched.\n" + err.Message);
            }
        }

        private static void LaunchGW2(Account Selected)
        {
            try
            {
                var gw2Client = Selected.CreateClient();
                if (Selected != AccountManager.DefaultAccount)
                {
                    Globals.LinkedAccs.Add(new AccountClient(Selected, gw2Client));
                    GFXManager.UseGFX(Selected.ConfigurationPath);
                }
                else
                {
                    Account undefacc = new Account("Acc Nr" + Globals.LinkedAccs.Count, null, null);
                    Globals.LinkedAccs.Add(new AccountClient(undefacc, gw2Client));
                }

                //Launching multilauch addons before.
                try
                {
                    AddOnManager.LaunchAllBefore();
                }
                catch (Exception err)
                {
                    MessageBox.Show("One or more multilaunch before AddOns could not be launched.\n" + err.Message);
                }

                try
                {
                    gw2Client.Start();
                }
                catch (Exception err)
                {
                    System.Windows.MessageBox.Show("Could not launch Gw2. Invalid path?\n" + err.Message);
                }
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        if (HandleManager.ClearMutex(ClientManager.ClientInfo.ProcessName, "AN-Mutex-Window-Guild Wars 2", ref nomutexpros)) i = 10;
                        //gw2Client.Process.WaitForInputIdle(10000);
                        //Thread.Sleep(1000);
                        //Register the new client to prevent problems.
                        //UpdateRegClients();
                        //Thread.Sleep(3000);
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message);
                    }
                }

                if (Properties.Settings.Default.use_priority)
                {
                    gw2Client.SetPriority(Properties.Settings.Default.priority);
                }

                //Launching multilauch addons after.
                try
                {
                    AddOnManager.LaunchAllAfter();
                }
                catch (Exception err)
                {
                    MessageBox.Show("One or more multilaunch after AddOns could not be launched.\n" + err.Message);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}