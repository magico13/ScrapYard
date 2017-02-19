using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace ScrapYard
{
    public class PartInventory
    {
        private bool disableEvents = false;
        private Dictionary<InventoryPart, int> internalInventory = new Dictionary<InventoryPart, int>();

        //public event EventHandler<InventoryChangedEventArgs> OnInventoryQuantityChanged;

        public PartInventory() { }
        /// <summary>
        /// Creates a new PartInventory that doesn't trigger events when the inventory changes
        /// </summary>
        /// <param name="DisableEvents">Disables event firing if true.</param>
        public PartInventory(bool DisableEvents)
        {
            disableEvents = DisableEvents;
        }

        public int AddPart(InventoryPart part, int quantity = 1)
        {
            /*if (quantity > int.MinValue)
                part.SetQuantity(quantity);*/
            int previousAmount = 0;
            InventoryPart existingPart = FindPart(part);
            if (existingPart != null)
            {
                //Logging.DebugLog("ScrapYard", "Found existing part.");
                //existingPart.AddQuantity(part.Quantity);
                previousAmount = internalInventory[existingPart];
                internalInventory[existingPart] += quantity;
            }
            else
            {
                //Logging.DebugLog("ScrapYard", "Didn't find existing part.");
                internalInventory.Add(part, quantity);
                existingPart = part;
            }
            int newAmount = internalInventory[existingPart];
            if (!disableEvents)
            {
                //OnInventoryQuantityChanged(this, new InventoryChangedEventArgs(existingPart, previousAmount, newAmount));
                Events.SYInventoryChanged.Fire(existingPart, previousAmount, newAmount);
            }
            return newAmount;
        }

        public int AddPart(Part part, int quantity = 1)
        {
            InventoryPart convertedPart = new InventoryPart(part);
            return AddPart(convertedPart, quantity);
        }

        public int AddPart(ProtoPartSnapshot protoPartSnapshot, int quantity = 1)
        {
            InventoryPart convertedPart = new InventoryPart(protoPartSnapshot);
            return AddPart(convertedPart, quantity);
        }

        public int AddPart(ConfigNode partNode, int quantity = 1)
        {
            InventoryPart convertedPart = new InventoryPart(partNode);
            return AddPart(convertedPart, quantity);
        }

       /* public int IncrementUsageCounter(InventoryPart part)
        {
            InventoryPart existingPart = FindPart(part);
            if (existingPart == null)
            {
                InternalInventory.Add(part);
                existingPart = part;
            }
            existingPart.AddUsage();
            return existingPart.Used;
        }*/

        public InventoryPart FindPart(InventoryPart part)
        {
            return internalInventory.FirstOrDefault(ip => ip.Key.IdenticalTo(part)).Key;
        }

        /*public InventoryPart GetPartByIndex(int index)
        {
            return InternalInventory
        }*/

        /*private int FindPartIndex(InventoryPart part)
        {
            InventoryPart target = FindPart(part);
            if (target == null)
                return -1;
            return InternalInventory.IndexOf(target);
        }*/

        public void SetPartQuantity(InventoryPart part, int quantity)
        {
            //int index = FindPartIndex(part);
            InventoryPart internalPart = FindPart(part);
            
            if (internalPart != null)
            {
                int previousAmount = internalInventory[internalPart];
                internalInventory[internalPart] = quantity;
                if (!disableEvents)
                {
                    //OnInventoryQuantityChanged(this, new InventoryChangedEventArgs(internalPart, previousAmount, quantity));
                    Events.SYInventoryChanged.Fire(internalPart, previousAmount, quantity);
                }
            }
            else
            {
                //part.SetQuantity(quantity);
                AddPart(part, quantity);
            }
        }

        public int GetPartQuantity(InventoryPart part)
        {
            if (part == null)
                return 0;

            InventoryPart internalPart = FindPart(part);

            if (internalPart != null)
            {
                return internalInventory[internalPart];
            }
            return 0;
        }

        public void SplitParts(List<InventoryPart> input, out List<InventoryPart> inInventory, out List<InventoryPart> notInInventory)
        {
            inInventory = new List<InventoryPart>();
            notInInventory = new List<InventoryPart>();
            PartInventory InventoryCopy = new PartInventory(true);
            InventoryCopy.State = State; //TODO: Make a copy method
            foreach (InventoryPart inputPart in input)
            {
                if (InventoryCopy.GetPartQuantity(inputPart) > 0)
                {
                    inInventory.Add(inputPart);
                    InventoryCopy.AddPart(inputPart, -1);
                }
                else
                {
                    notInInventory.Add(inputPart);
                }
            }
        }

        public ConfigNode State
        {
            get
            {
                ConfigNode returnNode = new ConfigNode("PartInventory");
                //Add module nodes
                foreach (KeyValuePair<InventoryPart, int> kvp in internalInventory)
                {
                    ConfigNode toAdd = kvp.Key.State;
                    toAdd.AddValue("quantity", kvp.Value);
                    returnNode.AddNode(toAdd);
                }
                return returnNode;
            }
            set
            {
                try
                {
                    internalInventory = new Dictionary<InventoryPart, int>();
                    foreach (ConfigNode inventoryPartNode in value.GetNodes(typeof(InventoryPart).FullName))
                    {
                        InventoryPart loading = new InventoryPart();
                        loading.State = inventoryPartNode;
                        //loading.Load(inventoryPart);
                        int count = 0;
                        if (int.TryParse(inventoryPartNode.GetValue("quantity"), out count))
                        {
                            internalInventory.Add(loading, count);
                        }
                    }
                    Logging.DebugLog("Printing PartInventory:");
                    foreach (KeyValuePair<InventoryPart, int> kvp in internalInventory)
                    {
                        Logging.DebugLog($"{kvp.Key.Name} : {kvp.Value}");
                    }
                }
                catch
                {
                    //Logging.Log("ScrapYard", "Error while loading PartInventory from a ConfigNode. Error: \n"+ex.Message);
                }
            }
        }
    }
}
