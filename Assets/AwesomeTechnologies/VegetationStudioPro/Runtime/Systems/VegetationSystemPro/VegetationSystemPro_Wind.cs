using AwesomeTechnologies.VegetationSystem.Wind;
using System;
using System.Linq;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        private void SetupWindControllers()
        {
            windControllerList.Clear();

            var windControllerTypes = typeof(IWindController).Assembly.GetTypes()
                .Where(x => typeof(IWindController).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance)
                .ToList();

            for (int i = 0; i < windControllerTypes.Count; i++)
            {
                IWindController windController = (IWindController)windControllerTypes[i];
                if (windController == null)
                    continue;

                WindControllerSettings windControllerSettings = GetWindControllerSettings(windController.WindControllerID);
                if (windControllerSettings == null)
                {
                    windControllerSettings = windController.CreateDefaultSettings();
                    windControllerSettingsList.Add(windControllerSettings);
                }
                else
                    windController.Settings = windControllerSettings;

                windControllerList.Add(windController);
            }
        }

        private WindControllerSettings GetWindControllerSettings(string _windControllerID)
        {
            for (int i = 0; i < windControllerSettingsList.Count; i++)
            {
                if (windControllerSettingsList[i] == null)
                    continue;

                if (windControllerSettingsList[i].windControllerID == _windControllerID)
                    return windControllerSettingsList[i];
            }

            return null;
        }

        public void RefreshWindControllerSettings()
        {
            for (int i = 0; i < windControllerList.Count; i++)
                windControllerList[i].RefreshSettings();
        }

        public void UpdateWindControllers()
        {
            for (int i = 0; i < windControllerList.Count; i++)
                windControllerList[i].UpdateWind(selectedWindZone, windSpeedFactor);
        }

        private void SetupSpeedTreeWindBridge() // "SpeedTree WindBridge" -- per camera parent GO => groups per item "bridge GOs" later to keep them within range
        {
            for (int i = 0; i < vegetationStudioCameraList.Count; i++)
            {
                if (vegetationStudioCameraList[i].eVegetationStudioCameraType == EVegetationStudioCameraType.SceneView) continue;
                GameObject stwbGO = vegetationStudioCameraList[i].speedTreeWindBridgeGO = new() { hideFlags = HideFlags.HideAndDontSave, name = "stwb_" + vegetationStudioCameraList[i].selectedCamera.name };
                stwbGO.transform.SetParent(transform, false);   // sort below main sys for better management
            }
        }

        public void UpdateSpeedTreeWindBridge(int _index) // "SpeedTree WindBridge" -- update per camera parent GOs to keep per item "bridge GOs" within range -- don't let the engine "cull" needed data
        {
            if (vegetationStudioCameraList[_index].speedTreeWindBridgeGO == null)
                return;
            vegetationStudioCameraList[_index].speedTreeWindBridgeGO.transform.SetPositionAndRotation(vegetationStudioCameraList[_index].selectedCamera.transform.position, vegetationStudioCameraList[_index].selectedCamera.transform.rotation);
        }

        private void FindWindZone()
        {
            if (selectedWindZone == null)
                selectedWindZone = (WindZone)FindAnyObjectByType(typeof(WindZone));
        }
    }
}