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
        public Settings Settings { get; } = new Settings();
        public VesselTracker ProcessedTracker { get; } = new VesselTracker();
        public PartTracker PartTracker { get; } = new PartTracker();

        private void Awake()
        {
            Logging.DebugLog("Awake Start");
            Instance = this;
            ScrapYardEvents.Initialize();
            Logging.DebugLog("Awake Complete");
        }

        void Start()
        {
            Logging.DebugLog("Start Start");
            EventListeners.Instance.RegisterListeners();
            Logging.DebugLog("Start Complete");
        }

        void OnDestroy()
        {
            Logging.DebugLog("OnDestroy Start");
            EventListeners.Instance.DeregisterListeners();
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

        private void OnGUI()
        {
            ApplyInventoryUI.OnGUIHandler();
        }
        #endregion GUI Code
    }
}
