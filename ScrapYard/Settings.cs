using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapYard
{
    public class Settings
    {
        public bool OverrideFunds = false;

        private string[] _trackedArray = null;
        private string _trackedModules = "TWEAKSCALE, PROCEDURAL";
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
        


        public Settings()
        {
            
        }

        public void LoadSettings()
        {
            //TODO: Module Manager
        }

        public void SaveSettings()
        {

        }
    }
}
