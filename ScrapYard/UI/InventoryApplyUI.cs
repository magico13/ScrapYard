using KSP.UI.Screens;
using UnityEngine;

namespace ScrapYard.UI
{
    public class InventoryApplyUI : WindowBase
    {
        internal InventoryApplyVM _viewModel;

        public InventoryApplyUI() : base(3741, "ScrapYard", true, false)
        {
            _viewModel = new InventoryApplyVM();
            //set the position to the bottom of the screen, near the button
            SetSize(Mouse.screenPos.x - (75 / 2), Screen.height - 175, 75, 100);
        }


        public override void Draw(int windowID)
        {
            GUILayout.BeginVertical();
            _viewModel.AutoApplyInventory = GUILayout.Toggle(_viewModel.AutoApplyInventory, "Auto-Apply");

            if (GUILayout.Button("Apply Inventory"))
            {
                _viewModel.ApplyInventoryToEditorVessel();
                //close the window
                Close();
            }

            if (GUILayout.Button("Reset Parts"))
            {
                _viewModel.MakeFresh();
                //close the window
                Close();
            }

            if (GUILayout.Button("Toggle Selector"))
            {
                _viewModel.ToggleSelectorUI();
                Close();
            }

            GUILayout.EndVertical();
            base.Draw(windowID);
        }


        public override void Show()
        {
            base.Show();
            
            //Activate the button
            EventListeners.Instance.Button.SetTrue(false);
        }

        public override void Close()
        {
            base.Close();

            //Deactivate the button
            EventListeners.Instance.Button.SetFalse(false);
        }
    }
}
