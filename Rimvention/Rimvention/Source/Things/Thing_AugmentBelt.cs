using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Rimvention
{
    public class Thing_AugmentBelt : Apparel
    {
        // int key for each "slot" e.g 1st, 2nd
        private Dictionary<int, Tuple<AllAugmentImbues,BaseImbueThingComp>> currentAugmentComps = new Dictionary<int, Tuple<AllAugmentImbues, BaseImbueThingComp>>();
        private const int MaxAugments = 3;
        public bool IsEmpty { get; private set; }

        internal Dictionary<int, Tuple<AllAugmentImbues, BaseImbueThingComp>> CurrentAugmentComps { get => currentAugmentComps; }

        public override void PostMake()
        {
            base.PostMake();
            currentAugmentComps.Add(0, null);
            currentAugmentComps.Add(1, null);
            currentAugmentComps.Add(2, null);
            IsEmpty = true;
        }

        private int GetAvailableAugmentSlot()
        {
            int index = -1;
            foreach(var slot in currentAugmentComps)
            {
                if(slot.Value == null)
                {
                    index = slot.Key;
                    return index;
                }
                else
                {
                    Log.Error("Slot " + slot.Key + " is taken");
                }
            }

            return index;
        }

        public void AddNewAugment(Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>> imbue, int specificSlot = -1)
        {
            if (imbue.NullOrEmpty())
            {
                Log.Error("Augment Belt new augment input is null or empty.");
                return;
            }

            var augSlot = GetAvailableAugmentSlot();

            if (augSlot == -1)
            {
                Log.Error("No Free Slots");
                IsEmpty = false;
            }
            else
            {               
                var newImbue = imbue.ElementAt(0).Value.Item1;
                int imbueRank = imbue.ElementAt(0).Value.Item2;
                var hediffDef = Current.Game.GetComponent<GameComponent_Rimvention>().RequestHediffDefByName(newImbue.HediffDefName);
                var imbueCompClass = Type.GetType("Rimvention." + newImbue.ImbueClassName);

                Log.Error("incoming imbue");
                Log.Error(newImbue.ImbueName);
                Log.Error(newImbue.ImbueID.ToString());
                Log.Error(imbueRank.ToString());

                if (specificSlot != -1)
                    augSlot = specificSlot;

                var comp = new BaseImbueCompProperties();
                comp.ID = augSlot;
                comp.compClass = imbueCompClass;
                this.def.comps.Add(comp);
                this.InitializeComps();

                // need to access specific comp instance of this current iteration
                // cant use something like TryGetComp, because multiple BaseImbueThingComps will attempt to return multiple
                BaseImbueThingComp newComp = GetImbueCompByBeltID(augSlot);

                if (newComp != null)
                {
                    if (!newComp.isActive)
                    {
                        currentAugmentComps[augSlot] = new Tuple<AllAugmentImbues,BaseImbueThingComp>(newImbue.ImbueID, newComp);
                        newComp.InitImbue(Wearer);

                        IsEmpty = false;

                        if (hediffDef != null)
                            newComp.InitHediff(hediffDef);

                        newComp.ActivateImbueStatic();

                        foreach(var t in currentAugmentComps)
                        {
                            if (t.Value != null)
                                Log.Error(t.Value.Item1.ToString());
                        }
                    }
                    else
                    {
                        Log.Error("Could not get correct comp, or somehow got one that already exists.");
                    }
                }
                else
                {
                    Log.Error("Could not get ImbueComp of type: " + newImbue.ImbueName);
                }
            }
        }

        public void RemoveAugment(int augSlot)
        {
            if (currentAugmentComps.NullOrEmpty())
            {
                Log.Error("Tried to remove augment from empty belt.");
            }

            if(currentAugmentComps[augSlot] == null)
            {
                Log.Error("Tried to remove augment that has already been removed.");
            }
            else
            {
                currentAugmentComps[augSlot].Item2.DeactivateImbues();
                currentAugmentComps.Remove(augSlot);
            }

        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            var wornGizmos = base.GetWornGizmos();

            yield return new Command_Action
            {// TODO: change icons
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedNeed"),
                defaultLabel = "View Worn Augments",
                defaultDesc = "Open the Worn Augments Window",
                action = delegate ()
                {
                    //open dialog window to see augments
                    Find.WindowStack.Add(new Dialog_ViewWornAugments(this));
                }
            };
        }

        private BaseImbueThingComp GetImbueCompByBeltID(int id)
        {
            BaseImbueThingComp newComp = null;
            foreach (var c in this.AllComps)
            {
                if (c is BaseImbueThingComp)
                {
                    var temp = (BaseImbueCompProperties)c.props;
                    if (temp.ID == id)
                    {
                        newComp = (BaseImbueThingComp)c;
                        return newComp;
                    }
                }
            }
            return newComp;
        }

        private List<BaseImbueThingComp> GetAllImbueComps()
        {
            List<BaseImbueThingComp> imbues = null;
            foreach (var c in this.AllComps)
            {
                if (c is BaseImbueThingComp)
                {
                    imbues.Add((BaseImbueThingComp)c);
                }
            }
            return imbues;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if(IsEmpty)
            {
                Log.Error("no augments equipped on equip");
            }
            else
            {
                foreach(var imbue in currentAugmentComps)
                {
                    if(imbue.Value != null)
                    {
                        imbue.Value.Item2.ActivateImbueStatic();
                    }
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (IsEmpty)
            {
                Log.Error("no augments equipped on unequip");
            }
            else
            {
                foreach (var imbue in currentAugmentComps)
                {
                    if (imbue.Value != null)
                    {
                        imbue.Value.Item2.DeactivateImbues();
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            var keys = currentAugmentComps.Keys.ToList();
            var values = currentAugmentComps.Values.ToList();
            Scribe_Collections.Look(ref currentAugmentComps, "currentAugmentComps", LookMode.Value, LookMode.Deep, ref keys, ref values);
        }
    }
}
