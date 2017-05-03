using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScrapYard
{
    public class ModuleTemplate
    {
        public string NameRegex { get; set; }
        public string[] Requirements { get; set; }
        public bool IsForbiddenType { get; set; }


        public ModuleTemplate(ConfigNode source)
        {
            NameRegex = source.GetValue("name");
            Requirements = source.GetValues("requirement");
            IsForbiddenType = string.Equals(source.name, "FORBIDDEN_TEMPLATE", StringComparison.Ordinal);
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
            
            if (string.Equals(final, "COUNT", StringComparison.Ordinal))
            {
                return (currentNode.CountNodes + currentNode.CountValues).ToString();
            }
            else if (string.Equals(final, "NODECOUNT", StringComparison.Ordinal))
            {
                return currentNode.CountNodes.ToString();
            }
            else if (string.Equals(final, "VALUECOUNT", StringComparison.Ordinal))
            {
                return currentNode.CountValues.ToString();
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
    }
}
