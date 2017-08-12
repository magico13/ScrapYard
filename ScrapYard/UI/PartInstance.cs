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
        private Part _toApply;
        private bool _selling;
        
        private string _sellOrDiscard = "Discard";
        private string _selectOrApply = "Select";

        public event EventHandler Updated;

        public PartInstance(PartInventory inventory, InventoryPart iPart, bool selling, Part toApply)
        {
            _backingPart = iPart;
            _backingInventory = inventory;
            _selling = selling;
            if (selling)
            {
                _sellOrDiscard = "Sell";
            }
            if (toApply != null)
            {
                _selectOrApply = "Apply";
            }
            _toApply = toApply;
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

            if (GUILayout.Button(_selectOrApply))
            {
                selectPart(_toApply);
            }

            GUILayout.EndVertical();
        }

        private void sellPart()
        {
            //confirm with user
            MultiOptionDialog diag = new MultiOptionDialog("confirmDiscard",
                $"Are you sure you want to {_sellOrDiscard.ToLower()} the part for {_backingPart.DryCost} funds?",
                _sellOrDiscard + " Part",
                HighLogic.UISkin,
                new DialogGUIButton(_sellOrDiscard, () => {
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
                }),
                new DialogGUIButton("Cancel", () => { }));
            PopupDialog.SpawnPopupDialog(diag, false, HighLogic.UISkin);
        }

        private void selectPart(Part selectedPart)
        {
            //If a part is already selected, apply to it. Otherwise spawn a new one and apply to that.
            if (selectedPart == null)
            {
                EditorLogic.fetch.SpawnPart(Utilities.Utils.AvailablePartFromName(_backingPart.Name));
                selectedPart = EditorLogic.SelectedPart;
                if (selectedPart == null && EditorLogic.fetch?.ship?.Count == 1)
                {
                    selectedPart = EditorLogic.fetch.ship[0];
                }
            }
            _backingPart.FullyApplyToPart(selectedPart);
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }
}
