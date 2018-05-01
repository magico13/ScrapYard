using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public class SaveSpecificSettings : GameParameters.CustomParameterNode
    {
        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false; //for now lets not have presets
            }
        }

        public override string Section
        {
            get
            {
                return "ScrapYard";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 1; //I think? I'm not 100% sure what this is for
            }
        }

        public override string Title
        {
            get
            {
                return "ScrapYard";
            }
        }

        public override string DisplaySection
        {
            get
            {
                return Title;
            }
        }

        [GameParameters.CustomParameterUI("Mod Enabled", toolTip = "Uncheck this to disable ScrapYard for this save.")]
        public bool ModEnabled = true;

        [GameParameters.CustomParameterUI("Enable Inventory", toolTip = "Uncheck this to disable the part inventory entirely.")]
        public bool UseInventory = true;

        [GameParameters.CustomParameterUI("Enable Part Tracker", toolTip = "Uncheck this to disable the part tracker entirely.")]
        public bool UseTracker = true;

        [GameParameters.CustomParameterUI("Override Funds (WIP)", toolTip = "Enable this to make it so pulling parts from the inventory reduces costs, but recovery costs funds.\nStill under development.")]
        public bool OverrideFunds = false;

        [GameParameters.CustomIntParameterUI("Sale Percentage", toolTip = "When overriding funds, defines the percentage you get back from selling used parts.", minValue = 0, maxValue = 100, stepSize = 1)]
        public int FundsSalePercent = 100;

        [GameParameters.CustomParameterUI("Debug Logging", toolTip = "Enabling this turns on debug logging, which provides additional information in the KSP log for ScrapYard.")]
        public bool DebugLogging = false;

        [GameParameters.CustomIntParameterUI("Editor Recalculation Frequency", toolTip = "The minimum number of tenths of seconds between verification of the ship in the editor.", minValue = 2, maxValue = 100, stepSize = 2)]
        public int RefreshTime = 10;
    }
}
