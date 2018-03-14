using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard_ContractConfigurator
{
    public enum Condition
    {
        SUCCEEDED,
        FAILED,
        FINISHED,
        ACCEPTED,
        CANCELLED,
        EXPIRED
    }

    public class ChangeInventoryPartsHandler
    {
        public bool Adding { get; set; } = true;

        public List<IntermediateInventoryPart> InventoryParts { get; set; } = new List<IntermediateInventoryPart>();

        public Condition TriggerCondition { get; set; } = Condition.SUCCEEDED;

        public ChangeInventoryPartsHandler() { }
        public ChangeInventoryPartsHandler(ConfigNode source)
        {
            FromConfigNode(source);
        }

        public void FromConfigNode(ConfigNode source)
        {
            if (bool.TryParse(source.GetValue("adding"), out bool adding))
            {
                Adding = adding;
            }
            else
            {
                Adding = true;
            }
            TriggerCondition = (Condition)Enum.Parse(typeof(Condition), source.GetValue("condition"), true);
            foreach (ConfigNode partNode in source.GetNodes("INVENTORY_PART"))
            {
                InventoryParts.Add(new IntermediateInventoryPart(partNode));
            }
        }

        public ConfigNode AsConfigNode()
        {
            ConfigNode node = new ConfigNode("PARTS");
            node.AddValue("condition", TriggerCondition);
            node.AddValue("adding", Adding);
            foreach (IntermediateInventoryPart part in InventoryParts)
            {
                node.AddNode(part.AsConfigNode());
            }

            return node;
        }
    }
}
