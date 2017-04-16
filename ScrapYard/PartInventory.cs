using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using ScrapYard.Modules;
using ScrapYard.Utilities;

namespace ScrapYard
{
    public class PartInventory
    {
        private bool disableEvents = false;
        private HashSet<InventoryPart> internalInventory = new HashSet<InventoryPart>();

        public PartInventory() { }
        /// <summary>
        /// Creates a new PartInventory that doesn't trigger events when the inventory changes
        /// </summary>
        /// <param name="DisableEvents">Disables event firing if true.</param>
        public PartInventory(bool DisableEvents)
        {
            disableEvents = DisableEvents;
        }

        public void AddPart(InventoryPart part)
        {
            internalInventory.Add(part);
            if (!disableEvents)
            {
                Events.SYInventoryChanged.Fire(part, true);
            }
        }

        public void AddPart(Part part)
        {
            InventoryPart convertedPart = new InventoryPart(part);
            AddPart(convertedPart);
        }

        public void AddPart(ProtoPartSnapshot protoPartSnapshot)
        {
            InventoryPart convertedPart = new InventoryPart(protoPartSnapshot);
            AddPart(convertedPart);
        }

        public void AddPart(ConfigNode partNode)
        {
            InventoryPart convertedPart = new InventoryPart(partNode);
            AddPart(convertedPart);
        }

       /* public int IncrementUsageCounter(InventoryPart part)
        {
            InventoryPart existingPart = FindPart(part);
            if (existingPart == null)
            {
                InternalInventory.Add(part);
                existingPart = part;
            }
            existingPart.AddUsage();
            return existingPart.Used;
        }*/

        public InventoryPart FindPart(InventoryPart part, ComparisonStrength strength = ComparisonStrength.MODULES)
        {
            return internalInventory.FirstOrDefault(ip => ip.IsSameAs(part, strength));
        }

        public InventoryPart RemovePart(InventoryPart part, ComparisonStrength strength = ComparisonStrength.MODULES)
        {
            InventoryPart found = FindPart(part, strength);
            if (found != null && internalInventory.Remove(found))
            {
                if (!disableEvents)
                {
                    Events.SYInventoryChanged.Fire(found, false);
                }
                return found;
            }
            return null;
        }

        public void SplitParts(List<InventoryPart> input, out List<InventoryPart> inInventory, out List<InventoryPart> notInInventory)
        {
            inInventory = new List<InventoryPart>();
            notInInventory = new List<InventoryPart>();
            PartInventory InventoryCopy = new PartInventory(true);
            InventoryCopy.State = State; //TODO: Make a copy method
            foreach (InventoryPart inputPart in input)
            {
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
        /// Applies the inventory to a vessel, specifically the part tracker
        /// </summary>
        /// <param name="input">The vessel as a list of parts</param>
        public void ApplyInventoryToVessel(List<Part> input)
        {
            foreach (Part part in input)
            {
                //convert it to an inventorypart
                InventoryPart iPart = new InventoryPart(part);
                //find a corresponding one in the inventory and remove it
                InventoryPart inInventory = RemovePart(iPart, ComparisonStrength.MODULES);

                //if one was found...
                if (inInventory != null)
                {
                    Logging.DebugLog("Found a part in inventory for " + inInventory.Name);
                    //copy it's part tracker over
                    if (inInventory.TrackerModule != null && part.Modules?.Contains("ModuleSYPartTracker") == true)
                    {
                        ModuleSYPartTracker tracker = part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker;
                        tracker.ID = inInventory.TrackerModule.GetValue("ID");
                        int.TryParse(inInventory.TrackerModule.GetValue("TimesRecovered"), out tracker.TimesRecovered);
                        Logging.DebugLog($"Copied tracker. Recovered {tracker.TimesRecovered} times with id {tracker.ID}");
                    }
                    //add funds back if active
                    if (ScrapYard.Instance.Settings.OverrideFunds && HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        Funding.Instance.AddFunds(inInventory.DryCost, TransactionReasons.VesselRollout);
                    }
                }
            }
        }

        /// <summary>
        /// Applies the inventory to a vessel, specifically the part tracker
        /// </summary>
        /// <param name="input">The vessel as a list of part ConfigNodes</param>
        public void ApplyInventoryToVessel(List<ConfigNode> input)
        {
            foreach (ConfigNode partNode in input)
            {
                //convert it to an inventorypart
                InventoryPart iPart = new InventoryPart(partNode);
                //find a corresponding one in the inventory and remove it
                InventoryPart inInventory = RemovePart(iPart, ComparisonStrength.MODULES);

                //if one was found...
                if (inInventory != null)
                {
                    Logging.DebugLog("Found a part in inventory for " + inInventory.Name);
                    //copy it's part tracker over
                    ConfigNode trackerNode;
                    if (inInventory.TrackerModule != null && (trackerNode = partNode.GetModuleNode("ModuleSYPartTracker")) != null)
                    {
                        string id = inInventory.TrackerModule.GetValue("ID");
                        string recovered = inInventory.TrackerModule.GetValue("TimesRecovered");
                        trackerNode.SetValue("ID", id);
                        trackerNode.SetValue("TimesRecovered", recovered);
                        Logging.DebugLog($"Copied tracker. Recovered {recovered} times with id {id}");
                    }
                    //add funds back if active
                    if (ScrapYard.Instance.Settings.OverrideFunds && HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        Funding.Instance.AddFunds(inInventory.DryCost, TransactionReasons.VesselRollout);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a ConfigNode representing the current state, or sets the state from a ConfigNode
        /// </summary>
        public ConfigNode State
        {
            get
            {
                ConfigNode returnNode = new ConfigNode("PartInventory");
                //Add module nodes
                foreach (InventoryPart part in internalInventory)
                {
                    ConfigNode toAdd = part.State;
                    returnNode.AddNode(toAdd);
                }
                return returnNode;
            }
            internal set
            {
                try
                {
                    internalInventory = new HashSet<InventoryPart>();
                    if (value == null)
                    {
                        return;
                    }

                    foreach (ConfigNode inventoryPartNode in value.GetNodes(typeof(InventoryPart).FullName))
                    {
                        InventoryPart loading = new InventoryPart();
                        loading.State = inventoryPartNode;
                        internalInventory.Add(loading);
                    }
                    Logging.DebugLog("Printing PartInventory:");
                    foreach (InventoryPart part in internalInventory)
                    {
                        Logging.DebugLog(part.Name);
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                }
            }
        }
    }
}
