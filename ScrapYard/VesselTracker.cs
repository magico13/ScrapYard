using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    /// <summary>
    /// Tracks whether a vessel (identified by a guid) has had parts removed already
    /// </summary>
    public class VesselTracker
    {
        private Dictionary<Guid, bool> tracker = new Dictionary<Guid, bool>();

        /// <summary>
        /// Adds or replaces a vessel in the tracker.
        /// </summary>
        /// <param name="vesselID">The ID of the vessel to track.</param>
        /// <param name="partsProcessed">Whether the vessel has been processed. Defaults to false.</param>
        /// <returns>The previous state of the vessel in the tracker.</returns>
        public bool TrackVessel(Guid? vesselID, bool partsProcessed = false)
        {
            if (vesselID == null)
            {
                return false;
            }
            Guid id = vesselID.GetValueOrDefault();
            bool originalState = false;
            tracker.TryGetValue(id, out originalState);
            tracker[id] = partsProcessed;

            Logging.DebugLog($"Tracking vessel '{id}'. Original state: {originalState} New state: {partsProcessed}");

            return originalState;
        }

        /// <summary>
        /// Returns whether the given vessel has been processed.
        /// </summary>
        /// <param name="vesselID">The ID of the vessel to check.</param>
        /// <returns>True if processed, false if not or not tracked.</returns>
        public bool IsProcessed(Guid? vesselID)
        {
            if (vesselID == null)
            {
                return false;
            }
            Guid id = vesselID.GetValueOrDefault();
            bool processed = false;
            tracker.TryGetValue(id, out processed);

            return processed;
        }

        /// <summary>
        /// Gets whether the vessel has been processed, or adds it to the tracker with the provided state
        /// </summary>
        /// <param name="vesselID">The vessel to check or add</param>
        /// <param name="partsProcessed">The value to store if not present</param>
        /// <returns>The value if in the tracker, or the provided value if added.</returns>
        public bool GetOrAdd(Guid? vesselID, bool partsProcessed = false)
        {
            if (vesselID == null)
            {
                return false;
            }
            Guid id = vesselID.GetValueOrDefault();
            bool processed = false;
            if (tracker.TryGetValue(id, out processed))
            {
                return processed;
            }
            else
            {
                tracker[id] = partsProcessed;
                return partsProcessed;
            }
        }

        /// <summary>
        /// Removes a vessel from the tracker (typically when launched).
        /// </summary>
        /// <param name="vesselID">The vessel to remove</param>
        /// <returns>True if removed, false otherwise.</returns>
        public bool Remove(Guid? vesselID)
        {
            if (!IsProcessed(vesselID))
            {
                return false;
            }

            Logging.DebugLog($"Removing tracking of vessel '{vesselID}'.");
            return tracker.Remove(vesselID.GetValueOrDefault());
        }

        /// <summary>
        /// Returns a ConfigNode representing the current state, or sets the state from a ConfigNode
        /// </summary>
        public ConfigNode State
        {
            get
            {
                ConfigNode node = new ConfigNode("VesselTracker");
                foreach (KeyValuePair<Guid, bool> kvp in tracker)
                {
                    node.AddValue(kvp.Key.ToString(), kvp.Value);
                }
                return node;
            }
            internal set
            {
                try
                {
                    tracker.Clear();
                    if (value == null)
                    {
                        return;
                    }
                    foreach (ConfigNode.Value val in value.values)
                    {
                        Guid? id = Utilities.Utils.StringToGuid(val.name);
                        if (id.HasValue)
                        {
                            bool tracked;
                            if (bool.TryParse(val.value, out tracked))
                            {
                                tracker[(Guid)id] = tracked;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("Exception while assigning VesselTracker");
                    Logging.LogException(ex);
                }
            }
        }
    }
}
