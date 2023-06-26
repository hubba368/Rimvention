using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace Rimvention
{
    public class Building_Disassembler : Building_WorkTable
    {
        private List<RimventionDisassembleStats> disassembleBillStack;

        public List<RimventionDisassembleStats> DisassembleBillStack { get => disassembleBillStack; }

        public Building_Disassembler()
        {
            disassembleBillStack = new List<RimventionDisassembleStats>();
        }

        public void AddBillToDisStack(RimventionDisassembleStats stats)
        {
            if (stats != null)
            {
                disassembleBillStack.Add(stats);
            }
            else
            {
                Log.Error("Incoming DisassembleStats is null");
            }
        }

        public void AddBillToDisStackByID(int billIndex, RimventionDisassembleStats stats)
        {
            disassembleBillStack[billIndex] = stats;
        }

        public void RemoveBillFromDisStack(int billIndex)
        {
            Log.Error("incoming index" + billIndex.ToString());
            if (this.billStack.Count == 0)
            {
                disassembleBillStack.Clear();
            }
            else
            {
                disassembleBillStack.RemoveAt(billIndex);
            }
        }

        public void ReorderDisStack(int billIndex, int offset)
        {
            int num = billIndex;
            num += offset;
            if (num >= 0)
            {
                var swap = disassembleBillStack[billIndex];
                disassembleBillStack.RemoveAt(billIndex);
                disassembleBillStack.Insert(num, swap);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            var list = disassembleBillStack;
            Scribe_Collections.Look(ref list, "disassembleBillStack", LookMode.Deep);
        }
    }
}
