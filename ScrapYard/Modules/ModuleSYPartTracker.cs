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
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false)]
        private uint id = 0;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false)]
        public int TimesRecovered = 0;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false)]
        public bool Inventoried = false;

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true)]
        public string ScrapYard = string.Empty;

        public uint ID
        {
            get { return id; }
            set
            {
                id = value;
                part.persistentId = value;
                updateDisplay();
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor && id == 0)
            {
                id = part.persistentId;
            }
            else if (id != 0)
            {
                ID = id; //set it on the part
            }
            updateDisplay();
        }
        public override void OnInitialize()
        {
            base.OnInitialize();
            if (id == 0)
            {
                id = part.persistentId;
            }
            else
            {
                ID = id; //set it on the part
            }
            updateDisplay();
        }

        public override void OnCopy(PartModule fromModule)
        {
            base.OnCopy(fromModule);
            MakeFresh();
        }

        public void MakeFresh()
        {
            ID = FlightGlobals.CheckPartpersistentId(0, part, false, true);
            TimesRecovered = 0;
            Inventoried = false;
            updateDisplay();
        }

        private void updateDisplay()
        {
            if (Inventoried)
            {
                string use = TimesRecovered != 1 ? "Uses" : "Use";
                ScrapYard = $"{TimesRecovered} Previous {use}";
            }
            else
            {
                ScrapYard = "Part Is New";
            }
            //ScrapYard += $" {ID}";
        }
    }
}
