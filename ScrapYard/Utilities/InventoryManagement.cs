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
        public static void SplitParts(IEnumerable<Part> input, out IList<InventoryPart> inInventory, out IList<InventoryPart> notInInventory)
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
        public static void ApplyInventoryToVessel(IEnumerable<Part> input)
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
                        tracker.Inventoried = inInventory.TrackerModule.Inventoried;
                        Logging.DebugLog($"Copied tracker. Recovered {tracker.TimesRecovered} times with id {tracker.ID}");
                    }
                }
            }

            ScrapYardEvents.OnSYInventoryAppliedToVessel.Fire();
        }

        /// <summary>
        /// Applies the inventory to a vessel, specifically the part tracker
        /// </summary>
        /// <param name="input">The vessel as a list of part ConfigNodes</param>
        public static void ApplyInventoryToVessel(IEnumerable<ConfigNode> input)
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
                        bool inventoried = inInventory.TrackerModule.Inventoried;
                        trackerNode.SetValue("ID", id);
                        trackerNode.SetValue("TimesRecovered", recovered);
                        trackerNode.SetValue("Inventoried", inventoried);
                        Logging.DebugLog($"Copied tracker. Recovered {recovered} times with id {id}");
                    }
                }
            }
            ScrapYardEvents.OnSYInventoryAppliedToVessel.Fire();
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
                        if (HighLogic.CurrentGame.Parameters.CustomParams<SaveSpecificSettings>().OverrideFunds)
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
                        Logging.DebugLog($"Found inventory part on vessel that is not in inventory. Resetting. {iPart.Name}:{iPart.ID}");
                        (part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                    }
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
                        if (HighLogic.CurrentGame.Parameters.CustomParams<SaveSpecificSettings>().OverrideFunds)
                        {
                            Funding.Instance?.AddFunds(inInventory.DryCost, TransactionReasons.VesselRollout);
                        }
                    }
                    else
                    {
                        //reset their tracker status
                        Logging.DebugLog($"Found inventory part on vessel that is not in inventory. Resetting. {iPart.Name}:{iPart.ID}");
                        ConfigNode tracker = partNode.GetNodes("MODULE").FirstOrDefault(n => n.GetValue("name") == "ModuleSYPartTracker");
                        tracker.SetValue("ID", Guid.NewGuid().ToString());
                        tracker.SetValue("TimeRecovered", 0);
                        tracker.SetValue("Inventoried", false);
                    }
                }
            }
        }

        /// <summary>
        /// Verifies that the inventory parts on the ship in the editor are valid
        /// </summary>
        public static void VerifyEditorShip()
        {
            foreach (Part part in EditorLogic.fetch?.ship?.Parts ?? new List<Part>())
            {
                InventoryPart iPart = new InventoryPart(part);
                if (iPart.ID == null)
                {
                    (part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                }
                if (iPart.TrackerModule.Inventoried)
                {
                    InventoryPart inInventory = ScrapYard.Instance.TheInventory.FindPart(iPart, ComparisonStrength.STRICT); //strict, we only remove parts that are exact
                    if (inInventory == null)
                    {
                        //reset their tracker status
                        Logging.DebugLog($"Found inventory part on vessel that is not in inventory. Resetting. {iPart.Name}:{iPart.ID}");
                        (part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                    }
                }
                else
                {
                    //check that we're not sharing an ID with something in the inventory
                    if (iPart.ID.HasValue)
                    {
                        InventoryPart inInventory = ScrapYard.Instance.TheInventory.FindPart(iPart.ID.Value);
                        if (inInventory != null)
                        {
                            //found a part that is sharing an ID but shouldn't be
                            Logging.DebugLog($"Found part on vessel with same ID as inventory part, but not matching. Resetting. {iPart.Name}:{iPart.ID}");
                            (part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                        }
                    }
                }
            }
        }

    }
}
