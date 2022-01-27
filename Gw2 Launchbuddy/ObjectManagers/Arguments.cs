using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Gw2_Launchbuddy.ObjectManagers
{
    [Serializable]
    public class Arguments:ObservableCollection<Argument>
    {
        public Arguments(bool import=false)
        {
            Add(new Argument("-32", "Forces the game to run in 32 bit.",false));
            Add(new Argument("-bmp", "Forces the game to create lossless screenshots as .BMP files. Use for creating high-quality screenshots at the expense of much larger files.", false));
            Add(new Argument("-dx9single", "Enables the Direct3D 9c renderer in single-threaded mode. Improves performance in Wine with CSMT.", false));
            Add(new Argument("-forwardrenderer", "Uses Forward Rendering instead of Deferred Rendering (unfinished). This currently may lead to shadows and lighting to not appear as expected.It may increase the framerate and responsiveness when using AMD graphics card", false));
            Add(new Argument("-log", "Enables the creation of a log file, used mostly by Support. The path for the generated file usually is found in the APPDATA folder", false));
            Add(new Argument("-mce", "Start the client with Windows Media Center compatibility, switching the game to full screen and restarting Media Center (if available) after the client is closed.", false));
            Add(new Argument("-nomusic", "Disables music and background music.", false));
            Add(new Argument("-noui", "Disables the user interface. This does the same thing as pressing Ctrl+Shift+H in the game.", false));
            Add(new Argument("-nosound", "Disables audio system completely.", false));
            Add(new Argument("-prefreset", "Resets game settings.", false));
            Add(new Argument("-uispanallmonitors", "Spreads user interface across all monitors in a triple monitor setup.", false));
            Add(new Argument("-useOldFov", "Disables the widescreen field-of-view enhancements and restores the original field-of-view.", false));
            Add(new Argument("-windowed", "Forces Guild Wars 2 to run in windowed mode. In game, you can switch to windowed mode by pressing Alt + Enter or clicking the window icon in the upper right corner.", false));
            Add(new Argument("-umbra gpu", "Forces the use of umbra's GPU accelerated culling. In most cases, using this results in higher cpu usage and lower gpu usage decreasing the frame-rate.", false));
            Add(new Argument("-maploadinfo", "Shows diagnostic information during map loads, including load percentages and elapsed time.", false));
            Add(new Argument("-dx9", "Forces the game to run using the DirectX 9 renderer. Keep in mind that injected Dlls have to work with DirectX 9! (Guild Wars 2 default)", false));
            Add(new Argument("-dx11", "Forces the game to run using the beta DirectX 11 renderer. Keep in mind that injected Dlls have to work with DirectX 11!", false));
        }

        private Arguments() { }

        public void SetActive(Argument arg,bool active)=>this.First<Argument>(a => a.Flag == arg.Flag).IsActive=active;
        public void SetActive(string arg, bool active) => this.First<Argument>(a => a.Flag == arg).IsActive = active;

        public ObservableCollection<Argument> GetActive() => new ObservableCollection<Argument>(this.Where<Argument>(a => a.IsActive));

        public void CheckCompatibility ()
        {
            Argument dx9arg= this.First<Argument>(a => a.Flag == "-dx9");
            Argument dx11arg = this.First<Argument>(a => a.Flag == "-dx11");

            if(dx9arg.IsActive == dx11arg.IsActive)
            {
                dx9arg.IsActive = false;
                dx11arg.IsActive = false;
            }
        }

        public void ImportCustomArguments()
        {
            foreach(Argument arg in LBConfiguration.Config.arguments_custom)
            {
                Add(arg);
            }
        }
    }
    [Serializable]
    public class Argument
    {
        public string Flag { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public bool DeleteAble { get; set; }

        private Argument() { }

        public Argument(string flag,string description,bool active,bool deleteable=false)
        {
            Flag = flag;
            Description = description;
            IsActive = active;
            DeleteAble = deleteable;
        }

        public override string ToString()
        {
            return Flag;
        }
    }

}
