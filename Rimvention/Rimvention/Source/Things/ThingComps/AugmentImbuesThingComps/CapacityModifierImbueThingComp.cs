using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Rimvention
{
    class CapacityModifierImbueThingComp : BaseImbueThingComp
    {
        private Hediff hediff;
        private HediffDef hediffDef;

        public override void InitImbue(Pawn pawn)
        {
            // setup anything before usage
            // e.g setup hediffcomps with correct defs etc
            parentPawn = pawn;
            base.isActive = true;
        }

        public override void InitHediff(HediffDef def)
        {
            hediffDef = def;
        }

        public override void CompTick()
        {
            base.CompTick();
            // tick for imbues if needed?
        }

        public override void ActivateImbueStatic()
        {
            // add hediffs/any other functionality that lasts 'forever' and doesnt change
            hediff = HediffMaker.MakeHediff(hediffDef, parentPawn, null);
            parentPawn.health.AddHediff(hediff);
            
        }

        public override void DeactivateImbues()
        {
            base.DeactivateImbues();
            parentPawn.health.RemoveHediff(hediff);        
        }

        protected override void UpdateCompClass(CompProperties props)
        {
            base.UpdateCompClass(props);
        }
    }
}
