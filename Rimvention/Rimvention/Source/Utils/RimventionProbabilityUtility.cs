using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace Rimvention
{
    public static class RimventionProbabilityUtility
    {
        // in runescape - mat chance is rolled for the number of parts that are given for a particular item, e.g. shortbow gives 12 materials
        // for each material in total number - roll against junk chance, if not junk give 1 relevant material based on what it can give

        // no need for junk parts, so we just need material totals - could base it on value
        // parts will drop based on the items stats
        static int materialTotal = 0;
        const float baseUncommonProb = 0.25f;
        const float basecommonProb = 1f;
        const float baseRareProb = 1f;
        static bool giveHighQualityRarePart = false;
        static bool giveTechLevelRarePart = false;
        
        public static Dictionary<string,int> GenerateMaterials(RimventionDisassembleStats stats)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            if(stats.MarketValue > 100f)
            {
                materialTotal = 3;
            }
            else if(stats.MarketValue < 100f && stats.MarketValue >= 500f)
            {
                materialTotal = 5;
            }
            else if(stats.MarketValue < 500f && stats.MarketValue >= 1000f)
            {
                materialTotal = 7;
            }
            else if(stats.MarketValue < 1000f)
            {
                materialTotal = 9;
            }

            if (stats.Quality == RimWorld.QualityCategory.Legendary
            || stats.Quality == RimWorld.QualityCategory.Masterwork)
            {
                giveHighQualityRarePart = true;
            }
            if(stats.TechLevel == TechLevel.Spacer || stats.TechLevel == TechLevel.Ultra || stats.TechLevel == TechLevel.Archotech)
            {
                giveTechLevelRarePart = true;
            }

            var parts = GetMaterialTypes(stats);
            var categories = parts.Keys.ToList();

            float randVal;

            // generate rare part once - for each rare part type
            if (giveHighQualityRarePart)
            {
                var randPartVal = Rand.Range(0, parts["rare"].Count);
                var partThingDefID = (AllMaterialParts)parts["rare"][randPartVal];
                int count = 0;
                result.Add(partThingDefID.ToString(), count++);
            }
            if (giveTechLevelRarePart)
            {
                var partThingDefID = (AllMaterialParts)parts["techLevel"][0];
                int count = 0;
                result.Add(partThingDefID.ToString(), count++);
            }

            for (int i = 0; i < materialTotal; i++)
            {
                randVal = Rand.Range(0f, 1f);
                int randPartVal;
                AllMaterialParts partThingDefID;
                if (randVal <= baseUncommonProb && parts.ContainsKey("uncommon"))
                {
                    randPartVal = Rand.Range(0, parts["uncommon"].Count);
                    partThingDefID = (AllMaterialParts)parts["uncommon"][randPartVal];
                }
                else
                {
                    randPartVal = Rand.Range(0, parts["common"].Count);
                    partThingDefID = (AllMaterialParts)parts["common"][randPartVal];
                }
                if (!result.ContainsKey(partThingDefID.ToString()))
                {
                    int count = 1;
                    result.Add(partThingDefID.ToString(), count);
                }
                else
                {
                    result[partThingDefID.ToString()]++;
                }               
            }
            
            return result;
        }

        public static List<string> GetPerksByMaterial(string partDefName)
        {
            var result = new List<string>();
            var temp = RimventionXMLUtility.GetMaterialPerks(partDefName);

            if (temp == null)
            {
                return null;
            }
               
            for (int i = 0; i < temp.Count; i++)
            {
                var perkID = (AllAugmentImbues)temp[i];
                var perkName = perkID.ToString();
                result.Add(perkName);
            }

            return result;
        }
      
        private static Dictionary<string, List<int>> GetConsolidatedMaterials(string type)
        {
            var resultDict = new Dictionary<string, List<int>>();

            switch (type)
            {   // ThingCategories  
                // Apparel
                case "Apparel":
                case "Headgear":
                case "ApparelUtility":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("Apparel");
                    break;
                case "ApparelArmor":
                case "ArmorHeadgear":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("ApparelArmor");
                    break;
                case "ApparelNoble":
                case "HeadgearNoble":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("ApparelNoble");
                    break;
                // Weapons MAYBE look at weaponClasses on thingdef - if possible to pull means can seperate weapons further
                case "WeaponsMelee":
                case "WeaponsMeleeBladelink":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("WeaponsMelee");
                    break;
                case "WeaponsRanged":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("WeaponsRanged");
                    break;
                case "Grenades":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("Grenades");
                    break;

                // Manufactured
                case "Manufactured":
                case "MortarShells":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("Manufactured");
                    break;
                case "Textiles":
                case "Leathers":
                case "Wools":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("Textiles");
                    break;
                case "Medicine":
                case "Drugs":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("Medicine");
                    break;

                // Foods
                case "FoodMeals":
                case "FoodRaw":
                case "MeatRaw":
                case "PlantFoodRaw":
                case "AnimalProductRaw":
                case "PlantMatter":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("Foods");
                    break;

                // Items
                case "Items":
                case "Artifacts":
                case "Neurotrainers":
                case "Techprints":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("Items");
                    break;
                case "BodyPartsProsthetic":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("BodyPartsProsthetic");
                    break;
                case "BodyPartsBionic":
                case "BodyPartsUltra":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("BodyPartsBionic");
                    break;
                case "BodyPartsArchotech":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("BodyPartsArchotech");
                    break;

                // ResourcesRaw
                case "ResourcesRaw":
                case "StoneBlocks":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("ResourcesRaw");
                    break;

                // StuffCategories
                case "Woody":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("StuffWoody");
                    break;
                case "Stony":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("StuffStony");
                    break;
                case "Metallic":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("StuffMetallic");
                    break;
                case "Leathery":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("StuffLeathery");
                    break;
                case "Fabric":
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("StuffFabric");
                    break;

                default:
                    resultDict = RimventionXMLUtility.GetMaterialsByCategory("BaseParts");
                    break;
            }

            return resultDict;
        }
        
        private static Dictionary<string, List<int>> GetMaterialTypes(RimventionDisassembleStats stats)
        {
            Dictionary<string, List<int>> finalList = new Dictionary<string, List<int>>();

            // loop through thingcategories - add to dict for each
            // check if stuffcategories is not null - add which ever one it is
            // if no category then add base parts

            if(stats.ThingCategories == null)
            {
                finalList = RimventionPatches.MergeDictionaries(GetConsolidatedMaterials(""), finalList);
            }
            else
            {
                for (int i = 0; i < stats.ThingCategories.Count; i++)
                {
                    if (stats.ThingCategories[i] != null)
                        finalList = RimventionPatches.MergeDictionaries(GetConsolidatedMaterials(stats.ThingCategories[i].ToString()), finalList);
                }
            }

            if (stats.StuffCategories == null)
            {
                finalList = RimventionPatches.MergeDictionaries(GetConsolidatedMaterials(""), finalList);
            }
            else
            {
                for (int i = 0; i < stats.StuffCategories.Count; i++)
                {
                    if (stats.StuffCategories[i] != null)
                        finalList = RimventionPatches.MergeDictionaries(GetConsolidatedMaterials(stats.StuffCategories[i].ToString()), finalList);
                }
            }

            if (giveHighQualityRarePart)
            {
                finalList = RimventionPatches.MergeDictionaries(RimventionXMLUtility.GetMaterialsByCategory("HighQualityParts"), finalList);              
            }

            if(stats.TechLevel == TechLevel.Spacer || stats.TechLevel == TechLevel.Archotech || stats.TechLevel == TechLevel.Ultra)
            {
                finalList = RimventionPatches.MergeDictionaries(RimventionXMLUtility.GetMaterialsByCategory(stats.TechLevel.ToString()), finalList);
            }

            foreach(var t in finalList)
            {
                Log.Error(t.Key + t.Value.Count.ToString());               
            }

            return finalList;
        }

        public static Dictionary<AllAugmentImbues,Tuple<RimventionImbueInfo,int>> GenerateImbuesOnCraft(List<string> imbues)
        {
            // prob func to "choose" which imbues to select - 1 per augment, but 1 imbue can have multiple effects
            // obviously if one in list return that
            // if duplicates, dont increase pick chance (e.g. remove them from the selection pool) but increase rank instead
            var result = new Dictionary<AllAugmentImbues, Tuple<RimventionImbueInfo, int>>();
            var prunedResults = new Dictionary<string, int>();
            
            // prune input list to get usable search space - get dupes and uniques
            imbues.Sort();
            var dupes = imbues.GroupBy(x => x).Where(group => group.Count() > 1).ToDictionary(x => x.Key, y => y.Count());
            var nonDupesTemp = imbues.Select(x => x).Distinct();

           // Log.Error("pruned");
            foreach (var t in nonDupesTemp)
            {
                if (!dupes.ContainsKey(t))
                {
                    prunedResults.Add(t, 1);
                }
                else
                {
                    Log.Error("already got " + t + " in dupes list");
                }
            }

            prunedResults.AddRange(dupes);
            var randVal = Rand.Range(0, prunedResults.Count);

            KeyValuePair<string, int> chosenImbue;

            if(prunedResults.Count > 1)
            {
                chosenImbue = prunedResults.ElementAt(randVal);
            }
            else
            {
                chosenImbue = prunedResults.ElementAt(0);
            }

            var imbueID = (AllAugmentImbues)Enum.Parse(typeof(AllAugmentImbues), chosenImbue.Key);
            var imbueInfo = new Tuple<RimventionImbueInfo, int>(Current.Game.GetComponent<GameComponent_Rimvention>().ImbueDatabase[imbueID], chosenImbue.Value);

            result.Add(imbueID,imbueInfo);

            return result;
        }
    }
}
