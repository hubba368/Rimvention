using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Rimvention
{
    class PartsBagCompProperties : CompProperties
    {
        public PartsBagCompProperties()
        {
            this.compClass = typeof(PartsBagThingComp);
        }

        public PartsBagCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
