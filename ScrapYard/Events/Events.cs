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
        public static EventData<Part> OnSYInventoryAppliedToPart;
        public static EventData<IEnumerable<InventoryPart>> OnSYTrackerUpdated;
        public static EventVoid OnSYReady;
        private static bool initialized;


        public static void Initialize()
        {
            if (!initialized)
            {
                Logging.DebugLog("Initializing Events.");
                OnSYInventoryChanged = new EventData<InventoryPart, bool>("OnSYInventoryChanged");
                OnSYInventoryAppliedToVessel = new EventVoid("OnSYInventoryAppliedToVessel");
                OnSYInventoryAppliedToPart = new EventData<Part>("OnSYInventoryAppliedToPart");
                OnSYTrackerUpdated = new EventData<IEnumerable<InventoryPart>>("OnSYTrackerUpdated");
                OnSYReady = new EventVoid("OnSYReady");

                initialized = true;
                Logging.DebugLog("Events Initialized.");
            }
        }
    }
}
