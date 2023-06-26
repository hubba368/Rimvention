using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Rimvention
{
    class AnimaAugmentCompProperties : CompProperties
    {
        public AnimaAugmentCompProperties()
        {
            this.compClass = typeof(AnimaAugmentThingComp);
        }

        public AnimaAugmentCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
