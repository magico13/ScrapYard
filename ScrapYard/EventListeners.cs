using KSP.UI.Screens;
using KSP.UI.Screens.SpaceCenter.MissionSummaryDialog;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ScrapYard
{
    public class EventListeners
    {
        private static EventListeners instance = new EventListeners();
        public static EventListeners Instance { get { return instance; } }

        private static MissionRecoveryDialog LastRecoveryUI = null;

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
            
            Events.InventoryChangedEvent.Add(InventoryChangedEventListener);

            Debug.Log("ScrapYard: Event Listeners Registered!");
            //end if
            }

        public void DeregisterListeners()
        {
            //GameEvents.onVesselRecoveryProcessing.Remove(VesselRecoverProcessingEvent);
            GameEvents.OnVesselRollout.Remove(VesselRolloutEvent);
            //GameEvents.onGUIRecoveryDialogSpawn.Remove(RecoveryDialogSpawn);
            GameEvents.onVesselRecovered.Remove(VesselRecovered);

            Events.InventoryChangedEvent.Remove(InventoryChangedEventListener);

            Debug.Log("ScrapYard: Event Listeners De-Registered!");
        }

        private void InventoryChangedEventListener(InventoryPart p, int o, int n)
        {
            Debug.Log($"InventoryChangedEvent - part: {p.Name} - old: {o} - new: {n}");
        }

        //private void RecoveryDialogSpawn(MissionRecoveryDialog dialog)
        //{
        //    Debug.Log("ScrapYard: dialog spawn");
        //    Debug.Log($"fm:{dialog.FundsModifier} - rf:{dialog.recoveryFactor} - fe:{dialog.fundsEarned}");
        //    object value = dialog.GetType().GetField("partWidgets", System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dialog);
        //    if (value == null)
        //        Debug.Log("Value is null :/");
        //    List<PartWidget> partWidgets = value as List<PartWidget>;
        //    if (partWidgets != null)
        //    {
        //        foreach (PartWidget widget in partWidgets)
        //        {
        //            //widget.partValue = widget.partValue * (dialog.recoveryFactor - 100) / 100.0;
        //            widget.partValue = 0;
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("Widgets were null :(");
        //    }
        //}

        //public void VesselRecoverProcessingEvent(ProtoVessel recovered, MissionRecoveryDialog dialog, float someNum)
        //{
        //    Debug.Log("ScrapYard: Vessel Recovery Processing!");
        //    Debug.Log($"fm:{dialog.FundsModifier} - rf:{dialog.recoveryFactor} - fe:{dialog.fundsEarned}");
        //    LastRecoveryUI = dialog;
        //    foreach (ProtoPartSnapshot pps in recovered.protoPartSnapshots)
        //    {
        //        InventoryPart recoveredPart = new InventoryPart(pps);
        //        ScrapYard.Instance.TheInventory.AddPart(recoveredPart, 1);
        //        Funding.Instance.AddFunds(-1*recoveredPart.DryCost, TransactionReasons.VesselRecovery);
        //        //dialog.fundsEarned -= recoveredPart.DryCost;

        //    }
        //    //Debug.Log($"Type is {(dialog.GetType().GetMember("partWidgets", BindingFlags.NonPublic)?.GetValue(0) as MemberInfo)?.MemberType ?? default(MemberTypes) }");
        //    object value = dialog.GetType().GetField("partWidgets", System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dialog);
        //    if (value == null)
        //        Debug.Log("Value is null :/");
        //    List<PartWidget> partWidgets = value as List<PartWidget>;
        //    if (partWidgets != null)
        //    {
        //        foreach (PartWidget widget in partWidgets)
        //        {
        //            //widget.partValue = widget.partValue * (dialog.recoveryFactor - 100) / 100.0;
        //            widget.partValue = 0;
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("Widgets were null :(");
        //    }
        //}

        public void VesselRecovered(ProtoVessel vessel, bool someBool)
        {
            Debug.Log("ScrapYard: Recovered");
            foreach (ProtoPartSnapshot pps in vessel.protoPartSnapshots)
            {
                InventoryPart recoveredPart = new InventoryPart(pps);
                ScrapYard.Instance.TheInventory.AddPart(recoveredPart, 1);
                if (ScrapYard.Instance.Settings.OverrideFunds)
                {
                    Funding.Instance.AddFunds(-1 * recoveredPart.DryCost, TransactionReasons.VesselRecovery);
                }
                //dialog.fundsEarned -= recoveredPart.DryCost;

            }
            //MissionRecoveryDialog dialog = LastRecoveryUI;
            //Debug.Log("ScrapYard: Recovered");
            //Debug.Log($"fm:{dialog.FundsModifier} - rf:{dialog.recoveryFactor} - fe:{dialog.fundsEarned}");
            //object value = dialog.GetType().GetField("partWidgets", System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dialog);
            //if (value == null)
            //    Debug.Log("Value is null :/");
            //List<PartWidget> partWidgets = value as List<PartWidget>;
            //if (partWidgets != null)
            //{
            //    foreach (PartWidget widget in partWidgets)
            //    {
            //        //widget.partValue = widget.partValue * (dialog.recoveryFactor - 100) / 100.0;
            //        widget.partValue = 0;
            //    }
            //}
            //else
            //{
            //    Debug.Log("Widgets were null :(");
            //}
        }

        public void VesselRolloutEvent(ShipConstruct vessel)
        {
            Debug.Log("ScrapYard: Vessel Rollout!");
            //List<InventoryPart> UniqueParts = new List<InventoryPart>(),
            List< InventoryPart > UsedParts = new List<InventoryPart>();

            foreach (Part part in vessel.parts)
            {
                InventoryPart inventoryPart = new InventoryPart(part);
                UsedParts.Add(inventoryPart);
            //    if (UniqueParts.Find(p => p.IdenticalTo(inventoryPart)) == null)
            //        UniqueParts.Add(inventoryPart);
            }

            //Increment the tracker
            //Logging.DebugLog("ScrapYard", "Incrementing the tracker.");
            /*foreach (InventoryPart part in UniqueParts)
            {
                ScrapYard.Instance.TheInventory.IncrementUsageCounter(part);
            }*/

            //Remove all possible inventory parts
            //Logging.DebugLog("ScrapYard", "Removing parts from inventory.");
            //Logging.DebugLog("ScrapYard", ScrapYard.instance.TheInventory.GetPartByIndex(0).Quantity);
            List<InventoryPart> inInventory, notInInventory;
            ScrapYard.Instance.TheInventory.SplitParts(UsedParts, out inInventory, out notInInventory);

            foreach (InventoryPart part in inInventory)
            {
                //Remove part
                //part.SetQuantity(-1);
                //Logging.DebugLog("ScrapYard", ScrapYard.instance.TheInventory.GetPartByIndex(0).Quantity);
                ScrapYard.Instance.TheInventory.AddPart(part, -1);

                //Refund its cost
                if (ScrapYard.Instance.Settings.OverrideFunds)
                {
                    Funding.Instance.AddFunds(part.DryCost, TransactionReasons.VesselRollout);
                }
            }
        }
    }
}
