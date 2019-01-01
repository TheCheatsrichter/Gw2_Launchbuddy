using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2_Launchbuddy
{
    public class MusicConfig
    {
        private ObservableCollection<Playlist> Playlists
        {
            get{
                ObservableCollection<Playlist> tmp = new ObservableCollection<Playlist>();
                tmp.Add(Ambient);
                tmp.Add(Battle);
                tmp.Add(BossBattle);
                tmp.Add(City);
                tmp.Add(Mainmenu);
                tmp.Add(Nighttime);
                tmp.Add(Underwater);
                return tmp;
            }
        }

        Playlist Ambient = new Playlist();
        Playlist Battle = new Playlist();
        Playlist BossBattle = new Playlist();
        Playlist City = new Playlist();
        Playlist Mainmenu = new Playlist();
        Playlist Nighttime = new Playlist();
        Playlist Underwater = new Playlist();

        ObservableCollection<string> FindInvalid()
        {
            ObservableCollection<string> songs = new ObservableCollection<string>();
            foreach (Playlist lists in Playlists)
            {
                lists.SelfClean();
            }
            return songs;
        }

        void SaveToFile(string path)
        {
            //xml stuff here

        }
    }

    public enum musictype { none, ambient, battle, bossbattle, city, mainmenu, nighttime, underwater };

    public class Playlist
    {
        public ObservableCollection<string> songs;
        musictype type = musictype.none;

        void ToM3U(string path)
        {
            SelfClean();
            string output = "";
            foreach (string song in songs)
            {
                output += song + @"\n";
            }
            File.WriteAllText(path, output);
        }

        void FromM3U(string path)
        {
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    songs.Add(line);
                }
                SelfClean();
            }
            else
            {
                MessageBox.Show("Invalid Playlist path.");
            }
        }

        public ObservableCollection<string> SelfClean()
        {
            ObservableCollection<string> invalid_songs = new ObservableCollection<string>();
            foreach (string song in songs)
            {
                if (!File.Exists(song))
                {
                    songs.Remove(song);
                    invalid_songs.Add(song);
                }
            }
            return invalid_songs;
        }

        void SetType(musictype mtype)
        {
            type = mtype;
        }
    }
}


