using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScrapYard.UI
{
    public class InstanceSelectorUI : WindowBase
    {
        protected Vector2 scrollPos;
        protected bool dragging = false;
        protected Vector2 lastMousePos;
        
        public InstanceSelectorVM InstanceVM { get; set; }

        public InstanceSelectorUI() : base(3742, "Inventory", true, false)
        {
            SetSize(500, 100, 300, Screen.height-200);

            OnMouseOver.Add(() => { InstanceVM.OnMouseOver(); });
            OnMouseExit.Add(() => { InstanceVM.OnMouseExit(); });
            //OnShow.Add(() => { GameEvents.onEditorPartPlaced.Add(OnEditorPartPlaced); });
            //OnClose.Add(() => { GameEvents.onEditorPartPlaced.Remove(OnEditorPartPlaced); });
        }

        public override void Draw(int windowID)
        {
            if (!HighLogic.LoadedSceneIsEditor)
            {
                Close();
                return;
            }
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(InstanceVM.SelectedPartName);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (InstanceVM.ApplyPart != null)
            {
                if (GUILayout.Button("Use New Part"))
                {
                    InstanceVM.RefreshApplyPart();
                }
            }
            else
            {
                if (GUILayout.Button("Select New Part"))
                {
                    //spawn a new part
                    InstanceVM.SpawnNewPart();
                }
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            //Show sets of parts in the inventory, grouped by usage/modules
            if (InstanceVM.Parts?.Any() == true)
            {
                foreach (List<PartInstance> list in InstanceVM.Parts)
                {
                    PartInstance instance = list.FirstOrDefault();
                    
                    if (instance != null)
                    {
                        GUILayout.BeginVertical(GUI.skin.textArea);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"{instance.BackingPart.TrackerModule.TimesRecovered} Previous Uses");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label($"{list.Count} In Inventory");
                        GUILayout.EndHorizontal();
                        instance.Draw();
                        GUILayout.EndVertical();
                    }
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Label("Whole-Vessel Quick Options:");
            InstanceVM.AutoApplyInventory = GUILayout.Toggle(InstanceVM.AutoApplyInventory, "Automatically Quick Apply");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Quick Apply"))
            {
                InstanceVM.ApplyInventoryToEditorVessel();
            }
            if (GUILayout.Button("New Parts"))
            {
                InstanceVM.MakeFresh();
            }
            GUILayout.EndHorizontal();

            //click near the bottom of the window, then drag
            if ((Mouse.Left.GetButton() && dragging)
                || (Mouse.Left.GetButtonDown() && Mouse.screenPos.x > WindowRect.xMin && Mouse.screenPos.x < WindowRect.xMax
                && Mouse.screenPos.y > WindowRect.yMax - 10 && Mouse.screenPos.y < WindowRect.yMax))
            {
                if (!dragging)
                {
                    lastMousePos = Mouse.screenPos;
                }

                float dy = Mouse.screenPos.y - lastMousePos.y;

                //resize
                Rect oldSize = WindowRect;
                SetSize(oldSize.xMin, oldSize.yMin, oldSize.width, oldSize.height + dy);
                dragging = true;
                Draggable = false;
                lastMousePos = Mouse.screenPos;
            }
            else
            {
                dragging = false;
                Draggable = true;
            }

            GUILayout.EndVertical();
            base.Draw(windowID);

            //if clicked and holding a part, disable the on_partDropped event
            if (MouseIsOver)
            {
                if (Mouse.Left.GetButtonDown())
                {
                    InstanceVM.DisablePartDropping();
                }
            }
            else
            {
                InstanceVM.RestorePartDropping();
            }
        }

        public void Show(Part basePart, Part applyTo)
        {
            base.Show();
            InstanceVM = new InstanceSelectorVM(ScrapYard.Instance.TheInventory,
                basePart,
                applyTo,
                ScrapYard.Instance.Settings.CurrentSaveSettings.OverrideFunds);
        }

        public override void Show()
        {
            base.Show();
            InstanceVM = new InstanceSelectorVM();
        }
    }
}
