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

            if (Properties.Settings.Default.useloadingui) LoadingWindow.Start("Launching Game");

            if (!ClientManager.ClientReg.CheckRegClients())
            {
                MessageBox.Show("At least one instance of Guild Wars is running that was not opened by LaunchBuddy. That instance needs to be closed.");
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
                foreach (Account Account in AccountManager.SelectedAccountCollection)
                    LaunchGW2(Account);
            }
            else
            {
                LaunchGW2(AccountManager.DefaultAccount);
            }

            GFXManager.RestoreDefault();

            //Launching AddOns
            try
            {
                AddOnManager.LaunchAll();
            }
            catch (Exception err)
            {
                MessageBox.Show("One or more AddOns could not be launched.\n" + err.Message);
            }

            if (Properties.Settings.Default.useloadingui) LoadingWindow.Stop();
        }

        public static void LaunchGW2(Account Selected)
        {
            try
            {
                if (Selected.ConfigurationPath != "Default")
                {
                    GFXManager.UseGFX(Selected.ConfigurationPath);
                }else
                {
                    GFXManager.RestoreDefault();
                }
            
            }
            catch { }

            try
            {
                Selected.CreateClient().Launch();
            }
            catch (Exception err)
            {
                MessageBox.Show("Launch failure. If you want to help, please provide the following:\r\n" + err.GetType().ToString() + "\r\n" + err.Message + "\r\n" + err.StackTrace);
            }
        }
    }
}