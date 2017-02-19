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
    }
}
