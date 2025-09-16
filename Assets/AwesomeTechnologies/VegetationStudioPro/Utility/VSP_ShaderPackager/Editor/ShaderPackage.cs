//////////////////////////////////////////////////////
// Shader Packager
// Copyright (c)2021 Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AwesomeTechnologies.ShaderPackager
{
    public class ShaderPackage : ScriptableObject
    {
        public enum SRPTarget
        {
            Standard,
            URP,
            HDRP
        }

        public enum UnityVersion
        {
            Min = 0,
            Unity2022_2 = 20222,
            Unity2022_3 = 20223,
            Unity2023_1 = 20231,
            Unity2023_2 = 20232,
            Unity2023_3 = 20233,
            Unity2024_1 = 20241,
            Unity2024_2 = 20242,
            Unity2024_3 = 20243,
            Unity2025_1 = 20251,
            Unity2025_2 = 20252,
            Unity2025_3 = 20253,
            Max = 30000
        }

        [System.Serializable]
        public class Entry
        {
            public SRPTarget srpTarget = SRPTarget.Standard;
            public UnityVersion UnityVersionMin = UnityVersion.Min;
            public UnityVersion UnityVersionMax = UnityVersion.Max;
            public Shader shader;
            public string shaderSrc;
        }

        public List<Entry> entries = new List<Entry>();
#if __BETTERSHADERS__
        public Shader betterShader;
        public string betterShaderPath;
        public JBooth.BetterShaders.OptionOverrides optionOverrides;
#endif

        public void Pack(bool warnErrors)
        {
#if __BETTERSHADERS__
            if (betterShader != null)
                betterShaderPath = AssetDatabase.GetAssetPath(betterShader);

            if (!string.IsNullOrEmpty(betterShaderPath))
            {
                var assetPath = betterShaderPath;
                if (assetPath.EndsWith(".surfshader"))
                {
                    entries.Clear();
                    ShaderPackage.Entry e;

                    e = new ShaderPackage.Entry();
                    entries.Add(e);
                    e.shaderSrc = JBooth.BetterShaders.BetterShaderImporterEditor.BuildExportShader(JBooth.BetterShaders.ShaderBuilder.RenderPipeline.Standard, optionOverrides, assetPath);
                    e.srpTarget = ShaderPackage.SRPTarget.Standard;
                    e.UnityVersionMin = ShaderPackage.UnityVersion.Unity2022_2;
                    e.UnityVersionMax = ShaderPackage.UnityVersion.Max;

                    e = new ShaderPackage.Entry();
                    entries.Add(e);
                    e.shaderSrc = JBooth.BetterShaders.BetterShaderImporterEditor.BuildExportShader(JBooth.BetterShaders.ShaderBuilder.RenderPipeline.URP2022, optionOverrides, assetPath);
                    e.srpTarget = ShaderPackage.SRPTarget.URP;
                    e.UnityVersionMin = ShaderPackage.UnityVersion.Unity2022_2;
                    e.UnityVersionMax = ShaderPackage.UnityVersion.Max;

                    e = new ShaderPackage.Entry();
                    entries.Add(e);
                    e.shaderSrc = JBooth.BetterShaders.BetterShaderImporterEditor.BuildExportShader(JBooth.BetterShaders.ShaderBuilder.RenderPipeline.HDRP2022, optionOverrides, assetPath);
                    e.srpTarget = ShaderPackage.SRPTarget.HDRP;
                    e.UnityVersionMin = ShaderPackage.UnityVersion.Unity2022_2;
                    e.UnityVersionMax = ShaderPackage.UnityVersion.Max;
                }
                else if (assetPath.EndsWith(".stackedshader"))
                {
                    entries.Clear();
                    ShaderPackage.Entry e;

                    e = new ShaderPackage.Entry();
                    entries.Add(e);
                    e.shaderSrc = JBooth.BetterShaders.StackedShaderImporterEditor.BuildExportShader(JBooth.BetterShaders.ShaderBuilder.RenderPipeline.Standard, optionOverrides, assetPath);
                    e.srpTarget = ShaderPackage.SRPTarget.Standard;
                    e.UnityVersionMin = ShaderPackage.UnityVersion.Unity2022_2;
                    e.UnityVersionMax = ShaderPackage.UnityVersion.Max;

                    e = new ShaderPackage.Entry();
                    entries.Add(e);
                    e.shaderSrc = JBooth.BetterShaders.StackedShaderImporterEditor.BuildExportShader(JBooth.BetterShaders.ShaderBuilder.RenderPipeline.URP2022, optionOverrides, assetPath);
                    e.srpTarget = ShaderPackage.SRPTarget.URP;
                    e.UnityVersionMin = ShaderPackage.UnityVersion.Unity2022_2;
                    e.UnityVersionMax = ShaderPackage.UnityVersion.Max;

                    e = new ShaderPackage.Entry();
                    entries.Add(e);
                    e.shaderSrc = JBooth.BetterShaders.StackedShaderImporterEditor.BuildExportShader(JBooth.BetterShaders.ShaderBuilder.RenderPipeline.HDRP2022, optionOverrides, assetPath);
                    e.srpTarget = ShaderPackage.SRPTarget.HDRP;
                    e.UnityVersionMin = ShaderPackage.UnityVersion.Unity2022_2;
                    e.UnityVersionMax = ShaderPackage.UnityVersion.Max;
                }
            }
#endif

            foreach (var e in entries)
            {
                if (e.shader
#if __BETTERSHADERS__
               && betterShader == null
#endif
                )
                {
                    if (warnErrors)
                        Debug.LogError("Shader is null, cannot pack");
                    break;
                }

                if (e.UnityVersionMax == ShaderPackage.UnityVersion.Min && e.UnityVersionMin == ShaderPackage.UnityVersion.Min)
                    e.UnityVersionMax = ShaderPackage.UnityVersion.Max;

                if (e.shader != null)
                {
                    var path = AssetDatabase.GetAssetPath(e.shader);
                    e.shaderSrc = System.IO.File.ReadAllText(path);
                }
            }
        }

        public string GetShaderSrc()
        {
            UnityVersion curVersion = UnityVersion.Min;
#if UNITY_2022_2_OR_NEWER
            curVersion = UnityVersion.Unity2022_2;
#endif
#if UNITY_2022_3_OR_NEWER
            curVersion = UnityVersion.Unity2022_3;
#endif
#if UNITY_2023_1_OR_NEWER
            curVersion = UnityVersion.Unity2023_1;
#endif
#if UNITY_2023_2_OR_NEWER
            curVersion = UnityVersion.Unity2023_2;
#endif
#if UNITY_2023_3_OR_NEWER
            curVersion = UnityVersion.Unity2023_3;
#endif
#if UNITY_2024_1_OR_NEWER
            curVersion = UnityVersion.Unity2024_1;
#endif
#if UNITY_2024_2_OR_NEWER
            curVersion = UnityVersion.Unity2024_2;
#endif
#if UNITY_2024_3_OR_NEWER
            curVersion = UnityVersion.Unity2024_3;
#endif
#if UNITY_2025_1_OR_NEWER
            curVersion = UnityVersion.Unity2025_1;
#endif
#if UNITY_2025_2_OR_NEWER
            curVersion = UnityVersion.Unity2025_2;
#endif
#if UNITY_2025_3_OR_NEWER
            curVersion = UnityVersion.Unity2025_3;
#endif

            SRPTarget target = SRPTarget.Standard;
#if USING_HDRP
      target = SRPTarget.HDRP;
#endif
#if USING_URP
      target = SRPTarget.URP;
#endif
            string s = null;
            foreach (var e in entries)
            {
                if (target != e.srpTarget)
                    continue;

                // default init state..
                if (e.UnityVersionMax == UnityVersion.Min && e.UnityVersionMin == UnityVersion.Min)
                    e.UnityVersionMax = UnityVersion.Max;

                if (curVersion >= e.UnityVersionMin && curVersion <= e.UnityVersionMax)
                {
                    if (s != null)
                        Debug.LogWarning("Found multiple possible entries of the engine version for the shader");

                    s = e.shaderSrc;
                }
            }

            return s;
        }
    }
}