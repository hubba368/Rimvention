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
    public class JobDriver_AddNewAugment : JobDriver
    {
        private Thing _heldThing = null;
        private Thing _augment => base.TargetThingA;
        private Thing_AugmentBelt _wornAugmentBelt => (Thing_AugmentBelt)base.TargetThingB;

        public const TargetIndex AugmentIndex = TargetIndex.A;
        public const TargetIndex BeltIndex = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.job.SetTarget(TargetIndex.A, _augment);
            this.job.count = 1;
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(AugmentIndex);

            // set up task
            Thing_AnimaAugment augment = this.job.GetTarget(TargetIndex.A).Thing as Thing_AnimaAugment;

            // found thing target, now go to target, pick up, take to destination
            this.job.SetTarget(AugmentIndex, augment);

            yield return Toils_Goto.GotoThing(AugmentIndex, PathEndMode.Touch);
            this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;

            // perform work at destination - add charge anima augment to augment belt free slot
            var AddAugment = new Toil();
            AddAugment.initAction = delegate
            {
                var newImbue = new Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>>();
                newImbue.Add(augment.StoredImbues.ElementAt(0).Key, 
                    new Tuple<RimventionImbueInfo, int>(augment.StoredImbues.ElementAt(0).Value.Item1, augment.StoredImbues.ElementAt(0).Value.Item2));

                var slot = Dialog_ViewWornAugments.CurrentAugmentSlotToSwapIndex;
                _wornAugmentBelt.RemoveAugment(slot);
                _wornAugmentBelt.AddNewAugment(newImbue, slot);
                _augment.Destroy();
            };

            yield return AddAugment;
        }
    }
}
