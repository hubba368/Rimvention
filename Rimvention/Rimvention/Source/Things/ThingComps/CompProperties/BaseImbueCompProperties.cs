using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Rimvention
{
    class BaseImbueCompProperties : CompProperties
    {
        public int ID = 0;
        public BaseImbueCompProperties()
        {
            this.compClass = typeof(BaseImbueThingComp);
        }

        public BaseImbueCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
