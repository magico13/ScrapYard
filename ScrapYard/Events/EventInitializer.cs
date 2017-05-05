using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScrapYard
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class EventInitializer : MonoBehaviour
    {
        private void Awake()
        {
            ScrapYardEvents.Initialize();
        }
    }
}
