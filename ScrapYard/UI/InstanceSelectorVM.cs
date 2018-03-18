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
        private const string DEFAULT_PART_TITLE = "No Part Active";
        private Part _cachedBasePart;
        private Part _cachedApplyPart;
        private PartInventory _cachedInventory;
        private bool _shouldSell;

        private static KFSMEventCondition on_partDropped_Backup = null;
        private static KFSMState st_place_Backup = null;

        public Part BasePart
        {
            get { return _cachedBasePart; }
            set
            {
                _cachedBasePart = value;
                BackingPartName = value?.partInfo.name;
                SelectedPartName = value?.partInfo.title ?? DEFAULT_PART_TITLE;
                UpdatePartList();
            }
        }
        public Part ApplyPart
        {
            get { return _cachedApplyPart; } private set { _cachedApplyPart = value; }
        }

        public string BackingPartName { get; set; }

        public string SelectedPartName { get; set; } = DEFAULT_PART_TITLE;

        public List<List<PartInstance>> Parts { get; set; }

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
            UpdatePartList(_cachedInventory, BackingPartName, _shouldSell);
        }

        public void UpdatePartList(PartInventory inventory, string partName, bool selling)
        {
            Parts = new List<List<PartInstance>>();
            if (partName == null)
            {
                return;
            }
            IEnumerable<InventoryPart> foundParts;
            foundParts = inventory?.FindPartsByName(partName) ?? new List<InventoryPart>();
            foundParts = EditorHandling.FilterOutUsedParts(foundParts);

            foreach (InventoryPart iPart in foundParts)
            {
                PartInstance instance = new PartInstance(inventory, iPart, selling, ApplyPart);
                instance.Updated += Instance_Updated;
                List<PartInstance> list = Parts.FirstOrDefault(l => l.FirstOrDefault()?.BackingPart.IsSameAs(iPart, ComparisonStrength.TRACKER) == true);
                if (list == null)
                {
                    list = new List<PartInstance>();
                    Parts.Add(list);
                }
                list.Add(instance);
            }

            //sort by number of uses
            Parts.Sort(leastToMostSorter);
        }

        public void RefreshApplyPart()
        {
            if (ApplyPart != null)
            {
                ApplyPart.Modules.GetModule<ModuleSYPartTracker>()?.MakeFresh();
                ScrapYardEvents.OnSYInventoryAppliedToPart.Fire(ApplyPart);
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
            EditorLogic.fetch?.Lock(true, true, true, "ScrapYard_EditorLock");   
        }

        public void OnMouseExit()
        {
            //remove a lock
            EditorLogic.fetch?.Unlock("ScrapYard_EditorLock");
        }

        private void Instance_Updated(object sender, EventArgs e)
        {
            UpdatePartList();   
        }


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
                ScrapYard.Instance.EditorVerificationRequired = true;
            }
        }

        /// <summary>
        /// Forcibly disables the ability to drop parts in the editor
        /// </summary>
        public void DisablePartDropping()
        {
            if (on_partDropped_Backup == null)
            {
                KFSMEvent on_partDropped = EditorLogic.fetch.GetType().GetField("on_partDropped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy)?.GetValue(EditorLogic.fetch) as KFSMEvent;
                if (on_partDropped != null)
                {
                    on_partDropped_Backup = on_partDropped.OnCheckCondition;
                    on_partDropped.OnCheckCondition = (s) => false;
                    Logging.DebugLog("Disabled on_partDropped");
                }
            }
            if (st_place_Backup == null)
            {
                KFSMState st_place = EditorLogic.fetch.GetType().GetField("st_place", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy)?.GetValue(EditorLogic.fetch) as KFSMState;
                if (st_place != null)
                {
                    st_place_Backup = st_place;
                    EditorLogic.fetch.GetType().GetField("st_place", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy)?.SetValue(EditorLogic.fetch, null);
                    Logging.DebugLog("Disabled st_place");
                }
            }
        }

        /// <summary>
        /// Forcibly restores the ability to drop parts in the editor
        /// </summary>
        public void RestorePartDropping()
        {
            if (st_place_Backup != null)
            {
                EditorLogic.fetch.GetType().GetField("st_place", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy)?.SetValue(EditorLogic.fetch, st_place_Backup);
                st_place_Backup = null;
                Logging.DebugLog("Restored st_place");
            }
            if (on_partDropped_Backup != null)
            {
                KFSMEvent on_partDropped = EditorLogic.fetch.GetType().GetField("on_partDropped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy)?.GetValue(EditorLogic.fetch) as KFSMEvent;
                if (on_partDropped != null)
                {
                    on_partDropped.OnCheckCondition = on_partDropped_Backup;
                    on_partDropped_Backup = null;
                    Logging.DebugLog("Restored on_partDropped");
                    
                }
                
            }
        }


        /// <summary>
        /// Sorts the part instances from least to most uses
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>-1 if a is used less, 0 if equal, 1 if a is used more</returns>
        private int leastToMostSorter(List<PartInstance> a, List<PartInstance> b)
        {
            int? usedA = a?.FirstOrDefault()?.BackingPart?.TrackerModule?.TimesRecovered;
            int? usedB = b?.FirstOrDefault()?.BackingPart?.TrackerModule?.TimesRecovered;
            if (usedA == null)
            {
                if (usedB != null)
                {
                    return 1;
                }
                return 0;
            }
            if (usedB == null)
            {
                return -1;
            }
            return usedA.Value.CompareTo(usedB.Value);
        }
    }
}
