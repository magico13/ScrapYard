using ScrapYard.Modules;
using ScrapYard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public sealed class APIManager
    {
        private static APIManager _instance = new APIManager();

        /// <summary>
        /// A static instance of the APIManager
        /// </summary>
        public static APIManager Instance
        {
            get
            {
                return _instance;
            }
        }

        #region Inventory Manipulation
        /// <summary>
        /// Takes a List of Parts and returns the Parts that are present in the inventory. 
        /// </summary>
        /// <param name="sourceParts">Source list of parts</param>
        /// <param name="strictness">The strictness enum value name.</param>
        /// <returns>List of Parts that are in the inventory</returns>
        public List<Part> GetPartsInInventory_Parts(List<Part> sourceParts, string strictness)
        {
            ComparisonStrength actualStrictness = (ComparisonStrength)Enum.Parse(typeof(ComparisonStrength), strictness);
            List<Part> inInventory = new List<Part>();
            PartInventory InventoryCopy = new PartInventory(true);
            InventoryCopy.State = ScrapYard.Instance.TheInventory.State;
            foreach (Part part in sourceParts)
            {
                InventoryPart inputPart = new InventoryPart(part);
                if (InventoryCopy.RemovePart(inputPart, actualStrictness) != null)
                {
                    inInventory.Add(part);
                }
            }
            return inInventory;
        }

        /// <summary>
        /// Takes a List of part ConfigNodes and returns the ConfigNodes that are present in the inventory. 
        /// Assumes the default strictness.
        /// </summary>
        /// <param name="sourceParts">Source list of parts</param>
        /// <returns>List of part ConfigNodes that are in the inventory</returns>
        public List<ConfigNode> GetPartsInInventory_ConfigNodes(List<ConfigNode> sourceParts, string strictness)
        {
            ComparisonStrength actualStrictness = (ComparisonStrength)Enum.Parse(typeof(ComparisonStrength), strictness);
            List<ConfigNode> inInventory = new List<ConfigNode>();
            PartInventory InventoryCopy = new PartInventory(true);
            InventoryCopy.State = ScrapYard.Instance.TheInventory.State;
            foreach (ConfigNode part in sourceParts)
            {
                InventoryPart inputPart = new InventoryPart(part);
                if (InventoryCopy.RemovePart(inputPart, actualStrictness) != null)
                {
                    inInventory.Add(part);
                }
            }
            return inInventory;
        }

        /// <summary>
        /// Adds a list of parts to the Inventory
        /// </summary>
        /// <param name="parts">The list of parts to add</param>
        /// <param name="incrementRecovery">If true, increments the number of recoveries in the tracker</param>
        public void AddPartsToInventory_Parts(List<Part> parts, bool incrementRecovery)
        {
            foreach (Part part in parts)
            {
                InventoryPart iPart = new InventoryPart(part);
                if (incrementRecovery && iPart.TrackerModule.HasModule)
                {
                    iPart.TrackerModule.TimesRecovered++;
                }
                ScrapYard.Instance.TheInventory.AddPart(iPart);
            }
        }

        /// <summary>
        /// Adds a list of parts to the Inventory
        /// </summary>
        /// <param name="parts">The list of parts to add</param>
        /// <param name="incrementRecovery">If true, increments the number of recoveries in the tracker</param>
        public void AddPartsToInventory_Nodes(List<ConfigNode> parts, bool incrementRecovery)
        {
            foreach (ConfigNode part in parts)
            {
                InventoryPart iPart = new InventoryPart(part);
                if (incrementRecovery && iPart.TrackerModule.HasModule)
                {
                    iPart.TrackerModule.TimesRecovered++;
                }
                ScrapYard.Instance.TheInventory.AddPart(iPart);
            }
        }
        #endregion Inventory Manipulation

        #region Vessel Processing
        /// <summary>
        /// Removes inventory parts, refunds funds, marks it as tracked
        /// </summary>
        /// <param name="parts">The vessel as a List of Parts</param>
        /// <returns>True if processed, false otherwise</returns>
        public bool ProcessVessel_Parts(List<Part> parts)
        {
            //try to get the ID out of the list
            Guid? guid = Utils.StringToGuid(parts[0].Modules.GetModule<ModuleSYPartTracker>()?.ID);
            if (!guid.HasValue)
            {
                return false; //for now we can't process this vessel. Sorry. Maybe later we'll be able to add the module
            }
            Guid ID = guid.GetValueOrDefault();

            //check that it isn't already processed
            if (ScrapYard.Instance.ProcessedTracker.IsProcessed(ID))
            {
                return false;
            }

            //remove parts
            InventoryManagement.RemovePartsFromInventory(parts);

            //Mark as processed
            ScrapYard.Instance.ProcessedTracker.TrackVessel(ID, true);

            return true;

        }

        /// <summary>
        /// Removes inventory parts, refunds funds, marks it as tracked
        /// </summary>
        /// <param name="parts">The vessel as a List of Parts</param>
        /// <returns>True if processed, false otherwise</returns>
        public bool ProcessVessel_Nodes(List<ConfigNode> partNodes)
        {
            //try to get the ID out of the list
            Guid? guid = Utils.StringToGuid(
                partNodes[0].GetNodes("MODULE").FirstOrDefault(n => n.GetValue("name").Equals("ModuleSYPartTracker", StringComparison.OrdinalIgnoreCase))?.GetValue("ID"));
            if (!guid.HasValue)
            {
                return false; //for now we can't process this vessel. Sorry. Maybe later we'll be able to add the module
            }
            Guid ID = guid.GetValueOrDefault();

            //check that it isn't already processed
            if (ScrapYard.Instance.ProcessedTracker.IsProcessed(ID))
            {
                return false;
            }

            //remove parts
            InventoryManagement.RemovePartsFromInventory(partNodes);

            //Mark as processed
            ScrapYard.Instance.ProcessedTracker.TrackVessel(ID, true);

            return true;

        }

        /// <summary>
        /// Records a build in the part tracker
        /// </summary>
        /// <param name="parts">The vessel as a list of Parts.</param>
        public void RecordBuild_Parts(List<Part> parts)
        {
            ScrapYard.Instance.PartTracker.AddBuild(parts);
        }

        /// <summary>
        /// Records a build in the part tracker
        /// </summary>
        /// <param name="parts">The vessel as a list of ConfigNodes.</param>
        public void RecordBuild_Nodes(List<ConfigNode> parts)
        {
            ScrapYard.Instance.PartTracker.AddBuild(parts);
        }

        /// <summary>
        /// Checks if the part is pulled from the inventory or is new
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>True if from inventory, false if new</returns>
        public bool PartIsFromInventory_Part(Part part)
        {
            InventoryPart iPart = new InventoryPart(part);
            return iPart.TrackerModule.Inventoried;
        }

        /// <summary>
        /// Checks if the part is pulled from the inventory or is new
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>True if from inventory, false if new</returns>
        public bool PartIsFromInventory_Node(ConfigNode part)
        {
            InventoryPart iPart = new InventoryPart(part);
            return iPart.TrackerModule.Inventoried;
        }
        #endregion

        #region Part Tracker
        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>Number of builds for the part</returns>
        public int GetBuildCount_Part(Part part)
        {
            return ScrapYard.Instance.PartTracker.GetBuildsForPart(part);
        }

        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="partNode">The ConfigNode of the part to check</param>
        /// <returns>Number of builds for the part</returns>
        public int GetBuildCount_Node(ConfigNode partNode)
        {
            return ScrapYard.Instance.PartTracker.GetBuildsForPart(partNode);
        }

        /// <summary>
        /// Gets the number of uses of a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>Number of uses of the part</returns>
        public int GetUseCount_Part(Part part)
        {
            return ScrapYard.Instance.PartTracker.GetUsesForPart(part);
        }

        /// <summary>
        /// Gets the number of uses of a part
        /// </summary>
        /// <param name="partNode">The ConfigNode of the part to check</param>
        /// <returns>Number of uses of the part</returns>
        public int GetUseCount_Node(ConfigNode partNode)
        {
            return ScrapYard.Instance.PartTracker.GetUsesForPart(partNode);
        }
        #endregion Part Tracker
    }
}
