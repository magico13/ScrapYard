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
            if (strength == ComparisonStrength.STRICT)
            {
                InventoryPart found = internalInventory.FirstOrDefault(ip => ip == part);
                if (found != null)
                {
                    return found;
                }
            }
            return internalInventory.FirstOrDefault(ip => ip.IsSameAs(part, strength));
        }

        /// <summary>
        /// Finds all parts in the inventory for the given InventoryPart and the provided strictness
        /// </summary>
        /// <param name="part">The source part to find a match for</param>
        /// <param name="strength">The strictness of the comparison. Defaults to MODULES.</param>
        /// <returns>An IEnumerable of InventoryParts that match</returns>
        public IEnumerable<InventoryPart> FindParts(InventoryPart part, ComparisonStrength strength = ComparisonStrength.MODULES)
        {
            if (!InventoryEnabled)
            {
                return null;
            }

            List<InventoryPart> foundParts = new List<InventoryPart>();
            PartInventory copy = Copy();
            InventoryPart found = null;
            do
            {
                found = copy.RemovePart(part, strength);
                if (found != null)
                {
                    foundParts.Add(found);
                }
            } while (found != null);

            return foundParts;
        }

        /// <summary>
        /// Returns an IEnumerable with all parts in the Inventory
        /// </summary>
        /// <returns>All inventory parts in an IEnumerable</returns>
        public IEnumerable<InventoryPart> GetAllParts()
        {
            if (!InventoryEnabled)
            {
                return null;
            }

            InventoryPart[] toReturn = new InventoryPart[internalInventory.Count];
            internalInventory.CopyTo(toReturn);
            return toReturn;
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
        /// Removes a part with the given ID
        /// </summary>
        /// <param name="id">The ID of the part to remove</param>
        /// <returns>The removed InventoryPart, or null if none found</returns>
        public InventoryPart RemovePart(Guid id)
        {
            if (!InventoryEnabled)
            {
                return null;
            }
            InventoryPart found = FindPart(id);
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
        /// Copies the PartInventory to a new PartInventory
        /// </summary>
        /// <param name="disableEventsOnCopy">If true, the copy will not fire events</param>
        /// <returns>A copy of the PartInventory</returns>
        public PartInventory Copy(bool disableEventsOnCopy = true)
        {
            PartInventory ret = null;
            bool originalDisable = disableEvents;
            try
            {
                disableEvents = true;
                ret = new PartInventory(disableEventsOnCopy);
                ret.internalInventory = new HashSet<InventoryPart>(internalInventory.Select(p => p.Copy()));
                //ret.State = State;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
            finally
            {
                disableEvents = originalDisable;
            }
            return ret;
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
                if (!disableEvents)
                {
                    Logging.DebugLog($"Saved Part Inventory of size {internalInventory.Count} in {watcher.ElapsedMilliseconds}ms");
                }
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
                if (!disableEvents)
                {
                    Logging.DebugLog($"Loaded Part Inventory of size {internalInventory.Count} in {watcher.ElapsedMilliseconds}ms");
                }
            }
        }
    }
}
