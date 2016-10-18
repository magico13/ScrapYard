using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ScrapYard.Utilities;

namespace ScrapYard
{
    public class InventoryPart
    {
        [Persistent]
        private string _name = "";
        [Persistent]
        private float _dryCost = 0;

        public string Name { get { return _name; } }
        public float DryCost { get { return _dryCost; } }

        private List<ConfigNode> savedModules = new List<ConfigNode>();

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
            _dryCost = originPart.GetModuleCosts(originPart.partInfo.cost) + originPart.partInfo.cost;
            foreach (PartResource resource in originPart.Resources)
            {
                _dryCost -= (float)(resource.maxAmount * PartResourceLibrary.Instance.GetDefinition(resource.resourceName).unitCost);
            }

            //Save modules (once we know which modules we want to save)
            if (originPart.Modules != null)
            {
                foreach (PartModule module in originPart.Modules)
                {
                    foreach (string trackedModuleName in ScrapYard.Instance.Settings.TrackedModules)
                    {
                        if (module.moduleName.ToUpper().Contains(trackedModuleName))
                            savedModules.Add(module.snapshot.moduleValues);
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
            float fuelCost;
            ShipConstruction.GetPartCosts(originPartSnapshot, originPartSnapshot.partInfo, out _dryCost, out fuelCost);

            //Save modules
            if (originPartSnapshot.modules != null)
            {
                foreach (ProtoPartModuleSnapshot module in originPartSnapshot.modules)
                {
                    foreach (string trackedModuleName in ScrapYard.Instance.Settings.TrackedModules)
                    {
                        if (module.moduleName.ToUpper().Contains(trackedModuleName))
                            savedModules.Add(module.moduleValues);
                    }
                }
            }
        }

        /// <summary>
        /// Create an InventoryPart from an origin ConfigNode, extracting the name, dry cost, and relevant MODULEs
        /// </summary>
        /// <param name="originPartConfigNode">The <see cref="ConfigNode"/> to use as the basis of the <see cref="InventoryPart"/>.</param>
        public InventoryPart(ConfigNode originPartConfigNode)
        {
            _name = ConfigNodeUtils.PartNameFromNode(originPartConfigNode);
            float fuelCost;
            AvailablePart availablePartForNode = ConfigNodeUtils.AvailablePartFromNode(originPartConfigNode);
            if (availablePartForNode != null)
            {
                float dryMass, fuelMass;
                ShipConstruction.GetPartCostsAndMass(originPartConfigNode, availablePartForNode, out _dryCost, out fuelCost, out dryMass, out fuelMass);
            }

            if (originPartConfigNode.HasNode("MODULE"))
            {
                foreach (ConfigNode module in originPartConfigNode.GetNodes("MODULE"))
                {
                    foreach (string trackedModuleName in ScrapYard.Instance.Settings.TrackedModules)
                    {
                        if (module.GetValue("name").ToUpper().Contains(trackedModuleName))
                            savedModules.Add(module);
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the passed InventoryPart is identical to this one, excluding Quantity and Used by default
        /// </summary>
        /// <param name="comparedPart"></param>
        /// <returns></returns>
        public bool IdenticalTo(InventoryPart comparedPart)
        {
            //Test to ensure the name, dry cost, and number of saved modules are identical
            if (Name == comparedPart.Name && DryCost == comparedPart.DryCost && savedModules.Count == comparedPart.savedModules.Count)
            {
                //If strict comparison, ensure the quantity and amount used are identical
                //if (strict && Quantity != comparedPart.Quantity)
                //    return false;

                //Compare the saved modules to ensure they are identical
                for (int index = 0; index < savedModules.Count; ++index)
                {
                    if (!savedModules[index].IsIdenticalTo(comparedPart.savedModules[index]))
                    {
                        return false;
                    }
                }
                //If everything has passed, they are considered equal
                return true;
            }
            return false;
        }

        public Part ToPart()
        {
            //Part retPart = new Part();
            AvailablePart aPart = Utils.AvailablePartFromName(Name);
            Part retPart = aPart.partPrefab;

            //set the modules to the ones we've saved
            foreach (PartModule mod in retPart.Modules)
            {
                foreach (string trackedModuleName in ScrapYard.Instance.Settings.TrackedModules)
                {
                    if (mod.moduleName.ToUpper().Contains(trackedModuleName))
                    {
                        //replace the module with the version we've saved
                        ConfigNode savedModule = savedModules.FirstOrDefault(c => c.GetValue("name").ToUpper().Contains(trackedModuleName));
                        if (savedModule != null)
                        {
                            mod.Load(savedModule);
                        }
                    }
                }
            }

            return retPart;
        }

        /// <summary>
        /// Gets the ConfigNode version of the InventoryPart, or sets the state of the InventoryPart from a ConfigNode
        /// </summary>
        public ConfigNode State
        {
            get
            {
                ConfigNode returnNode = ConfigNode.CreateConfigFromObject(this);

                //Add module nodes
                foreach (ConfigNode module in savedModules)
                {
                    returnNode.AddNode(module);
                }
                return returnNode;
            }
            set
            {
                try
                {
                    //  ConfigNode cnUnwrapped = node.GetNode(this.GetType().Name);
                    //plug it in to the object
                    ConfigNode.LoadObjectFromConfig(this, value);
                    savedModules = new List<ConfigNode>();
                    foreach (ConfigNode module in value.GetNodes("MODULE"))
                        savedModules.Add(module);

                    Debug.Log($"Name: {Name} DryCost: {DryCost}");
                }
                catch (Exception ex)
                {
                    Debug.Log("ScrapYard: Error while loading InventoryPart from a ConfigNode. Error: \n" + ex.Message);
                }
            }
        }
    }
}
