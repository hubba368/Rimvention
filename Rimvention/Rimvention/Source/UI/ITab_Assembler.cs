using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimvention
{
    public class ITab_Assembler : ITab_Bills
    {
        private float _viewHeight = 1000f;
        private Vector2 _scrollPosition = default(Vector2);
        private Bill _mouseoverBill;
        private static readonly Vector2 _winSize = new Vector2(420f, 480f);
        protected Building_Assembler SelectedAssembler => (Building_Assembler)base.SelThing;

        public ITab_Assembler()
        {
            size = _winSize;
            labelKey = "TabBills";
            tutorTag = "Assembler";
        }

        protected override void FillTab()
        {
            Vector2 windowSize = _winSize;
            Rect rect1 = new Rect(0f, 0f, windowSize.x, windowSize.y).ContractedBy(10f);

            //create drop down
            Func<List<FloatMenuOption>> asmOptionsMaker = delegate
            {
                List<FloatMenuOption> dropList = new List<FloatMenuOption>();
                var beltRecipe = SelTable.def.AllRecipes.Where(x => x.defName == "MakeAugmentBelt").ElementAt(0);
                var shellRecipe = SelTable.def.AllRecipes.Where(x => x.defName == "MakeAugmentShell").ElementAt(0);
                // MAYBE - make it so this bill is able to be increased, however will need patches?  to handle according to part counts, i.e. cant repeat if not enough parts to do so

                dropList.Add(new FloatMenuOption("Assemble New Augment", delegate
                {
                    if (SelectedAssembler.CurrentStoredParts.NullOrEmpty())
                    {
                        Messages.Message("No Stored Parts in Assembler.", MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        Find.WindowStack.Add(new Dialog_GizmoConfig(SelectedAssembler));
                    }                  
                }));

                dropList.Add(new FloatMenuOption("Assemble Anima Augment Shell", delegate
                {
                    Bill newBill = shellRecipe.MakeNewBill();
                    SelectedAssembler.stack.AddBill(newBill);
                    SelectedAssembler.ImbueBillStack.Add(new ImbueBillStackInstance());
                }));
                dropList.Add(new FloatMenuOption("Assemble Augment Belt", delegate
                {
                    if(beltRecipe.AvailableNow && beltRecipe.AvailableOnNow(SelTable))
                    {
                        Bill newBill = beltRecipe.MakeNewBill();
                        SelectedAssembler.stack.AddBill(newBill);
                        SelectedAssembler.ImbueBillStack.Add(new ImbueBillStackInstance());
                    }
                }));
                
                if (!dropList.Any())
                {
                    dropList.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                }
                return dropList;
            };

            _mouseoverBill = SelectedAssembler.stack.DoListing(rect1, asmOptionsMaker, ref _scrollPosition, ref _viewHeight);
        }
    }
}
