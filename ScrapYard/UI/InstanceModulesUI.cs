using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScrapYard.UI
{
    public class InstanceModulesUI : WindowBase
    {
        private Vector2 moduleListScroll;
        private Vector2 moduleDisplayedScroll;

        protected InstanceModulesVM _viewModel;
        public InstanceModulesUI() : base(3743, "Modules", true, false)
        {
            SetVisibleScenes(GameScenes.EDITOR);
            SetSize((Screen.width - 500) / 2, 500, 500, 500);
            SetResizeable(true, true);
        }

        public void SetUp(InstanceModulesVM viewModel)
        {
            _viewModel = viewModel;
        }

        public override void Draw(int windowID)
        {
            if (!HighLogic.LoadedSceneIsEditor || _viewModel == null)
            {
                Close();
                return;
            }

            //show a list of all modules on the part on the left
            //click a module and see the confignode of it (or maybe just the differences from default?)

            //split into two parts, a smaller left and a larger right
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(200));
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            moduleListScroll = GUILayout.BeginScrollView(moduleListScroll);

            _viewModel.SelectedGridItem = GUILayout.SelectionGrid(_viewModel.SelectedGridItem, _viewModel.GetModules().Select(m => m.Name).ToArray(), 1);
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            moduleDisplayedScroll = GUILayout.BeginScrollView(moduleDisplayedScroll);
            if (_viewModel.DisplayedModule != null)
            {
                GUILayout.TextArea(_viewModel.DisplayedModule.ToString());
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            base.Draw(windowID);
        }
    }
}
