using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public class PartTracker
    {
        private ComparisonStrength _strictness = ComparisonStrength.NAME;
        private Dictionary<InventoryPart, int> _buildTracker = new Dictionary<InventoryPart, int>();
        private Dictionary<InventoryPart, int> _useTracker = new Dictionary<InventoryPart, int>();
        
        /// <summary>
        /// Takes a list of parts and registers it as a build
        /// </summary>
        /// <param name="parts">The vessel as a list of parts</param>
        public void AddBuild(List<Part> parts)
        {
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
                InventoryPart found = _buildTracker.Keys.FirstOrDefault(ip => ip.IsSameAs(part, _strictness));
                if (found != null)
                {
                    _buildTracker[found] += 1; //increment it
                }
                else
                {
                    _buildTracker[part] = 1; //add it
                }
            }
        }

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
                _useTracker[source] = 1; //add it
            }
            return found ?? source;
        }

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
    }
}
