using UnityEngine;

namespace ScrapYard.UI
{
    public class InventoryApplyUI : WindowBase
    {
        internal InventoryApplyVM _viewModel;

        public InventoryApplyUI() : base(3741, "ScrapYard", false, false)
        {
            _viewModel = new InventoryApplyVM();
        }


        public override void Draw(int windowID)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("Apply Inventory"))
            {
                _viewModel.ApplyInventoryToVessel();
            }

            GUILayout.EndVertical();
            base.Draw(windowID);
        }


        public override void Show()
        {
            base.Show();

            //set the position to the bottom of the screen, near the button
            //SetSize(EventListeners.Instance.Button.GetAnchorUL().x, EventListeners.Instance.Button.GetAnchorUL().y - 20, 75, 20);
            SetSize(0, 0, 75, 0);
            centerWindow();
        }
    }
}
