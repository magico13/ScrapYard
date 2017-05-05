using KSP.UI.Screens;
using KSP.UI.Screens.SpaceCenter.MissionSummaryDialog;
using ScrapYard.Modules;
using ScrapYard.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ScrapYard
{
    public class EventListeners
    {
        public static EventListeners Instance { get; } = new EventListeners();

        public ApplicationLauncherButton Button { get; set; }

        public void RegisterListeners()
        {
            if (ScrapYard.Instance.Settings.EnabledForSave)
            {
                GameEvents.onVesselRecovered.Add(VesselRecovered);
                GameEvents.OnVesselRollout.Add(VesselRolloutEvent);
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                GameEvents.onGUIApplicationLauncherUnreadifying.Add(OnGUIAppLauncherUnReadying);
                GameEvents.onEditorShipModified.Add(OnEditorShipModified);

                ScrapYardEvents.OnSYInventoryChanged.Add(InventoryChangedEventListener);

                Logging.DebugLog("Event Listeners Registered!");
            }
        }

        public void DeregisterListeners()
        {
            //GameEvents.onVesselRecoveryProcessing.Remove(VesselRecoverProcessingEvent);
            GameEvents.OnVesselRollout.Remove(VesselRolloutEvent);
            //GameEvents.onGUIRecoveryDialogSpawn.Remove(RecoveryDialogSpawn);
            GameEvents.onVesselRecovered.Remove(VesselRecovered);
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherUnreadifying.Remove(OnGUIAppLauncherUnReadying);
            GameEvents.onEditorShipModified.Remove(OnEditorShipModified);

            ScrapYardEvents.OnSYInventoryChanged.Remove(InventoryChangedEventListener);

            Logging.DebugLog("Event Listeners De-Registered!");
        }

        public void InventoryChangedEventListener(InventoryPart p, bool added)
        {
            //if removed then check if we should remove a corresponding part from the EditorLogic vessel
            if (!added)
            {
                if (EditorLogic.fetch?.ship?.Parts?.Count > 0)
                {
                    //see if this part is on it, if so, overwrite it with a new one
                    foreach (Part part in EditorLogic.fetch.ship.Parts)
                    {
                        InventoryPart shipPart = new InventoryPart(part);
                        if (p.ID == shipPart.ID)
                        {
                            ModuleSYPartTracker module = part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker;
                            module.MakeFresh();
                            break; //There can only be one part with this ID
                        }
                    }
                }
            }

            Logging.DebugLog($"InventoryChangedEvent - part: {p.Name} - added: {added}");
        }

        public void VesselRecovered(ProtoVessel vessel, bool someBool)
        {
            if (!ScrapYard.Instance.Settings.EnabledForSave)
            {
                return;
            }
            Logging.DebugLog("Recovered");
            foreach (ProtoPartSnapshot pps in vessel.protoPartSnapshots)
            {
                InventoryPart recoveredPart = new InventoryPart(pps);
                recoveredPart.TrackerModule.TimesRecovered++;
                ScrapYard.Instance.TheInventory.AddPart(recoveredPart);
                if (HighLogic.CurrentGame.Parameters.CustomParams<SaveSpecificSettings>().OverrideFunds)
                {
                    Funding.Instance?.AddFunds(-1 * recoveredPart.DryCost, TransactionReasons.VesselRecovery);
                }
            }
        }

        public void VesselRolloutEvent(ShipConstruct vessel)
        {
            if (!ScrapYard.Instance.Settings.EnabledForSave)
            {
                return;
            }
            Logging.DebugLog("Vessel Rollout!");

            //If vessel not processed, then take parts
            //If already processed, just return

            if (ScrapYard.Instance.ProcessedTracker.Remove(Utils.StringToGuid(
                vessel.Parts[0].Modules.GetModule<ModuleSYPartTracker>()?.ID)))
            {
                return;
            }

            InventoryManagement.RemovePartsFromInventory(vessel.Parts);
            ScrapYard.Instance.PartTracker.AddBuild(vessel.Parts);
        }

        public void OnGUIAppLauncherReady()
        {
            if (!ScrapYard.Instance.Settings.EnabledForSave)
            {
                return;
            }
            bool vis;
            if (ApplicationLauncher.Ready && (Button == null || !ApplicationLauncher.Instance.Contains(Button, out vis))) //Add Stock button
            {
                Button = ApplicationLauncher.Instance.AddModApplication(
                    () => { ScrapYard.Instance.ApplyInventoryUI.Show(); },
                    () => { ScrapYard.Instance.ApplyInventoryUI.Close(); },
                    null,
                    null,
                    null,
                    null,
                    (ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB),
                    GameDatabase.Instance.GetTexture("ScrapYard/icon", false));
            }
        }

        public void OnGUIAppLauncherUnReadying(GameScenes scene)
        {
            if (Button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(Button);
            }
        }

        public void OnEditorShipModified(ShipConstruct ship)
        {
            ScrapYard.Instance.EditorVerificationRequired = true;
        }
    }
}
