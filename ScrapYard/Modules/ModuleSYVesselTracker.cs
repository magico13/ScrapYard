using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.Modules
{
    //A module that stores a unique ID for a vessel
    public class ModuleSYVesselTracker : PartModule
    {
        [KSPField]
        public Guid ID;
    }
}
