using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using Gw2_Launchbuddy.ObjectManagers;

namespace Gw2_Launchbuddy
{
    public static class VersionSwitcher
    {
        private static string URL_Releases = @"https://api.github.com/repos/TheCheatsrichter/Gw2_Launchbuddy/releases";
        private static List<string> URL_Versions = new List<string>();
        private static string Repo_User, Repo_Name;
        public static ObservableCollection<Release> Releaselist = new ObservableCollection<Release>();

        public static bool ShouldCheckForUpdate()
        {
            return LBConfiguration.Config.notifylbupdate;
        }

        public async static void CheckForUpdate()
        {
            if (Releaselist.Count == 0)
            {
                await GetReleaseList();
            }
            Version newest_version = new Version();
            Release newest_release = new Release();
            foreach (Release release in Releaselist)
            {
                if (release.Version.CompareTo(EnviromentManager.LBVersion) > 0)
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

            //Run helper and delete after finish
            ProcessStartInfo Info = new ProcessStartInfo();
            // Running cmd
            Info.FileName = "cmd.exe";
            // Tell it we are running a command directly, no user input
            Info.Arguments = "/C" +
                // Call the helper and pass expected params
                "CALL \"" + pathToUH + "\" " + Process.GetCurrentProcess().Id + " \"" + rel.Version + "\" \"" + rel.DownloadURL + "\" \"" + System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName) + "\" " +
                // Then delete UH when complete
                " & DEL \"" + pathToUH + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Process.Start(Info);
        }

        public static void DeleteUpdater()
        {
            try
            {
                if (File.Exists("Updater.exe")) File.Delete("Updater.exe");
            }
            catch
            {

            }
        }

        private static async System.Threading.Tasks.Task GetReleaseList()
        {
            /*
            Match Repomatches = Regex.Match(URL_Releases, @"github.com\/(?<User>\w+|\d)\/(?<Name>\w+)\/");

            Repo_User = Repomatches.Groups["User"].Value;
            Repo_Name = Repomatches.Groups["Name"].Value;
            */


            Octokit.GitHubClient client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Gw2Launchbuddy"));
            IReadOnlyList<Octokit.Release> raw_releases = await client.Repository.Release.GetAll("TheCheatsrichter", "Gw2_Launchbuddy");

            ObservableCollection<Release> fetched_releases = new ObservableCollection<Release>();

            foreach (var version in raw_releases)
            {
                try
                {
                    fetched_releases.Add(new Release(version));
                }
                catch
                {
                    Console.WriteLine("Unable to parse github release");
                }
            }
            Releaselist = fetched_releases;


                /*
                Regex filter = new Regex(@"<div class=""release-entry"">(\s|\S)+?<\/div><!-- \/.release -->");
                filter = new Regex(@"<div class=""d-flex flex-column flex-md-row my-5 flex-justify-center"">(\s|\S)+?<div class=""comment-reactions"); // temp test


                MatchCollection releases_raw = filter.Matches(HTML_Raw);

                ObservableCollection<Release> releases = new ObservableCollection<Release>();


                string repoprefix = @"\/" + Repo_User + @"\/" + Repo_Name;
                //Filters
                //string example = @"@"<h1 class=""release-title"">\s.*"">(?<Name>.*)<\/a>"";
                /*
                        string datefilter = @"<relative-time datetime=""((?<Date>\d+-\d+-\d+).*)"">.*<\/relative-time>";
                        string namefilter = @"<a href=""\/TheCheatsrichter\/Gw2_Launchbuddy\/releases\/tag\/(.*)"">(?<Name>.*)<\/a>";
                        string versionfilter = @"<a href=""" + repoprefix + @"\/releases\/tag\/(?<Version>\d+.\d+.*)"">";
                        versionfilter = @"<a href=""" + repoprefix + @"\/releases\/tag\/(?<Version>\d+.\d+.*)"">";
                        string downloadurlfilter = @"<a href="".*\/releases\/download\/.+\/(?<Exename>.*\.exe).*rel=""nofollow""";
                        string descriptionfilter = @"<div class=""markdown-body"">\s*?(?<Description>\s|\S)+?<\/div>";



                */

                /*
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
                */
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

        public Release (Octokit.Release release)
        {
            Name = release.Name;
            Version = new Version(release.TagName);
            Description = release.Body;
            DownloadURL = release.Assets.First(x=>x.Name.Contains(".exe")).BrowserDownloadUrl;
            Date = release.PublishedAt.ToString();
        }

        public Release() { }
    }

}