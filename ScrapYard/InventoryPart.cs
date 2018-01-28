using System;
using System.Collections.Generic;
using System.Linq;
using ScrapYard.Utilities;
using ScrapYard.Modules;

namespace ScrapYard
{
    /// <summary>
    /// The strictness of comparing two parts for equivalency
    /// </summary>
    public enum ComparisonStrength
    {
        /// <summary>
        /// Equivalent if their names match
        /// </summary>
        NAME,
        /// <summary>
        /// EqualEquivalent if name and dry cost match
        /// </summary>
        COSTS,
        /// <summary>
        /// Equivalent if name, dry cost, and Modules (except ModuleSYPartTracker) match
        /// </summary>
        MODULES,
        /// <summary>
        /// Equivalent if name, dry cost, Modules, and TimesRecovered match
        /// </summary>
        TRACKER,
        /// <summary>
        /// Equivalent if name, dry cost, Modules, TimesRecovered and IDs match
        /// </summary>
        STRICT
    }
    public class InventoryPart
    {
        [Persistent]
        private string _name = "";
        [Persistent]
        private float _dryCost = 0;

        public string Name { get { return _name; } }
        public float DryCost { get { return _dryCost; } }
        public bool DoNotStore { get; set; } = false;
        public TrackerModuleWrapper TrackerModule { get; private set; } = new TrackerModuleWrapper(null);
        public Guid? ID
        {
            get
            {
                return TrackerModule?.ID;
            }
        }


        private List<ConfigNode> _savedModules;
        private List<ConfigNode> savedModules
        {
            get
            {
                //lazy load the module list
                if (_savedModules == null)
                {
                    _savedModules = new List<ConfigNode>();
                    foreach(ConfigNode module in allModules)
                    {
                        storeModuleNode(Name, module);
                    }
                }
                return _savedModules;
            }
            set { _savedModules = value; }
        }
        private List<ConfigNode> allModules = new List<ConfigNode>();
        private int _hash = 0;

        /// <summary>
        /// Creates an empty InventoryPart.
        /// </summary>
        public InventoryPart() { }

        /// <summary>
        /// Create an InventoryPart from an origin Part, extracting the name, dry cost, and relevant MODULEs
        /// </summary>
        /// <param name="originPart">The <see cref="Part"/> used as the basis of the <see cref="InventoryPart"/>.</param>
        public InventoryPart(Part originPart)
        {
            _name = originPart.partInfo.name;
            if (ScrapYard.Instance.Settings.PartBlacklist.Contains(Name))
            {
                DoNotStore = true;
            }
            _dryCost = originPart.GetModuleCosts(originPart.partInfo.cost) + originPart.partInfo.cost;
            foreach (PartResource resource in originPart.Resources)
            {
                _dryCost -= (float)(resource.maxAmount * PartResourceLibrary.Instance.GetDefinition(resource.resourceName).unitCost);
            }

            //Save modules
            if (originPart.Modules != null)
            {
                foreach (PartModule module in originPart.Modules)
                {
                    ConfigNode saved = new ConfigNode("MODULE");
                    module.Save(saved);
                    allModules.Add(saved);
                    //storeModuleNode(_name, saved);
                    if (module.moduleName.Equals("ModuleSYPartTracker"))
                    {
                        TrackerModule = new TrackerModuleWrapper(saved);
                    }
                }
            }
        }

        /// <summary>
        /// Create an InventoryPart from an origin ProtoPartSnapshot, extracting the name, dry cost, and relevant MODULEs
        /// </summary>
        /// <param name="originPartSnapshot">The <see cref="ProtoPartSnapshot"/> to use as the basis of the <see cref="InventoryPart"/>.</param>
        public InventoryPart(ProtoPartSnapshot originPartSnapshot)
        {
            _name = originPartSnapshot.partInfo.name;
            if (ScrapYard.Instance.Settings.PartBlacklist.Contains(Name))
            {
                DoNotStore = true;
            }
            float fuelCost;
            ShipConstruction.GetPartCosts(originPartSnapshot, originPartSnapshot.partInfo, out _dryCost, out fuelCost);

            //Save modules
            if (originPartSnapshot.modules != null)
            {
                foreach (ProtoPartModuleSnapshot module in originPartSnapshot.modules)
                {
                    allModules.Add(module.moduleValues);
                    if (module.moduleName.Equals("ModuleSYPartTracker"))
                    {
                        TrackerModule = new TrackerModuleWrapper(module.moduleValues);
                    }
                    //storeModuleNode(_name, module.moduleValues);
                }
            }
        }

        /// <summary>
        /// Create an InventoryPart from an origin ConfigNode, extracting the name, dry cost, and relevant MODULEs
        /// </summary>
        /// <param name="originPartConfigNode">The <see cref="ConfigNode"/> to use as the basis of the <see cref="InventoryPart"/>.</param>
        public InventoryPart(ConfigNode originPartConfigNode)
        {
            //if the ConfigNode given is already an InventoryPart, just load it instead
            if (originPartConfigNode.name == typeof(InventoryPart).FullName)
            {
                State = originPartConfigNode;
            }
            else
            {
                _name = ConfigNodeUtils.PartNameFromNode(originPartConfigNode);
                if (ScrapYard.Instance.Settings.PartBlacklist.Contains(Name))
                {
                    DoNotStore = true;
                }
                AvailablePart availablePartForNode = ConfigNodeUtils.AvailablePartFromNode(originPartConfigNode);
                if (availablePartForNode != null)
                {
                    float dryMass, fuelMass, fuelCost;
                    ShipConstruction.GetPartCostsAndMass(originPartConfigNode, availablePartForNode, out _dryCost, out fuelCost, out dryMass, out fuelMass);
                }

                if (originPartConfigNode.HasNode("MODULE"))
                {
                    foreach (ConfigNode module in originPartConfigNode.GetNodes("MODULE"))
                    {
                        allModules.Add(module);
                        //storeModuleNode(_name, module);
                        if (module.GetValue("name").Equals("ModuleSYPartTracker"))
                        {
                            TrackerModule = new TrackerModuleWrapper(module);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the passed InventoryPart is identical to this one, for a given strictness of "identical"
        /// </summary>
        /// <param name="comparedPart">The part to compare to</param>
        /// <param name="strictness">The strength of the comparison (just name? modules? everything?)</param>
        /// <returns>True if mathing, false otherwise</returns>
        public bool IsSameAs(InventoryPart comparedPart, ComparisonStrength strictness)
        {
            //Test that the name is the same
            if (Name != comparedPart.Name)
            {
                return false;
            }
            if (strictness == ComparisonStrength.NAME) //If we're just comparing name then we're done
            {
                return true;
            }

            //Verify the costs are within 1 funds
            if (Math.Abs(DryCost - comparedPart.DryCost) > 1.0)
            {
                return false;
            }
            if (strictness == ComparisonStrength.COSTS)
            {
                return true;
            }

            if (strictness == ComparisonStrength.STRICT) //Strict comparison, the ids must be the same
            { //Compare IDs now so we can avoid the full module comparison if they don't have the same ID
                if (comparedPart.ID != ID)
                {
                    return false;
                }
            }

            //Test to ensure the number of saved modules are identical
            if (savedModules.Count == comparedPart.savedModules.Count)
            {
                //Compare the saved modules to ensure they are identical
                for (int index = 0; index < savedModules.Count; ++index)
                {
                    if (!savedModules[index].IsIdenticalTo(comparedPart.savedModules[index]))
                    {
                        return false;
                    }
                }
                //If everything has passed, they are considered equal
            }
            else
            {
                return false;
            }
            if (strictness == ComparisonStrength.MODULES)
            {
                return true;
            }

            //Tracker comparison, the times used must match
            if (TrackerModule.TimesRecovered != comparedPart.TrackerModule.TimesRecovered)
            {
                return false;
            }
            if (TrackerModule.Inventoried != comparedPart.TrackerModule.Inventoried)
            {
                return false;
            }
            if (strictness == ComparisonStrength.TRACKER)
            {
                return true;
            }

            //Everything must match, they are the same
            return true;
        }

        /// <summary>
        /// Converts the InventoryPart into a Part using the stored modules
        /// </summary>
        /// <returns></returns>
        public Part ToPart()
        {
            //Part retPart = new Part();
            AvailablePart aPart = Utils.AvailablePartFromName(Name);
            Part retPart = aPart.partPrefab;


            //set the modules to the ones we've saved
            if (retPart.Modules?.Count > 0)
            {
                foreach (ConfigNode saved in savedModules)
                {
                    //look for this module on the partInfo and replace it
                    string moduleName = saved.GetValue("name");
                    if (retPart.Modules.Contains(moduleName))
                    {
                        PartModule correspondingModule = retPart.Modules[moduleName];
                        correspondingModule.Load(saved);
                    }
                }
            }
            //foreach (PartModule mod in retPart.Modules)
            //{
            //    foreach (string trackedModuleName in ScrapYard.Instance.Settings.TrackedModules)
            //    {
            //        if (mod.moduleName.ToUpper().Contains(trackedModuleName))
            //        {
            //            //replace the module with the version we've saved
            //            ConfigNode savedModule = savedModules.FirstOrDefault(c => c.GetValue("name").ToUpper().Contains(trackedModuleName));
            //            if (savedModule != null)
            //            {
            //                mod.Load(savedModule);
            //            }
            //        }
            //    }
            //}

            return retPart;
        }
        
        /// <summary>
        /// Fully applies stored modules to the provided part
        /// </summary>
        /// <param name="part">The Part to apply onto</param>
        /// <returns>True if part is the right type</returns>
        public bool FullyApplyToPart(Part part)
        {
            if (part?.partInfo.name != Name)
            {
                return false;
            }

            if (part.Modules?.Count > 0)
            {
                //we can't just copy saved modules over, since that won't work for going back to default
                //instead we copy all modules over from the default part and load our saved modules instead of default (where appropriate)
                foreach (PartModule defaultModule in part.partInfo.partPrefab.Modules)
                {
                    try
                    {
                        string modName = defaultModule.moduleName;
                        if (modName == "ModuleSYPartTracker")
                        {
                            continue;
                        }
                        ConfigNode copyNode;
                        if ((copyNode = savedModules.Find(m => m.GetValue("name") == modName)) == null)
                        {
                            copyNode = new ConfigNode("MODULE");
                            defaultModule?.Save(copyNode);
                        }
                        if (copyNode.HasData)
                        {
                            if (modName == "TweakScale")
                            {
                                ConfigNode current = new ConfigNode("MODULE");
                                part.Modules[modName].Save(current);
                                if (ScrapYard.Instance.Settings.ModuleTemplates.CheckForMatch(Name, copyNode) || ScrapYard.Instance.Settings.ModuleTemplates.CheckForMatch(Name, current))
                                {
                                    //Because of how tweakscale works, we have to change values on the module itself for the update to work
                                    EditorApplySpecialCases.TweakScale(part, defaultModule, copyNode);
                                }
                            }
                            else
                            {
                                part.Modules[modName].Load(copyNode);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error while applying module '{defaultModule.moduleName}'. Error: \n {ex}", Logging.LogType.ERROR);
                    }
                }

                if (part.Modules.Contains("ModuleSYPartTracker"))
                {
                    ModuleSYPartTracker tracker = part.Modules["ModuleSYPartTracker"] as ModuleSYPartTracker;
                    tracker.ID = TrackerModule.ID.ToString();
                    tracker.TimesRecovered = TrackerModule.TimesRecovered;
                    tracker.Inventoried = TrackerModule.Inventoried;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the list of saved modules on the part
        /// </summary>
        /// <returns>The list of saved modules</returns>
        public IList<ConfigNode> ListModules()
        {
            return new List<ConfigNode>(savedModules);
        }

        /// <summary>
        /// Gets the ConfigNode version of the InventoryPart, or sets the state of the InventoryPart from a ConfigNode
        /// </summary>
        public ConfigNode State
        {
            get
            {
                //if (_state == null) //if we can cache this correctly it would help to reduce lag
                try
                {
                    ConfigNode returnNode = ConfigNode.CreateConfigFromObject(this);

                    returnNode.AddValue("_id", ID);
                    returnNode.AddValue("_timesRecovered", TrackerModule.TimesRecovered);
                    returnNode.AddValue("_inventoried", TrackerModule.Inventoried);

                    //Add module nodes
                    foreach (ConfigNode module in savedModules)
                    {
                        returnNode.AddNode(module);
                    }
                    return returnNode;
                }
                catch (Exception ex)
                {
                    Logging.Log($"Error while saving InventoryPart to a ConfigNode. Error: \n {ex}", Logging.LogType.ERROR);
                }
                return null;
            }
            set
            {
                try
                {
                    if (value == null)
                    {
                        return;
                    }
                    //  ConfigNode cnUnwrapped = node.GetNode(this.GetType().Name);
                    //plug it in to the object
                    ConfigNode.LoadObjectFromConfig(this, value);

                    //try to get tracker stuff
                    int timesRecovered = 0;
                    bool inventoried = false;
                    string idStr = null;

                    if (value.TryGetValue("_id", ref idStr) |
                        value.TryGetValue("_timesRecovered", ref timesRecovered) |
                        value.TryGetValue("_inventoried", ref inventoried)) // the single | makes all of them happen, we need at least one to succeed
                    {
                        Guid? idGuid = Utilities.Utils.StringToGuid(idStr);
                        if (idGuid.HasValue)
                        {
                            TrackerModule = new TrackerModuleWrapper(idGuid.Value, timesRecovered, inventoried);
                        }
                    }


                    savedModules = new List<ConfigNode>();
                    foreach (ConfigNode module in value.GetNodes("MODULE"))
                    {
                        if (module.GetValue("name").Equals("ModuleSYPartTracker")) //Deprecated, but not a bad idea to keep
                        {
                            TrackerModule = new TrackerModuleWrapper(module);
                        }
                        else
                        {
                            savedModules.Add(module);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log($"Error while loading InventoryPart from a ConfigNode. Error: \n {ex}", Logging.LogType.ERROR);
                }
            }
        }

        public override int GetHashCode()
        {
            if (_hash == 0)
            {
                foreach (char s in ID?.ToString() ?? string.Empty)
                {
                    _hash += s;
                }
                _hash *= 31;
            }
            return _hash;
        }

        public override bool Equals(object obj)
        {
            //return ReferenceEquals(this, obj);
            InventoryPart other = obj as InventoryPart;
            if (obj == null)
            {
                return false;
            }
            return (GetHashCode() == other.GetHashCode() && IsSameAs(other, ComparisonStrength.STRICT));
        }

        public InventoryPart Copy()
        {
            InventoryPart copy = new InventoryPart();
            copy._dryCost = _dryCost;
            copy._name = _name;
            copy.savedModules = new List<ConfigNode>(savedModules);
            copy.TrackerModule = new TrackerModuleWrapper(TrackerModule?.TrackerNode?.CreateCopy());
            if (!copy.TrackerModule.HasModule && TrackerModule != null)
            {
                copy.TrackerModule = new TrackerModuleWrapper(TrackerModule.ID.Value, TrackerModule.TimesRecovered, TrackerModule.Inventoried);
            }

            return copy;
        }

        private bool storeModuleNode(string partName, ConfigNode moduleNode)
        {
            bool saved = false;
            //If it matches a template, save it
            if (ScrapYard.Instance.Settings.ModuleTemplates.CheckForMatch(partName, moduleNode))
            {
                savedModules.Add(moduleNode);
                saved = true;
            }

            //check if this is one of the forbidden modules, and if so then set DoNotStore
            //If we already have DoNotStore set, there's no reason to check again
            if (!DoNotStore && ScrapYard.Instance.Settings.ForbiddenTemplates.CheckForMatch(partName, moduleNode))
            {
                Logging.DebugLog("Matched forbidden template with module "+moduleNode.GetValue("name"));
                DoNotStore = true;
            }

            //check for the part tracker and add it
            //done in constructor
            //if (moduleNode.GetValue("name").Equals("ModuleSYPartTracker"))
            //{
            //    TrackerModule = new TrackerModuleWrapper(moduleNode);
            //    saved = true;
            //}

            return saved;
        }
    }
}
