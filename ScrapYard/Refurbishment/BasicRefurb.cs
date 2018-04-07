using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.Refurbishment
{
    /// <summary>
    /// Basic Refurbishment resets some values on the module when the part is added to the inventory.
    /// Things like repacking a parachute, resetting a fairing's deployed state, etc
    /// </summary>
    public class BasicRefurb
    {
        public string ModuleName { get; set; }

        public List<ValueModifier> RefurbishmentOperations { get; set; } = new List<ValueModifier>();

        public BasicRefurb(ConfigNode source)
        {
            ModuleName = source.GetValue("Module");
            foreach (ConfigNode.Value val in source.values)
            {
                string[] split = val.name.Split(new char[] { '_' }, 1);
                string tree = split.Length > 1 ? split[1] : string.Empty;
                switch (split[0])
                {
                    case "addValue": RefurbishmentOperations.Add(new ValueAdd(tree, val.value)); break;
                    case "setValue": RefurbishmentOperations.Add(new ValueSet(tree, val.value, false)); break;
                    case "setOrAddValue": RefurbishmentOperations.Add(new ValueSet(tree, val.value, true)); break;
                    case "removeValue": RefurbishmentOperations.Add(new ValueRemove(tree, val.value)); break;
                    case "removeNode": RefurbishmentOperations.Add(new NodeRemove(tree, val.value)); break;
                    default: break;
                }
            }
        }

        public void Refurbish(InventoryPart part)
        {
            //check that the module is on the part, then process it
            IList<ConfigNode> modules = part.ListModules();
            for (int i=modules.Count-1; i>=0; i-- )
            {
                ConfigNode module = modules[i];
                if (string.Equals(module.GetValue("name"), ModuleName, StringComparison.Ordinal))
                {
                    //do refurbishment
                    for (int j=RefurbishmentOperations.Count-1; j>=0; j--)
                    {
                        RefurbishmentOperations[j].PerformOperation(module);
                    }
                }
            }
        }
    }


    public abstract class ValueModifier
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string[] Tree { get; set; }

        /// <summary>
        /// Breaks the full path into its tree and name. Uses periods, butI worry about that not always working
        /// </summary>
        /// <param name="fullPath"></param>
        public void BreakPath(string fullPath)
        {
            string[] split = fullPath.Split('.');
            if (split.Length > 1)
            {
                Tree = split.Take(split.Length - 1).ToArray();
            }
            else
            {
                Tree = new string[0];
            }
            Name = split.LastOrDefault();
        }

        public ConfigNode TraverseTree(ConfigNode moduleNode)
        {
            Queue<string> tempTree = new Queue<string>(Tree);
            ConfigNode current = moduleNode;
            while (tempTree.Any())
            {
                string currentName = tempTree.Dequeue();
                if (current.HasNode(currentName))
                {
                    current = current.GetNode(currentName);
                }
                else
                {
                    return null;
                }
            }
            return current;
        }

        /// <summary>
        /// Performs the operation on the passed in module
        /// </summary>
        /// <param name="moduleNode">The module to operate on</param>
        /// <returns>Success</returns>
        public abstract bool PerformOperation(ConfigNode moduleNode);
    }

    /// <summary>
    /// Adds a value to a ConfigNode
    /// </summary>
    public class ValueAdd : ValueModifier
    {
        public ValueAdd(string fullPath, string value)
        {
            Value = value;
            BreakPath(fullPath);
            Logging.DebugLog($"New {GetType().Name} with Name '{Name}' and value '{Value}'");
        }

        public override bool PerformOperation(ConfigNode moduleNode)
        {
            //traverse the tree, then add the new value
            ConfigNode current = TraverseTree(moduleNode);
            if (current != null)
            {
                current.AddValue(Name, Value);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Adds a node to a ConfigNode
    /// </summary>
    public class NodeAdd : ValueModifier
    {
        public NodeAdd(string fullPath, ConfigNode value)
        {
            Value = value;
            BreakPath(fullPath);
            Logging.DebugLog($"New {GetType().Name} with Name '{Name}' and node '{value.GetValue("name")}'");
        }

        public override bool PerformOperation(ConfigNode moduleNode)
        {
            //traverse the tree, then add the new value
            ConfigNode current = TraverseTree(moduleNode);
            if (current != null)
            {
                ConfigNode final = Value as ConfigNode;
                if (final != null)
                {
                    current.AddNode(Name, final);
                }
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Sets a value on a ConfigNode
    /// </summary>
    public class ValueSet : ValueModifier
    {
        public bool AddIfMissing { get; set; }

        public ValueSet(string fullPath, string value, bool addIfMissing)
        {
            Value = value;
            BreakPath(fullPath);
            AddIfMissing = addIfMissing;
            Logging.DebugLog($"New {GetType().Name} with Name '{Name}' and value '{Value}' and adding={addIfMissing}");
        }

        public override bool PerformOperation(ConfigNode moduleNode)
        {
            //traverse the tree, then set the new value
            ConfigNode current = TraverseTree(moduleNode);
            if (current != null)
            {
                current.SetValue(Name, Value.ToString(), AddIfMissing);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Removes a value from a ConfigNode
    /// </summary>
    public class ValueRemove : ValueModifier
    {
        public ValueRemove(string fullPath, string value)
        {
            string finalPath = fullPath;
            if (!string.IsNullOrEmpty(fullPath))
            {
                finalPath += ".";
            }
            finalPath += value;
            BreakPath(finalPath);
            Value = value;
            Logging.DebugLog($"New {GetType().Name} with Name '{Name}' and Value '{Value}'");
        }

        public override bool PerformOperation(ConfigNode moduleNode)
        {
            //traverse the tree, then add the new value
            ConfigNode current = TraverseTree(moduleNode);
            if (current != null)
            {
                current.RemoveValues(Value.ToString());
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Removes a node from a ConfigNode
    /// </summary>
    public class NodeRemove : ValueModifier
    {
        public NodeRemove(string fullPath, string value)
        {
            string finalPath = fullPath;
            if (!string.IsNullOrEmpty(fullPath))
            {
                finalPath += ".";
            }
            finalPath += value;
            BreakPath(finalPath);
            Value = value;
            Logging.DebugLog($"New {GetType().Name} with Name '{Name}' and Value '{Value}'");
        }

        public override bool PerformOperation(ConfigNode moduleNode)
        {
            //traverse the tree, then add the new value
            ConfigNode current = TraverseTree(moduleNode);
            if (current != null)
            {
                current.RemoveNodes(Value.ToString());
                return true;
            }
            return false;
        }
    }
}
