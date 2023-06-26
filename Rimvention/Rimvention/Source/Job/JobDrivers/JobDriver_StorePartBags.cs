using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace Rimvention
{
    public class JobDriver_StorePartBags : JobDriver
    {
        private Thing _heldThing = null;
        private Building _assembler => (Building)base.TargetThingA;

        public const TargetIndex AssemblerIndex = TargetIndex.A;
        public const TargetIndex BagIndex = TargetIndex.B;
        public const TargetIndex HeldBagIndex = TargetIndex.C;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.job.SetTarget(TargetIndex.A, _assembler);
            this.job.count = 1;
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(AssemblerIndex);

            // set up task
            Building_Assembler assembler = this.job.GetTarget(TargetIndex.A).Thing as Building_Assembler;
            var thingList = RimventionPatches.GetAllOfThingOnMap("PartsBag");

            if(thingList == null || thingList.Count == 0)
            {
                yield break;
            }

            var curThing = thingList[thingList.Count - 1] as Thing_PartsBag;

            if (thingList.Count > 0)
            {// this appears to be only way to enqueue new custom jobs without using work/jobgiver
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] == curThing)
                    {
                        break;
                    }
                    var newJob = JobMaker.MakeJob(StorePartBagJobDefOf.AssemblerStorePartsBag, assembler);
                    newJob.count = 1;
                    pawn.jobs.jobQueue.EnqueueFirst(newJob);
                }
            }
            // found thing target, now go to target, pick up, take to destination
            this.job.SetTarget(BagIndex, curThing);

            yield return Toils_Goto.GotoThing(BagIndex, PathEndMode.Touch);
            this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;

            yield return Toils_Haul.StartCarryThing(BagIndex, false, false);

            var GetHeldThing = new Toil();
            GetHeldThing.initAction = delegate
            {
                this.job.SetTarget(HeldBagIndex, this.pawn.carryTracker.CarriedThing);
                _heldThing = this.job.GetTarget(HeldBagIndex).Thing;
            };
            yield return GetHeldThing;

            yield return Toils_Goto.GotoThing(AssemblerIndex, PathEndMode.Touch);
            yield return Toils_Haul.PlaceHauledThingInCell(AssemblerIndex, Toils_Goto.GotoThing(AssemblerIndex, PathEndMode.Touch), false);

            // perform work at destination
            var StoreParts = new Toil();
            StoreParts.initAction = delegate
            {
                assembler.StorePartsFromPartsBag(_heldThing);
                _heldThing.Destroy();
            };

            yield return StoreParts;
        }
    }
}
