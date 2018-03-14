using ContractConfigurator;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrapYard;

namespace ScrapYard_ContractConfigurator
{
    public class ChangeInventoryParts : ContractBehaviour
    {
        protected List<ChangeInventoryPartsHandler> _handlers;

        public ChangeInventoryParts() { }

        public ChangeInventoryParts(List<ChangeInventoryPartsHandler> handlers)
        {
            _handlers = handlers;
        }

        protected override void OnAccepted() { ChangeParts(Condition.ACCEPTED); }
        protected override void OnCancelled() { ChangeParts(Condition.CANCELLED); }
        protected override void OnCompleted() { ChangeParts(Condition.SUCCEEDED); }
        protected override void OnDeadlineExpired() { ChangeParts(Condition.EXPIRED); }
        protected override void OnDeclined() { }
        protected override void OnFailed() { ChangeParts(Condition.FAILED); }
        protected override void OnFinished() { ChangeParts(Condition.FINISHED); }
        protected override void OnGenerateFailed() { }
        protected override void OnOffered() { }
        protected override void OnOfferExpired() { }
        protected override void OnParameterStateChange(ContractParameter param) { }
        protected override void OnRegister() { }
        protected override void OnUnregister() { }
        protected override void OnUpdate() { }
        protected override void OnWithdrawn() { }
        protected override void OnLoad(ConfigNode configNode)
        {
            _handlers = new List<ChangeInventoryPartsHandler>();
            foreach (ConfigNode handler in configNode.GetNodes("PARTS"))
            {
                _handlers.Add(new ChangeInventoryPartsHandler(handler));
            }
        }

        protected override void OnSave(ConfigNode configNode)
        {
            foreach (ChangeInventoryPartsHandler handler in _handlers)
            {
                configNode.AddNode(handler.AsConfigNode());
            }
        }

        protected void ChangeParts(Condition currentCondition)
        {
            foreach (ChangeInventoryPartsHandler handler in _handlers)
            {
                if (handler.TriggerCondition == currentCondition) //if it's the right condition, do the thing
                {
                    foreach (IntermediateInventoryPart iip in handler.InventoryParts)
                    {
                        foreach (InventoryPart ip in iip.ToInventoryParts())
                        {
                            if (handler.Adding)
                            {
                                ip.TrackerModule.TimesRecovered = Math.Max(0, ip.TrackerModule.TimesRecovered);
                                ScrapYard.ScrapYard.Instance.TheInventory.AddPart(ip);
                            }
                            else
                            {
                                ComparisonStrength compareStrength = ip.TrackerModule.TimesRecovered < 0 ? ComparisonStrength.MODULES : ComparisonStrength.TRACKER; //if negative, just care about modules. If 0+ then be strict
                                ScrapYard.ScrapYard.Instance.TheInventory.RemovePart(ip, compareStrength);
                            }
                        }
                    }
                }
            }
        }
    }
}
