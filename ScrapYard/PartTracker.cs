using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public class PartTracker
    {
        #region Fields
        private ComparisonStrength _strictness = ComparisonStrength.NAME;
        private Dictionary<InventoryPart, int> _buildTracker = new Dictionary<InventoryPart, int>();
        private Dictionary<InventoryPart, int> _useTracker = new Dictionary<InventoryPart, int>();
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
        public void AddBuild(List<Part> parts)
        {
            if (!TrackerEnabled)
            {
                return;
            }
            Logging.DebugLog("Adding build (parts)");
            List<InventoryPart> uniqueParts = new List<InventoryPart>();
            foreach (Part part in parts) //add a use for each part and get the unique parts for the build tracker
            {
                InventoryPart converted = new InventoryPart(part);
                InventoryPart found = addUse(converted);
                if (!uniqueParts.Exists(ip => ip.IsSameAs(converted, _strictness)))
                {
                    uniqueParts.Add(found);
                }
            }

            //Increment the build tracker
            foreach (InventoryPart part in uniqueParts)
            {
                addBuild(part);
            }
        }

        /// <summary>
        /// Takes a list of part ConfigNodes and registers it as a build
        /// </summary>
        /// <param name="parts">The vessel as a list of part ConfigNodes</param>
        public void AddBuild(List<ConfigNode> partNodes)
        {
            if (!TrackerEnabled)
            {
                return;
            }
            Logging.DebugLog("Adding build (nodes)");
            List<InventoryPart> uniqueParts = new List<InventoryPart>();
            foreach (ConfigNode partNode in partNodes) //add a use for each part and get the unique parts for the build tracker
            {
                InventoryPart converted = new InventoryPart(partNode);
                InventoryPart found = addUse(converted);
                if (!uniqueParts.Exists(ip => ip.IsSameAs(converted, _strictness)))
                {
                    uniqueParts.Add(found);
                }
            }

            //Increment the build tracker
            foreach (InventoryPart part in uniqueParts)
            {
                addBuild(part);
            }
        }
        #endregion Add

        #region Get
        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>Number of builds</returns>
        public int GetBuildsForPart(Part part)
        {
            if (!TrackerEnabled)
            {
                return 0;
            }
            InventoryPart ip = new InventoryPart(part);
            return getBuilds(ip);
        }

        /// <summary>
        /// Gets the number of builds for a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>Number of builds</returns>
        public int GetBuildsForPart(ConfigNode part)
        {
            if (!TrackerEnabled)
            {
                return 0;
            }
            InventoryPart ip = new InventoryPart(part);
            return getBuilds(ip);
        }

        /// <summary>
        /// Gets the number of uses of a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>Number of uses</returns>
        public int GetUsesForPart(Part part)
        {
            if (!TrackerEnabled)
            {
                return 0;
            }
            InventoryPart ip = new InventoryPart(part);
            return getUses(ip);
        }

        /// <summary>
        /// Gets the number of uses of a part
        /// </summary>
        /// <param name="part">The part to check</param>
        /// <returns>Number of uses</returns>
        public int GetUsesForPart(ConfigNode part)
        {
            if (!TrackerEnabled)
            {
                return 0;
            }
            InventoryPart ip = new InventoryPart(part);
            return getUses(ip);
        }
        #endregion Get
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Adds a single use to the useTracker
        /// </summary>
        /// <param name="source">The InventoryPart to increment</param>
        /// <returns>The corresponding InventoryPart that's actually stored</returns>
        private InventoryPart addUse(InventoryPart source)
        {
            InventoryPart found = _useTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(source, _strictness));
            if (found != null)
            {
                _useTracker[found] += 1; //increment it
            }
            else
            {
                found = source;
                _useTracker[source] = 1; //add it
            }
            Logging.DebugLog($"{found.Name} has been used {_useTracker[found]} total times.");
            return found;
        }

        /// <summary>
        /// Adds a single build to the buildTracker
        /// </summary>
        /// <param name="source">The InventoryPart to increment</param>
        /// <returns>The corresponding InventoryPart that's actually stored</returns>
        private InventoryPart addBuild(InventoryPart source)
        {
            InventoryPart found = _buildTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(source, _strictness));
            if (found != null)
            {
                _buildTracker[found] += 1; //increment it
            }
            else
            {
                found = source;
                _buildTracker[source] = 1; //add it
            }
            Logging.DebugLog($"{found.Name} has been used in {_buildTracker[found]} builds.");
            return found;
        }

        /// <summary>
        /// Gets the number of builds for an InventoryPart
        /// </summary>
        /// <param name="part">Part to get builds for</param>
        /// <returns>Number of builds</returns>
        private int getBuilds(InventoryPart part)
        {
            InventoryPart found = _buildTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(part, _strictness));
            if (found != null)
            {
                return _buildTracker[found];
            }
            return 0;
        }

        /// <summary>
        /// Gets the number of uses for an InventoryPart
        /// </summary>
        /// <param name="part">Part to get uses for</param>
        /// <returns>Number of uses</returns>
        private int getUses(InventoryPart part)
        {
            InventoryPart found = _useTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(part, _strictness));
            if (found != null)
            {
                return _useTracker[found];
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
                //create a list of unique parts
                List<InventoryPart> uniqueParts = new List<InventoryPart>(_useTracker.Keys);
                //add the _buildTracker to the list when unique
                foreach (InventoryPart part in _buildTracker.Keys)
                {
                    if (!uniqueParts.Exists(ip => ip.IsSameAs(part, _strictness)))
                    {
                        uniqueParts.Add(part);
                    }
                }

                //save a node with an inventory part and a count for use and builds
                foreach (InventoryPart part in uniqueParts)
                {
                    ConfigNode trackItem = new ConfigNode("TrackedItem");
                    trackItem.AddValue("builds", _buildTracker.FirstOrDefault(kvp => kvp.Key.IsSameAs(part, _strictness)).Value);
                    trackItem.AddValue("uses", _useTracker.FirstOrDefault(kvp => kvp.Key.IsSameAs(part, _strictness)).Value);
                    trackItem.AddNode(part.State);

                    trackerNode.AddNode(trackItem);
                }
                return trackerNode;
            }
            internal set
            {
                //assign builds and uses from the node
                _buildTracker.Clear();
                _useTracker.Clear();

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

                        int uses = 0;
                        int builds = 0;

                        int.TryParse(node.GetValue("uses"), out uses);
                        int.TryParse(node.GetValue("builds"), out builds);

                        _useTracker[part] = uses;
                        _buildTracker[part] = builds;
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Error while loading part tracker with node: " + node.name);
                        Logging.LogException(ex);
                    }
                }
            }
        }
        #endregion State
    }
}
