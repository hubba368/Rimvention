using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Rimvention
{
    public enum AllMaterialParts
    {   
        // defaults
        BasePart,
        SimplePart,
        // melee - blades
        BladedPart,
        SharpPart,
        //unc
        TemperedPart,

        //melee - blunt
        HeadPart,
        SmoothPart,
        //unc
        WeightedPart,

        //ranged
        TensilePart,
        LongPart,
        //unc
        AccuratePart,

        // clothes
        SewnPart,
        FlexiblePart,
        //unc
        CraftedPart,

        // armour                         
        PlatedPart,
        PaddedPart,
        //unc
        ProtectivePart,

        // Food
        OrganicPart,
        //unc
        RestorativePart,

        // Medicinal
        HealthyPart,
        //unc
        EnhancingPart,

        // stuff 
        MetallicPart,
        StonyPart,
        WoodyPart,
        FabricPart,
        LeatheryPart,

        //Items - wide variety of items in this category
        PreciousPart,

        // rare parts - drop based on techlevel,quality (dependent on item type) and what its made from
        SpacerPart,
        UltraPart,
        ArchotechPart,
        // quality
        WealthyPart,
        ResilientPart,
        FortunatePart,
        // material specific parts
        FungalPart, //devilstrand
        SyntheticPart,
        WeavePart,
        PlastalloyPart,
        NoblePart,
        // misc parts
        WoolyPart,
        
    }

    public enum AllAugmentImbues
    {
        Empty,
        // Capacities
        Encumbered, Energised,  //Moving
        Straining, Relieving,   //Pain
        Unfocused, Focused,     // Conc
        Clumsy, Graceful,       // Manip
        Muting, Vocalizing,     // Talking
        Starving, Gluttonous,   // eating + metab
        Blinding, EagleEyed,    //sight
        Deafening, Perceptive,  //hearing
        Gasping, Flowing,        // breathing
        Draining, Elevating,    // blood pump+filt

        // Mood - generic
        Depressing, Encouraging,
        Talking, Berating, // mood static
        
        // uniques
        Crackling,
        Flamebreath,
        Dryad,
        ChippedShoulder,
        EncouragingWords
    }

    public class RimventionDisassembleStats
    {
       /* bool _isApparel;
        bool _isMeleeWeapon;
        bool _isRangedWeapon;
        bool _isMetal;
        bool _isDrug;
        bool _isStuff;*/

        float _marketValue;

        TechLevel _techLevel;

        QualityCategory _quality;

        List<ThingCategoryDef> _thingCategories;
        List<StuffCategoryDef> _stuffCategories;

        /*public bool IsApparel { get => _isApparel; }
        public bool IsMeleeWeapon { get => _isMeleeWeapon; }
        public bool IsRangedWeapon { get => _isRangedWeapon; }
        public bool IsMetal { get => _isMetal; }
        public bool IsDrug { get => _isDrug; }
        public bool IsStuff { get => _isStuff; }*/
        public float MarketValue { get => _marketValue; }
        public TechLevel TechLevel { get => _techLevel; }
        public QualityCategory Quality { get => _quality; }
        public List<ThingCategoryDef> ThingCategories { get => _thingCategories; }
        public List<StuffCategoryDef> StuffCategories { get => _stuffCategories; }

        public RimventionDisassembleStats(Thing thing)
        {
            ThingDef thingDef = thing.def;
            if(thing != null)
            {
                /*_isApparel = thingDef.IsApparel;
                _isRangedWeapon = thingDef.IsRangedWeapon;
                _isMeleeWeapon = thingDef.IsMeleeWeapon;
                _isMetal = thingDef.IsMetal;
                _isDrug = thingDef.IsDrug;
                _isStuff = thingDef.IsStuff;*/
                
                _marketValue = thing.MarketValue;
                _techLevel = thingDef.techLevel;

                QualityCategory temp;
                thing.TryGetQuality(out temp);
                _quality = temp;
                _thingCategories = thingDef.thingCategories;
                _stuffCategories = thingDef.stuffCategories;
            }
        }
    }


    [StaticConstructorOnStartup]
    static class RimventionPatches
    {
        static RimventionPatches()
        {
            var harmony = new Harmony("rimworld.mods.rimvention");

            // disassembly
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_DoBill), "TryStartNewDoBillJob"), new HarmonyMethod(typeof(RimventionPatches).GetMethod("GetDisassembleIngredient")));
            harmony.Patch(AccessTools.Method(typeof(GenRecipe), "MakeRecipeProducts"), new HarmonyMethod(typeof(RimventionPatches).GetMethod("GeneratePartOnDisassemble")));

            // augment crafting
           // harmony.Patch(AccessTools.Method(typeof(GenRecipe), "PostProcessProduct"), new HarmonyMethod(typeof(RimventionPatches).GetMethod("GenerateImbuesOnCraft")));
            harmony.Patch(AccessTools.Method(typeof(GenRecipe), "MakeRecipeProducts"), new HarmonyMethod(typeof(RimventionPatches).GetMethod("FinaliseImbuesOnAugmentCraft")));

            // assembler/disassembler UI
            harmony.Patch(AccessTools.Method(typeof(Bill), "DoInterface"), new HarmonyMethod(typeof(RimventionPatches).GetMethod("EditAssemblerBillInterface")));
            harmony.Patch(AccessTools.Method(typeof(Bill), "DoInterface"), new HarmonyMethod(typeof(RimventionPatches).GetMethod("EditDisassemblerBillInterface")));
        }

        public static void GetDisassembleIngredient(ref IBillGiver giver, ref List<ThingCount> chosenIngThings, Bill bill)
        {
            if (giver != null)
            {
                if (giver.LabelShort == "disassembler")
                {
                    if (chosenIngThings != null)
                    {
                        for (int i = 0; i < chosenIngThings.Count; i++)
                        {
                            var disassembler = (Building_Disassembler)giver;
                            disassembler.AddBillToDisStackByID(bill.billStack.IndexOf(bill), GenerateDisassembleStatsForIng(chosenIngThings[i].Thing));
                            // THis should technically work considering stats are generated when bill is started
                            // however MAYBE will need to think of ways for 'small' dismantling e.g organic items - possibly stacks?
                            // would have to make a new dismantle recipedef tho                           
                        }
                    }                    
                }
            }
        }

        public static bool GeneratePartOnDisassemble(ref IEnumerable<Thing> __result, RecipeDef recipeDef, IBillGiver billGiver)
        {
            if(recipeDef.defName == "DismantleObject")
            {
                // make the replacement Thing - wouldnt need to do this if patching over PostProcessProduct, but doesnt work very well for this mod functionality
                var thingDefCountClass = recipeDef.products[0];
                var product = ThingMaker.MakeThing(thingDefCountClass.thingDef);
                product.stackCount = 1;

                var disassembler = (Building_Disassembler)billGiver;

                if (product.def.defName == "PartsBag")
                {
                    var comp = product.TryGetComp<PartsBagThingComp>();
                    var thing = (Thing_PartsBag)product;
                    if(comp != null)
                    {
                        var stats = disassembler.DisassembleBillStack[0];
                        // call prob func
                        var parts = RimventionProbabilityUtility.GenerateMaterials(stats);
                        comp.AddPartToStore(parts);
                        thing.InitPartStore();

                        var final = new List<Thing>();
                        final.Add(product);
                        IEnumerable<Thing> final2 = final;
                        __result = final2;

                        return false;
                    }
                    else
                    {
                        Log.Error("Could not get PartsBagThingComp.");
                    }                   
                }
                else
                {
                    Log.Error("Could not get PartsBag Thing from Bill Product list.");
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private static int GetCurrentWorkTableUsedByPawn(IBillGiver billGiver)
        {
            switch (billGiver.LabelShort)
            {
                case "assembler":
                    var assembler = (Building_Assembler)billGiver;
                    var tempAssemblerID = assembler.thingIDNumber;
                    //Log.Error("Got assembler of ID: " + tempAssemblerID);
                    return tempAssemblerID;

                case "disassembler":
                    var disassembler = (Building_Disassembler)billGiver;
                    var tempDisID = disassembler.thingIDNumber;
                    Log.Error("Got assembler of ID: " + tempDisID);
                    return tempDisID;

                default:
                    Log.Error("Could not Get WorkTable ID num from BillGiver.");
                    return -1;

            }
        }

        public static bool FinaliseImbuesOnAugmentCraft(ref IEnumerable<Thing> __result, RecipeDef recipeDef, IBillGiver billGiver)
        {
            if (recipeDef.defName == "AssembleAugment")
            {
                var thingDefCountClass = recipeDef.products[0];
                var product = ThingMaker.MakeThing(thingDefCountClass.thingDef); // __result
                product.stackCount = 1;
                if (product.def.defName == "ChargedAnimaAugment")
                {
                    var comp = product.TryGetComp<AnimaAugmentThingComp>();
                    var thing = (Thing_AnimaAugment)product;
                    if (comp != null)
                    {
                        var assemblerID = GetCurrentWorkTableUsedByPawn(billGiver);
                        var temp = Current.Game.GetComponent<GameComponent_Rimvention>().GetCurrentImbueBillAtAssemblerByID(assemblerID);

                        if (temp != null)
                        {
                            // set up imbue details to be held in ChargedAugmentComps
                            var parts = RimventionProbabilityUtility.GenerateImbuesOnCraft(temp);
                            comp.AddToStore(parts);
                            thing.InitPartStore();
                            // override return var of patched func
                            var final = new List<Thing>();
                            final.Add(product);
                            IEnumerable<Thing> final2 = final;
                            __result = final2;
                            return false;
                        }
                        else
                        {
                            Log.Error("Could not get Imbues list from assembler.");
                        }
                    }
                    else
                    {
                        Log.Error("Could not get AnimaAugmentThingComp.");
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        // Str8 ripped from RW source again - only needed to edit bill interface slightly
        #region Bill UI Editors
        public static bool EditDisassemblerBillInterface(float x, float y, float width, int index, ref Bill __instance, ref Rect __result)
        {
            if (__instance.billStack.billGiver.LabelShort == "disassembler")
            {
                Building_Disassembler disassembler = (Building_Disassembler)__instance.billStack.billGiver;
                var billProductionInstance = (Bill_Production)__instance;
                var billIndex = __instance.billStack.IndexOf(__instance);

                // setup lil info text
                var imbueBillInstance = disassembler.DisassembleBillStack[__instance.billStack.IndexOf(__instance)];
                string StatusString = "";

                if (billProductionInstance.paused)
                {
                    StatusString = " " + "Paused".Translate();
                }

                __result = new Rect(x, y, width, 53f);
                float num = 0f;
                if (!StatusString.NullOrEmpty())
                {
                    num = Mathf.Max(17f, 24f);
                }
                __result.height += num;
                Color color = Color.white;
                if (!__instance.ShouldDoNow())
                {
                    color = new Color(1f, 0.7f, 0.7f, 0.7f);
                }
                GUI.color = color;
                Text.Font = GameFont.Small;
                if (index % 2 == 0)
                {
                    Widgets.DrawAltRect(__result);
                }
                GUI.BeginGroup(__result);
                Rect rect2 = new Rect(0f, 0f, 24f, 24f);
                if (billIndex > 0)
                {
                    if (Widgets.ButtonImage(rect2, TexButton.ReorderUp, color))
                    {
                        __instance.billStack.Reorder(__instance, -1);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        disassembler.ReorderDisStack(billIndex + 1, -1);
                    }
                    TooltipHandler.TipRegionByKey(rect2, "ReorderBillUpTip");
                }
                if (billIndex < __instance.billStack.Count - 1)
                {
                    Rect rect3 = new Rect(0f, 24f, 24f, 24f);
                    if (Widgets.ButtonImage(rect3, TexButton.ReorderDown, color))
                    {
                        __instance.billStack.Reorder(__instance, 1);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        disassembler.ReorderDisStack(billIndex - 1, 1);
                    }
                    TooltipHandler.TipRegionByKey(rect3, "ReorderBillDownTip");
                }
                Widgets.Label(new Rect(28f, 0f, __result.width - 48f - 20f, __result.height + 5f), __instance.LabelCap);
                DoConfigInterface(__result.AtZero(), color, billProductionInstance);
                Rect rect4 = new Rect(__result.width - 24f, 0f, 24f, 24f);
                if (Widgets.ButtonImage(rect4, TexButton.DeleteX, color, color * GenUI.SubtleMouseoverColor))
                {
                    __instance.billStack.Delete(__instance);
                    __instance.deleted = true;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    disassembler.RemoveBillFromDisStack(billIndex);
                }
                TooltipHandler.TipRegionByKey(rect4, "DeleteBillTip");

                Rect rect5 = new Rect(rect4);
                rect5.x -= rect5.width + 4f;
                if (Widgets.ButtonImage(rect5, TexButton.Suspend, color))
                {
                    __instance.suspended = !__instance.suspended;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegionByKey(rect5, "SuspendBillTip");
                if (!StatusString.NullOrEmpty())
                {
                    Text.Font = GameFont.Tiny;
                    Rect rect7 = new Rect(24f, __result.height - num, __result.width - 24f, num);
                    Widgets.Label(rect7, StatusString);
                    __instance.DoStatusLineInterface(rect7);
                }
                GUI.EndGroup();

                if (__instance.suspended)
                {
                    Text.Font = GameFont.Medium;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Rect rect8 = new Rect(__result.x + __result.width / 2f - 70f, __result.y + __result.height / 2f - 20f, 140f, 40f);
                    GUI.DrawTexture(rect8, TexUI.GrayTextBG);
                    Widgets.Label(rect8, "SuspendedCaps".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                }
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                return false;
            }
            else
            {
                return true;
            }
        }

        // Again taken straight from rimworld source, want to have same functionality as vanilla, but og method is protected
        // In future, should just do a transpile of the bill interface func to stop this uncessary reuse
        private static void DoConfigInterface(Rect baseRect, Color baseColor, Bill_Production instance)
        {
            Rect rect = new Rect(28f, 32f, 100f, 30f);
            GUI.color = new Color(1f, 1f, 1f, 0.65f);
            Widgets.Label(rect, instance.RepeatInfoText);
            GUI.color = baseColor;
            WidgetRow widgetRow = new WidgetRow(baseRect.xMax, baseRect.y + 29f, UIDirection.LeftThenUp);
            if (widgetRow.ButtonText("Details".Translate() + "..."))
            {
                Find.WindowStack.Add(new Dialog_BillConfig(instance, ((Thing)instance.billStack.billGiver).Position));
            }
            if (widgetRow.ButtonText(instance.repeatMode.LabelCap.Resolve().PadRight(20)))
            {
                BillRepeatModeUtility.MakeConfigFloatMenu(instance);
            }
            if (widgetRow.ButtonIcon(TexButton.Plus))
            {
                if (instance.repeatMode == BillRepeatModeDefOf.Forever)
                {
                    instance.repeatMode = BillRepeatModeDefOf.RepeatCount;
                    instance.repeatCount = 1;
                }
                else if (instance.repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    int num = instance.recipe.targetCountAdjustment * GenUI.CurrentAdjustmentMultiplier();
                    instance.targetCount += num;
                    instance.unpauseWhenYouHave += num;
                }
                else if (instance.repeatMode == BillRepeatModeDefOf.RepeatCount)
                {
                    instance.repeatCount += GenUI.CurrentAdjustmentMultiplier();
                }
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                if (TutorSystem.TutorialMode && instance.repeatMode == BillRepeatModeDefOf.RepeatCount)
                {
                    TutorSystem.Notify_Event(instance.recipe.defName + "-RepeatCountSetTo-" + instance.repeatCount);
                }
            }
            if (widgetRow.ButtonIcon(TexButton.Minus))
            {
                if (instance.repeatMode == BillRepeatModeDefOf.Forever)
                {
                    instance.repeatMode = BillRepeatModeDefOf.RepeatCount;
                    instance.repeatCount = 1;
                }
                else if (instance.repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    int num2 = instance.recipe.targetCountAdjustment * GenUI.CurrentAdjustmentMultiplier();
                    instance.targetCount = Mathf.Max(0, instance.targetCount - num2);
                    instance.unpauseWhenYouHave = Mathf.Max(0, instance.unpauseWhenYouHave - num2);
                }
                else if (instance.repeatMode == BillRepeatModeDefOf.RepeatCount)
                {
                    instance.repeatCount = Mathf.Max(0, instance.repeatCount - GenUI.CurrentAdjustmentMultiplier());
                }
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                if (TutorSystem.TutorialMode && instance.repeatMode == BillRepeatModeDefOf.RepeatCount)
                {
                    TutorSystem.Notify_Event(instance.recipe.defName + "-RepeatCountSetTo-" + instance.repeatCount);
                }
            }
        }

        public static bool EditAssemblerBillInterface(float x, float y, float width, int index, ref Bill __instance, ref Rect __result)
        {
            if(__instance.billStack.billGiver.LabelShort == "assembler")
            {
                Building_Assembler assembler = (Building_Assembler)__instance.billStack.billGiver;
                var billProductionInstance = (Bill_Production)__instance;
                var billIndex = __instance.billStack.IndexOf(__instance);

                // setup lil info text
                var imbueBillInstance = assembler.ImbueBillStack[__instance.billStack.IndexOf(__instance)];
                string StatusString = "";
                if(imbueBillInstance.AugmentTempPartsInUse != null)
                {
                    StatusString = "Parts: "; 
                    for(int i = 0; i < imbueBillInstance.AugmentTempPartsInUse.Count; i++)
                    {
                        var imb = imbueBillInstance.AugmentTempPartsInUse[i];
                        if (imb != null)
                            StatusString += imb + "|";
                    }                       
                }
                
                __result = new Rect(x, y, width, 53f);
                float num = 0f;
                if (!StatusString.NullOrEmpty())
                {
                    num = Mathf.Max(17f, 24f);
                }
                __result.height += num;
                Color color = Color.white;
                if (!__instance.ShouldDoNow())
                {
                    color = new Color(1f, 0.7f, 0.7f, 0.7f);
                }
                GUI.color = color;
                Text.Font = GameFont.Small;
                if (index % 2 == 0)
                {
                    Widgets.DrawAltRect(__result);
                }
                GUI.BeginGroup(__result);
                Rect rect2 = new Rect(0f, 0f, 24f, 24f);
                if (billIndex > 0)
                {
                    if (Widgets.ButtonImage(rect2, TexButton.ReorderUp, color))
                    {
                        __instance.billStack.Reorder(__instance, -1);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        assembler.ReorderImbueStack(billIndex + 1, -1);
                    }
                    TooltipHandler.TipRegionByKey(rect2, "ReorderBillUpTip");
                }
                if (billIndex < __instance.billStack.Count - 1)
                {
                    Rect rect3 = new Rect(0f, 24f, 24f, 24f);
                    if (Widgets.ButtonImage(rect3, TexButton.ReorderDown, color))
                    {
                        __instance.billStack.Reorder(__instance, 1);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        assembler.ReorderImbueStack(billIndex - 1, 1);
                    }
                    TooltipHandler.TipRegionByKey(rect3, "ReorderBillDownTip");
                }
                Widgets.Label(new Rect(28f, 0f, __result.width - 48f - 20f, __result.height + 5f), __instance.LabelCap);
                //DoConfigInterface(rect.AtZero(), color);
                Rect rect4 = new Rect(__result.width - 24f, 0f, 24f, 24f);
                if (Widgets.ButtonImage(rect4, TexButton.DeleteX, color, color * GenUI.SubtleMouseoverColor))
                {
                    __instance.billStack.Delete(__instance);
                    __instance.deleted = true;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    
                    // re-add augment parts if the bill is deleted, and make sure they arent when a completed bill is deleted
                    if(billProductionInstance != null)
                    {
                        var targetCount = billProductionInstance.repeatCount;
                        var currentCount = billProductionInstance.recipe.WorkerCounter.CountProducts(billProductionInstance);
                        if(currentCount != targetCount)
                        {
                            assembler.RestoreUsedPartsFromAugmentCraft(billIndex);
                        }
                    }
                    assembler.RemoveImbueBillFromImbueStack(billIndex);
                }
                TooltipHandler.TipRegionByKey(rect4, "DeleteBillTip");

                Rect rect5 = new Rect(rect4);
                rect5.x -= rect5.width + 4f;
                if (Widgets.ButtonImage(rect5, TexButton.Suspend, color))
                {
                    __instance.suspended = !__instance.suspended;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegionByKey(rect5, "SuspendBillTip");
                if (!StatusString.NullOrEmpty())
                {
                    Text.Font = GameFont.Tiny;
                    Rect rect7 = new Rect(24f, __result.height - num, __result.width - 24f, num);
                    Widgets.Label(rect7, StatusString);
                    __instance.DoStatusLineInterface(rect7);
                }
                GUI.EndGroup();

                if (__instance.suspended)
                {
                    Text.Font = GameFont.Medium;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Rect rect8 = new Rect(__result.x + __result.width / 2f - 70f, __result.y + __result.height / 2f - 20f, 140f, 40f);
                    GUI.DrawTexture(rect8, TexUI.GrayTextBG);
                    Widgets.Label(rect8, "SuspendedCaps".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                }
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        private static RimventionDisassembleStats GenerateDisassembleStatsForIng(Thing thing)
        {
            // shouldnt really need to check if thing is what we want cause that is handled by the recipeDef - will leave for now
            var thingStats = new RimventionDisassembleStats(thing);
            //Log.Error("test" + thing.Stuff.label.ToString()); <-- how to get specific stuff thing is made from e.g. steel from a steel sword
            /*Log.Error("is apparel: " + thingStats.IsApparel.ToString());
            Log.Error("is ranged weapon: " + thingStats.IsRangedWeapon.ToString());
            Log.Error("is melee weapon: " + thingStats.IsMeleeWeapon.ToString());
            Log.Error("is metal: " + thingStats.IsMetal.ToString());
            Log.Error("is drug: " + thingStats.IsDrug.ToString());
            Log.Error("is meat: " + thingStats.IsFood.ToString());
            Log.Error("is leather: " + thingStats.IsLeather.ToString());
            Log.Error("is stuff: " + thingStats.IsStuff.ToString());
            Log.Error("market Value: " + thingStats.MarketValue.ToString());
            Log.Error("techlevel: " + thingStats.TechLevel.ToString());
            Log.Error("thing quality: " + thingStats.Quality.ToString());*/

            if(thingStats.ThingCategories == null)
            {
                Log.Error("thingCategories is null");
            }
            else
            {
                foreach (var t in thingStats.ThingCategories)
                {
                    if (t == null)
                        Log.Error("Thing Category null");
                    else
                        Log.Error("thing Category: " + t.ToString());
                }
            }

            if(thingStats.StuffCategories == null)
            {
                Log.Error("stuffCategories is null");
            }
            else
            {
                foreach (var t in thingStats.StuffCategories)
                {
                    if (t == null)
                        Log.Error("Stuff Category null");
                    else
                        Log.Error("stuff Category: " + t.ToString());
                }
            }             
            return thingStats;
        }

        public static Dictionary<string, List<int>> MergeDictionaries(Dictionary<string, List<int>> dictToMerge, Dictionary<string, List<int>> resultDict)
        {
            foreach (var t in dictToMerge)
            {
                if (resultDict.ContainsKey(t.Key))
                {
                    resultDict[t.Key].AddRange(t.Value);
                }
                else
                {
                    resultDict.Add(t.Key, t.Value);
                }
            }
            return resultDict;
        }

        public static Dictionary<string, int> MergeDictionaries(Dictionary<string, int> dictToMerge, Dictionary<string, int> resultDict)
        {
            foreach (var t in dictToMerge)
            {
                if (resultDict.ContainsKey(t.Key))
                {
                    resultDict[t.Key] += t.Value;
                }
                else
                {
                    resultDict.Add(t.Key, t.Value);
                }
            }
            return resultDict;
        }

        public static List<Thing> GetAllOfThingOnMap(string defName)
        {
            List<Thing> result = null;

            if (!(from t in DefDatabase<ThingDef>.AllDefs where t.defName == defName select t).TryRandomElement(out ThingDef finalDef))
            {
                Log.Error("Unable to locate " + defName + " in DefDatabase.");
            }
            else
            {
                var def = finalDef;
                var req = ThingRequest.ForDef(def);
                var thingList = new List<Thing>();
                thingList = Find.CurrentMap.listerThings.ThingsMatching(req);

                if (thingList.Count == 0)
                {
                    //Log.Error("No things found in colony.");
                    return null;
                }
                result = thingList;
                // Log.Error("Num of found things in colony: " + thingList.Count);
            }
            return result;
        }
    }
}
