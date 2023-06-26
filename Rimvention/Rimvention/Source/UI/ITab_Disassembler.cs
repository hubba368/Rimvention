using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimvention
{
    public class ITab_Disassembler : ITab_Bills
    {
        private float _viewHeight = 1000f;
        private Vector2 _scrollPosition = default(Vector2);
        private Bill _mouseoverBill;
        private static readonly Vector2 _winSize = new Vector2(420f, 480f);
        protected Building_Disassembler SelDisassembler => (Building_Disassembler)base.SelThing;

        public ITab_Disassembler()
        {
            size = _winSize;
            labelKey = "TabBills";
            tutorTag = "Disassembler";
        }

        protected override void FillTab()
        {
            Vector2 windowSize = _winSize;
            Rect rect1 = new Rect(0f, 0f, windowSize.x, windowSize.y).ContractedBy(10f);

            //create drop down
            Func<List<FloatMenuOption>> optionsMaker = delegate
            {
                List<FloatMenuOption> dropList = new List<FloatMenuOption>();
                var recipe = SelTable.def.AllRecipes.Where(x => x.defName == "DismantleObject").ElementAt(0);

                // MAYBE - make it so this bill is able to be increased, however will need patches?  to handle according to part counts, i.e. cant repeat if not enough parts to do so

                dropList.Add(new FloatMenuOption("Dismantle Object Into Parts", delegate
                {
                    if (recipe.AvailableNow && recipe.AvailableOnNow(SelTable))
                    {
                        Bill newBill = recipe.MakeNewBill();
                        SelDisassembler.billStack.AddBill(newBill);
                        SelDisassembler.DisassembleBillStack.Add(null);
                    }
                }));
                if (!dropList.Any())
                {
                    dropList.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                }
                return dropList;
            };

            _mouseoverBill = SelDisassembler.billStack.DoListing(rect1, optionsMaker, ref _scrollPosition, ref _viewHeight);
        }
    }
}
