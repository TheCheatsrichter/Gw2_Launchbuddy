using Gw2_Launchbuddy.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Gw2HookPlugin
{
    public class Gw2HookPlugin : IOverlay
    {
        private string PluginSubDirectory => "Plugins/Gw2Hook/";
        public string Name => "Gw2Hook LaunchBuddy Plugin";

        public string Version => "0.0.1";

        public string ProjectName => "Gw2Hook";

        public string ProjectURL => "https://04348.github.io/Gw2Hook/";
        public string OverlayDll => PluginSubDirectory + "bin64/d3d9.dll";

        public void Init()
        {
            var downloadCache = PluginSubDirectory + "DownloadCache/";
            System.IO.Directory.CreateDirectory(downloadCache);
            using (var client = new WebClient())
            {
                var result = client.DownloadString(ProjectURL + "download.html");
                var text = result.Replace("\r", "").Split('\n');
                var line = text.Where(a => a.Contains("window.open(\"")).FirstOrDefault();
                var filename = line.Split('"')[1];
                
                if (!File.Exists(downloadCache + filename) || !File.Exists(PluginSubDirectory + "bin64/d3d9.dll"))
                {
                    client.DownloadFile(ProjectURL + filename, downloadCache + filename);

                    ZipFile.OpenRead(downloadCache + filename).ExtractToDirectory(PluginSubDirectory, true);
                    
                    //ZipFile.ExtractToDirectory(downloadCache + filename, PluginSubDirectory);
                }
            }
        }
    }
    public static class ZipArchiveExtensions
    {
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }
        }
    }
}