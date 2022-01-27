using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using Gw2_Launchbuddy.ObjectManagers;
using Gw2_Launchbuddy;

public static class MusicManager
{
    public static ObservableCollection<MusicPlaylist> Playlists = new ObservableCollection<MusicPlaylist>();

    public static MusicPlaylist Edit_Playlist;
    public static void Init()
    {
        ImportPlaylists();
        CleanUpPlaylists();
        AddBasePlaylists();
    }
    public static void CleanUpPlaylists()
    {
        foreach (var playlist in GetInvalidPlaylists())
        {
            playlist.PurgeInvalidSources();
        }
    }
    public static bool AllPlaylistsValid()
    {
        return GetInvalidPlaylists().Count() <= 0;
    }
    public static List<MusicPlaylist> GetInvalidPlaylists()
    {
        return Playlists.Where(x => !x.IsValid()).ToList();
    }

    private static void AddBasePlaylists()
    {
        string[] pl_names = new string[]
        {
            "Ambient",
            "Battle",
            "BossBattle",
            "Crafting",
            "City",
            "Defeated",
            "MainMenu",
            "NighTime",
            "Underwater",
            "Victory"
        };

        foreach (string pl_name in pl_names)
        {
            if (!HasPlaylist(pl_name))
            {
                Playlists.Add(new MusicPlaylist(pl_name,true));
            }
        }
    }
    public static bool HasPlaylist(string name)
    {
        return Playlists.Any(x => x.Name == name);
    }

    public static void ImportPlaylists()
    {
        string folder = EnviromentManager.CustomMusic_Folderpath;

        foreach (string playlistpath in Directory.GetFiles(folder, "*.m3u", SearchOption.TopDirectoryOnly))
        {
            string pl_name = Path.GetFileNameWithoutExtension(playlistpath);
            MusicPlaylist pl_imported = new MusicPlaylist(pl_name,true);
            pl_imported.LoadFromM3U(playlistpath);
            if (HasPlaylist(pl_name))
            {
                var playlist = Playlists.FirstOrDefault(x => x.Name == pl_name);
                playlist?.Add(pl_imported);
            }else
            {
                Playlists.Add(pl_imported);
            }
        }

        folder = EnviromentManager.CustomMusicDisabled_Folderpath;

        foreach (string playlistpath in Directory.GetFiles(folder, "*.m3u", SearchOption.TopDirectoryOnly))
        {
            string pl_name = Path.GetFileNameWithoutExtension(playlistpath);
            MusicPlaylist pl_imported = new MusicPlaylist(pl_name,false);
            pl_imported.Enabled = false;
            pl_imported.LoadFromM3U(playlistpath);
            if (HasPlaylist(pl_name))
            {
                var playlist = Playlists.FirstOrDefault(x => x.Name == pl_name);
                playlist?.Add(pl_imported);
            }
            else
            {
                Playlists.Add(pl_imported);
            }
        }
    }

    public static void ExportPlaylists()
    {
        foreach (var playlist in Playlists.Where(x => x.Enabled))
        {
            playlist.SaveToM3U();
        }
        foreach (var playlist in Playlists.Where(x => !x.Enabled))
        {
            playlist.SaveToM3U();
        }
    }

}

public class MusicSource
{
    public string SourcePath { set; get; }

    public string SourceName
    {
        get
        {
            return Path.GetFileName(SourcePath);
        }
    }

    private MusicSourceType musicsourcetype = MusicSourceType.local;

    enum MusicSourceType
    {
        local = 0,
        streamed = 1
    }

    public MusicSource() { }
    public MusicSource(string sourcepath)
    {
        SourcePath = sourcepath;
        if (Regex.IsMatch(SourcePath.ToString(), @"https?:\/\/.+"))
        {
            musicsourcetype = MusicSourceType.streamed;
        }
    }
    public virtual bool IsValid()
    {
        bool isvalid = SourcePath != null;
        switch (musicsourcetype)
        {
            case MusicSourceType.local:
                if (isvalid)
                {
                    isvalid = LocalSourceExists();
                }
                return isvalid;

            case MusicSourceType.streamed:
                return Regex.IsMatch(SourcePath.ToString(), @"https?:\/\/.+");
        }
        return false;
    }

    public virtual bool LocalSourceExists()
    {
        return File.Exists(SourcePath.ToString());
    }
}


public static class PlaylistDescriptions
{
    static Dictionary<string, string> description = new Dictionary<string, string>
    {
        {"Ambient","Default playlist when no other criteria are met" },
        {"Battle","Large fights involving many foes (~5 or more enemies); some personal story steps; PvP; when player's health reaches 50% or lower (the custom music will continue to play until combat ends)" },
        {"BossBattle","World bosses and some Dungeon bosses; some personal story steps; some Activities" },
        {"Crafting","While using a crafting station. Attention: Seems to be pretty buggy" },
        {"City","When inside one of the main cities, such as The Black Citadel, but not always" },
        {"Defeated","Upon being Defeated" },
        {"MainMenu","At the character select screen" },
        {"NightTime","When in some explorable areas and it is nighttime (the moon is out)" },
        {"Underwater","Any time the breathing apparatus is equipped. Overrides most other playlists" },
        {"Victory","Music that plays after World bosses or Meta events. Additionally, plays in some personal story steps" }
    };

    public static string GetDescription(string playlistname)
    {
        try
        {
            return description[playlistname];
        }
        catch
        {
            return "No Description available. Keep in mind that this playlist might not is supported by Guild Wars 2.";
        }

    }
}
public class MusicPlaylist : ObservableCollection<MusicSource>
{
    public string Name { get; set; }

    private bool enabled = false;
    public bool Enabled
    {
        set
        {
            if(value!=enabled)
            {
                if (File.Exists(Path))
                {
                    string oldpath = Path;
                    enabled = value;
                    if (File.Exists(Path)) File.Delete(Path);
                    File.Move(oldpath, Path);
                }
            }
        }
        get
        {
            return enabled;
        }
    }

    public bool HasSources
    {
        get
        {
            return Count > 0;
        }
    }

    public string Description
    {
        get
        {
            return PlaylistDescriptions.GetDescription(Name);
        }
        set { }
    }

    string Path
    {
        get
        {
            if(Enabled)
            {
                return EnviromentManager.CustomMusic_Folderpath + Name + ".m3u";
            }
            return EnviromentManager.CustomMusicDisabled_Folderpath + Name + ".m3u";
        }
    }
    public MusicPlaylist(string name,bool enabled)
    {
        Name = name;
        this.enabled = enabled;
    }
    public bool IsValid(bool purge_invalid = true)
    {
        return this.Any(x => !x.IsValid());
    }

    public void PurgeInvalidSources()
    {
        foreach (var source in InvalidSources())
        {
            this.Remove(source);
        }
    }
    public List<MusicSource> InvalidSources()
    {
        return this.Where(x => !x.IsValid()).ToList();
    }
    public new void Add(MusicSource source)
    {
        if (source.IsValid())
        {
            base.Add(source);
        }
    }
    public new void Remove(MusicSource source)
    {
        base.Remove(source);
        if(Count==0)
        {
            if(File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }

    public void Add(MusicPlaylist playlist)
    {
        playlist.PurgeInvalidSources();

        if (playlist.IsValid())
        {
            foreach (var song in playlist.Items)
            {
                this.Add(song);
            }
        }
    }

    public void SaveToM3U(string path ="")
    {
        if (path == "") path = Path;

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (Count <= 0)
        {
            return;
        }
        using (var f = File.Open(path, FileMode.Create))
        {
            string output = "QuagganRandomizer.mp3\n";

            foreach (var musicfile in this.Items)
            {
                output += $"{musicfile.SourcePath}\n";
            }

            byte[] outputst = Encoding.UTF8.GetBytes(output);

            f.Write(outputst,0,outputst.Length);
        }
    }

    public bool LoadFromM3U(string path)
    {
        if (!File.Exists(path))
        {
            return false;
            throw new FileNotFoundException("Could not open M3U playlist at =" + path);
        }

        var sr = File.ReadAllLines(path);

        //Ignore line one as it is used randomize playlist
        for(int i =1; i<sr.Length;i++)
        {
            Add(new MusicSource(sr[i]));
        }

        return true;
    }
}