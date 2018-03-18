using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    /// <summary>
    /// Tracks whether a vessel (identified by a uint) has had parts removed already
    /// </summary>
    public class VesselTracker
    {
        private Dictionary<uint, bool> tracker = new Dictionary<uint, bool>();

        /// <summary>
        /// Adds or replaces a vessel in the tracker.
        /// </summary>
        /// <param name="vesselID">The ID of the vessel to track.</param>
        /// <param name="partsProcessed">Whether the vessel has been processed. Defaults to false.</param>
        /// <returns>The previous state of the vessel in the tracker.</returns>
        public bool TrackVessel(uint? vesselID, bool partsProcessed = false)
        {
            if (vesselID == null)
            {
                return false;
            }
            uint id = vesselID.GetValueOrDefault();
            bool originalState = false;
            tracker.TryGetValue(id, out originalState);
            tracker[id] = partsProcessed;

            Logging.Log($"Tracking vessel '{id}'. Original state: {originalState} New state: {partsProcessed}");

            return originalState;
        }

        /// <summary>
        /// Returns whether the given vessel has been processed.
        /// </summary>
        /// <param name="vesselID">The ID of the vessel to check.</param>
        /// <returns>True if processed, false if not or not tracked.</returns>
        public bool IsProcessed(uint? vesselID)
        {
            if (vesselID == null)
            {
                return false;
            }
            bool processed = false;
            tracker.TryGetValue(vesselID.GetValueOrDefault(), out processed);

            return processed;
        }

        /// <summary>
        /// Gets whether the vessel has been processed, or adds it to the tracker with the provided state
        /// </summary>
        /// <param name="vesselID">The vessel to check or add</param>
        /// <param name="partsProcessed">The value to store if not present</param>
        /// <returns>The value if in the tracker, or the provided value if added.</returns>
        public bool GetOrAdd(uint? vesselID, bool partsProcessed = false)
        {
            if (vesselID == null)
            {
                return false;
            }
            uint id = vesselID.GetValueOrDefault();
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
        public bool Remove(uint? vesselID)
        {
            bool status = tracker.Remove(vesselID.GetValueOrDefault());

            if (status)
            {
                Logging.Log($"Removing tracking of vessel '{vesselID}'.");
            }
            return status;
        }

        /// <summary>
        /// Returns a ConfigNode representing the current state, or sets the state from a ConfigNode
        /// </summary>
        public ConfigNode State
        {
            get
            {
                ConfigNode node = new ConfigNode("VesselTracker");
                foreach (KeyValuePair<uint, bool> kvp in tracker)
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
                        if (uint.TryParse(val.name, out uint id))
                        {
                            bool tracked;
                            if (bool.TryParse(val.value, out tracked))
                            {
                                tracker[id] = tracked;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("Exception while assigning VesselTracker", Logging.LogType.ERROR);
                    Logging.LogException(ex);
                }
            }
        }
    }
}
