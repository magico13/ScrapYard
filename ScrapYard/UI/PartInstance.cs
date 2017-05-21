using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScrapYard.UI
{
    public class PartInstance
    {
        private InventoryPart _backingPart;
        private PartInventory _backingInventory;
        private string _sellOrDiscard = "Discard";
        private bool _selling;

        public event EventHandler Updated;

        public PartInstance(PartInventory inventory, InventoryPart iPart, bool selling)
        {
            _backingPart = iPart;
            _backingInventory = inventory;
            _selling = selling;
            if (selling)
            {
                _sellOrDiscard = "Sell";
            }
        }

        public void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.Label(string.Format("Times Used: {0}", _backingPart?.TrackerModule?.TimesRecovered ?? 0));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_sellOrDiscard))
            {
                sellPart();
            }
            if (GUILayout.Button("Modules"))
            {
                //show module window
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Select"))
            {
                selectPart();
            }

            GUILayout.EndVertical();
        }

        private void sellPart()
        {
            InventoryPart removed = _backingInventory.RemovePart(_backingPart, ComparisonStrength.STRICT);
            if (removed != null)
            {
                Logging.DebugLog($"Sold/Discarded {removed.Name}:{removed.ID}");
                if (_selling)
                {
                    Funding.Instance?.AddFunds(removed.DryCost, TransactionReasons.Vessels);
                }
                Updated?.Invoke(this, EventArgs.Empty);
            }
        }

        private void selectPart()
        {
            EditorLogic.fetch.SpawnPart(Utilities.Utils.AvailablePartFromName(_backingPart.Name));
            if (!_backingPart.FullyApplyToPart(EditorLogic.SelectedPart) && EditorLogic.fetch.ship.Count == 1)
            {
                _backingPart.FullyApplyToPart(EditorLogic.fetch.ship[0]);
            }
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }
}
