using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public static class Events
    {
        public static EventData<InventoryPart, bool> SYInventoryChanged = new EventData<InventoryPart, bool>("SYInventoryChanged");
    }
}
