using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// Finds an AvailablePart from the LoadedPartsList based on the provided part name
        /// </summary>
        /// <param name="name">The name of the part.</param>
        /// <returns>An <see cref="AvailablePart"/> for the given name.</returns>
        public static AvailablePart AvailablePartFromName(string name)
        {
            return PartLoader.LoadedPartsList.Find(aP => aP.name == name);
        }
    }
}
