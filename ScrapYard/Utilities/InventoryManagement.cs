using ScrapYard.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.Utilities
{
    /// <summary>
    /// Various utility functions for interacting with the inventory on an entire Vessel basis
    /// </summary>
    public static class InventoryManagement
    {
        /// <summary>
        /// Splits a list of parts into a list of those that are in the inventory and those that are not
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inInventory"></param>
        /// <param name="notInInventory"></param>
        public static void SplitParts(List<Part> input, out List<InventoryPart> inInventory, out List<InventoryPart> notInInventory)
        {
            inInventory = new List<InventoryPart>();
            notInInventory = new List<InventoryPart>();
            PartInventory InventoryCopy = new PartInventory(true);
            InventoryCopy.State = ScrapYard.Instance.TheInventory.State;
            foreach (Part part in input)
            {
                InventoryPart inputPart = new InventoryPart(part);
                if (InventoryCopy.RemovePart(inputPart) != null)
                {
                    inInventory.Add(inputPart);
                }
                else
                {
                    notInInventory.Add(inputPart);
                }
            }
        }

        /// <summary>
        /// Applies the inventory to a vessel, specifically the part tracker. Happens in the Editor
        /// </summary>
        /// <param name="input">The vessel as a list of parts</param>
        public static void ApplyInventoryToVessel(List<Part> input)
        {
            PartInventory copy = new PartInventory(true);
            copy.State = ScrapYard.Instance.TheInventory.State;
            foreach (Part part in input)
            {
                //convert it to an inventorypart
                InventoryPart iPart = new InventoryPart(part);
                //find a corresponding one in the inventory and remove it
                InventoryPart inInventory = copy.RemovePart(iPart, ComparisonStrength.MODULES);

                //if one was found...
                if (inInventory != null)
                {
                    Logging.DebugLog("Found a part in inventory for " + inInventory.Name);
                    //copy it's part tracker over
                    if (inInventory.TrackerModule != null && part.Modules?.Contains("ModuleSYPartTracker") == true)
                    {
                        ModuleSYPartTracker tracker = part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker;
                        tracker.ID = inInventory.ID?.ToString();
                        tracker.TimesRecovered = inInventory.TrackerModule.TimesRecovered;
                        Logging.DebugLog($"Copied tracker. Recovered {tracker.TimesRecovered} times with id {tracker.ID}");
                    }
                }
            }
        }

        /// <summary>
        /// Applies the inventory to a vessel, specifically the part tracker
        /// </summary>
        /// <param name="input">The vessel as a list of part ConfigNodes</param>
        public static void ApplyInventoryToVessel(List<ConfigNode> input)
        {
            PartInventory copy = new PartInventory(true);
            copy.State = ScrapYard.Instance.TheInventory.State;
            foreach (ConfigNode partNode in input)
            {
                //convert it to an inventorypart
                InventoryPart iPart = new InventoryPart(partNode);
                //find a corresponding one in the inventory and remove it
                InventoryPart inInventory = copy.RemovePart(iPart, ComparisonStrength.MODULES);

                //if one was found...
                if (inInventory != null)
                {
                    Logging.DebugLog("Found a part in inventory for " + inInventory.Name);
                    //copy it's part tracker over
                    ConfigNode trackerNode;
                    if (inInventory.TrackerModule != null && (trackerNode = partNode.GetModuleNode("ModuleSYPartTracker")) != null)
                    {
                        string id = inInventory.ID?.ToString();
                        int recovered = inInventory.TrackerModule.TimesRecovered;
                        trackerNode.SetValue("ID", id);
                        trackerNode.SetValue("TimesRecovered", recovered);
                        Logging.DebugLog($"Copied tracker. Recovered {recovered} times with id {id}");
                    }
                }
            }
        }


        /// <summary>
        /// Removes any inventory parts from the inventory (vessel rollout, KCT construction)
        /// </summary>
        /// <param name="input">The vessel as a list of parts.</param>
        public static void RemovePartsFromInventory(List<Part> input)
        {
            foreach (Part part in input)
            {
                InventoryPart iPart = new InventoryPart(part);
                InventoryPart inInventory = ScrapYard.Instance.TheInventory.RemovePart(iPart, ComparisonStrength.STRICT); //strict, we only remove parts that are exact

                if (inInventory != null)
                {
                    Logging.DebugLog($"Removed a part in inventory for {inInventory.Name} id: {inInventory.ID}");
                    //add funds back if active
                    if (ScrapYard.Instance.Settings.OverrideFunds && HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        Funding.Instance.AddFunds(inInventory.DryCost, TransactionReasons.VesselRollout);
                    }
                }
            }
        }

        /// <summary>
        /// Removes any inventory parts from the inventory (vessel rollout, KCT construction)
        /// </summary>
        /// <param name="input">The vessel as a list of part ConfigNodes.</param>
        public static void RemovePartsFromInventory(List<ConfigNode> input)
        {
            foreach (ConfigNode partNode in input)
            {
                //convert it to an inventorypart
                InventoryPart iPart = new InventoryPart(partNode);
                //find a corresponding one in the inventory and remove it
                InventoryPart inInventory = ScrapYard.Instance.TheInventory.RemovePart(iPart, ComparisonStrength.STRICT);

                //if one was found...
                if (inInventory != null)
                {
                    Logging.DebugLog($"Removed a part in inventory for {inInventory.Name} id: {inInventory.ID}");
                    //add funds back if active
                    if (ScrapYard.Instance.Settings.OverrideFunds && HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        Funding.Instance.AddFunds(inInventory.DryCost, TransactionReasons.VesselRollout);
                    }
                }
            }
        }

    }
}
