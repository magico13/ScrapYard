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

            GUILayout.EndVertical();
            base.Draw(windowID);
        }


        public override void Show()
        {
            base.Show();

            //set the position to the bottom of the screen, near the button
            //SetSize(EventListeners.Instance.Button.GetAnchorUL().x, EventListeners.Instance.Button.GetAnchorUL().y - 20, 75, 20);
            SetSize(Mouse.screenPos.x - (75/2), Screen.height - 150, 75, 75);
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
