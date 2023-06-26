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
    public class ImbueBillStackInstance
    {
        List<string> augmentBill;
        List<string> augmentTempPartsInUse;

        public List<string> AugmentBill { get => augmentBill; }
        public List<string> AugmentTempPartsInUse { get => augmentTempPartsInUse; }

        public ImbueBillStackInstance()
        {
            augmentBill = null;
        }

        public ImbueBillStackInstance(List<string> augmentList, List<string> partList)
        {
            augmentBill = augmentList;
            augmentTempPartsInUse = partList;
        }     
    }

    public class Building_Assembler : Building_WorkTable
    {
        private Dictionary<string, int> currentStoredParts;
        private List<ImbueBillStackInstance> imbueBillStack;
                                                   
        public BillStack stack;
        public Dictionary<string, int> CurrentStoredParts { get => currentStoredParts; }
        public List<ImbueBillStackInstance> ImbueBillStack { get => imbueBillStack; }

        public Building_Assembler()
        {
            stack = base.billStack;
            stack.billGiver = this;
            currentStoredParts = new Dictionary<string, int>();
            imbueBillStack = new List<ImbueBillStackInstance>();
        }// Maybe: want to add the store all parts jobdriver as a UI button when clicking on table (where dismantle button is) gizmo button <---

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            //base.GetFloatMenuOptions(selPawn);
            if (!selPawn.CanReach(this, PathEndMode.OnCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseReason".Translate("NoPath".Translate().CapitalizeFirst()), null);
            }
            else
            {
                if(RimventionPatches.GetAllOfThingOnMap("PartsBag") == null)
                {                  
                    yield return new FloatMenuOption("No Parts Bags To Store", delegate {
                        Messages.Message("No Parts Bags Located in Colony.", this, MessageTypeDefOf.RejectInput, historical: false);
                    });
                }
                else
                {
                    yield return new FloatMenuOption("Store All Nearby Parts", delegate
                    {
                        selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(StorePartBagJobDefOf.AssemblerStorePartsBag, this), JobTag.Misc);
                    });
                }
            }        
        }

        public void AddImbueBillToImbueStack(List<string> results = null, List<string> partList = null)
        {
            if(results != null)
            {
                imbueBillStack.Add(new ImbueBillStackInstance(results,partList));
            }
            else
            {
                imbueBillStack.Add(new ImbueBillStackInstance());
            }
        }

        public void RemoveImbueBillFromImbueStack(int billIndex)
        {
            Log.Error("incoming index" + billIndex.ToString());
            if (this.stack.Count == 0)
            {
                imbueBillStack.Clear();
            }
            else
            {
                imbueBillStack.RemoveAt(billIndex);
            }
        }

        public void ReorderImbueStack(int billIndex, int offset)
        {
            int num = billIndex;
            num += offset;
            if (num >= 0)
            {
                var swap = imbueBillStack[billIndex];
                imbueBillStack.RemoveAt(billIndex);
                imbueBillStack.Insert(num, swap);
            }
        }

        public void RestoreUsedPartsFromAugmentCraft(int billIndex)
        {
            ImbueBillStackInstance imbueBill = imbueBillStack[billIndex];
            if (imbueBill != null && !imbueBill.AugmentTempPartsInUse.NullOrEmpty())
            {
                for(int i = 0; i < imbueBill.AugmentTempPartsInUse.Count; i++)
                {
                    if (currentStoredParts.ContainsKey(imbueBill.AugmentTempPartsInUse[i]))
                    {
                        currentStoredParts[imbueBill.AugmentTempPartsInUse[i]]++;
                    }
                } 
            }
        }

        public void UpdateStoredPartCountsFromCraft(string partName, int newValue)
        {
            if (currentStoredParts.ContainsKey(partName))
            {
                currentStoredParts[partName] = newValue;
            }
            else
            {
                Log.Error("Could not find Key with name: " + partName + ".");
            }
        }

        public void StorePartsFromPartsBag(Thing bag)
        {
            var partsBag = bag as Rimvention.Thing_PartsBag;
            if(partsBag.StoredParts == null)
            {
                Log.Error("Most recent Parts Bag to Store has null StoredParts.");
            }
            else
            {
                currentStoredParts = RimventionPatches.MergeDictionaries(partsBag.StoredParts, currentStoredParts);
            }           
        }
        // TODO figure out why first part in dict has extra count added when this is called.
        private void OnDestroyWithStoredParts()
        {
            if (currentStoredParts.NullOrEmpty())
            {
                Log.Error("all parts removed.");
                return;
            }

            var partBagDef = Current.Game.GetComponent<GameComponent_Rimvention>().RequestThingDefByName("PartsBag");

            var product = GenSpawn.Spawn(partBagDef, this.Position, this.Map);
            var comp = product.TryGetComp<PartsBagThingComp>();
            var thing = (Thing_PartsBag)product;

            var temp = new List<string>();

            foreach (var part in currentStoredParts)
            {
                if (comp != null)
                {
                    if (!comp.isFull)
                    {
                        for (int i = 0; i < part.Value; i++)
                        {
                            comp.AddPartToStore(part.Key);
                        }                      
                        temp.Add(part.Key);
                    }
                    else
                    {
                        Log.Error("current Part Bag is full. Creating another.");
                        break;
                    }                
                }
            }
            foreach(var t in temp)
            {
                CurrentStoredParts.Remove(t);
            }
            thing.InitPartStore();
            OnDestroyWithStoredParts();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // spawn parts bag(s) with stored parts when destroyed
            if (!currentStoredParts.NullOrEmpty())
            {
                OnDestroyWithStoredParts();
            }

            base.Destroy(mode);
        }


        public override void TickRare()
        {
            base.TickRare();
        }

        public override void ExposeData()
        {
            //TODO store stored parts list
            base.ExposeData();
            var keys = currentStoredParts.Keys.ToList();
            var values = currentStoredParts.Values.ToList();
            Scribe_Collections.Look(ref currentStoredParts, "currentStoredParts", LookMode.Value, LookMode.Value, ref keys, ref values);

            var imbues = imbueBillStack;
            Scribe_Collections.Look(ref imbues, "imbueBillStack", LookMode.Deep);
        }
    }
}
