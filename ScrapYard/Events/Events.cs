using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public static class ScrapYardEvents
    {
        public static EventData<InventoryPart, bool> OnSYInventoryChanged;// = new EventData<InventoryPart, bool>("OnSYInventoryChanged");
        public static EventVoid OnSYInventoryAppliedToVessel;// = new EventVoid("OnSYInventoryAppliedToVessel");
        public static EventData<IEnumerable<InventoryPart>> OnSYTrackerUpdated;
        private static bool initialized;


        public static void Initialize()
        {
            if (!initialized)
            {
                Logging.DebugLog("Initializing Events.");
                OnSYInventoryChanged = new EventData<InventoryPart, bool>("OnSYInventoryChanged");
                OnSYInventoryAppliedToVessel = new EventVoid("OnSYInventoryAppliedToVessel");
                OnSYTrackerUpdated = new EventData<IEnumerable<InventoryPart>>("OnSYTrackerUpdated");

                initialized = true;
                Logging.DebugLog("Events Initialized.");
            }
        }
    }
}
