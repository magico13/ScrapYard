using ScrapYard.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace ScrapYard.Utilities
{
    public static class EditorHandling
    {
        private static float costCache = 0;


        /// <summary>
        /// Verifies that the inventory parts on the ship in the editor are valid
        /// </summary>
        public static void VerifyEditorShip()
        {
            //make a copy of the inventory
            PartInventory copy;
            using (Logging.Timer.StartNew("Copy"))
            {
                copy = ScrapYard.Instance.TheInventory.Copy();
            }
            using (Logging.Timer.StartNew("Check Parts"))
            {
                long constTime = 0;
                long removeTime = 0;
                long findTime = 0;
                //long freshTime = 0;

                //foreach (Part part in EditorLogic.fetch?.ship?.Parts ?? new List<Part>())
                List<InventoryPart> editorParts = null;
                using (Logging.Timer.StartNew("Convert To IParts"))
                {
                    editorParts = EditorLogic.fetch?.ship?.Parts?.Select(p => new InventoryPart(p))?.ToList();
                }
                if (editorParts != null)
                {
                    for (int i = 0; i < editorParts.Count; i++)
                    {
                        Stopwatch constWatch = Stopwatch.StartNew();
                        InventoryPart iPart = editorParts[i];//new InventoryPart(part);
                        constTime += constWatch.ElapsedMilliseconds;
                        //Stopwatch freshWatch = Stopwatch.StartNew();
                        //if (iPart.ID == null)
                        //{
                        //    (EditorLogic.fetch.ship.Parts[i].Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                        //}
                        //freshTime += freshWatch.ElapsedMilliseconds;
                        if (iPart.TrackerModule.Inventoried)
                        {
                            Stopwatch remWatch = Stopwatch.StartNew();
                            InventoryPart inInventory = copy.RemovePart(iPart, ComparisonStrength.STRICT); //strict, we only remove parts that are exact
                            if (inInventory == null)
                            {
                                //reset their tracker status
                                Logging.Log($"Found inventory part on vessel that is not in inventory. Resetting. {iPart.Name}:{iPart.ID}");
                                (EditorLogic.fetch.ship.Parts[i].Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                            }
                            removeTime += remWatch.ElapsedMilliseconds;
                        }
                        else
                        {
                            //check that we're not sharing an ID with something in the inventory
                            Stopwatch findWatch = Stopwatch.StartNew();
                            InventoryPart inInventory = copy.FindPart(iPart.ID);
                            if (inInventory != null)
                            {
                                //found a part that is sharing an ID but shouldn't be
                                Logging.Log($"Found part on vessel with same ID as inventory part, but not matching. Resetting. {iPart.Name}:{iPart.ID}");
                                (EditorLogic.fetch.ship.Parts[i].Modules["ModuleSYPartTracker"] as ModuleSYPartTracker).MakeFresh();
                            }
                            
                            findTime += findWatch.ElapsedMilliseconds;
                        }
                    }
                }
                Logging.DebugLog($"Constructor: {constTime}");
                Logging.DebugLog($"Removal: {removeTime}");
                Logging.DebugLog($"Finding: {findTime}");
                //Logging.Log($"Freshening: {freshTime}");
            }
            using (Logging.Timer.StartNew("Update Part List"))
            {
                //update the part list if visible
                if (ScrapYard.Instance.InstanceSelectorUI.IsVisible)
                {
                    ScrapYard.Instance.InstanceSelectorUI.InstanceVM?.UpdatePartList();
                }
            }
        }

        public static void UpdateEditorCost()
        {
            if (!ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds)
            {
                return;
            }
            float dry, fuel;
            float totalCost = EditorLogic.fetch.ship.GetShipCosts(out dry, out fuel);

            foreach (Part part in EditorLogic.fetch?.ship?.Parts ?? new List<Part>())
            {
                InventoryPart iPart = new InventoryPart(part);
                if (iPart.TrackerModule.Inventoried)
                {
                    totalCost -= iPart.DryCost;
                }
            }
            //set visible cost in editor UI
            UpdateCostUI(totalCost);
        }

        /// <summary>
        /// Updates the cost UI with the cached cost value
        /// </summary>
        public static void UpdateCostUI()
        {
            if (!ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds)
            {
                return;
            }
            CostWidget widget = UnityEngine.Object.FindObjectOfType<CostWidget>();
            if (widget != null)
            {
                MethodInfo costMethod = widget.GetType().GetMethod("onCostChange", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                costMethod?.Invoke(widget, new object[] { costCache });
            }
        }

        /// <summary>
        /// Updates the cost UI with the provided value and caches it
        /// </summary>
        /// <param name="cost">The new cost value</param>
        public static void UpdateCostUI(float cost)
        {
            costCache = cost;
            UpdateCostUI();
        }

        /// <summary>
        /// Takes a list of InventoryParts and removes any that are in use by the current vessel
        /// </summary>
        /// <param name="sourceList">The list of parts to search in</param>
        /// <returns>A List of parts that aren't being used</returns>
        public static IList<InventoryPart> FilterOutUsedParts(IEnumerable<InventoryPart> sourceList)
        {
            List<InventoryPart> retList = new List<InventoryPart>(sourceList);

            if (!HighLogic.LoadedSceneIsEditor)
            {
                return retList;
            }

            foreach (Part part in EditorLogic.fetch.ship)
            {
                InventoryPart iPart = new InventoryPart(part);
                InventoryPart found = retList.FirstOrDefault(ip => ip.IsSameAs(iPart, ComparisonStrength.STRICT));
                if (found != null)
                {
                    retList.Remove(found);
                }
            }

            if (EditorLogic.SelectedPart != null)
            {
                foreach (Part part in EditorLogic.FindPartsInChildren(EditorLogic.SelectedPart))
                {
                    InventoryPart iPart = new InventoryPart(part);
                    InventoryPart found = retList.FirstOrDefault(ip => ip.IsSameAs(iPart, ComparisonStrength.STRICT));
                    if (found != null)
                    {
                        retList.Remove(found);
                    }
                }
            }
            return retList;
        }

        public static void UpdateSelectionUI()
        {
            //either apply to the selected part, the part told to be updated by the UI, or spawn a new part if neither case met
            if (ScrapYard.Instance.InstanceSelectorUI.IsVisible)
            {
                if (EditorLogic.SelectedPart != null &&
                    ScrapYard.Instance.InstanceSelectorUI.InstanceVM.SelectedPartName != EditorLogic.SelectedPart.partName)
                {
                    ScrapYard.Instance.InstanceSelectorUI.Show(EditorLogic.SelectedPart, EditorLogic.SelectedPart);
                }
            }
        }
    }
}
