using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Controls;
using System.Net.NetworkInformation;
using System.Xml;
using System.IO;
using System.Diagnostics;
using IWshRuntimeLibrary;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO.Compression;
using System.Windows.Input;

namespace Gw2_Launchbuddy
{

    //Small Custom UI Brainstorming

    public class LBUIElement
    {
        public int ElementID { set; get; }
        public bool IsMultilaunch { set; get; }
        public UIWindow Window { set; get; }
        public Icon Icon { set; get; }
        public bool IsDragged { set; get; }
        public Point GrabPoint { set; get; }

        public  void StartDrag(object sender, MouseButtonEventArgs e)
        {
            UserControl control = sender as UserControl;
            control.CaptureMouse();
            IsDragged = true;
            //GrabPoint = e.GetPosition(this);
        }

        public  void StopDrag(object sender)
        {
            (sender as UserControl).ReleaseMouseCapture();
            IsDragged = false;
        }

        public void Drag(object sender,Point MousePos)
        {
            var UserControl = sender as UserControl;

            //Would have to change Canvas offset
        }
    }

    public class UIWindow
    {
        public Point StartPos { set; get; }
        public int Width { set; get; }
        public int Height { set; get; }
    }


    public static class CustomUI
    {
        public static List<LBUIElement> UIElements = new List<LBUIElement>();
        public static Canvas UiCanvas = new Canvas();

        //And here would go the functions
        public static void S (object sender)
        {
            var UIControl = sender as UserControl;

        }

    }
}
