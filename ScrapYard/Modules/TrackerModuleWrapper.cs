using ScrapYard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.Modules
{
    public class TrackerModuleWrapper
    {
        public ConfigNode TrackerNode { get; }

        /// <summary>
        /// Creates a new wrapper around the ModuleSYPartTracker ConfigNode
        /// </summary>
        /// <param name="trackerConfigNode">The ModuleSYPartTracker ConfigNode</param>
        public TrackerModuleWrapper(ConfigNode trackerConfigNode)
        {
            TrackerNode = trackerConfigNode;
        }

        /// <summary>
        /// Creates a new wrapper for the ModuleSYPartTracker, without the actual backing ConfigNode
        /// </summary>
        /// <param name="id">The ID</param>
        /// <param name="recovered">The number of times recovered</param>
        /// <param name="inventoried">Whether the part is from the inventory</param>
        public TrackerModuleWrapper(uint id, int recovered, bool inventoried)
        {
            _id = id;
            _timesRecovered = recovered;
            _inventoried = inventoried;
        }

        /// <summary>
        /// True if the wrapper has an actual module applied
        /// </summary>
        public bool HasModule { get { return TrackerNode != null; } }

        private uint? _id = null;
        /// <summary>
        /// The unique ID for this part
        /// </summary>
        public uint? ID
        {
            get
            {
                if (_id == null && HasModule)
                {
                    uint id = 0;
                    if (TrackerNode.TryGetValue("ID", ref id))
                    {
                        _id = id;
                    }
                }
                return _id;
            }
            set
            {
                //set the ID in the actual node
                if (HasModule && value.HasValue)
                {
                    TrackerNode.SetValue("ID", value.Value.ToString());
                }
                 _id = value;
            }
        }

        private int? _timesRecovered = null;
        /// <summary>
        /// The number of times this part has been recovered
        /// </summary>
        public int TimesRecovered
        {
            get
            {
                if (_timesRecovered == null && HasModule)
                {
                    int recovered = 0;
                    if (TrackerNode.TryGetValue("TimesRecovered", ref recovered))
                    {
                        _timesRecovered = recovered;
                    }
                }
                return _timesRecovered.GetValueOrDefault();
            }
            set
            {
                //set the number in the actual node
                if (HasModule)
                {
                    TrackerNode.SetValue("TimesRecovered", value);
                }
                _timesRecovered = value;
            }
        }

        private bool? _inventoried = null;
        /// <summary>
        /// True if the part has been in the inventory, false if it is new
        /// </summary>
        public bool Inventoried
        {
            get
            {
                if (_inventoried == null && HasModule)
                {
                    bool inventoried = false;
                    if (TrackerNode.TryGetValue("Inventoried", ref inventoried))
                    {
                        _inventoried = inventoried;
                    }
                }
                return _inventoried.GetValueOrDefault();
            }
            set
            {
                if (HasModule)
                {
                    TrackerNode.SetValue("Inventoried", value);
                }
                _inventoried = value;
            }
        }
    }
}
