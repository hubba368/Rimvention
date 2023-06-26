using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Rimvention
{
    public class Dialog_ViewWornAugments : Window
    {
        private Thing_AugmentBelt augmentBelt;
        private Pawn beltWearer;

        private WindowDrawingUtility DrawingUtil = new WindowDrawingUtility();

        public static int CurrentAugmentSlotToSwapIndex { get; private set; }

        public Dialog_ViewWornAugments(Apparel belt)
        {
            augmentBelt = (Thing_AugmentBelt)belt;
            beltWearer = augmentBelt.Wearer;
            var emptySlot = Current.Game.GetComponent<GameComponent_Rimvention>().RimventionImbueUITextures[0];

            // setup initial UI
            if (augmentBelt.IsEmpty)
            {
                DrawingUtil.InitSingleImage(emptySlot, 0, new Vector2(30f, 50f), false);
                DrawingUtil.InitSingleImage(emptySlot, 1, new Vector2(30f, 50f), false);
                DrawingUtil.InitSingleImage(emptySlot, 2, new Vector2(30f, 50f), false);
            }

            if (augmentBelt.CurrentAugmentComps.ContainsKey(0) && augmentBelt.CurrentAugmentComps[0] != null)
            {
                var slot1 = Current.Game.GetComponent<GameComponent_Rimvention>().RimventionImbueUITextures[augmentBelt.CurrentAugmentComps[0].Item1];
                DrawingUtil.InitSingleImage(slot1, 0, new Vector2(30f, 50f), false);
            }
            if (augmentBelt.CurrentAugmentComps.ContainsKey(1) && augmentBelt.CurrentAugmentComps[1] != null)
            {
                var slot2 = Current.Game.GetComponent<GameComponent_Rimvention>().RimventionImbueUITextures[augmentBelt.CurrentAugmentComps[1].Item1];
                DrawingUtil.InitSingleImage(slot2, 1, new Vector2(30f, 50f), false);
            }
            if (augmentBelt.CurrentAugmentComps.ContainsKey(2) && augmentBelt.CurrentAugmentComps[2] != null)
            {
                var slot3 = Current.Game.GetComponent<GameComponent_Rimvention>().RimventionImbueUITextures[augmentBelt.CurrentAugmentComps[2].Item1];
                DrawingUtil.InitSingleImage(slot3, 2, new Vector2(30f, 50f), false);
            }         
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(600f, 380f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            //title
            Rect rect1 = new Rect(inRect.center.x - 80f, inRect.yMin + 5f, 200f, 74f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Check Augments");

            // Exit button
            Rect exitRect = new Rect(inRect.xMax - 50f, inRect.yMin + 5f, 50f, 30f);
            if (Widgets.ButtonText(exitRect, "X"))
            {
                this.Close();
            }

            Rect augmentLeft = new Rect(inRect.xMin + 50f, inRect.yMax - 200f, 200f, 100f);
            DrawingUtil.DrawSingleImage(augmentLeft, 0, "Slot 1");

            Rect augmentMiddle = new Rect(inRect.xMin + 200f, inRect.yMax - 200f, 200f, 100f);
            DrawingUtil.DrawSingleImage(augmentMiddle, 1, "Slot 2");

            Rect augmentRight = new Rect(inRect.xMax - 150f, inRect.yMax - 200f, 200f, 100f);
            DrawingUtil.DrawSingleImage(augmentRight, 2, "Slot 3");

            Rect leftButton = new Rect(inRect.xMin + 50f, inRect.yMax - 50f, 100f, 30f);
            Rect middleButton = new Rect(inRect.xMin + 200f, inRect.yMax - 50f, 100f, 30f);
            Rect rightButton = new Rect(inRect.xMax - 150f, inRect.yMax - 50f, 100f, 30f);

            // TODO - make updateImage function for windowDrawingUtil - saves user having to open and reload the UI
            // OR just make a "updating" ui to show it is being swapped out

            if (Widgets.ButtonText(leftButton, "Swap Augment"))
            {
                if(DrawingUtil.GetCachedEntriesByID(0) != null)
                {
                    CurrentAugmentSlotToSwapIndex = 0;
                    MakeAugmentSwapFloatMenu();
                }            
            }
            if (Widgets.ButtonText(middleButton, "Swap Augment"))
            {
                if (DrawingUtil.GetCachedEntriesByID(1) != null)
                {
                    CurrentAugmentSlotToSwapIndex = 1;
                    MakeAugmentSwapFloatMenu();
                }
            }
            if (Widgets.ButtonText(rightButton, "Swap Augment"))
            {
                if (DrawingUtil.GetCachedEntriesByID(2) != null)
                {
                    CurrentAugmentSlotToSwapIndex = 2;
                    MakeAugmentSwapFloatMenu();
                }
            }
        }

        private void MakeAugmentSwapFloatMenu()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            var thingList = RimventionPatches.GetAllOfThingOnMap("ChargedAnimaAugment");

            if(!thingList.NullOrEmpty())
            {
                foreach(var temp in thingList)
                {
                    var aug = (Thing_AnimaAugment)temp;
                    var augInfo = aug.StoredImbues.ElementAt(0).Value.Item1;

                    list.Add(new FloatMenuOption("Swap with: " + augInfo.ImbueName, delegate
                    {
                        // jobdriv
                        beltWearer.jobs.TryTakeOrderedJob(JobMaker.MakeJob(AddNewAugmentToBeltDefOf.AddNewAugmentToBelt, aug, augmentBelt), JobTag.Misc);
                    }));
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
        }
    }
}
