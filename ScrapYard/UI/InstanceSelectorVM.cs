using ScrapYard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.UI
{
    public class InstanceSelectorVM
    {
        private PartInventory _cachedInventory;
        private InventoryPart _cachedPart;
        private bool _shouldSell;

        public InstanceSelectorVM()
        {
            _cachedInventory = ScrapYard.Instance.TheInventory;
            _cachedPart = null;
            _shouldSell = ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds;
            UpdatePartList();
        }


        public string SelectedPartName { get; set; } = "A Part";

        public List<PartInstance> Parts { get; set; }


        public void UpdatePartList()
        {
            UpdatePartList(_cachedInventory, _cachedPart, _shouldSell);
        }

        public void UpdatePartList(PartInventory inventory, InventoryPart partBase, bool selling)
        {
            Parts = new List<PartInstance>();
            IEnumerable<InventoryPart> foundParts;
            if (partBase != null)
            {
                foundParts = inventory?.FindParts(partBase, ComparisonStrength.NAME);
            }
            else
            {
                foundParts = inventory?.GetAllParts();
            }

            foundParts = foundParts ?? new List<InventoryPart>();

            foundParts = EditorHandling.FilterOutUsedParts(foundParts);

            foreach (InventoryPart iPart in foundParts)
            {
                PartInstance instance = new PartInstance(inventory, iPart, selling);
                instance.Updated += Instance_Updated;
                Parts.Add(instance);
            }
        }

        private void Instance_Updated(object sender, EventArgs e)
        {
            UpdatePartList();   
        }
    }
}
