using System;

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

        /// <summary>
        /// Safely converts a string to a "Guid?" Is fine with null, empty string, or things that throw exceptions when parsed
        /// </summary>
        /// <param name="guidString">The Guid string</param>
        /// <returns>Either a Guid or null</returns>
        public static Guid? StringToGuid(string guidString)
        {
            try
            {
                if (!string.IsNullOrEmpty(guidString))
                {
                    return new Guid(guidString);
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
            return null;
        }
    }
}
