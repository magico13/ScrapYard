using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ScrapYard.Utilities
{
    public static class ConfigNodeUtils
    {
        /// <summary>
        /// Returns the internal name of a part given a valid ConfigNode of the part. Parts saved to a craft file are saved as "part = $partname_$idNumber", 
        /// while parts from an active Vessel are saved as "name = $partname". This can handle both situations
        /// </summary>
        /// <param name="part">The <see cref="ConfigNode"/> form of the part whose name you want.</param>
        /// <returns>The name of the <see cref="Part"/></returns>
        public static string PartNameFromNode(ConfigNode part)
        {
            string name = "";
            if (part.HasValue("part"))
            {
                name = part.GetValue("part");
                name = name.Split('_')[0];
            }
            else if (part.HasValue("name"))
                name = part.GetValue("name");
            return name;
        }

        /// <summary>
        /// Finds an AvailablePart from the LoadedPartsList based on the part name stored in the ConfigNode
        /// </summary>
        /// <param name="part">The <see cref="ConfigNode"/> form of the part you want the <see cref="AvailablePart"/> of.</param>
        /// <returns>An <see cref="AvailablePart"/> for the given part.</returns>
        public static AvailablePart AvailablePartFromNode(ConfigNode part)
        {
            return Utils.AvailablePartFromName(PartNameFromNode(part));
        }

        /// <summary>
        /// Checks two <see cref="ConfigNode"/>s to see if they are identical.
        /// </summary>
        /// <param name="node1">The first <see cref="ConfigNode"/></param>
        /// <param name="node2">The second <see cref="ConfigNode"/></param>
        /// <returns>True if identical, false if not.</returns>
        public static bool IsIdenticalTo(this ConfigNode node1, ConfigNode node2)
        {
            //Check that the number of subnodes are equal
            if (node1.CountNodes != node2.CountNodes)
                return false;
            //Check that the value counts are the same
            if (node1.CountValues != node2.CountValues)
                return false;
            //Check that all the values are identical
            foreach (string valueName in node1.values.DistinctNames())
            {
                if (!node2.HasValue(valueName))
                    return false;
                if (node1.GetValue(valueName) != node2.GetValue(valueName))
                    return false;
            }

            //Check all subnodes for equality
            for (int index = 0; index < node1.GetNodes().Length; ++index)
            {
                if (!node1.nodes[index].IsIdenticalTo(node2.nodes[index])) //recursion!
                    return false;
            }

            //If all these tests pass, we consider the nodes to be identical
            return true;
        }

        /// <summary>
        /// Extracts a Module node from a Part node
        /// </summary>
        /// <param name="partNode">The part ConfigNode</param>
        /// <param name="moduleName">The module to extract</param>
        /// <returns>The Module node or null if could not be found</returns>
        public static ConfigNode GetModuleNode(this ConfigNode partNode, string moduleName)
        {
            return partNode.GetNodes("MODULE")?.FirstOrDefault(n => n.GetValue("name").Equals(moduleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
