using ScrapYard.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static void SplitParts(IEnumerable<Part> input, out IList<InventoryPart> inInventory, out IList<InventoryPart> notInInventory)
        {
            inInventory = new List<InventoryPart>();
            notInInventory = new List<InventoryPart>();
            PartInventory InventoryCopy = ScrapYard.Instance.TheInventory.Copy();
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
        /// Applies the inventory to a vessel, specifically the part tracker module. Happens in the Editor
        /// </summary>
        /// <param name="input">The vessel as a list of parts</param>
        public static void ApplyInventoryToVessel(IEnumerable<Part> input)
        {
            PartInventory copy = ScrapYard.Instance.TheInventory.Copy();
            foreach (Part part in input)
            {
                //convert it to an inventorypart
                InventoryPart iPart = new InventoryPart(part);
                //find a corresponding one in the inventory and remove it
                
                InventoryPart inInventory = copy.RemovePart(iPart.ID);
                if (inInventory == null)
                {
                    inInventory = copy.RemovePart(iPart, ComparisonStrength.MODULES);
                }

                //if one was found...
                if (inInventory != null)
                {
                    Logging.DebugLog("Found a part in inventory for " + inInventory.Name);
                    //copy it's part tracker over
                    if (inInventory.TrackerModule != null && part.Modules?.Contains("ModuleSYPartTracker") == true)
                    {
                        ModuleSYPartTracker tracker = part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker;
                        tracker.TimesRecovered = inInventory.TrackerModule.TimesRecovered;
                        tracker.Inventoried = inInventory.TrackerModule.Inventoried;
                        tracker.ID = inInventory.ID;
                        Logging.Log($"Copied tracker. Recovered {tracker.TimesRecovered} times with id {tracker.ID}");
                    }
                }
            }

            ScrapYardEvents.OnSYInventoryAppliedToVessel.Fire();
            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        /// <summary>
        /// Applies the inventory to a vessel, specifically the part tracker module. Happens in the Editor
        /// </summary>
        /// <param name="input">The vessel as a list of part ConfigNodes</param>
        public static void ApplyInventoryToVessel(IEnumerable<ConfigNode> input)
        {
            PartInventory copy = ScrapYard.Instance.TheInventory.Copy();
            foreach (ConfigNode partNode in input)
            {
                //convert it to an inventorypart
                InventoryPart iPart = new InventoryPart(partNode);
                //find a corresponding one in the inventory and remove it
                InventoryPart inInventory = copy.RemovePart(iPart.ID);
                if (inInventory == null)
                {
                    inInventory = copy.RemovePart(iPart, ComparisonStrength.MODULES);
                }

                //if one was found...
                if (inInventory != null)
                {
                    Logging.DebugLog("Found a part in inventory for " + inInventory.Name);
                    //copy it's part tracker over
                    ConfigNode trackerNode;
                    if (inInventory.TrackerModule != null && (trackerNode = partNode.GetModuleNode("ModuleSYPartTracker")) != null)
                    {
                        string id = inInventory.ID.ToString();
                        int recovered = inInventory.TrackerModule.TimesRecovered;
                        bool inventoried = inInventory.TrackerModule.Inventoried;
                        trackerNode.SetValue("ID", id);
                        trackerNode.SetValue("TimesRecovered", recovered);
                        trackerNode.SetValue("Inventoried", inventoried);
                        Logging.DebugLog($"Copied tracker. Recovered {recovered} times with id {id}");
                    }
                }
            }
            ScrapYardEvents.OnSYInventoryAppliedToVessel.Fire();
            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        /// <summary>
        /// Sells/Discards parts from the inventory. Removes the parts from the inventory and refunds the correct amount.
        /// </summary>
        /// <param name="parts">The parts to sell</param>
        /// <returns>The total value of the sale</returns>
        public static double SellParts(IEnumerable<InventoryPart> parts)
        {
            double totalValue = 0;
            foreach (InventoryPart part in parts)
            {
                double value = 0;
                if (part.TrackerModule.Inventoried)
                {
                    InventoryPart inInventory = ScrapYard.Instance.TheInventory.RemovePart(part, ComparisonStrength.STRICT); //strict, we only remove parts that are exact
                    if (inInventory != null)
                    {
                        value = inInventory.DryCost * ScrapYard.Instance.Settings.CurrentSaveSettings.FundsSalePercent / 100.0;
                        Logging.DebugLog($"Selling/Discarding a part in inventory for {inInventory.Name} for {value} funds ({ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds}). id: {inInventory.ID}");
                        //add funds back if active
                        if (ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds)
                        {
                            Funding.Instance?.AddFunds(value, TransactionReasons.VesselRollout);
                        }
                    }
                }
                totalValue += value;
            }
            return totalValue;
        }

        /// <summary>
        /// Removes any inventory parts from the inventory (vessel rollout, KCT construction)
        /// </summary>
        /// <param name="input">The vessel as a list of parts.</param>
        public static void RemovePartsFromInventory(IEnumerable<Part> input)
        {
            foreach (Part part in input)
            {
                InventoryPart iPart = new InventoryPart(part);
                if (iPart.TrackerModule.Inventoried)
                {
                    InventoryPart inInventory = ScrapYard.Instance.TheInventory.RemovePart(iPart, ComparisonStrength.STRICT); //strict, we only remove parts that are exact

                    if (inInventory != null)
                    {
                        Logging.DebugLog($"Removed a part in inventory for {inInventory.Name} id: {inInventory.ID}");
                        //add funds back if active
                        if (ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds)
                        {
                            Funding.Instance?.AddFunds(inInventory.DryCost, TransactionReasons.VesselRollout);
                        }
                    }
                    else
                    {
                        //we couldn't find the appropriate part in the inventory, should we "make it fresh"? Or do we allow this...
                        //I mean, we kind of should allow it. Maybe. Maybe not though.
                        //we do need to verify they havent changed a part after applying it. may as well do it here
                        //but then kct edits need to add all parts back to the inventory. thats fair
                        //basically we say that you cant bring your own inventory parts and must buy ours for 4x the cost :P

                        //reset their tracker status
                        Logging.Log($"Found inventory part on vessel that is not in inventory. Resetting. {iPart.Name}:{iPart.ID}");
                        (part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                    }
                }
                else
                {
                    //It's not in the inventory, great, but it could be saved (imagine launching a saved craft from the launchpad UI)
                    //KCT gets around this by basically requiring new builds all the time, but that won't fly for UPFM
                    //So we should ALWAYS make them fresh if we can't find it in the inventory, but we don't need to log that
                    (part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker)?.MakeFresh();
                }
            }
        }

        /// <summary>
        /// Removes any inventory parts from the inventory (vessel rollout, KCT construction)
        /// </summary>
        /// <param name="input">The vessel as a list of part ConfigNodes.</param>
        public static void RemovePartsFromInventory(IEnumerable<ConfigNode> input)
        {
            foreach (ConfigNode partNode in input)
            {
                //convert it to an inventorypart
                InventoryPart iPart = new InventoryPart(partNode);
                if (iPart.TrackerModule.Inventoried)
                {
                    //find a corresponding one in the inventory and remove it
                    InventoryPart inInventory = ScrapYard.Instance.TheInventory.RemovePart(iPart, ComparisonStrength.STRICT);

                    //if one was found...
                    if (inInventory != null)
                    {
                        Logging.DebugLog($"Removed a part in inventory for {inInventory.Name} id: {inInventory.ID}");
                        //add funds back if active
                        if (ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds)
                        {
                            Funding.Instance?.AddFunds(inInventory.DryCost, TransactionReasons.VesselRollout);
                        }
                    }
                    else
                    {
                        //reset their tracker status
                        Logging.Log($"Found inventory part on vessel that is not in inventory. Resetting. {iPart.Name}:{iPart.ID}");
                        ConfigNode tracker = partNode.GetNodes("MODULE").FirstOrDefault(n => n.GetValue("name") == "ModuleSYPartTracker");
                        tracker.SetValue("ID", Guid.NewGuid().ToString());
                        tracker.SetValue("TimeRecovered", 0);
                        tracker.SetValue("Inventoried", false);
                    }
                }
                else
                {
                    //Not inventortied, so we should enforce that it's a new part (imagine a saved craft, the parts aren't in the inventory anymore
                    //but the tracking data is still saved)
                    ConfigNode tracker = partNode.GetNodes("MODULE").FirstOrDefault(n => n.GetValue("name") == "ModuleSYPartTracker");
                    if (tracker != null)
                    {
                        tracker.SetValue("ID", Guid.NewGuid().ToString());
                        tracker.SetValue("TimeRecovered", 0);
                        tracker.SetValue("Inventoried", false);
                    }
                }
            }
        }
    }
}
