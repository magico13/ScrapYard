using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScrapYard.Utilities
{
    public static class EditorApplySpecialCases
    {
        /// <summary>
        /// Updates a TweakScale module to act as if the correct scaling was set by the user.
        /// Forces a proper rescaling
        /// </summary>
        /// <param name="part">The part being acted on</param>
        /// <param name="defaultModule">The module from the partPrefab</param>
        /// <param name="copyNode">The ConfigNode to copy info from</param>
        public static void TweakScale(Part part, PartModule defaultModule, ConfigNode copyNode)
        {
            string modName = defaultModule.moduleName;
            BindingFlags publicFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            PartModule mod = part.Modules[modName];
            Type TSType = mod.GetType();
            TSType.GetField("tweakScale", publicFlags).SetValue(mod, float.Parse(copyNode.GetValue("currentScale")));
            int scaleIndex = (TSType.GetField("ScaleFactors", privateFlags).GetValue(mod) as float[]).IndexOf(float.Parse(copyNode.GetValue("currentScale")));
            if (scaleIndex >= 0)
            {
                TSType.GetField("tweakName", publicFlags).SetValue(mod, scaleIndex);
            }
            else
            {
                TSType.GetField("isFreeScale", publicFlags).SetValue(mod, true);
            }
        }

    }
}
