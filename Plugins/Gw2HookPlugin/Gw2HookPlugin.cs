using Gw2_Launchbuddy.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.IO;

namespace Gw2HookPlugin
{
    public class Gw2HookPlugin : IOverlay
    {
        public string Name => "Gw2Hook LaunchBuddy Plugin";

        public string Version => "0.0.1";

        public string ProjectName => "Gw2Hook";

        public string ProjectURL => "https://github.com/04348/Gw2Hook";
        public string OverlayDll => "Gw2HookOverlay.dll";

        public void Init()
        {
            return;
        }
    }
}