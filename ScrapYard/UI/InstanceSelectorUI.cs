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

        public InstanceSelectorVM InstanceVM { get; set; }

        public InstanceSelectorUI() : base(3742, "Inventory", true, false)
        {
            SetSize(500, 100, 300, Screen.height-200);

            //OnMouseOver.Add(() => { InstanceVM.OnMouseOver(); });
            //OnMouseExit.Add(() => { InstanceVM.OnMouseExit(); });
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
            scrollPos = GUILayout.BeginScrollView(scrollPos);

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

            if (InstanceVM.Parts?.Any() == true)
            {
                foreach (long uses in InstanceVM.Parts.Keys)
                {
                    PartInstance instance = InstanceVM.Parts[uses].FirstOrDefault();
                    
                    if (instance != null)
                    {
                        //TODO: THIS DOESN'T WORK IF THE MODULES ARE DIFFERENT
                        GUILayout.BeginVertical(GUI.skin.textArea);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"{uses} Previous Uses");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label($"{InstanceVM.Parts[uses].Count} In Inventory");
                        GUILayout.EndHorizontal();
                        instance.Draw();
                        GUILayout.EndVertical();
                    }
                }
            }


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            base.Draw(windowID);    
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


        /// <summary>
        /// Used to put the part back in the editor's hand, since no locks will stop placement
        /// </summary>
        /// <param name="p"></param>
        //private void OnEditorPartPlaced(Part p)
        //{
        //    if (MouseIsOver)
        //    {
        //        InstanceVM.PutPartInEditorHand(p);
        //    }
        //}
    }
}
