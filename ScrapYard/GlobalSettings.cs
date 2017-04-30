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

        [Persistent]
        private string _trackedModules = "TWEAKSCALE, PROCEDURAL";
        private string[] _trackedArray = null;
        public string[] TrackedModules
        {
            get
            {
                if (_trackedArray == null)
                { 
                    string[] arr = _trackedModules.Split(',');
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = arr[i].Trim();
                    }
                    _trackedArray = arr;
                }
                return _trackedArray;
            }
            set
            {
                _trackedArray = value;
                _trackedModules = string.Join(", ", value);
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
            }
        }

        public void SaveSettings()
        {
            Directory.CreateDirectory(scrapYardPath + "/PluginData");
            ConfigNode settingsNode = ConfigNode.CreateConfigFromObject(this);
            settingsNode.Save(settingsPath);
        }
    }
}
