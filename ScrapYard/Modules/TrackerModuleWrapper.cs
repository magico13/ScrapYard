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
        /// True if the wrapper has an actual module applied
        /// </summary>
        public bool HasModule { get { return TrackerNode != null; } }

        private Guid? _id = null;
        /// <summary>
        /// The unique ID for this part
        /// </summary>
        public Guid? ID
        {
            get
            {
                if (_id == null)
                {
                    if (TrackerNode != null)
                    {
                        string id = null;
                        if (TrackerNode.TryGetValue("ID", ref id))
                        {
                            _id = Utils.StringToGuid(id);
                        }
                    }
                }
                return _id;
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
                if (_timesRecovered == null)
                {
                    int recovered = 0;
                    if (TrackerNode.TryGetValue("TimesRecovered", ref recovered))
                    {
                        _timesRecovered = recovered;
                    }
                }
                return _timesRecovered.GetValueOrDefault();
            }
        }
    }
}
