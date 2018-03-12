using ContractConfigurator;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrapYard;

namespace ScrapYard_ContractConfigurator
{
    public class ChangeInventoryPartsBehaviour : ContractBehaviour
    {
        protected bool _adding;
        protected List<ConfigNode> _parts;

        public ChangeInventoryPartsBehaviour(bool adding, List<ConfigNode> parts)
        {
            _adding = adding;
            _parts = parts;
        }

        protected override void OnAccepted() { }
        protected override void OnCancelled() { }
        protected override void OnCompleted() { }
        protected override void OnDeadlineExpired() { }
        protected override void OnDeclined() { }
        protected override void OnFailed() { }
        protected override void OnFinished() { }
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
            _adding = ConfigNodeUtil.ParseValue<bool>(configNode, "ShouldAdd");
            _parts = configNode.GetNodes("ScrapYard.InventoryPart").ToList();
        }

        protected override void OnSave(ConfigNode configNode)
        {
            configNode.AddValue("ShouldAdd", _adding);
            foreach (ConfigNode node in _parts)
            {
                configNode.AddNode(node);
            }
        }



        protected void ChangeParts()
        {
            foreach (ConfigNode node in _parts)
            {
                InventoryPart part = new InventoryPart() { State = node };
                if (_adding)
                {
                    ScrapYard.ScrapYard.Instance.TheInventory.AddPart(part);
                }
                else
                {
                    ScrapYard.ScrapYard.Instance.TheInventory.RemovePart(part, ComparisonStrength.STRICT);
                }
            }
        }
    }
}
