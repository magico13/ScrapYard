using ScrapYard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.UI
{
    public class InventoryApplyVM
    {
        public bool AutoApplyInventory
        {
            get
            {
                return ScrapYard.Instance.Settings.AutoApplyInventory;
            }
            set
            {
                ScrapYard.Instance.Settings.AutoApplyInventory = value;
            }
        }

        public void ApplyInventoryToEditorVessel()
        {
            if (EditorLogic.fetch != null && EditorLogic.fetch.ship != null && EditorLogic.fetch.ship.Parts.Any())
            {
                InventoryManagement.ApplyInventoryToVessel(EditorLogic.fetch.ship.Parts);
            }
        }
    }
}
