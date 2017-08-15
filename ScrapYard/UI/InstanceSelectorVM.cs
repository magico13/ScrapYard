using ScrapYard.Modules;
using ScrapYard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.UI
{
    public class InstanceSelectorVM
    {
        private Part _cachedBasePart;
        private Part _cachedApplyPart;
        private PartInventory _cachedInventory;
        private InventoryPart _cachedInventoryPart;
        private bool _shouldSell;

        public Part BasePart
        {
            get { return _cachedBasePart; }
            set
            {
                _cachedBasePart = value;
                BackingInventoryPart = value != null ? new InventoryPart(value) : null;
            }
        }
        public Part ApplyPart
        {
            get { return _cachedApplyPart; } private set { _cachedApplyPart = value; }
        }

        public InventoryPart BackingInventoryPart
        {
            get { return _cachedInventoryPart; }
            set
            {
                _cachedInventoryPart = value;
                SelectedPartName = "A Part";
                if (!string.IsNullOrEmpty(_cachedInventoryPart?.Name))
                {
                    SelectedPartName = Utils.AvailablePartFromName(_cachedInventoryPart.Name).title;
                }
                UpdatePartList();
            }
        }

        public string SelectedPartName { get; set; } = "A Part";

        public List<PartInstance> Parts { get; set; }

        public InstanceSelectorVM(PartInventory inventory, Part basePart, Part applyToPart, bool shouldSell)
        {
            _cachedInventory = inventory;
            _shouldSell = shouldSell;
            ApplyPart = (applyToPart == basePart) ? applyToPart : null;
            BasePart = basePart;
        }

        public InstanceSelectorVM()
        {
            _cachedInventory = ScrapYard.Instance.TheInventory;
            _shouldSell = ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds;
            ApplyPart = null;
            BasePart = EditorLogic.SelectedPart;
        }

        public void UpdatePartList()
        {
            UpdatePartList(_cachedInventory, _cachedInventoryPart, _shouldSell);
        }

        public void UpdatePartList(PartInventory inventory, InventoryPart partBase, bool selling)
        {
            Parts = new List<PartInstance>();
            if (partBase == null)
            {
                return;
            }
            IEnumerable<InventoryPart> foundParts;
            foundParts = inventory?.FindParts(partBase, ComparisonStrength.NAME);
            //if (partBase != null)
            //{
                
            //}
            //else
            //{
            //    foundParts = inventory?.GetAllParts();
            //}

            foundParts = foundParts ?? new List<InventoryPart>();

            foundParts = EditorHandling.FilterOutUsedParts(foundParts);

            foreach (InventoryPart iPart in foundParts)
            {
                PartInstance instance = new PartInstance(inventory, iPart, selling, ApplyPart);
                instance.Updated += Instance_Updated;
                Parts.Add(instance);
            }
        }

        public void RefreshApplyPart()
        {
            if (ApplyPart != null)
            {
                ApplyPart.Modules.GetModule<ModuleSYPartTracker>()?.MakeFresh();
                UpdatePartList();
            }
        }

        public void SpawnNewPart()
        {
            if (BasePart != null)
            {
                EditorLogic.fetch.SpawnPart(BasePart.partInfo);
            }
        }

        public void OnMouseOver()
        {
            //set a lock
            //EditorLogic.fetch?.Lock(true, true, true, "ScrapYard_EditorLock");   
        }

        public void OnMouseExit()
        {
            //remove a lock
            //EditorLogic.fetch?.Unlock("ScrapYard_EditorLock");
        }

        public void PutPartInEditorHand(Part p)
        {
            System.Reflection.MethodInfo picker = typeof(EditorLogic).GetMethod("pickPart", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
            object result = picker?.Invoke(EditorLogic.fetch, new object[] { LayerUtil.DefaultEquivalent | 4 | 2097152, false, false });
            Logging.DebugLog(result);
        }

        private void Instance_Updated(object sender, EventArgs e)
        {
            UpdatePartList();   
        }
    }
}
