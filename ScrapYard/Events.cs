using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    /*public class InventoryChangedEventArgs : EventArgs
    {
        public InventoryChangedEventArgs(InventoryPart partChanged, int oldAmount, int newAmount)
        {
            ChangedPart = partChanged;
            PreviousAmount = oldAmount;
            NewAmount = newAmount;
        }

        public InventoryPart ChangedPart { get; private set; }
        public int PreviousAmount { get; private set; }
        public int NewAmount { get; private set; }

        public int ChangedAmount { get { return NewAmount - PreviousAmount; } }
    }*/

    public static class Events
    {
        public static EventData<InventoryPart, int, int> InventoryChangedEvent = new EventData<InventoryPart, int, int>("InventoryChangedEvent");
    }
}
