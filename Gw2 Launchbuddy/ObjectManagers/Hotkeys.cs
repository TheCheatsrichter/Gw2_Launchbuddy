using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using System.Windows.Input;
using System.Runtime.InteropServices;
using NHotkey.Wpf;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public interface IHotkey
    {
        uint ID { get; set; }
        object TargetObject { get; set; }
        string Command { get; set; }
        ModifierKeys Modifiers { get; set; }
        string KeyAsString { get; }

        ObservableCollection<string> GetCommands();
        Key KeyValue { get; set; }
        void Execute(object sender,NHotkey.HotkeyEventArgs e);
        EventHandler<NHotkey.HotkeyEventArgs> ExecuteEvent { get; set; }
    }

    public static class Hotkeys
    {
        //Blacklist which Functions actually should not be visible
        public static List<string> Command_Whitelist = new List<string>
        {
            "Launch",
            "Resume",
            "Suspend",
            "Focus",
            "Minimize",
            "Maximize",
            "Close"
        };
        private static ObservableCollection<IHotkey> HotkeyCollection = new ObservableCollection<IHotkey>();

        private static uint index=0;
        public static uint Index
        {
            get { index++; return index; }
        }

        public static void Add(IHotkey hotkey) { if (!HotkeyCollection.Contains(hotkey)) HotkeyCollection.Add(hotkey); }
        public static void Remove(IHotkey hotkey) { if (HotkeyCollection.Contains(hotkey)) HotkeyCollection.Remove(hotkey); }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static void RegisterAll()
        {
            foreach (IHotkey hotkey in HotkeyCollection)
            {
                try
                {
                    HotkeyManager.Current.AddOrReplace(hotkey.ID.ToString(), hotkey.KeyValue, hotkey.Modifiers, hotkey.ExecuteEvent);
                }
                catch
                {
                    HotkeyManager.Current.Remove(hotkey.ID.ToString());
                }
            }
        }

        public static void UnregisterAll()
        {
            foreach (IHotkey hotkey in HotkeyCollection)
            {
                HotkeyManager.Current.Remove(hotkey.ID.ToString());
            }
        }
    }


    [Serializable]
    public class Hotkey:IHotkey
    {
        [XmlIgnore]
        public uint ID { set; get; }
        [XmlIgnore]
        private object targetobject;
        [XmlIgnore]
        public virtual object TargetObject { set { targetobject = value; } get { return targetobject; } }
        [XmlIgnore]
        public virtual Type TargetType { get { if(TargetObject!=null)return TargetObject.GetType(); return null; } }
        public string Command { set; get; }
        private Key keyvalue { set; get; }
        public ModifierKeys Modifiers { set; get; }
        [XmlIgnore]
        public ObservableCollection<string> Commands { get { return GetCommands();}}
        [XmlIgnore]
        public string KeyAsString {
            get {
                if(Modifiers!=ModifierKeys.None)
                    return Modifiers.ToString() + "+" + keyvalue.ToString();
                if(keyvalue!= Key.None)
                    return keyvalue.ToString();
                return "No Key set";
            }
        }
        [XmlIgnore]
        private EventHandler<NHotkey.HotkeyEventArgs> executevent;
        [XmlIgnore]
        public EventHandler<NHotkey.HotkeyEventArgs> ExecuteEvent { set { executevent = value; } get { return executevent; } }

        public virtual ObservableCollection<string> GetCommands()
        {
            if (TargetObject == null) return null;
            ObservableCollection<string> commands = new ObservableCollection<string>();
            foreach (var func in TargetType.GetMethods())
            {
                if (func.ReturnType.Name == "Void" && func.IsPublic && func.GetParameters().Length==0 && Hotkeys.Command_Whitelist.Contains(func.Name))
                {
                    commands.Add(func.Name);
                }
            }
            return commands;
        }

        public Key KeyValue
        {
            set { keyvalue = value; }
            get { return keyvalue; }
        }

        public void Init()
        {
            Hotkeys.Add(this);
            executevent += Execute;
            ID = Hotkeys.Index;
        }

        public Hotkey() { Init(); }

        public Hotkey(Object Target)
        {
            TargetObject = Target;
            Init();
        }
        public Hotkey(Object target, string command, Key keyvalue)
        {
            Command = command;
            KeyValue = keyvalue;
            TargetObject = target;
            Init();
        }

        public virtual void Execute(object sender,NHotkey.HotkeyEventArgs e)
        {
            try
            {
                foreach (var func in TargetType.GetMethods())
                {
                    if (func.Name == Command) func.Invoke(TargetObject,null);
                }
            }
            catch
            {
                MessageBox.Show($"Hotkey did not execute successfuly.\n");
            }
        }
    }

    public class AccountHotkey : Hotkey
    {
        public int AccountId { get; set; }
        private Account Account { get { return AccountManager.GetAccountByID(AccountId); } }
        public override object TargetObject { get { return Account.Client; } }

        public AccountHotkey(int accountid)
        {
            AccountId= accountid;
            Init();
        }
        private AccountHotkey() { Init(); }

        public override ObservableCollection<string> GetCommands()
        {
            return new ObservableCollection<string> {"Launch","Close","Focus","Suspend","Resume","Maximize","Minimize"};
        }
    }
}

