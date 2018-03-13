using ContractConfigurator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard_ContractConfigurator
{
    public class ChangeInventoryPartsFactory : BehaviourFactory
    {
        protected bool _adding = false;
        protected List<ConfigNode> _partNodes = new List<ConfigNode>();

        public override bool Load(ConfigNode configNode)
        {
            bool valid = base.Load(configNode);

            if (!configNode.TryGetValue("ShouldAdd", ref _adding)) //whether to Add the parts (true) or Remove the parts (false)
            {
                valid = false;
            }
            
            foreach (ConfigNode node in configNode.GetNodes("ScrapYard.InventoryPart"))
            {
                _partNodes.Add(node);
            }

            if (!_partNodes.Any())
            {
                LoggingUtil.LogError(this, "Must include at least on ScrapYard.InventoryPart node!");
                valid = false;
            }

            return valid;
        }

        public override ContractBehaviour Generate(ConfiguredContract contract)
        {
            return new ChangeInventoryParts(_adding, _partNodes);
        }
    }
}
