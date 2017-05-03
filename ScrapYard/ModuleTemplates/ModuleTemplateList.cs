using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public class ModuleTemplateList : List<ModuleTemplate>
    {

        /// <summary>
        /// Checks the list for a matching template and returns it if found
        /// </summary>
        /// <param name="moduleNode">A PartModule ConfigNode</param>
        /// <returns>The matching template or null if no match</returns>
        public ModuleTemplate FindMatchingTemplate(ConfigNode moduleNode)
        {
            foreach (ModuleTemplate template in this)
            {
                if (template.Matches(moduleNode))
                {
                    return template;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if any templates match
        /// </summary>
        /// <param name="moduleNode">The PartModule ConfigNode</param>
        /// <returns>True if a match is found, false otherwise</returns>
        public bool CheckForMatch(ConfigNode moduleNode)
        {
            return (FindMatchingTemplate(moduleNode) != null);
        }
    }
}
