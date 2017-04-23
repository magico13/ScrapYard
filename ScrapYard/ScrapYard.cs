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
        void Start()
        {
            Instance = this;
            
            EventListeners.Instance.RegisterListeners();
        }

        void OnDestroy()
        {
            EventListeners.Instance.DeregisterListeners();
        }

        public override void OnLoad(ConfigNode node)
        {
            Logging.DebugLog("OnLoad");
            base.OnLoad(node);

            TheInventory.State = node.GetNode("PartInventory");
            PartTracker.State = node.GetNode("PartTracker");
            ProcessedTracker.State = node.GetNode("VesselTracker");
            //load settings?
            
        }

        public override void OnSave(ConfigNode node)
        {
            Logging.DebugLog("OnSave");
            base.OnSave(node);

            node.AddNode(TheInventory.State);
            node.AddNode(PartTracker.State);
            node.AddNode(ProcessedTracker.State);
            //save settings?
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
