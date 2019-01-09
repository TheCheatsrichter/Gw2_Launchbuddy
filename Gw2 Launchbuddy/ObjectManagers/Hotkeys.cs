using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using System.Windows.Input;

namespace Gw2_Launchbuddy.ObjectManagers
{
    public static class Hotkeys
    {
        //Blacklist which Functions actually should not be visible
        public static List<string> Command_Whitelist = new List<string>
        {
            "Launch",
            "Resume",
            "Suspend",
            "Focus",
            "Close"
        };
        public static ObservableCollection<Hotkey> HotkeyCollection = new ObservableCollection<Hotkey>();

        public static void Add(string name, Type type, string command, Key keyvalue) { HotkeyCollection.Add(new Hotkey(name, type, command, keyvalue)); }
        public static void Add(Type Targettype) { HotkeyCollection.Add(new Hotkey(Targettype)); }
        public static void Remove(Hotkey hotkey) { if (HotkeyCollection.Contains(hotkey)) HotkeyCollection.Remove(hotkey); }
    }


    [Serializable]
    public class Hotkey
    {
        public string Name { set; get; }
        [XmlIgnore]
        public object TargetObject { set; get; }
        [XmlIgnore]
        public Type TargetType { get { if(TargetObject!=null)return TargetObject.GetType(); return null; } }
        public string Command { set; get; }
        private Key keyvalue { set; get; }
        public ModifierKeys Modifiers { set; get; }
        [XmlIgnore]
        public ObservableCollection<string> Commands { get { return GetCommands(); } }
        [XmlIgnore]
        public string KeyAsString { get { return keyvalue.ToString() + "+" + Modifiers.ToString(); } }

        private ObservableCollection<string> GetCommands()
        {
            if (TargetObject == null) return null;
            ObservableCollection<string> commands = new ObservableCollection<string>();
            foreach (var func in TargetType.GetMethods())
            {
                if (func.ReturnType.Name == "Void" && func.IsPublic && func.GetParameters().Length == 0 && Hotkeys.Command_Whitelist.Contains(func.Name))
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

        public Hotkey() { Hotkeys.HotkeyCollection.Add(this); }

        public Hotkey(Object Target)
        {
            TargetObject = Target;
            Hotkeys.HotkeyCollection.Add(this);
        }
        public Hotkey(string name, Object target, string command, Key keyvalue)
        {
            Name = name;
            Command = command;
            KeyValue = keyvalue;
            TargetObject = target;
            Hotkeys.HotkeyCollection.Add(this);
        }

        public void Execute()
        {
            try
            {
                foreach (var func in TargetType.GetMethods())
                {
                    if (func.Name == Command) func.Invoke(TargetObject,null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Hotkey {Name} did not execute successfully.\n{e.Message}");
            }
        }
    }
}
