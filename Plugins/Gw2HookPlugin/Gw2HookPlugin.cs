using Gw2_Launchbuddy.Interfaces.Plugins;
using Gw2_Launchbuddy.Interfaces.Plugins.Injectable;
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
    public class Gw2HookPlugin : IPluginInjectable, IPluginDerived
    {
        public PluginInfo Plugin => new PluginInfo()
        {
            // Info for the plugin
            Name = "Gw2Hook Plugin",
            Version = "0.0.1",
            Author = "KairuByte",
            Url = null,
            Description = "A plugin to download/update/inject Gw2Hook by 04348(Grenbur), which can be found here: https://04348.github.io/Gw2Hook/"
        };
        public ProjectInfo Project => new ProjectInfo()
        {
            // Project info for the file being injected.
            Name = "Gw2Hook",
            Version = projectversion,
            Author = "04348",
            Url = "https://04348.github.io/Gw2Hook/",
        };
        public PluginSettings Settings => new PluginSettings()
        {
            // The subdirectory within Plugins where data is stored for this plugin.
            Subdirectory = "Gw2Hook",
            Target = "bin64\\d3d9.dll",
        };
        public static string projectversion = null;

        public void Init()
        {
            var downloadCache = System.IO.Directory.CreateDirectory(Settings.Subdirectory + "DownloadCache").FullName + "\\";
            var filename = "";

            using (var client = new WebClient())
            {
                var result = client.DownloadString(Project.Url + "download.html");
                var text = result.Replace("\r", "").Split('\n');
                var line = text.Where(a => a.Contains("window.open(\"")).FirstOrDefault();
                filename = line.Split('"')[1];

                if (!File.Exists(downloadCache + filename) || !File.Exists(Settings.Target))
                {
                    client.DownloadFile(Project.Url + filename, downloadCache + filename);

                    ZipFile.OpenRead(downloadCache + filename).ExtractToDirectory(Settings.Subdirectory, true);
                }
            }

            var filenameSplit = Path.GetFileNameWithoutExtension(filename).Split(new char[] { '_' }, 2);
            // Getting version is bad because it relies on internet and github up.
            projectversion = filenameSplit[1].Replace('_', '.');
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