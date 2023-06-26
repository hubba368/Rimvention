using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Rimvention
{
    public class Thing_PartsBag : ThingWithComps
    {
        private PartsBagThingComp partsBagComp;
        private Dictionary<string, int> storedParts;

        public Dictionary<string, int> StoredParts { get => storedParts; }

        public override void PostMake()
        {
            base.PostMake();

        }

        public void InitPartStore()
        {
            partsBagComp = this.TryGetComp<PartsBagThingComp>();
            storedParts = partsBagComp.StoredParts;
        }

        public override string GetInspectString()
        {
            this.def.comps.Add(new AnimaAugmentCompProperties());
            if (this.TryGetComp<AnimaAugmentThingComp>() != null)
            {
              
            }
            if (this.TryGetComp<PartsBagThingComp>() == null)
            {
                Log.Error("parts bag thing comp is null");
                return "";
            }
            if (storedParts == null)
            {
                Log.Error("storedParts in partsBag is null");
                return "";
            }
                

            StringBuilder sBuilder = new StringBuilder();

            sBuilder.Append("Part Contents:" + "\n");
            int count = 0;
            foreach(var t in storedParts)
            {
                if ((count % 2) == 1)
                    sBuilder.Append(t.Key + ": " + t.Value + "\n");
                else
                    sBuilder.Append(t.Key + ": " + t.Value + " ");
                count++;
            }

            return sBuilder.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                Scribe_Collections.Look(ref storedParts, "storedParts", LookMode.Value);
            }
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Collections.Look(ref storedParts, "storedParts", LookMode.Value);
            }
        }
    }
}
