using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Gw2_Launchbuddy.ObjectManagers
{
    [Serializable]
    public class ProcessAffinityConfig
    {
        private bool[] procMask;
        public bool[] ProcMask { set { procMask = value; ValidateConfig(); } get { return procMask; } }
        [XmlIgnore]
        public List<CheckBox> UIBoxes { set; get; }

        public IntPtr MaskAsPtr {get
            {
                IntPtr sum = IntPtr.Zero;
                for(int i=0;i<ProcMask.Length;i++)
                {
                    sum += (procMask[i]? 1:0) << i;
                }
                
                if(sum == IntPtr.Zero)
                {
                    for (int i = 0; i < ProcMask.Length; i++)
                    {
                        sum += 1 << i;
                    }
                }
                
                return sum;
            }
        }

        public ProcessAffinityConfig()
        {
            SetupBitArray();
        }

        private void SetupBitArray()
        {
            var cpus = new bool[Environment.ProcessorCount];
            for (int i = 0; i < cpus.Length; i++)
            {
                cpus[i] = true;
            }
            ProcMask = cpus;
        }

        private void ValidateConfig()
        {
            int availableCores = Environment.ProcessorCount;
            if (ProcMask!=null)
            {
                BitArray newarray = new BitArray(availableCores, false);

                for(int i=0;i<procMask.Length && i<newarray.Length;i++)
                {
                    newarray[i]=procMask[i];
                }
                procMask = new bool[newarray.Length];
                newarray.CopyTo(procMask,0);
            }else
            {
                procMask = new bool[availableCores];
                new BitArray(availableCores,true).CopyTo(procMask,0);
            }

            if(UIBoxes!=null)
            {
                foreach (var box in UIBoxes)
                {
                    box.Click -= OnChecked;
                }
            }
            UIBoxes = GetUIFromConfig();
        }

        public List<CheckBox> GetUIFromConfig()
        {
            CheckBox[] combs = new CheckBox[ProcMask.Length];
            for (int i = 0; i < combs.Length; i++)
            {
                if (combs[i]==null) combs[i]=new CheckBox();
                combs[i].IsChecked = ProcMask[i];
                combs[i].Content = "Use CPU "+i;
                combs[i].Click += OnChecked;
            }
            return combs.ToList<CheckBox>();
        }

        private void OnChecked(object sender, EventArgs e)
        {
            var checkb = sender as CheckBox;
            if (checkb == null) return;
            int processorindex= UIBoxes.IndexOf(checkb);
            if(checkb.IsChecked!=null)ProcMask[processorindex]=(bool)checkb.IsChecked;
        }
    }
}
