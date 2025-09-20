// Vegetation Studio Pro Integration (version-tolerant)
using System.Reflection;
using UnityEngine;
#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationStudio;
#endif

[AddComponentMenu("Enviro/Integration/VS Pro Integration")]
public class EnviroVegetationStudioPro : MonoBehaviour
{
#if VEGETATION_STUDIO_PRO
    public bool setWindZone = true;
    public bool syncRain = true;
    public bool syncSnow = true;

    void Start()
    {
        if (VegetationStudioManager.Instance == null || EnviroSkyMgr.instance == null) return;

        if (!setWindZone) return;

        var wind = EnviroSkyMgr.instance.Components.windZone;
        var systems = VegetationStudioManager.Instance.VegetationSystemList;
        for (int i = 0; i < systems.Count; i++)
        {
            var sys = systems[i];
            // Try: sys.SelectedWindZone = wind;
            TrySetProperty(sys, "SelectedWindZone", wind);

            // Some VSP versions expose a helper on the manager:
            // VegetationStudioManager.SetWindZone(windZone) – try it reflectively as well.
            TryCall(VegetationStudioManager.Instance, "SetWindZone", new object[] { wind });
        }
    }

    void Update()
    {
        if (VegetationStudioManager.Instance == null || EnviroSkyMgr.instance == null) return;

        float envRain = EnviroSkyMgr.instance.Weather.wetness;
        float envSnow = EnviroSkyMgr.instance.Weather.snowStrength;

        var systems = VegetationStudioManager.Instance.VegetationSystemList;
        for (int i = 0; i < systems.Count; i++)
        {
            var sys = systems[i];

            // Try to reach sys.EnvironmentSettings
            object envSettings = TryGetProperty(sys, "EnvironmentSettings");
            bool needsRefresh = false;

            if (envSettings != null)
            {
                if (syncRain)
                {
                    float? currentRain = TryGetProperty(envSettings, "RainAmount") as float?;
                    if (!currentRain.HasValue || !Mathf.Approximately(currentRain.Value, envRain))
                    {
                        TrySetProperty(envSettings, "RainAmount", envRain);
                        needsRefresh = true;
                    }
                }

                if (syncSnow)
                {
                    float? currentSnow = TryGetProperty(envSettings, "SnowAmount") as float?;
                    if (!currentSnow.HasValue || !Mathf.Approximately(currentSnow.Value, envSnow))
                    {
                        TrySetProperty(envSettings, "SnowAmount", envSnow);
                        needsRefresh = true;
                    }
                }
            }

            if (needsRefresh)
            {
                // sys.RefreshMaterials();
                TryCall(sys, "RefreshMaterials", null);
            }
        }
    }

    // ---- Reflection helpers ----
    static object TryGetProperty(object target, string name)
    {
        if (target == null) return null;
        var pi = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        return pi != null && pi.CanRead ? pi.GetValue(target) : null;
    }

    static bool TrySetProperty(object target, string name, object value)
    {
        if (target == null) return false;
        var pi = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (pi != null && pi.CanWrite)
        {
            // Handle float conversions, etc.
            var destType = pi.PropertyType;
            if (value != null && !destType.IsInstanceOfType(value))
            {
                try { value = System.Convert.ChangeType(value, destType); } catch { }
            }
            pi.SetValue(target, value);
            return true;
        }
        return false;
    }

    static bool TryCall(object target, string method, object[] args)
    {
        if (target == null) return false;
        var mi = target.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (mi == null) return false;
        mi.Invoke(target, args);
        return true;
    }
#endif
}
