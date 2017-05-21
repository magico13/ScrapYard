using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScrapYard.UI
{
    public class InstanceModulesUI : WindowBase
    {
        protected InstanceModulesVM _viewModel;
        public InstanceModulesUI(InstanceModulesVM viewModel) : base(3743, "Modules", true, false)
        {
            _viewModel = viewModel;
            SetSize((Screen.width - 500) / 2, 500, 500, 1);
        }


    }
}
