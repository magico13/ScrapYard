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


        #region Vessel Processing
        /// <summary>
        /// Removes inventory parts, refunds funds, marks it as tracked
        /// </summary>
        /// <param name="parts">The vessel as a List of Parts</param>
        /// <returns>True if processed, false otherwise</returns>
        public bool ProcessVessel(List<Part> parts)
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

            //have ID, can now apply inventory
            ScrapYard.Instance.TheInventory.ApplyInventoryToVessel(parts);

            //Mark as processed
            ScrapYard.Instance.ProcessedTracker.TrackVessel(ID, true);

            return true;

        }

        /// <summary>
        /// Removes inventory parts, refunds funds, marks it as tracked
        /// </summary>
        /// <param name="parts">The vessel as a List of Parts</param>
        /// <returns>True if processed, false otherwise</returns>
        public bool ProcessVessel(List<ConfigNode> partNodes)
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

            //have ID, can now apply inventory
            ScrapYard.Instance.TheInventory.ApplyInventoryToVessel(partNodes);

            //Mark as processed
            ScrapYard.Instance.ProcessedTracker.TrackVessel(ID, true);

            return true;

        }
        #endregion
    }
}
