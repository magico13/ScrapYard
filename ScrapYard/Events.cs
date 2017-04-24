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
        private static bool initialized;


        public static void Initialize()
        {
            if (!initialized)
            {
                Logging.DebugLog("Initializing Events.");
                OnSYInventoryChanged = new EventData<InventoryPart, bool>("OnSYInventoryChanged");
                OnSYInventoryAppliedToVessel = new EventVoid("OnSYInventoryAppliedToVessel");

                initialized = true;
                Logging.DebugLog("Events Initialized.");
            }
        }
    }
}
