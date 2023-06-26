using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Rimvention
{
    public class GameComponent_Rimvention : GameComponent
    {
        private Dictionary<int, List<RimventionUIElement>> _rimventionUITextures;
        private Dictionary<AllAugmentImbues, RimventionUIElement> _rimventionImbueUITextures; // stored by id to get rid of that pesky looping otherwise
        private Dictionary<AllAugmentImbues, RimventionImbueInfo> _rimventionImbues; // all imbues without ranks

        public Dictionary<int, List<RimventionUIElement>> RimventionUITextures { get => _rimventionUITextures; }
        public Dictionary<AllAugmentImbues, RimventionUIElement> RimventionImbueUITextures { get => _rimventionImbueUITextures; }
        public Dictionary<AllAugmentImbues, RimventionImbueInfo> ImbueDatabase { get => _rimventionImbues; }


        public GameComponent_Rimvention(Game game) : base()
        {
            _rimventionUITextures = new Dictionary<int, List<RimventionUIElement>>();
            _rimventionImbueUITextures = new Dictionary<AllAugmentImbues, RimventionUIElement>();

            _rimventionImbues = new Dictionary<AllAugmentImbues, RimventionImbueInfo>();
            
            InitImbueDatabase();
            InitAssemblerTextures();
            InitImbueTextures();
        }
     
        private void InitAssemblerTextures()
        {// MAYBE - move this to xml file and just deserialise when requested?
            var partTextures = new List<RimventionUIElement>();
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "BasePart", "Base Part", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "SimplePart", "Simple Part", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "PlatedPart", "Plated Part", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "PaddedPart", "Padded Part", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "ProtectivePart", "Protective Part", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            partTextures.Add(new RimventionUIElement(RimventionTextures.TestIcon, "", "", 1));
            var craftArea = new List<RimventionUIElement>();
            craftArea.Add(new RimventionUIElement(RimventionTextures.TestIcon2, "", "", 0));
            craftArea.Add(new RimventionUIElement(RimventionTextures.TestIcon2, "", "", 0));
            craftArea.Add(new RimventionUIElement(RimventionTextures.TestIcon2, "", "", 0));
            _rimventionUITextures.Add(0, partTextures);
            _rimventionUITextures.Add(1, craftArea);
        }

        private void InitImbueTextures()
        {
            // empty slot
            _rimventionImbueUITextures.Add(AllAugmentImbues.Empty, new RimventionUIElement(RimventionTextures.TestIcon, new RimventionImbueInfo(AllAugmentImbues.Empty,"","","","","")));

            _rimventionImbueUITextures.Add(ImbueDatabase[AllAugmentImbues.Encumbered].ImbueID, new RimventionUIElement(RimventionTextures.TestIcon, ImbueDatabase[AllAugmentImbues.Encumbered]));
            _rimventionImbueUITextures.Add(ImbueDatabase[AllAugmentImbues.Energised].ImbueID, new RimventionUIElement(RimventionTextures.TestIcon, ImbueDatabase[AllAugmentImbues.Energised]));
        }

        private void InitImbueDatabase()
        {
            _rimventionImbues.AddRange(RimventionXMLUtility.GetAllImbueInformation());
        }

        // probably didnt need any of these, because each class that uses them has innate access to the worktables regardless if they are a patch or not
        #region WorkTable Functions 
        public void SetCurrentPartsForCraftingAugment(int assemblerID, int billStackIndex, List<string> imbueNames)
        {
            var assemblers = RimventionPatches.GetAllOfThingOnMap("Assembler");

            if (!assemblers.NullOrEmpty())
            {
                foreach (var asm in assemblers)
                {
                    var current = (Building_Assembler)asm;
                    if (current.thingIDNumber == assemblerID)
                    {
                        Log.Error("Found correct assembler to set");
                        //current.ImbueBillStack.Add(imbueNames);
                    }
                }
            }
        }

        public List<string> GetCurrentImbueBillAtAssemblerByID(int assemblerID)
        {
            var assemblers = RimventionPatches.GetAllOfThingOnMap("Assembler");

            if (!assemblers.NullOrEmpty())
            {
                foreach(var asm in assemblers)
                {
                    var current = (Building_Assembler)asm;
                    if (current.thingIDNumber == assemblerID)
                    {
                        //Log.Error("Found correct assembler to get of ID: "  + assemblerID.ToString());
                        var imbueStack = current.ImbueBillStack[0];
                        return imbueStack.AugmentBill;
                    }
                }
            }
            return null;
        }

        public void DeleteCurrentBillAtAssemblerByID(int assemblerID)
        {
            var assemblers = RimventionPatches.GetAllOfThingOnMap("Assembler");

            if (!assemblers.NullOrEmpty())
            {
                foreach (var asm in assemblers)
                {
                    var current = (Building_Assembler)asm;
                    if (current.thingIDNumber == assemblerID)
                    {
                        current.RemoveImbueBillFromImbueStack(0);
                    }
                }
            }
        }

        public RimventionDisassembleStats GetCurrentBillDisassemblerByID(int disassemblerID)
        {
            var disassemblers = RimventionPatches.GetAllOfThingOnMap("Disassembler");

            if (!disassemblers.NullOrEmpty())
            {
                foreach (var dsm in disassemblers)
                {
                    var current = (Building_Disassembler)dsm;
                    if (current.thingIDNumber == disassemblerID)
                    {
                        //Log.Error("Found correct disassembler to get of ID: " + assemblerID.ToString());
                        var stats = current.DisassembleBillStack[0];
                        return stats;
                    }
                }
            }
            return null;
        }

        public void DeleteCurrentBillAtDisassemblerByID(int disassemblerID)
        {
            var disassemblers = RimventionPatches.GetAllOfThingOnMap("Disassembler");

            if (!disassemblers.NullOrEmpty())
            {
                foreach (var dsm in disassemblers)
                {
                    var current = (Building_Disassembler)dsm;
                    if (current.thingIDNumber == disassemblerID)
                    {
                        current.RemoveBillFromDisStack(0);
                    }
                }
            }
        }
        #endregion

        #region Def Requesters
        public ThingDef RequestThingDefByName(string defName)
        {
            var defs = DefDatabase<ThingDef>.AllDefsListForReading;

            if (!(from t in defs where t.defName == defName select t).TryRandomElement(out ThingDef finalDef))
            {
                Log.Error("No Def with name of " + defName + " exists.");
                return null;
            }
            else
            {
                return finalDef;
            }
        }
        public RecipeDef RequestRecipeDefByName(string defName)
        {
            var defs = DefDatabase<RecipeDef>.AllDefsListForReading;

            if (!(from t in defs where t.defName == defName select t).TryRandomElement(out RecipeDef finalDef))
            {
                Log.Error("No Def with name of " + defName + " exists.");
                return null;
            }
            else
            {
                return finalDef;
            }
        }

        public HediffDef RequestHediffDefByName(string imbueName)
        {
            if (!(from t in DefDatabase<HediffDef>.AllDefs where t.defName == imbueName select t).TryRandomElement(out HediffDef finalDef))
            {
                Log.Error("No Def with name of " + imbueName + " exists.");
                return null;
            }
            else
            {
                var hediff = finalDef;
                return hediff;
            }
        }
        #endregion
    }
}
