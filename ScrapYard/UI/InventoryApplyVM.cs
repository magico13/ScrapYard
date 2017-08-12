using ScrapYard.Modules;
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
                if (value != AutoApplyInventory)
                {
                    ScrapYard.Instance.EditorVerificationRequired = true;
                    ScrapYard.Instance.Settings.AutoApplyInventory = value;
                }
            }
        }

        public void ApplyInventoryToEditorVessel()
        {
            if (EditorLogic.fetch != null && EditorLogic.fetch.ship != null && EditorLogic.fetch.ship.Parts.Any())
            {
                InventoryManagement.ApplyInventoryToVessel(EditorLogic.fetch.ship.Parts);
            }
        }

        public void MakeFresh()
        {
            if (EditorLogic.fetch != null && EditorLogic.fetch.ship != null && EditorLogic.fetch.ship.Parts.Any())
            {
                foreach (Part part in EditorLogic.fetch.ship)
                {
                    if (part.Modules.Contains("ModuleSYPartTracker"))
                    {
                        (part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                    }
                }
                ScrapYardEvents.OnSYInventoryAppliedToVessel.Fire();
            }
        }

        public void ToggleSelectorUI()
        {
            if (ScrapYard.Instance.InstanceSelectorUI.IsVisible)
            {
                ScrapYard.Instance.InstanceSelectorUI.Close();
            }
            else
            {
                ScrapYard.Instance.InstanceSelectorUI.Show();
            }
        }
    }
}
