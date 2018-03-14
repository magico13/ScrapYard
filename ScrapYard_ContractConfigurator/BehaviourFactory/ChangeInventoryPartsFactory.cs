using ContractConfigurator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard_ContractConfigurator
{
    public class ChangeInventoryPartsFactory : BehaviourFactory
    {
        protected List<ChangeInventoryPartsHandler> _partHandlers = new List<ChangeInventoryPartsHandler>();

        public override bool Load(ConfigNode configNode)
        {
            bool valid = base.Load(configNode);

            foreach (ConfigNode handler in configNode.GetNodes("PARTS"))
            {
                _partHandlers.Add(new ChangeInventoryPartsHandler(handler));
            }

            if (!_partHandlers.Any())
            {
                valid = false;
                LoggingUtil.LogWarning(this, "Must include at least one PART node!");
            }

            return valid;
        }

        public override ContractBehaviour Generate(ConfiguredContract contract)
        {
            return new ChangeInventoryParts(_partHandlers);
        }
    }
}
