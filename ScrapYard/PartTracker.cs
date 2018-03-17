using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public class PartTracker
    {
        public enum TrackType
        {
            TOTAL,
            NEW,
            INVENTORIED
        }

        public class TrackObject
        {
            public int buildsTotal = 0;
            public int buildsNew = 0;
            public int buildsInventoried = 0;
            public int usesTotal = 0;
            public int usesNew = 0;
            public int usesInventoried = 0;
        }

        #region Fields
        private ComparisonStrength _strictness = ComparisonStrength.NAME;
        private Dictionary<InventoryPart, TrackObject> _buildTracker = new Dictionary<InventoryPart, TrackObject>();
        #endregion Fields

        #region Properties
        public bool TrackerEnabled
        {
            get
            {
                return ScrapYard.Instance.Settings.EnabledForSave && ScrapYard.Instance.Settings.CurrentSaveSettings.UseTracker;
            }
        }
        #endregion Properties

        #region Public Methods

        #region Add
        /// <summary>
        /// Takes a list of parts and registers it as a build
        /// </summary>
        /// <param name="parts">The vessel as a list of parts</param>
        public void AddBuild(IEnumerable<Part> parts)
        {
            if (!TrackerEnabled)
            {
                return;
            }
            Logging.Log("Adding build (parts)");
            Dictionary<InventoryPart, TrackType> uniqueParts = new Dictionary<InventoryPart, TrackType>();
            foreach (Part part in parts) //add a use for each part and get the unique parts for the build tracker
            {
                InventoryPart converted = new InventoryPart(part);
                InventoryPart found = addUse(converted);
                InventoryPart found_bak = found;
                TrackType thisType = converted.TrackerModule.Inventoried ? TrackType.INVENTORIED : TrackType.NEW;

                if ((found = uniqueParts.Keys.FirstOrDefault(ip => ip.IsSameAs(converted, _strictness))) != null)
                { 
                    if (uniqueParts[found] != TrackType.TOTAL && uniqueParts[found] != thisType)
                    {
                        uniqueParts[found] = TrackType.TOTAL;
                    }
                }
                else
                { //not previously in dictionary, so add
                    found = found_bak;
                    uniqueParts.Add(found, thisType);
                }
            }

            //Increment the build tracker
            foreach (var partPair in uniqueParts)
            {
                addBuild(partPair.Key, partPair.Value);
            }

            ScrapYardEvents.OnSYTrackerUpdated.Fire(uniqueParts.Keys);
        }

        /// <summary>
        /// Takes a list of part ConfigNodes and registers it as a build
        /// </summary>
        /// <param name="parts">The vessel as a list of part ConfigNodes</param>
        public void AddBuild(IEnumerable<ConfigNode> partNodes)
        {
            if (!TrackerEnabled)
            {
                return;
            }
            Logging.Log("Adding build (nodes)");
            Dictionary<InventoryPart, TrackType> uniqueParts = new Dictionary<InventoryPart, TrackType>();
            foreach (ConfigNode partNode in partNodes) //add a use for each part and get the unique parts for the build tracker
            {
                InventoryPart converted = new InventoryPart(partNode);
                InventoryPart found = addUse(converted);
                InventoryPart found_bak = found;
                TrackType thisType = converted.TrackerModule.Inventoried ? TrackType.INVENTORIED : TrackType.NEW;

                if ((found = uniqueParts.Keys.FirstOrDefault(ip => ip.IsSameAs(converted, _strictness))) != null)
                {
                    if (uniqueParts[found] != TrackType.TOTAL && uniqueParts[found] != thisType)
                    {
                        uniqueParts[found] = TrackType.TOTAL;
                    }
                }
                else
                { //not previously in dictionary, so add
                    found = found_bak;
                    uniqueParts.Add(found, thisType);
                }
            }

            //Increment the build tracker
            foreach (var partPair in uniqueParts)
            {
                addBuild(partPair.Key, partPair.Value);
            }
            ScrapYardEvents.OnSYTrackerUpdated.Fire(uniqueParts.Keys);
        }
        #endregion Add

        #region Get
        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// /// <param name="type">Which build counter to check</param>
        /// <returns>Number of builds</returns>
        public int GetBuildsForPart(Part part, TrackType type = TrackType.TOTAL)
        {
            if (!TrackerEnabled)
            {
                return 0;
            }
            InventoryPart ip = new InventoryPart(part);
            return getBuilds(ip, type);
        }

        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// /// <param name="type">Which build counter to check</param>
        /// <returns>Number of builds</returns>
        public int GetBuildsForPart(ConfigNode part, TrackType type = TrackType.TOTAL)
        {
            if (!TrackerEnabled)
            {
                return 0;
            }
            InventoryPart ip = new InventoryPart(part);
            return getBuilds(ip, type);
        }

        /// <summary>
        /// Gets the number of uses of a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// /// <param name="type">Which use counter to check</param>
        /// <returns>Number of uses</returns>
        public int GetUsesForPart(Part part, TrackType type = TrackType.TOTAL)
        {
            if (!TrackerEnabled)
            {
                return 0;
            }
            InventoryPart ip = new InventoryPart(part);
            return getUses(ip, type);
        }

        /// <summary>
        /// Gets the number of uses of a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <param name="type">Which use counter to check</param>
        /// <returns>Number of uses</returns>
        public int GetUsesForPart(ConfigNode part, TrackType type = TrackType.TOTAL)
        {
            if (!TrackerEnabled)
            {
                return 0;
            }
            InventoryPart ip = new InventoryPart(part);
            return getUses(ip, type);
        }
        #endregion Get
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Adds a single use to the useTracker. Automatically determines type
        /// </summary>
        /// <param name="source">The InventoryPart to increment</param>
        /// <returns>The corresponding InventoryPart that's actually stored</returns>
        private InventoryPart addUse(InventoryPart source)
        {
            InventoryPart found = _buildTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(source, _strictness));
            if (found != null)
            {
                _buildTracker[found].usesTotal++; //increment it
            }
            else
            {
                found = source;
                _buildTracker[source] = new TrackObject();
                _buildTracker[source].usesTotal = 1; //add it
            }
            if (source.TrackerModule.Inventoried)
            {
                _buildTracker[found].usesInventoried++;
            }
            else
            {
                _buildTracker[found].usesNew++;
            }
            Logging.Log($"{found.Name} has been used {_buildTracker[found].usesTotal}/{_buildTracker[found].usesNew}/{_buildTracker[found].usesInventoried} (T/N/I) times.");
            return found;
        }

        /// <summary>
        /// Adds a single build to the buildTracker
        /// </summary>
        /// <param name="source">The InventoryPart to increment</param>
        /// <param name="typeToAdd">Says whether this is a new build, inventoried build, or combo build (TOTAL)</param>
        /// <returns>The corresponding InventoryPart that's actually stored</returns>
        private InventoryPart addBuild(InventoryPart source, TrackType typeToAdd)
        {
            InventoryPart found = _buildTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(source, _strictness));
            if (found != null)
            {
                _buildTracker[found].buildsTotal++; //increment it
            }
            else
            {
                found = source;
                _buildTracker[source] = new TrackObject();
                _buildTracker[source].buildsTotal = 1; //add it
            }
            if (typeToAdd == TrackType.INVENTORIED || typeToAdd == TrackType.TOTAL)
            {
                _buildTracker[found].buildsInventoried++;
            }
            if (typeToAdd == TrackType.NEW || typeToAdd == TrackType.TOTAL)
            {
                _buildTracker[found].buildsNew++;
            }
            Logging.Log($"{found.Name} has been used in {_buildTracker[found].buildsTotal}/{_buildTracker[found].buildsNew}/{_buildTracker[found].buildsInventoried} (T/N/I) builds.");
            return found;
        }

        /// <summary>
        /// Gets the number of builds for an InventoryPart
        /// </summary>
        /// <param name="part">Part to get builds for</param>
        /// <returns>Number of builds</returns>
        private int getBuilds(InventoryPart part, TrackType type)
        {
            InventoryPart found = _buildTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(part, _strictness));
            if (found != null)
            {
                switch (type)
                {
                    case TrackType.TOTAL: return _buildTracker[found].buildsTotal;
                    case TrackType.NEW: return _buildTracker[found].buildsNew;
                    case TrackType.INVENTORIED: return _buildTracker[found].buildsInventoried;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the number of uses for an InventoryPart
        /// </summary>
        /// <param name="part">Part to get uses for</param>
        /// <returns>Number of uses</returns>
        private int getUses(InventoryPart part, TrackType type)
        {
            InventoryPart found = _buildTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(part, _strictness));
            if (found != null)
            {
                switch (type)
                {
                    case TrackType.TOTAL: return _buildTracker[found].usesTotal;
                    case TrackType.NEW: return _buildTracker[found].usesNew;
                    case TrackType.INVENTORIED: return _buildTracker[found].usesInventoried;
                }
            }
            return 0;
        }
        #endregion Private Methods

        #region State
        public ConfigNode State
        {
            get
            {
                ConfigNode trackerNode = new ConfigNode("PartTracker");

                //save a node with an inventory part and a count for use and builds
                foreach (InventoryPart part in _buildTracker.Keys)
                {
                    ConfigNode trackItem = new ConfigNode("TrackedItem");
                    trackItem.AddValue("buildsTotal", _buildTracker.FirstOrDefault(kvp => kvp.Key.IsSameAs(part, _strictness)).Value.buildsTotal);
                    trackItem.AddValue("buildsNew", _buildTracker.FirstOrDefault(kvp => kvp.Key.IsSameAs(part, _strictness)).Value.buildsNew);
                    trackItem.AddValue("buildsInventoried", _buildTracker.FirstOrDefault(kvp => kvp.Key.IsSameAs(part, _strictness)).Value.buildsInventoried);
                    trackItem.AddValue("usesTotal", _buildTracker.FirstOrDefault(kvp => kvp.Key.IsSameAs(part, _strictness)).Value.usesTotal);
                    trackItem.AddValue("usesNew", _buildTracker.FirstOrDefault(kvp => kvp.Key.IsSameAs(part, _strictness)).Value.usesNew);
                    trackItem.AddValue("usesInventoried", _buildTracker.FirstOrDefault(kvp => kvp.Key.IsSameAs(part, _strictness)).Value.usesInventoried);
                    trackItem.AddNode(part.State);

                    trackerNode.AddNode(trackItem);
                }
                return trackerNode;
            }
            internal set
            {
                //assign builds and uses from the node
                _buildTracker.Clear();

                if (value == null)
                {
                    return;
                }

                foreach (ConfigNode node in value.GetNodes("TrackedItem"))
                {
                    try
                    {
                        InventoryPart part = new InventoryPart();
                        part.State = node.GetNode(typeof(InventoryPart).FullName);

                        _buildTracker[part] = new TrackObject();

                        int uses = 0;
                        int builds = 0;

                        if (int.TryParse(node.GetValue("uses"), out uses) |
                        int.TryParse(node.GetValue("builds"), out builds)) //Old style. TODO: Remove before release
                        {
                            _buildTracker[part].buildsTotal = builds;
                            _buildTracker[part].usesTotal = uses;
                        }
                        else
                        {
                            int.TryParse(node.GetValue("buildsTotal"), out _buildTracker[part].buildsTotal);
                            int.TryParse(node.GetValue("buildsNew"), out _buildTracker[part].buildsNew);
                            int.TryParse(node.GetValue("buildsInventoried"), out _buildTracker[part].buildsInventoried);
                            int.TryParse(node.GetValue("usesTotal"), out _buildTracker[part].usesTotal);
                            int.TryParse(node.GetValue("usesNew"), out _buildTracker[part].usesNew);
                            int.TryParse(node.GetValue("usesInventoried"), out _buildTracker[part].usesInventoried);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Error while loading part tracker with node: " + node.name, Logging.LogType.ERROR);
                        Logging.LogException(ex);
                    }
                }
            }
        }
        #endregion State
    }
}
