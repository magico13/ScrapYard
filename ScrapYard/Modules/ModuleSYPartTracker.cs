using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard.Modules
{
    /// <summary>
    /// Applied to individual parts, it tracks how often that part has been used. Added and/or incremented by one each recovery.
    /// Strict comparisons between parts with different values will fail, but semi-soft comparisons will ignore this
    /// </summary>
    public class ModuleSYPartTracker : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true)]
        public string ID = null;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true)]
        public int TimesRecovered = 0;
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true)]
        public bool Inventoried = false;

        [KSPEvent(guiActiveEditor = true, guiName = "Select From Inventory")]
        public void OpenInventory()
        {
            ScrapYard.Instance.InstanceSelectorUI.Show(part, part);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor && string.IsNullOrEmpty(ID))
            {
                ID = NewID();
            }
        }
        public override void OnInitialize()
        {
            base.OnInitialize();
            if (string.IsNullOrEmpty(ID))
            {
                //MakeFresh();
                ID = NewID();
            }
        }

        public override void OnCopy(PartModule fromModule)
        {
            base.OnCopy(fromModule);
            MakeFresh();
        }

        protected string NewID()
        {
            return Guid.NewGuid().ToString();
        }

        public void MakeFresh()
        {
            ID = NewID();
            TimesRecovered = 0;
            Inventoried = false;
        }
    }
}
