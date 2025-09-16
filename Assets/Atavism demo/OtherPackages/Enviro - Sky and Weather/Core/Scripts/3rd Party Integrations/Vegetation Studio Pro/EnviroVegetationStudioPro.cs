// Vegetation Studio Pro Integration (reflection-safe)
// Replaces direct calls to SelectedWindZone and EnvironmentSettings.* with reflection.
// This avoids CS1061 when those members are missing/renamed in your VSP build.

using System;
using System.Reflection;
using UnityEngine;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationStudio; // Keep the original namespace; only members are accessed via reflection.
#endif

[AddComponentMenu("Enviro/Integration/VS Pro Integration (Safe)")]
public class EnviroVegetationStudioPro : MonoBehaviour
{
#if VEGETATION_STUDIO_PRO
    public bool setWindZone = true;
    public bool syncRain = true;
    public bool syncSnow = true;

    void Start()
    {
        if (VegetationStudioManager.Instance == null || EnviroSkyMgr.instance == null)
            return;

        if (!setWindZone) return;

        var vsList = VegetationStudioManager.Instance.VegetationSystemList;
        if (vsList == null) return;

        // Try to set SelectedWindZone if that property exists in this VSP build
        var windZone = EnviroSkyMgr.instance.Components != null
            ? EnviroSkyMgr.instance.Components.windZone
            : null;

        if (windZone == null) return;

        for (int i = 0; i < vsList.Count; i++)
        {
            var vs = vsList[i];
            TrySetProperty(vs, "SelectedWindZone", windZone);
        }
    }

    void Update()
    {
        if (VegetationStudioManager.Instance == null || EnviroSkyMgr.instance == null)
            return;

        var vsList = VegetationStudioManager.Instance.VegetationSystemList;
        if (vsList == null || vsList.Count == 0) return;

        float targetWetness = EnviroSkyMgr.instance.Weather != null ? EnviroSkyMgr.instance.Weather.wetness : 0f;
        float targetSnow = EnviroSkyMgr.instance.Weather != null ? EnviroSkyMgr.instance.Weather.snowStrength : 0f;

        for (int i = 0; i < vsList.Count; i++)
        {
            var vs = vsList[i];

            // Get EnvironmentSettings object if present on this VegetationSystemPro
            if (!TryGetProperty(vs, "EnvironmentSettings", out object envSettings) || envSettings == null)
                continue;

            bool changed = false;

            if (syncRain)
            {
                if (TryGetFloat(envSettings, "RainAmount", out float currentRain) && !Mathf.Approximately(currentRain, targetWetness))
                {
                    TrySetFloat(envSettings, "RainAmount", targetWetness);
                    changed = true;
                }
            }

            if (syncSnow)
            {
                if (TryGetFloat(envSettings, "SnowAmount", out float currentSnow) && !Mathf.Approximately(currentSnow, targetSnow))
                {
                    TrySetFloat(envSettings, "SnowAmount", targetSnow);
                    changed = true;
                }
            }

            // If we changed Rain/Snow, try to call RefreshMaterials() on the VegetationSystemPro
            if (changed)
            {
                TryInvokeMethod(vs, "RefreshMaterials");
            }
        }
    }

    // -------- Reflection helpers --------
    static bool TryGetProperty(object target, string propertyName, out object value)
    {
        value = null;
        if (target == null) return false;
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop == null || !prop.CanRead) return false;
        try { value = prop.GetValue(target, null); return true; } catch { return false; }
    }

    static bool TrySetProperty(object target, string propertyName, object val)
    {
        if (target == null) return false;
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop == null || !prop.CanWrite) return false;
        try { prop.SetValue(target, val, null); return true; } catch { return false; }
    }

    static bool TryGetFloat(object target, string propertyName, out float result)
    {
        result = 0f;
        if (!TryGetProperty(target, propertyName, out object val) || val == null) return false;
        try
        {
            if (val is float f) { result = f; return true; }
            if (val is double d) { result = (float)d; return true; }
            result = Convert.ToSingle(val);
            return true;
        }
        catch { return false; }
    }

    static bool TrySetFloat(object target, string propertyName, float val)
    {
        if (target == null) return false;
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop == null || !prop.CanWrite) return false;
        try
        {
            if (prop.PropertyType == typeof(float)) prop.SetValue(target, val, null);
            else if (prop.PropertyType == typeof(double)) prop.SetValue(target, (double)val, null);
            else prop.SetValue(target, Convert.ChangeType(val, prop.PropertyType), null);
            return true;
        }
        catch { return false; }
    }

    static bool TryInvokeMethod(object target, string methodName)
    {
        if (target == null) return false;
        var mi = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (mi == null) return false;
        try { mi.Invoke(target, null); return true; } catch { return false; }
    }
#endif
}
