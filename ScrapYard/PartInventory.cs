using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using ScrapYard.Modules;
using ScrapYard.Utilities;
using System.Diagnostics;

namespace ScrapYard
{
    public class PartInventory
    {
        private bool disableEvents = false;
        private HashSet<InventoryPart> internalInventory = new HashSet<InventoryPart>();


        #region Properties
        /// <summary>
        /// Determines if the Inventory feature is enabled for this save
        /// </summary>
        public bool InventoryEnabled
        {
            get
            {
                return ScrapYard.Instance.Settings.EnabledForSave && ScrapYard.Instance.Settings.CurrentSaveSettings.UseInventory;
            }
        }
        #endregion Properties

        /// <summary>
        /// Creates a new empty part inventory
        /// </summary>
        public PartInventory() { }
        /// <summary>
        /// Creates a new PartInventory that doesn't trigger events when the inventory changes
        /// </summary>
        /// <param name="DisableEvents">Disables event firing if true.</param>
        public PartInventory(bool DisableEvents)
        {
            disableEvents = DisableEvents;
        }

        /// <summary>
        /// Adds a part to the inventory using an InventoryPart
        /// </summary>
        /// <param name="part">The Inventory Part to add</param>
        public InventoryPart AddPart(InventoryPart part)
        {
            if (!InventoryEnabled || part.DoNotStore) //if not using the inventory, or the part shouldn't be stored, then do not add it
            {
                return null;
            }
            part.TrackerModule.Inventoried = true;
            internalInventory.Add(part);
            if (!disableEvents)
            {
                ScrapYardEvents.OnSYInventoryChanged.Fire(part, true);
            }
            return part;
        }

        /// <summary>
        /// Adds a part to the inventory using a Part
        /// </summary>
        /// <param name="part">The Part to add</param>
        public InventoryPart AddPart(Part part)
        {
            InventoryPart convertedPart = new InventoryPart(part);
            return AddPart(convertedPart);
        }

        /// <summary>
        /// Adds a part to the inventory using a ProtoPartSnapshot
        /// </summary>
        /// <param name="protoPartSnapshot">The ProtoPartSnapshot to add</param>
        public InventoryPart AddPart(ProtoPartSnapshot protoPartSnapshot)
        {
            InventoryPart convertedPart = new InventoryPart(protoPartSnapshot);
            return AddPart(convertedPart);
        }

        /// <summary>
        /// Adds a part to the inventory using a ConfigNode of a Part
        /// </summary>
        /// <param name="partNode">The ConfigNode to add</param>
        public InventoryPart AddPart(ConfigNode partNode)
        {
            InventoryPart convertedPart = new InventoryPart(partNode);
            return AddPart(convertedPart);
        }

        /// <summary>
        /// Finds a part in the inventory for the given id (ModuleSYPartTracker)
        /// </summary>
        /// <param name="id">The ID to search for</param>
        /// <returns>The stored InventoryPart or null if not found</returns>
        public InventoryPart FindPart(Guid id)
        {
            if (!InventoryEnabled)
            {
                return null;
            }
            return internalInventory.FirstOrDefault(ip => ip.ID == id);
        }

        /// <summary>
        /// Finds a part in the inventory for the given InventoryPart and a strictness of comparison
        /// </summary>
        /// <param name="part">The source part to find a match for</param>
        /// <param name="strength">The strictness of the comparison. Defaults to MODULES.</param>
        /// <returns>The InventoryPart or null if not found.</returns>
        public InventoryPart FindPart(InventoryPart part, ComparisonStrength strength = ComparisonStrength.MODULES)
        {
            if (!InventoryEnabled)
            {
                return null;
            }
            return internalInventory.FirstOrDefault(ip => ip.IsSameAs(part, strength));
        }

        /// <summary>
        /// Removes a part from the inventory given an InventoryPart to compare and the strictness of comparison
        /// </summary>
        /// <param name="part">The source part to find a match for</param>
        /// <param name="strength">The strictness of the comparison. Defaults to MODULES</param>
        /// <returns>The removed InventoryPart, or null if none found</returns>
        public InventoryPart RemovePart(InventoryPart part, ComparisonStrength strength = ComparisonStrength.MODULES)
        {
            if (!InventoryEnabled)
            {
                return null;
            }
            InventoryPart found = FindPart(part, strength);
            if (found != null && internalInventory.Remove(found))
            {
                if (!disableEvents)
                {
                    ScrapYardEvents.OnSYInventoryChanged.Fire(found, false);
                }
                return found;
            }
            return null;
        }

        /// <summary>
        /// Returns a ConfigNode representing the current state, or sets the state from a ConfigNode
        /// </summary>
        public ConfigNode State
        {
            get
            {
                Stopwatch watcher = Stopwatch.StartNew();
                ConfigNode returnNode = new ConfigNode("PartInventory");
                //Add module nodes
                foreach (InventoryPart part in internalInventory)
                {
                    ConfigNode toAdd = part.State;
                    returnNode.AddNode(toAdd);
                }
                watcher.Stop();
                Logging.DebugLog($"Saved Part Inventory of size {internalInventory.Count} in {watcher.ElapsedMilliseconds}ms");
                return returnNode;
            }
            internal set
            {
                Stopwatch watcher = Stopwatch.StartNew();
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
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                }
                watcher.Stop();
                Logging.DebugLog($"Loaded Part Inventory of size {internalInventory.Count} in {watcher.ElapsedMilliseconds}ms");
            }
        }
    }
}
