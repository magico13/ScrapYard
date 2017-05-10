using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public class GlobalSettings
    {
        //private static string scrapYardPath = KSPUtil.ApplicationRootPath + "/GameData/ScrapYard";
        //private static string settingsPath = scrapYardPath + "/PluginData/ScrapYard.cfg";

        public ModuleTemplateList ModuleTemplates { get; private set; } = new ModuleTemplateList();

        public ModuleTemplateList ForbiddenTemplates { get; private set; } = new ModuleTemplateList();


        private List<string> _partBlacklist = new List<string>();
        public IEnumerable<string> PartBlacklist
        {
            get
            {
                return _partBlacklist;
            }
        }

        /// <summary>
        /// Settings for the current save
        /// </summary>
        public SaveSpecificSettings CurrentSaveSettings
        {
            get
            {
                return HighLogic.CurrentGame?.Parameters.CustomParams<SaveSpecificSettings>();
            }
        }

        /// <summary>
        /// Returns whether ScrapYard is enabled for the save
        /// </summary>
        public bool EnabledForSave
        {
            get
            {
                return CurrentSaveSettings?.ModEnabled ?? false;
            }
        }

        public GlobalSettings()
        {
            
        }

        public void LoadSettings()
        {
            //load the setting file, a confignode
            //if (File.Exists(settingsPath))
            //{
            //    ConfigNode settingsNode = ConfigNode.Load(settingsPath);
            //    ConfigNode.LoadObjectFromConfig(this, settingsNode);
            //}

            _partBlacklist.Clear();
            foreach (ConfigNode blacklistNode in GameDatabase.Instance.GetConfigNodes("SY_PART_BLACKLIST"))
            {
                _partBlacklist.AddRange(blacklistNode.GetValues("name"));
            }
            Logging.DebugLog($"Blacklisted {PartBlacklist.Count()} parts.");

            ModuleTemplates.Clear();
            ForbiddenTemplates.Clear();

            foreach (ConfigNode moduleTemplate in GameDatabase.Instance.GetConfigNodes("SY_MODULE_TEMPLATE"))
            {
                ModuleTemplate template = new ModuleTemplate(moduleTemplate);
                ModuleTemplates.Add(template);
            }

            foreach (ConfigNode moduleTemplate in GameDatabase.Instance.GetConfigNodes("SY_FORBIDDEN_TEMPLATE"))
            {
                ModuleTemplate template = new ModuleTemplate(moduleTemplate);
                ForbiddenTemplates.Add(template);
            }
            Logging.DebugLog($"Loaded {ModuleTemplates.Count} module templates and {ForbiddenTemplates.Count} forbidden templates.");
        }

        public void SaveSettings()
        {
            //Directory.CreateDirectory(scrapYardPath + "/PluginData");
            //ConfigNode settingsNode = ConfigNode.CreateConfigFromObject(this);
            //settingsNode.Save(settingsPath);
        }
    }
}
