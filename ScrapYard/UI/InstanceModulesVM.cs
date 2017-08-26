using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.UI
{
    public class InstanceModulesVM
    {
        private InventoryPart _backingPart;
        private List<DisplayModule> _modules;


        public ConfigNode DisplayedModule { get; private set; }

        private int _selectedGridItem = 0;
        public int SelectedGridItem
        {
            get
            {
                return _selectedGridItem;
            }
            set
            {
                if (_selectedGridItem != value)
                {
                    _selectedGridItem = value;
                    ShowModule(_modules.Count > _selectedGridItem ? _modules[_selectedGridItem].Module : null);
                }
            }
        }

        public InstanceModulesVM(InventoryPart part)
        {
            _backingPart = part;
        }

        /// <summary>
        /// Gets the list of all saved modules on the backing part
        /// </summary>
        /// <returns>The list of saved modules</returns>
        public List<DisplayModule> GetModules()
        {
            if (_modules == null)
            {
                _modules = new List<DisplayModule>();
                foreach (ConfigNode node in _backingPart.ListModules())
                {
                    _modules.Add(new DisplayModule(node));
                }
                ShowModule(_modules.Count > _selectedGridItem ? _modules[_selectedGridItem].Module : null);
            }
            return _modules;
        }

        public void ShowModule(ConfigNode node)
        {
            DisplayedModule = node;
        }


        public class DisplayModule
        {
            public string Name { get; private set; }
            public ConfigNode Module { get; private set; }

            public DisplayModule(ConfigNode module)
            {
                string name = string.Empty;
                module.TryGetValue("name", ref name);
                Name = name;
                Module = module;
            }
        }
    }
}
