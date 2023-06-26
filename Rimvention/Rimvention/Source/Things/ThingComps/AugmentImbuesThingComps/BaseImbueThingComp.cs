using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Rimvention
{
    abstract class BaseImbueThingComp : ThingComp
    {
        public BaseImbueCompProperties Properties => (BaseImbueCompProperties)this.Properties;
        // inheritors of this class DO NOT need to make new compproperties, they will simply inherit this one
        // you would then (in xml) define which compclass you want to use.
        // can change compclass if needed in code
        public Pawn parentPawn;
        public bool isActive;

        public override void CompTick()
        {
            base.CompTick();
        }

        public abstract void InitImbue(Pawn pawn);

        public abstract void InitHediff(HediffDef hediff);

        protected virtual void UpdateCompClass(CompProperties props)
        {
            this.props = props;
        }


        public abstract void ActivateImbueStatic();
        public virtual void DeactivateImbues()
        {
            isActive = false;
        }
    }
}
