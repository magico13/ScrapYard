using UnityEngine;

namespace ScrapYard
{
    internal static class Logging
    {
        internal static void DebugLog(object msg)
        {
            #if DEBUG
                Debug.Log("[ScrapYard] " + msg.ToString());
            #endif
        }

        internal static void Log(object msg)
        {
            Debug.Log("[ScrapYard] " + msg);
        }
    }
}
