using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.IO;

namespace Gw2_Launchbuddy
{
    public static class VersionSwitcher
    {
        private static string URL_Releases = @"https://github.com/TheCheatsrichter/Gw2_Launchbuddy/releases";
        private static List<string> URL_Versions = new List<string>();
        private static string Repo_User, Repo_Name;
        public static ObservableCollection<Release> Releaselist = new ObservableCollection<Release>();

        public static void CheckForUpdate()
        {
            if (Releaselist.Count == 0)
            {
                GetReleaseList();
            }
            Version newest_version = new Version();
            Release newest_release = new Release();
            foreach (Release release in Releaselist)
            {
                if (release.Version.CompareTo(Globals.LBVersion) > 0)
                {
                    if (release.Version.CompareTo(newest_version) > 0)
                    {
                        newest_version = release.Version;
                        newest_release = release;
                    }
                }
            }
            if (newest_version.ToString() != "0.0")
            {
                MessageBoxResult win = MessageBox.Show("A new Version of Gw2 Launchbuddy is available!\n\nDo you want to update to Gw2 Launchbuddy V" + newest_version.ToString() + "?\n\nIt is also possible to manually update Launchbuddy or to disable the autoupdatecheck in the 'LB settings' tab", "Release Download", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (win.ToString() == "Yes")
                {
                    ApplyRelease(newest_release);
                }
            }
        }

        public static void ApplyRelease(Release rel)
        {
            //Create Update Helper
            string pathToUH = System.IO.Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "\\Updater.exe";
            if (!System.IO.File.Exists(pathToUH)) System.IO.File.WriteAllBytes(pathToUH, Properties.Resources.Update_Helper);

            //Execute Update Helper
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = Process.GetCurrentProcess().Id + " \"" + rel.Version + "\" \"" + rel.DownloadURL + "\" \"" + System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName) + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = pathToUH;
            Process.Start(Info);
        }

        public static void DeleteUpdater()
        {
            if (File.Exists("Updater.exe")) File.Delete("Updater.exe");
        }

        public static void GetReleaseList()
        {
            Match Repomatches = Regex.Match(URL_Releases, @"github.com\/(?<User>\w+|\d)\/(?<Name>\w+)\/");

            Repo_User = Repomatches.Groups["User"].Value;
            Repo_Name = Repomatches.Groups["Name"].Value;

            string HTML_Raw = "";
            using (WebClient downloader = new WebClient())
            {
                try
                {
                    HTML_Raw = downloader.DownloadString(URL_Releases);
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("Unable to check for Launchbuddy Updates.\n Please check your internet connection");
                    return;
                }
            }
            Regex filter = new Regex(@"<div class=""release-entry"">(\s|\S)+?<\/div><!-- \/.release -->");
            MatchCollection releases_raw = filter.Matches(HTML_Raw);

            ObservableCollection<Release> releases = new ObservableCollection<Release>();

            string repoprefix = @"\/" + Repo_User + @"\/" + Repo_Name;
            //Filters
            //string example = @"@"<h1 class=""release-title"">\s.*"">(?<Name>.*)<\/a>"";
            string datefilter = @"<relative-time datetime=""((?<Date>\d+-\d+-\d+).*)"">.*<\/relative-time>";
            string namefilter = @"<a href=""\/TheCheatsrichter\/Gw2_Launchbuddy\/releases\/tag\/(.*)"">(?<Name>.*)<\/a>";
            string versionfilter = @"<a href=""" + repoprefix + @"\/releases\/tag\/(?<Version>\d+.\d+.*)"">";
            versionfilter = @"<a href=""" + repoprefix + @"\/releases\/tag\/(?<Version>\d+.\d+.*)"">";
            string downloadurlfilter = @"<a href="".*\/releases\/download\/.+\/(?<Exename>.*\.exe).*rel=""nofollow""";
            string descriptionfilter = @"<div class=""markdown-body"">\s*?(?<Description>\s|\S)+?<\/div>";

            foreach (Match version in releases_raw)
            {
                try
                {
                    Release release = new Release
                    {
                        HTML_raw = version.Value,
                        Date = Regex.Match(version.Value, datefilter).Groups["Date"].Value,
                        Name = Regex.Match(version.Value, namefilter).Groups["Name"].Value,
                        Version = new Version(Regex.Match(version.Value, versionfilter).Groups["Version"].Value),
                        Description = "<html>\n" + Regex.Match(version.Value, descriptionfilter).Value + "\n</html>",
                        DownloadURL = URL_Releases + "/download/" + Regex.Match(version.Value, versionfilter).Groups["Version"].Value+ "/" + Regex.Match(version.Value, downloadurlfilter).Groups["Exename"].Value,
                    };
                    releases.Add(release);
                }
                catch
                {
                    Console.Write("Skipping one release");
                }
                
            }
            Releaselist = releases;
        }
    }

    public class Release
    {
        public string HTML_raw { set; get; }
        public string Name { set; get; }
        public Version Version { set; get; }
        public string Description { set; get; }
        public string DownloadURL { set; get; }
        public string Date { set; get; }
    }
}