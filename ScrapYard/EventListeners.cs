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
        private static EventListeners instance = new EventListeners();
        public static EventListeners Instance { get { return instance; } }

        //private static MissionRecoveryDialog LastRecoveryUI = null;

        public void RegisterListeners()
        {
            //GameEvents.onVesselRecoveryProcessing.Add(VesselRecoverProcessingEvent);
            GameEvents.onVesselRecovered.Add(VesselRecovered);
            //if (KCT2Utils.fetch(KCT2)
            //On KCT Vessel Added To Build List -> remove parts
            //On KCT Vessel Build Complete -> Increment tracker
            //else
            GameEvents.OnVesselRollout.Add(VesselRolloutEvent);
            //GameEvents.onGUIRecoveryDialogSpawn.Add(RecoveryDialogSpawn);
            
            //For debugging
            Events.SYInventoryChanged.Add(InventoryChangedEventListener);

            Logging.DebugLog("Event Listeners Registered!");
            //end if
            }

        public void DeregisterListeners()
        {
            //GameEvents.onVesselRecoveryProcessing.Remove(VesselRecoverProcessingEvent);
            GameEvents.OnVesselRollout.Remove(VesselRolloutEvent);
            //GameEvents.onGUIRecoveryDialogSpawn.Remove(RecoveryDialogSpawn);
            GameEvents.onVesselRecovered.Remove(VesselRecovered);

            Events.SYInventoryChanged.Remove(InventoryChangedEventListener);

            Logging.DebugLog("Event Listeners De-Registered!");
        }

        private void InventoryChangedEventListener(InventoryPart p, bool added)
        {
            Logging.DebugLog($"InventoryChangedEvent - part: {p.Name} - added: {added}");
        }

        public void VesselRecovered(ProtoVessel vessel, bool someBool)
        {
            Logging.DebugLog("Recovered");
            foreach (ProtoPartSnapshot pps in vessel.protoPartSnapshots)
            {
                ProtoPartModuleSnapshot tracker;
                if ((tracker = pps.modules.Find(m => m.moduleName == "ModuleSYPartTracker")) != null)
                {
                    long timesRecovered = 0;
                    long.TryParse(tracker.moduleValues.GetValue("TimesRecovered"), out timesRecovered);
                    tracker.moduleValues.SetValue("TimesRecovered", timesRecovered + 1);
                }
                //else
                //{
                //    //add the module, no idea if this works at all
                //    //pps.modules.Add(new ProtoPartModuleSnapshot(new ModuleSYPartTracker() { TimesRecovered = 1 }));
                //}

                InventoryPart recoveredPart = new InventoryPart(pps);
                ScrapYard.Instance.TheInventory.AddPart(recoveredPart);
                if (ScrapYard.Instance.Settings.OverrideFunds)
                {
                    Funding.Instance.AddFunds(-1 * recoveredPart.DryCost, TransactionReasons.VesselRecovery);
                }
            }
        }

        public void VesselRolloutEvent(ShipConstruct vessel)
        {
            Logging.DebugLog("Vessel Rollout!");

            //If vessel not processed, then take parts
            //If already processed, just return

            if (ScrapYard.Instance.ProcessedTracker.Remove(Utils.StringToGuid(
                vessel.Parts[0].Modules.GetModule<ModuleSYPartTracker>()?.ID)))
            {
                return;
            }

            ScrapYard.Instance.TheInventory.ApplyInventoryToVessel(vessel.parts);
            ScrapYard.Instance.PartTracker.AddBuild(vessel.parts);

            //List<InventoryPart> UniqueParts = new List<InventoryPart>(),
            //List< InventoryPart > UsedParts = new List<InventoryPart>();

            //foreach (Part part in vessel.parts)
            //{
            //    InventoryPart inventoryPart = new InventoryPart(part);
            //    UsedParts.Add(inventoryPart);
            ////    if (UniqueParts.Find(p => p.IdenticalTo(inventoryPart)) == null)
            ////        UniqueParts.Add(inventoryPart);
            //}

            ////Increment the tracker
            ////Logging.DebugLog("ScrapYard", "Incrementing the tracker.");
            ///*foreach (InventoryPart part in UniqueParts)
            //{
            //    ScrapYard.Instance.TheInventory.IncrementUsageCounter(part);
            //}*/

            ////Remove all possible inventory parts
            ////Logging.DebugLog("ScrapYard", "Removing parts from inventory.");
            ////Logging.DebugLog("ScrapYard", ScrapYard.instance.TheInventory.GetPartByIndex(0).Quantity);
            //List<InventoryPart> inInventory, notInInventory;
            //ScrapYard.Instance.TheInventory.SplitParts(UsedParts, out inInventory, out notInInventory);

            //foreach (InventoryPart part in inInventory)
            //{
            //    //Remove part
            //    //part.SetQuantity(-1);
            //    //Logging.DebugLog("ScrapYard", ScrapYard.instance.TheInventory.GetPartByIndex(0).Quantity);
            //    ScrapYard.Instance.TheInventory.RemovePart(part);

            //    //Refund its cost
            //    if (ScrapYard.Instance.Settings.OverrideFunds)
            //    {
            //        Funding.Instance.AddFunds(part.DryCost, TransactionReasons.VesselRollout);
            //    }
            //}

            //ScrapYard.Instance.ProcessedTracker.TrackVessel(Utilities.Utils.StringToGuid(vessel.Parts[0].Modules.GetModule<ModuleSYPartTracker>()?.ID), true);
        }
    }
}
