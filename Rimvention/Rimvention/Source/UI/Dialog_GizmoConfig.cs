using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;


namespace Rimvention
{
    public class RimventionImbueInfo
    {
        public RimventionImbueInfo(AllAugmentImbues id, string name, string def, string className, string hediff, string desc)
        {
            ImbueID = id;
            ImbueName = name;
            ImbueDefName = def;
            ImbueClassName = className;
            HediffDefName = hediff;
            ImbueDescription = desc;
        }

        public AllAugmentImbues ImbueID { get; }
        public string ImbueDefName { get; }
        public string ImbueClassName { get; }
        public string HediffDefName { get; }
        public string ImbueDescription { get; }
        public string ImbueName { get; }
    }

    public class RimventionUIElement
    {
        public Texture2D UIIcon { get; }
        public string PartName { get; }
        public string UIText { get; }

        public int PartCount { get; set; }

        public RimventionImbueInfo ImbueInfo { get; set; }

        public RimventionUIElement(Texture2D icon, string def, string text, int count)
        {
            UIIcon = icon;
            PartName = def;
            UIText = text;
            PartCount = count;
            
        }

        public RimventionUIElement(Texture2D icon, RimventionImbueInfo imbueInfo)
        {
            UIIcon = icon;
            ImbueInfo = imbueInfo;
            UIText = ImbueInfo.ImbueDescription;
        }
    }

    public class Dialog_GizmoConfig : Window
    {
        private Building_Assembler _selectedTable;

        private WindowDrawingUtility DrawingUtil = new WindowDrawingUtility();

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(760f, 760f);
            }
        }

        public Dialog_GizmoConfig(Building_Assembler assembler)
        {
            _selectedTable = assembler;
            var partsUIElems = Current.Game.GetComponent<GameComponent_Rimvention>().RimventionUITextures[0];
            var craftingUIElems = Current.Game.GetComponent<GameComponent_Rimvention>().RimventionUITextures[1];

            // set UI elems with relevant info
            var partsList = _selectedTable.CurrentStoredParts;
            var temp = partsUIElems.Where(x => partsList.Keys.Contains(x.PartName)).ToList();

            for(int i = 0; i < temp.Count; i++)
            {
                var newElem = new RimventionUIElement(temp[i].UIIcon, temp[i].PartName, temp[i].UIText, partsList[temp[i].PartName]);
                temp[i] = newElem;
            }

            // part storage
            DrawingUtil.InitImageTable(temp, 0, 6, new Vector2(20f, 20f), true, true);
            // assembler crafting area table
            DrawingUtil.InitImageTable(craftingUIElems, 1, 3, new Vector2(20f, 5f), false, false);

            Log.Error("Stored Parts True Count:\n");
            Log.Error("Count of Unique Parts: " + _selectedTable.CurrentStoredParts.Count.ToString());

            foreach(var t in _selectedTable.CurrentStoredParts)
            {
                Log.Error("PartName: " + t.Key + ", Count: " + t.Value.ToString());
            }
        }


        public override void DoWindowContents(Rect inRect)
        {
            bool isReadyToCraft = false;

            //title
            Rect rect1 = new Rect(inRect.center.x - 80f, inRect.yMin + 5f, 200f, 74f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Augment Assembly");

            // Exit button
            Rect exitRect = new Rect(inRect.xMax - 50f, inRect.yMin + 5f, 50f, 30f);
            if (Widgets.ButtonText(exitRect, "X"))
            {
                this.Close();
            }

            // explain text
            Rect rect2 = new Rect(inRect);
            rect2.yMin = rect1.yMax;
            rect2.yMax -= 38f;
            Text.Font = GameFont.Small;
            Widgets.Label(rect2, "You can assemble Charged Anima Augments to provide unique effects to your colonists. Try out different combinations to see what you can discover.");

            // 'stored parts' list
            Rect partsRect = new Rect(rect2);
            partsRect.width = 550f;//275f;
            partsRect.height /= 2;
            partsRect.y += 70f;
            partsRect.x += 50f;

            DrawingUtil.DrawImageList(partsRect, 0, "Current Stored Parts", true, true);

            Rect craftingRect = new Rect(partsRect);
            craftingRect.height /= 2;
            craftingRect.x += 330f;
            //craftingRect.y = inRect.yMax / 2;

            DrawingUtil.DrawImageList(craftingRect, 1, "Augment Shell", false, true, true);

            DrawingUtil.DrawDragImageArea(craftingRect, 1);

            // 'possible results' text list
            var resultList = new List<string>();
            var tempPartList = new List<string>();
            var craftingArea = DrawingUtil.GetCachedEntriesByID(1);

            if(craftingArea != null)
            {
                for (int i = 0; i < craftingArea.Count; i++)
                {
                    // get imbue enum name (Imbue ID)
                    var perks = RimventionProbabilityUtility.GetPerksByMaterial(craftingArea[i].EntryUIInfo.PartName);

                    if (perks != null)
                    {
                        resultList.AddRange(perks);
                    }
                    if(craftingArea[i].EntryUIInfo.PartName != "")
                    {
                        tempPartList.Add(craftingArea[i].EntryUIInfo.PartName);
                    }
                }

                Rect resultRect = new Rect(partsRect);
                resultRect.height /= 2;
                resultRect.x += 330f;
                resultRect.y = inRect.yMax / 2;

                DrawingUtil.DrawDynamicTextList(resultRect, 2, resultList, "Possible Imbues");
            }

            if (!resultList.NullOrEmpty())
                isReadyToCraft = true;

            // confirm button
            Rect rect6 = new Rect(inRect.center.x - 95f, inRect.yMax - 35f, 150f, 29f);
            if (Widgets.ButtonText(rect6, "Confirm"))
            {
                if (isReadyToCraft == true)
                {
                    var parts = DrawingUtil.GetCachedEntriesByID(0);
                    for(int i = 0; i < parts.Count; i++)
                    { // update part counts
                        _selectedTable.UpdateStoredPartCountsFromCraft(parts[i].EntryUIInfo.PartName, parts[i].EntryUIInfo.PartCount);
                    }

                    // send off recipe
                    var recipe = Current.Game.GetComponent<GameComponent_Rimvention>().RequestRecipeDefByName("AssembleAugment");
                    Bill newBill = recipe.MakeNewBill();
                    _selectedTable.stack.AddBill(newBill);

                    var billIndex = _selectedTable.stack.IndexOf(newBill);
                    _selectedTable.AddImbueBillToImbueStack(resultList, tempPartList);
                    isReadyToCraft = false;
                    this.Close();
                }
                else
                {
                    Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("No Craftable Imbue(s) Selected", delegate
                    {
                    }, destructive: true);
                    Find.WindowStack.Add(window);
                }
            }
        }
    }
}
