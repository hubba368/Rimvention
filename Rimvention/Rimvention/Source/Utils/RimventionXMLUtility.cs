using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System;
using Verse;

namespace Rimvention
{

    public static class RimventionXMLUtility
    {
        public static string GetMainModDirectory(bool getSteamDirectory = false, bool getLocalDirectory = false)
        {
            DirectoryInfo d = Directory.GetParent(Environment.CurrentDirectory);
            string temp = d.FullName;
            string relativePath = "";

            if (!getSteamDirectory && !getLocalDirectory)
                return Path.GetFullPath(Path.Combine(temp, @"..\workshop\\content\\294100"));

            if (getSteamDirectory)
                return Path.GetFullPath(Path.Combine(temp, @"..\workshop\\content\\294100\\1732571153"));

            if (getLocalDirectory) // MAYBE probs change this to be 1.2 if including 1.2 compat (will probs work anyway)
                return Path.GetFullPath(Path.Combine(temp, @"RimWorld\\Mods\\Rimvention\\v1.3"));

            return relativePath;
        }

        public static Dictionary<AllAugmentImbues, RimventionImbueInfo> GetAllImbueInformation()
        {
            // + partDefName + ".xml"
            var result = new Dictionary<AllAugmentImbues, RimventionImbueInfo>();
            string imbuePath = GetMainModDirectory(false, true) + "\\AugmentImbues" + "\\" + "Imbues" + "\\";
            var imbueFiles = Directory.EnumerateFiles(imbuePath);

            for(int i = 0; i < imbueFiles.Count(); i++)
            {
                var fileName = imbuePath + imbueFiles.ElementAt(i).Substring(imbuePath.Length);
                XDocument document;
                try
                {
                    document = XDocument.Load(fileName);
                }
                catch
                {
                    Log.Error("Could not find Imbue XML file. Skipping...");
                    Log.Error("Current Path string: " + fileName);
                    continue;
                }

                AllAugmentImbues id = 0;
                string className = "";
                string hediffDefName = "";
                string imbueName = "";
                string description = "";

                if (document.Descendants("Imbue") != null)
                {
                    var temp = int.Parse(document.Descendants("Imbue").Elements("ID").ElementAt(0).Value);
                    id = (AllAugmentImbues)temp;
                    className = document.Descendants("Imbue").Elements("ClassName").ElementAt(0).Value;
                    hediffDefName = document.Descendants("Imbue").Elements("HediffDef").ElementAt(0).Value;
                    imbueName = document.Descendants("Imbue").Elements("Name").ElementAt(0).Value;
                    description = document.Descendants("Imbue").Elements("Description").ElementAt(0).Value;
                }

                RimventionImbueInfo currentInfo = new RimventionImbueInfo(id, imbueName, "", className, hediffDefName, description);

                Log.Error("test xml check");
                Log.Error("ID: " + id);
                Log.Error("class name: " + className);
                Log.Error("hediff: " + hediffDefName);
                Log.Error("name: " + imbueName);
                Log.Error("description: " + description);

                result.Add(id, currentInfo);
            }

            return result;
        }


        public static List<int> GetMaterialPerks(string partDefName)
        {
            if (partDefName == "")
                return null;

            var result = new List<int>();

            XDocument document;
            document = XDocument.Load(GetMainModDirectory(false, true) + "\\AugmentImbues" + "\\" + "PartDropsByImbueIDs" + "\\" + partDefName + ".xml");
            var root = document.Root;

            if (document.Descendants("imbues") != null)
            {
                var list = new List<int>();
                var temp = document.Descendants("Source").Select(x => new
                {
                    list = x.Elements("imbues").Elements().Select(y => int.Parse(y.Value)).ToList()
                });
                result = temp.ElementAt(0).list;
            }

            return result;
        }

        public static Dictionary<string, List<int>> GetMaterialsByCategory(string category)
        {
            var resultDict = new Dictionary<string, List<int>>();

            XDocument document;
            document = XDocument.Load(GetMainModDirectory(false, true) + "\\PartCategories" + "\\" + category + ".xml");
            var root = document.Root;


            if (document.Descendants("common") != null)
            {
                var list = new List<int>();
                var temp = document.Descendants("Source").Select(x => new
                {
                    list = x.Elements("common").Elements().Select(y => int.Parse(y.Value)).ToList()    
                });
                resultDict.Add("common", temp.ElementAt(0).list);
            }
           
            if(document.Descendants("uncommon") != null)
            {
                var list = new List<int>();
                var temp = document.Descendants("Source").Select(x => new
                {
                    list = x.Elements("uncommon").Elements().Select(y => int.Parse(y.Value)).ToList()
                });
                resultDict.Add("uncommon", temp.ElementAt(0).list);
            }

            if(document.Descendants("rare") != null)
            {
                var list = new List<int>();
                var temp = document.Descendants("Source").Select(x => new
                {
                    list = x.Elements("rare").Elements().Select(y => int.Parse(y.Value)).ToList()
                });
                resultDict.Add("rare", temp.ElementAt(0).list);
            }

            if (document.Descendants("techlevel") != null)
            {
                var list = new List<int>();
                var temp = document.Descendants("Source").Select(x => new
                {
                    list = x.Elements("techlevel").Elements().Select(y => int.Parse(y.Value)).ToList()
                });
                resultDict.Add("techlevel", temp.ElementAt(0).list);
            }

            return resultDict;
        }
    }
}
