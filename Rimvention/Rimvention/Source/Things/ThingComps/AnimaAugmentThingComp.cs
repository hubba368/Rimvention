using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Rimvention
{
    class AnimaAugmentThingComp : ThingComp
    {
        private Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>> storedImbues = new Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>>();

        public AnimaAugmentCompProperties Properties => (AnimaAugmentCompProperties)this.Properties;
        public Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>> StoredImbues { get => storedImbues; }

        public override void CompTick()
        {
            base.CompTick();
        }

        public void AddToStore(Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>> imbues)
        {
            if (storedImbues == null || storedImbues.Count == 0)
            {
                storedImbues.AddRange(imbues);
            }
        }
    }
}
