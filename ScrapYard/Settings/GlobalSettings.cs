using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public class GlobalSettings
    {
        private static string scrapYardPath = KSPUtil.ApplicationRootPath + "/GameData/ScrapYard";
        private static string settingsPath = scrapYardPath + "/PluginData/ScrapYard.cfg";
        private static string templatePath = scrapYardPath + "/PluginData/ModuleTemplates.cfg";

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
            //TODO: Module Manager?

            //load the setting file, a confignode
            if (File.Exists(settingsPath))
            {
                ConfigNode settingsNode = ConfigNode.Load(settingsPath);
                ConfigNode.LoadObjectFromConfig(this, settingsNode);
                if (settingsNode.HasNode("Blacklist"))
                {
                    _partBlacklist.Clear();
                    _partBlacklist.AddRange(settingsNode.GetNode("Blacklist").GetValues("name"));
                }
            }

            ModuleTemplates.Clear();
            ForbiddenTemplates.Clear();

            if (File.Exists(templatePath))
            {
                ConfigNode settingsNode = ConfigNode.Load(templatePath);
                foreach (ConfigNode templateNode in settingsNode.GetNodes())
                {
                    ModuleTemplate template = new ModuleTemplate(templateNode);
                    if (!template.IsForbiddenType)
                    {
                        ModuleTemplates.Add(template);
                    }
                    else
                    {
                        ForbiddenTemplates.Add(template);
                    }
                }
                Logging.DebugLog($"Loaded {ModuleTemplates.Count} module templates and {ForbiddenTemplates.Count} forbidden templates.");
            }
        }

        public void SaveSettings()
        {
            Directory.CreateDirectory(scrapYardPath + "/PluginData");
            ConfigNode settingsNode = ConfigNode.CreateConfigFromObject(this);
            //add blacklist
            ConfigNode blacklist = new ConfigNode("Blacklist");
            foreach (string listed in PartBlacklist)
            {
                blacklist.AddValue("name", listed);
            }
            settingsNode.AddNode(blacklist);
            settingsNode.Save(settingsPath);
        }
    }
}
