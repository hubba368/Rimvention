using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Rimvention
{
    public class Thing_AnimaAugment : ThingWithComps
    {
        private AnimaAugmentThingComp augmentComp;
        private Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>> storedImbues;

        public Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>> StoredImbues { get => storedImbues; }

        public override void PostMake()
        {
            base.PostMake();
        }
        public void InitPartStore()
        {
            augmentComp = this.TryGetComp<AnimaAugmentThingComp>();
            storedImbues = augmentComp.StoredImbues;
            test = 1;
            foreach (var t in storedImbues)
                Log.Error(t.Key + t.Value.Item1.ImbueClassName);
        }
        private int test;
        public override string GetInspectString()
        {
            if (this.TryGetComp<AnimaAugmentThingComp>() == null)
            {
                Log.Error("augment thing comp is null");
                return "";
            }
            if (storedImbues == null)
            {
                Log.Error(test.ToString());
                Log.Error("storedAugments in comp is null");
                return "";
            }

            StringBuilder sBuilder = new StringBuilder();

            sBuilder.Append("Imbues:" + "\n");
            int count = 0;
            foreach (var t in storedImbues)
            {
                if ((count % 2) == 1)
                    sBuilder.Append(t.Value.Item1.ImbueName + " | " + " Rank:" + t.Value.Item2 + "\n");
                else
                    sBuilder.Append(t.Value.Item1.ImbueName + " | " + " Rank:" + t.Value.Item2 + " ");
                count++;
            }

            return sBuilder.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Scribe_Collections.Look(ref storedImbues, "storedImbues", LookMode.Value);
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Collections.Look(ref storedImbues, "storedImbues", LookMode.Value);
            }
        }
    }
}
