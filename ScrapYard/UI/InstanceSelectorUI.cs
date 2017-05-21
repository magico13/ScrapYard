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
            SetSize(500, 100, 300, Screen.height-100);

            //if (HighLogic.LoadedSceneIsEditor)
            //{
            //    Show();
            //}
        }

        public override void Draw(int windowID)
        {
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(InstanceVM.SelectedPartName);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            foreach (PartInstance instance in InstanceVM.Parts)
            {
                instance.Draw();
            }


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            base.Draw(windowID);
        }

        public override void Show()
        {
            base.Show();
            InstanceVM = new InstanceSelectorVM();
        }

        public override void Close()
        {
            base.Close();
        }
    }
}
