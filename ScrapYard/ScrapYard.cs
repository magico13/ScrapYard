using ScrapYard.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ScrapYard
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT })]
    public class ScrapYard : ScenarioModule
    {
        public static ScrapYard Instance { get; private set; }
        public PartInventory TheInventory { get; } = new PartInventory();
        public GlobalSettings Settings { get; } = new GlobalSettings();
        public VesselTracker ProcessedTracker { get; } = new VesselTracker();
        public PartTracker PartTracker { get; } = new PartTracker();
        public PartCategoryFilter PartCategory { get; } = new PartCategoryFilter();

        public bool EditorVerificationRequired { get; set; }
        void Start()
        {
            Logging.DebugLog("Start Start");
            Instance = this;

            InvokeRepeating("VerifyEditor", Settings.CurrentSaveSettings.RefreshTime/10f, Settings.CurrentSaveSettings.RefreshTime/10f);

            //load settings
            Settings.LoadSettings();
            EventListeners.Instance.RegisterListeners();
            if (HighLogic.LoadedSceneIsEditor)
            {
                PartCategory.CreateInventoryPartCategory();
                EditorVerificationRequired = true;
            }

            ScrapYardEvents.OnSYReady.Fire();
            Logging.DebugLog("Start Complete");
        }

        void OnDestroy()
        {
            Logging.DebugLog("OnDestroy Start");
            EventListeners.Instance.DeregisterListeners();
            //Save settings
            Settings.SaveSettings();
            Logging.DebugLog("OnDestroy Complete");
        }

        public override void OnLoad(ConfigNode node)
        {
            Logging.DebugLog("OnLoad");
            base.OnLoad(node);

            TheInventory.State = node.GetNode("PartInventory");
            PartTracker.State = node.GetNode("PartTracker");
            ProcessedTracker.State = node.GetNode("VesselTracker");
            //load settings?
            Logging.DebugLog("OnLoad Complete");

        }

        public override void OnSave(ConfigNode node)
        {
            Logging.DebugLog("OnSave");
            base.OnSave(node);

            node.AddNode(TheInventory.State);
            node.AddNode(PartTracker.State);
            node.AddNode(ProcessedTracker.State);
            //save settings?
            Logging.DebugLog("OnSave Complete");
        }

        #region GUI Code
        public UI.InstanceSelectorUI InstanceSelectorUI { get; } = new UI.InstanceSelectorUI();
        public UI.InstanceModulesUI InstanceModulesUI { get; } = new UI.InstanceModulesUI();

        private void OnGUI()
        {
            InstanceSelectorUI.OnGUIHandler();
            InstanceModulesUI.OnGUIHandler();
        }
        #endregion GUI Code

        private void VerifyEditor()
        {
            if (EditorVerificationRequired)
            {
                try
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    long time = 0;
                    if (Settings.AutoApplyInventory && EditorLogic.fetch?.ship?.Count > 0)
                    {
                        Utilities.InventoryManagement.ApplyInventoryToVessel(EditorLogic.fetch.ship.parts);
                        Logging.DebugLog($"VerifyEditor.ApplyInventoryToVessel: {watch.ElapsedMilliseconds - time}ms");
                        time = watch.ElapsedMilliseconds;
                    }
                    Utilities.EditorHandling.VerifyEditorShip();
                    Logging.DebugLog($"VerifyEditor.VerifyEditorShip: {watch.ElapsedMilliseconds - time}ms");
                    time = watch.ElapsedMilliseconds;
                    Utilities.EditorHandling.UpdateEditorCost();
                    Logging.DebugLog($"VerifyEditor.UpdateEditorCost: {watch.ElapsedMilliseconds - time}ms");
                    time = watch.ElapsedMilliseconds;
                    Utilities.EditorHandling.UpdateSelectionUI();
                    Logging.DebugLog($"VerifyEditor.UpdateSelectionUI: {watch.ElapsedMilliseconds - time}ms");
                    time = watch.ElapsedMilliseconds;

                    Logging.DebugLog($"VerifyEditor: {watch.ElapsedMilliseconds}ms");
                }
                catch
                {
                    throw;
                }
                finally
                {
                    EditorVerificationRequired = false;
                }
            }
        }
    }
}
