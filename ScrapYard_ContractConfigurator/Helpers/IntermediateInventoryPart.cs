using ContractConfigurator;
using ScrapYard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard_ContractConfigurator
{
    public class IntermediateInventoryPart
    {
        public string PartName = string.Empty;
        public int TimesRecovered = -1;
        public int Count = 1;
        public ConfigNode ModulesNode = new ConfigNode("MODULES");

        public IntermediateInventoryPart() { }
        public IntermediateInventoryPart(ConfigNode source)
        {
            FromConfigNode(source);
        }

        public ConfigNode AsConfigNode()
        {
            ConfigNode node = new ConfigNode("INVENTORY_PART");
            node.AddValue("name", PartName);
            node.AddValue("timesRecovered", TimesRecovered);
            node.AddValue("count", Count);
            node.AddNode(ModulesNode);
            return node;
        }

        public void FromConfigNode(ConfigNode source)
        {
            if (!source.TryGetValue("name", ref PartName))
            {
                throw new InvalidOperationException("A 'name' must be included in an INVENTORY_PART node!");
            }
            if (source.TryGetValue("timesRecovered", ref TimesRecovered))
            {

            }
            if (source.TryGetValue("count", ref Count))
            {
                if (Count < 1)
                {
                    throw new InvalidOperationException("The 'count' must be >0 in an INVENTORY_PART node!");
                }
            }

            source.TryGetNode("MODULES", ref ModulesNode);
        }

        private Part copyPart(AvailablePart partInfo)
        {
            Part part = UnityEngine.Object.Instantiate(partInfo.partPrefab);
            part.gameObject.SetActive(true);
            part.name = partInfo.name;

            return part;
        }

        /// <summary>
        /// Converts the IntermediateInventoryPart into a list of InventoryParts
        /// </summary>
        /// <returns>A List of InventoryParts</returns>
        public List<InventoryPart> ToInventoryParts()
        {
            List<InventoryPart> parts = new List<InventoryPart>();

            try
            {
                //find availablepart
                AvailablePart aPart = PartLoader.LoadedPartsList.Find(aP => aP.name == PartName);

                if (aPart == null)
                {
                    LoggingUtil.LogError(this, $"No AvailablePart found with name '{PartName}'");
                    return parts;
                }

                //copy the part out
                Part prefab = copyPart(aPart);

                //generate an InventoryPart confignode
                ConfigNode iConfig = new ConfigNode("ScrapYard.InventoryPart");
                iConfig.AddValue("_name", PartName);
                iConfig.AddValue("_id", 0);
                iConfig.AddValue("_timesRecovered", TimesRecovered);
                iConfig.AddValue("_inventoried", true);
                if (ModulesNode != null && ModulesNode.CountNodes > 0)
                {
                    foreach (ConfigNode module in ModulesNode.GetNodes("MODULE"))
                    {
                        string modName = module.GetValue("name");

                        //replace module if exists, otherwise add it
                        if (prefab.Modules?.Count > 0)
                        {
                            //look for this module on the partInfo and replace it
                            if (prefab.Modules.Contains(modName))
                            {
                                prefab.Modules[modName].Load(module);
                            }
                            else
                            {
                                LoggingUtil.LogError(this, $"Prefab didn't have module: {modName}");
                            }
                        }

                        iConfig.AddNode(module);
                    }
                }

                //find the dry cost
                float dryCost = 0;
                dryCost = prefab.GetModuleCosts(aPart.cost) + aPart.cost;
                foreach (PartResource resource in prefab.Resources)
                {
                    dryCost -= (float)(resource.maxAmount * PartResourceLibrary.Instance.GetDefinition(resource.resourceName).unitCost);
                }
                //set the dry cost
                iConfig.AddValue("_dryCost", dryCost);

                //create the parts
                for (int i = 0; i < Count; i++)
                {
                    InventoryPart iPart = new InventoryPart();
                    iPart.State = iConfig;
                    //and add it
                    parts.Add(iPart);
                }
            }
            catch (Exception ex)
            {
                LoggingUtil.LogException(ex);
            }
            return parts;
        }
    }
}
