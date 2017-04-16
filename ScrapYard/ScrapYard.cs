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
            //Logging.DebugLog(TAG, "Loading from persistence.");
            base.OnLoad(node);

            TheInventory.State = node.GetNode("PartInventory");
            PartTracker.State = node.GetNode("PartTracler");
            //load settings?
            //load vessel tracker
        }

        public override void OnSave(ConfigNode node)
        {
            //Logging.DebugLog(TAG, "Saving to persistence.");
            Logging.DebugLog("OnSave");
            base.OnSave(node);

            node.AddNode(TheInventory.State);
            node.AddNode(PartTracker.State);
            //save settings?
            //save vessel tracker
        }
    }
}
