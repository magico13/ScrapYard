using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ScrapYard
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT })]
    class ScrapYard : ScenarioModule
    {
        public static ScrapYard Instance { get; private set; }
        public PartInventory TheInventory { get; } = new PartInventory();
        public GlobalSettings Settings { get; } = new GlobalSettings();
        public VesselTracker ProcessedTracker { get; } = new VesselTracker();
        public PartTracker PartTracker { get; } = new PartTracker();


        public bool EditorVerificationRequired { get; set; }
        void Start()
        {
            Logging.DebugLog("Start Start");
            Instance = this;

            InvokeRepeating("VerifyEditor", 1f, 1f);

            //load settings
            Settings.LoadSettings();
            EventListeners.Instance.RegisterListeners();
            if (HighLogic.LoadedSceneIsEditor)
            {
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
        public UI.InventoryApplyUI ApplyInventoryUI { get; } = new UI.InventoryApplyUI();
        public UI.InstanceSelectorUI InstanceSelectorUI { get; } = new UI.InstanceSelectorUI();

        private void OnGUI()
        {
            ApplyInventoryUI.OnGUIHandler();
            InstanceSelectorUI.OnGUIHandler();
        }
        #endregion GUI Code

        private void VerifyEditor()
        {
            if (EditorVerificationRequired)
            {
                try
                {
                    if (Settings.AutoApplyInventory && EditorLogic.fetch?.ship?.Count > 0)
                    {
                        Utilities.InventoryManagement.ApplyInventoryToVessel(EditorLogic.fetch.ship.parts);
                    }
                    Utilities.EditorHandling.VerifyEditorShip();
                    Utilities.EditorHandling.UpdateEditorCost();
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
