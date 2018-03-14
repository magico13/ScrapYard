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
        private InstanceModulesVM _moduleVM;
        private InstanceModulesUI _moduleUI;

        private string _sellOrDiscard = "Discard";

        public event EventHandler Updated;
        public InventoryPart BackingPart { get { return _backingPart; } }

        public PartInstance(PartInventory inventory, InventoryPart iPart, bool selling, Part toApply)
        {
            _backingPart = iPart;
            _backingInventory = inventory;
            _selling = selling;
            if (selling)
            {
                _sellOrDiscard = "Sell";
            }
            _toApply = toApply;
            _moduleVM = new InstanceModulesVM(_backingPart);
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_sellOrDiscard))
            {
                sellPart();
            }
            if (_moduleVM.GetModules().Count > 0 && GUILayout.Button("Modules"))
            {
                //show module window
                _moduleUI = ScrapYard.Instance.InstanceModulesUI;
                _moduleUI.SetUp(_moduleVM);
                _moduleUI.Show();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select"))
            {
                selectPart(null);
            }
            if (_toApply != null && GUILayout.Button("Apply"))
            {
                selectPart(_toApply);
            }
            GUILayout.EndHorizontal();
        }

        private void sellPart()
        {
            //confirm with user
            string msg = "Are you sure you want to discard the part?";
            if (_selling)
            {
                msg = $"Are you sure you want to sell the part for {_backingPart.DryCost} funds?";
            }

            MultiOptionDialog diag = new MultiOptionDialog("confirmDiscard",
                msg,
                _sellOrDiscard + " Part",
                HighLogic.UISkin,
                new DialogGUIButton(_sellOrDiscard, () => {
                    InventoryPart removed = _backingInventory.RemovePart(_backingPart, ComparisonStrength.STRICT);
                    if (removed != null)
                    {
                        Logging.Log($"Sold/Discarded {removed.Name}:{removed.ID}");
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
