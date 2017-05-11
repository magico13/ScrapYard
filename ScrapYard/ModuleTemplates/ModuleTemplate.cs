using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ScrapYard
{
    public class ModuleTemplate
    {
        public string NameRegex { get; set; }
        public string[] Requirements { get; set; }
        public bool IsForbiddenType { get; set; }
        public bool StoreIfDefault { get; set; }

        public ModuleTemplate(ConfigNode source)
        {
            NameRegex = source.GetValue("name");
            Requirements = source.GetValues("requirement");
            IsForbiddenType = string.Equals(source.name, "SY_FORBIDDEN_TEMPLATE", StringComparison.Ordinal);

            bool store = false;
            source.TryGetValue("storeIfDefault", ref store);
            StoreIfDefault = store;
        }


        /// <summary>
        /// Checks if the provided name matches the template
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <returns>True if the name matches the template</returns>
        public bool NameMatches(string name)
        {
            return Regex.IsMatch(name, NameRegex);
        }

        public bool Matches(ConfigNode module)
        {
            if (!NameMatches(module.GetValue("name")))
            {
                return false;
            }

            //check requirements
            foreach (string requirement in Requirements)
            {
                if (!processRequirement(module, requirement))
                {
                    return false;
                }
            }

            return true;
        }


        private string getFieldValue(ConfigNode sourceNode, string fieldName)
        {
            string[] fieldSplit = fieldName.Split('.');

            ConfigNode currentNode = sourceNode;
            int i = 0;
            for (; i<fieldSplit.Length-1; i++)
            {
                if (currentNode.HasNode(fieldSplit[i]))
                {
                    currentNode = currentNode.GetNode(fieldSplit[i]);
                }
                else
                {
                    return string.Empty;
                }
            }

            string final = fieldSplit[i];

            string specialValue = getSpecialFieldValue(currentNode, final);
            if (!string.IsNullOrEmpty(specialValue))
            {
                return specialValue;
            }

            if (currentNode.HasValue(final))
            {
                return currentNode.GetValue(final);
            }
            return string.Empty;
        }

        private bool processRequirement(ConfigNode sourceNode, string requirement)
        {
            List<string> usedVariables = variablesUsed(requirement);
            Dictionary<string, string> variables = new Dictionary<string, string>();
            foreach (string variable in usedVariables)
            {
                variables.Add(variable, getFieldValue(sourceNode, variable));
            }

            //call upon the powers of MagiCore to do some parsing
            //right now this just uses the Math Parser, but I'd like to have string comparisons as well

            double result = MagiCore.MathParsing.ParseMath("SY_REQUIREMENT_PROCESSING", requirement, variables);

            return (result > 0);
        }

        private List<string> variablesUsed(string requirementString)
        {
            List<string> used = new List<string>();
            //find everything within square brackets
            MatchCollection matches = Regex.Matches(requirementString, @"\[.+?\]");
            foreach (Match match in matches)
            {
                used.Add(match.Value.Trim('[', ']'));
            }

            return used;
        }

        private string getSpecialFieldValue(ConfigNode node, string field)
        {
            if (string.Equals(field, "COUNT", StringComparison.Ordinal))
            { //Get the number of nodes and values
                return (node.CountNodes + node.CountValues).ToString();
            }
            else if (string.Equals(field, "NODECOUNT", StringComparison.Ordinal))
            { //Get the number of nodes
                return node.CountNodes.ToString();
            }
            else if (string.Equals(field, "VALUECOUNT", StringComparison.Ordinal))
            { //Get the number of values
                return node.CountValues.ToString();
            }

            return null;
        }
    }
}
