using ScrapYard.Refurbishment;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScrapYard
{
    public class GlobalSettings
    {
        private static string _scrapYardPath;
        private static string _settingsPath;
        private static bool _initialized = false;


        [Persistent]
        private bool _autoApplyInventory = false;
        public bool AutoApplyInventory
        {
            get { return _autoApplyInventory; }
            set { _autoApplyInventory = value; }
        }


        public ModuleTemplateList ModuleTemplates { get; private set; } = new ModuleTemplateList();

        public ModuleTemplateList ForbiddenTemplates { get; private set; } = new ModuleTemplateList();

        public List<BasicRefurb> AutomaticRefurbishment { get; private set; } = new List<BasicRefurb>();

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

        public void Initialize()
        {
            if (!_initialized)
            {
                Logging.DebugLog("Initializing GlobalSettings");
                _scrapYardPath = KSPUtil.ApplicationRootPath + "/GameData/ScrapYard";
                _settingsPath = _scrapYardPath + "/PluginData/ScrapYard.cfg";
                _initialized = true;
            }
        }

        public void LoadSettings()
        {
            Initialize();
            //load the setting file, a confignode
            if (File.Exists(_settingsPath))
            {
                ConfigNode settingsNode = ConfigNode.Load(_settingsPath);
                ConfigNode.LoadObjectFromConfig(this, settingsNode);

                foreach (ConfigNode posNode in settingsNode.GetNodes("POSITION"))
                {
                    string name = posNode.GetValue("name");
                    if (name == ScrapYard.Instance.InstanceSelectorUI.Title)
                    {
                        ScrapYard.Instance.InstanceSelectorUI.LoadPosition(posNode);
                    }
                    else if (name == ScrapYard.Instance.InstanceModulesUI.Title)
                    {
                        ScrapYard.Instance.InstanceModulesUI.LoadPosition(posNode);
                    }
                }
            }

            _partBlacklist.Clear();
            foreach (ConfigNode blacklistNode in GameDatabase.Instance.GetConfigNodes("SY_PART_BLACKLIST"))
            {
                _partBlacklist.AddRange(blacklistNode.GetValues("name"));
            }
            Logging.Log($"Blacklisted {PartBlacklist.Count()} parts.");

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
            Logging.Log($"Loaded {ModuleTemplates.Count} module templates and {ForbiddenTemplates.Count} forbidden templates.");

            AutomaticRefurbishment.Clear();
            foreach (ConfigNode refurbTemplate in GameDatabase.Instance.GetConfigNodes("SY_AUTO_REFURB"))
            {
                BasicRefurb refurb = new BasicRefurb(refurbTemplate);
                AutomaticRefurbishment.Add(refurb);
            }
            Logging.Log($"Automatically refurbishing {AutomaticRefurbishment.Count} modules.");
        }

        public void SaveSettings()
        {
            Initialize();
            Directory.CreateDirectory(_scrapYardPath + "/PluginData");
            ConfigNode settingsNode = ConfigNode.CreateConfigFromObject(this);

            settingsNode.AddNode(ScrapYard.Instance.InstanceSelectorUI.SavePosition(true));
            settingsNode.AddNode(ScrapYard.Instance.InstanceModulesUI.SavePosition(false));
            settingsNode.Save(_settingsPath);
        }
    }
}
