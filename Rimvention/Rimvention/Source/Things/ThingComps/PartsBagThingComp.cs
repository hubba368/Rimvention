using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Rimvention
{
    class PartsBagThingComp : ThingComp
    {
        private Dictionary<string, int> storedParts = new Dictionary<string, int>();
        const int maxUniquePartCategories = 6;
        int currentUniqueCategories;

        public PartsBagCompProperties Properties => (PartsBagCompProperties)this.Properties;
        public Dictionary<string, int> StoredParts { get => storedParts; }     
        public bool isFull = false;

        public override void CompTick()
        {
            UpdatePartCategoryCount();
            if (currentUniqueCategories == maxUniquePartCategories)
            {
                isFull = true;
            }
            else
            {
                isFull = false;
            }
            base.CompTick();
        }

        public void AddPartToStore(string part)
        {
            if(storedParts == null || storedParts.Count == 0)
            {
                storedParts.Add(part, 1);
            }

            if (storedParts.ContainsKey(part))
                storedParts[part]++;
            else if(currentUniqueCategories == maxUniquePartCategories)
            {
                // NO call some UI message to say cant add due to being full
                Log.Error("Part bag is full - cannot add more than 6 unique types. \n");
                Log.Error("Tried to add part of type: " + part);
                
            }
            else if(currentUniqueCategories < maxUniquePartCategories)
            {
                storedParts.Add(part, 1);
            }
        }
        public void AddPartToStore(Dictionary<string, int> part)
        {
            for(int i = 0; i < part.Count; i++)
            {
                for(int j = 0; j < part.ElementAt(i).Value; j++)
                {
                    AddPartToStore(part.ElementAt(i).Key);
                }
            }
        }

        private void UpdatePartCategoryCount()
        {
            currentUniqueCategories = storedParts.Keys.Count;
        }
    }
}
