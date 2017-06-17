using ScrapYard.Modules;
using ScrapYard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    /// <summary>
    /// This is the remote-side of the API, where all calls through the wrapper funnel through.
    /// It can also be referenced directly, and it is recommended that you interact with ScrapYard through these methods exclusively.
    /// This may be required later on, with everything else being made internal (probably not, but it'd be better design)
    /// </summary>
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
        public IList<Part> GetPartsInInventory_Parts(IEnumerable<Part> sourceParts, string strictness)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return new List<Part>();
            }
            ComparisonStrength actualStrictness = parseStrictnessString(strictness);
            List<Part> inInventory = new List<Part>();
            PartInventory InventoryCopy = ScrapYard.Instance.TheInventory.Copy();
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
        public IList<ConfigNode> GetPartsInInventory_ConfigNodes(IEnumerable<ConfigNode> sourceParts, string strictness)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return new List<ConfigNode>();
            }
            ComparisonStrength actualStrictness = parseStrictnessString(strictness);
            List<ConfigNode> inInventory = new List<ConfigNode>();
            PartInventory InventoryCopy = ScrapYard.Instance.TheInventory.Copy();
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
        public void AddPartsToInventory_Parts(IEnumerable<Part> parts, bool incrementRecovery)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return;
            }
            foreach (Part part in parts)
            {
                InventoryPart iPart = new InventoryPart(part);
                if (incrementRecovery)
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
        public void AddPartsToInventory_Nodes(IEnumerable<ConfigNode> parts, bool incrementRecovery)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return;
            }
            foreach (ConfigNode part in parts)
            {
                InventoryPart iPart = new InventoryPart(part);
                if (incrementRecovery)
                {
                    iPart.TrackerModule.TimesRecovered++;
                }
                ScrapYard.Instance.TheInventory.AddPart(iPart);
            }
        }

        /// <summary>
        /// Adds a single part to the inventory, increments the recovery tracker if specified
        /// </summary>
        /// <param name="sourcePart">The source part to add</param>
        /// <param name="incrementRecovery">If true, increases the number of recoveries</param>
        /// <returns>True if added, false otherwise</returns>
        public bool AddPartToInventory_Part(Part sourcePart, bool incrementRecovery)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return false;
            }

            InventoryPart iPart = new InventoryPart(sourcePart);
            if (incrementRecovery)
            {
                iPart.TrackerModule.TimesRecovered++;
            }

            return (ScrapYard.Instance.TheInventory.AddPart(iPart) != null);
        }

        /// <summary>
        /// Adds a single part to the inventory, increments the recovery tracker if specified
        /// </summary>
        /// <param name="sourcePart">The source part to add</param>
        /// <param name="incrementRecovery">If true, increases the number of recoveries</param>
        /// <returns>True if added, false otherwise</returns>
        public bool AddPartToInventory_Node(ConfigNode sourcePart, bool incrementRecovery)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return false;
            }

            InventoryPart iPart = new InventoryPart(sourcePart);
            if (incrementRecovery)
            {
                iPart.TrackerModule.TimesRecovered++;
            }

            return (ScrapYard.Instance.TheInventory.AddPart(iPart) != null);
        }

        /// <summary>
        /// Removes a single part from the inventory.
        /// </summary>
        /// <param name="sourcePart">The part to remove.</param>
        /// <param name="strictness">The strictness to use when searching for the appropriate part.</param>
        /// <returns>True if removed, false otherwise.</returns>
        public bool RemovePartFromInventory_Part(Part sourcePart, string strictness)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return false;
            }

            ComparisonStrength actualStrictness = parseStrictnessString(strictness);

            return (ScrapYard.Instance.TheInventory.RemovePart(new InventoryPart(sourcePart), actualStrictness) != null);
        }

        /// <summary>
        /// Removes a single part from the inventory.
        /// </summary>
        /// <param name="sourcePart">The part to remove.</param>
        /// <param name="strictness">The strictness to use when searching for the appropriate part.</param>
        /// <returns>True if removed, false otherwise.</returns>
        public bool RemovePartFromInventory_Node(ConfigNode sourcePart, string strictness)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return false;
            }

            ComparisonStrength actualStrictness = parseStrictnessString(strictness);

            return (ScrapYard.Instance.TheInventory.RemovePart(new InventoryPart(sourcePart), actualStrictness) != null);
        }

        /// <summary>
        /// Finds an InventoryPart for a given part
        /// </summary>
        /// <param name="sourcePart">The part to search for</param>
        /// <param name="strictness">The strictness to use when searching for the part</param>
        /// <returns>The ConfigNode for the InventoryPart, or null if not found</returns>
        public ConfigNode FindInventoryPart_Part(Part sourcePart, string strictness)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return null;
            }

            ComparisonStrength actualStrictness = parseStrictnessString(strictness);

            return ScrapYard.Instance.TheInventory.FindPart(new InventoryPart(sourcePart), actualStrictness)?.State;
        }

        /// <summary>
        /// Finds an InventoryPart for a given part
        /// </summary>
        /// <param name="sourcePart">The part to search for</param>
        /// <param name="strictness">The strictness to use when searching for the part</param>
        /// <returns>The ConfigNode for the InventoryPart, or null if not found</returns>
        public ConfigNode FindInventoryPart_Node(ConfigNode sourcePart, string strictness)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return null;
            }

            ComparisonStrength actualStrictness = parseStrictnessString(strictness);

            return ScrapYard.Instance.TheInventory.FindPart(new InventoryPart(sourcePart), actualStrictness)?.State;
        }

        /// <summary>
        /// Finds an InventoryPart for a given ID
        /// </summary>
        /// <param name="id">The id of the part to search for.</param>
        /// <returns>The ConfigNode for the InventoryPart, or null if not found</returns>
        public ConfigNode FindInventoryPart_ID(string id)
        {
            if (!ScrapYard.Instance.TheInventory.InventoryEnabled)
            {
                return null;
            }

            Guid? guid = Utilities.Utils.StringToGuid(id);
            if (!guid.HasValue)
            {
                return null;
            }
            return ScrapYard.Instance.TheInventory.FindPart(guid.Value)?.State;
        }
        #endregion Inventory Manipulation

        #region Vessel Processing
        /// <summary>
        /// Removes inventory parts, refunds funds, marks it as tracked
        /// </summary>
        /// <param name="parts">The vessel as a List of Parts</param>
        /// <returns>True if processed, false otherwise</returns>
        public bool ProcessVessel_Parts(IEnumerable<Part> parts)
        {
            if (!ScrapYard.Instance.Settings.EnabledForSave)
            {
                return true;
            }
            //try to get the ID out of the list
            Guid? guid = Utils.StringToGuid(parts.FirstOrDefault()?.Modules.GetModule<ModuleSYPartTracker>()?.ID);
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
        public bool ProcessVessel_Nodes(IEnumerable<ConfigNode> partNodes)
        {
            if (!ScrapYard.Instance.Settings.EnabledForSave)
            {
                return true;
            }
            //try to get the ID out of the list
            Guid? guid = Utils.StringToGuid(
                partNodes.FirstOrDefault()?.GetNodes("MODULE").FirstOrDefault(n => n.GetValue("name").Equals("ModuleSYPartTracker", StringComparison.OrdinalIgnoreCase))?.GetValue("ID"));
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
        public void RecordBuild_Parts(IEnumerable<Part> parts)
        {
            ScrapYard.Instance.PartTracker.AddBuild(parts);
        }

        /// <summary>
        /// Records a build in the part tracker
        /// </summary>
        /// <param name="parts">The vessel as a list of ConfigNodes.</param>
        public void RecordBuild_Nodes(IEnumerable<ConfigNode> parts)
        {
            ScrapYard.Instance.PartTracker.AddBuild(parts);
        }

        /// <summary>
        /// Sets whether a vessel is tracked or not
        /// </summary>
        /// <param name="id">The ID of the vessel</param>
        /// <param name="newStatus">The status to set</param>
        /// <returns>The previous status</returns>
        public bool SetProcessedStatus_ID(string id, bool newStatus)
        {
            return ScrapYard.Instance.ProcessedTracker.TrackVessel(Utils.StringToGuid(id), newStatus);
        }
        #endregion Vessel Processing

        #region Part Tracker
        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <param name="type">The type of build counter to check</param>
        /// <returns>Number of builds for the part</returns>
        public int GetBuildCount_Part(Part part, string type)
        {
            return ScrapYard.Instance.PartTracker.GetBuildsForPart(part, parseTrackType(type));
        }

        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="partNode">The ConfigNode of the part to check</param>
        /// <param name="type">The type of build counter to check</param>
        /// <returns>Number of builds for the part</returns>
        public int GetBuildCount_Node(ConfigNode partNode, string type)
        {
            return ScrapYard.Instance.PartTracker.GetBuildsForPart(partNode, parseTrackType(type));
        }

        /// <summary>
        /// Gets the number of uses of a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <param name="type">The type of use counter to check</param>
        /// <returns>Number of uses of the part</returns>
        public int GetUseCount_Part(Part part, string type)
        {
            return ScrapYard.Instance.PartTracker.GetUsesForPart(part, parseTrackType(type));
        }

        /// <summary>
        /// Gets the number of uses of a part
        /// </summary>
        /// <param name="partNode">The ConfigNode of the part to check</param>
        /// <param name="type">The type of use counter to check</param>
        /// <returns>Number of uses of the part</returns>
        public int GetUseCount_Node(ConfigNode partNode, string type)
        {
            return ScrapYard.Instance.PartTracker.GetUsesForPart(partNode, parseTrackType(type));
        }

        /// <summary>
        /// Gets the unique ID for the current part.
        /// It is recommended to cache this.
        /// </summary>
        /// <param name="part">The part to get the ID of</param>
        /// <returns>The part's ID (a Guid) as a string or null if it can't be gotten</returns>
        public string GetPartID_Part(Part part)
        {
            return new InventoryPart(part).ID?.ToString();
        }

        /// <summary>
        /// Gets the unique ID for the current part.
        /// It is recommended to cache this.
        /// </summary>
        /// <param name="part">The part to get the ID of</param>
        /// <returns>The part's ID (a Guid) as a string or null if it can't be gotten</returns>
        public string GetPartID_Node(ConfigNode part)
        {
            return new InventoryPart(part).ID?.ToString();
        }

        /// <summary>
        /// Gets the number of times a part has been recovered. 
        /// It is recommended to cache this.
        /// </summary>
        /// <param name="part">The part to get the TimesRecovered count of.</param>
        /// <returns>The number of times the part has been recovered.</returns>
        public int GetTimesUsed_Part(Part part)
        {
            return new InventoryPart(part).TrackerModule.TimesRecovered;
        }

        /// <summary>
        /// Gets the number of times a part has been recovered.
        /// It is recommended to cache this.
        /// </summary>
        /// <param name="part">The part to get the TimesRecovered count of.</param>
        /// <returns>The number of times the part has been recovered.</returns>
        public int GetTimesUsed_Node(ConfigNode part)
        {
            return new InventoryPart(part).TrackerModule.TimesRecovered;
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
        #endregion Part Tracker


        private ComparisonStrength parseStrictnessString(string strictness)
        {
            try
            {
                ComparisonStrength actualStrictness = (ComparisonStrength)Enum.Parse(typeof(ComparisonStrength), strictness);
                return actualStrictness;
            }
            catch
            {
                return ComparisonStrength.MODULES;
            }
        }

        private PartTracker.TrackType parseTrackType(string type)
        {
            try
            {
                PartTracker.TrackType actualStrictness = (PartTracker.TrackType)Enum.Parse(typeof(PartTracker.TrackType), type);
                return actualStrictness;
            }
            catch
            {
                return PartTracker.TrackType.TOTAL;
            }
        }
    }
}
