using System;
using AwesomeTechnologies.Utility;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem.Wind
{
    public interface IWindController
    {
        string WindControllerID
        {
            get;
        }

        WindControllerSettings Settings
        {
            get;
            set;
        }

        WindControllerSettings CreateDefaultSettings();

        void RefreshSettings();

        void UpdateWind(WindZone _windZone, float _windSpeedFactor);
    }

    [Serializable]
    public class WindControllerSettings : BaseControllerSettings
    {
        public string heading;
        public string description;
        public string windControllerID;
    }
}