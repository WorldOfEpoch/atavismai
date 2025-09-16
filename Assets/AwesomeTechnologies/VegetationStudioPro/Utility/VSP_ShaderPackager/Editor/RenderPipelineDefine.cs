//////////////////////////////////////////////////////
// Shader Packager
// Copyright (c)2021 Jason Booth
//////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

// installs defines for render pipelines, so we can #if USING_HDRP and do stuff. Can't believe Unity doesn't provide this crap, they
// really go out of their way to make it hard to work across pipelines.

namespace JBooth.ShaderPackager
{
    public static class RenderPipelineDefine
    {
        private const string HDRP_PACKAGE = "com.unity.render-pipelines.high-definition";
        private const string URP_PACKAGE = "com.unity.render-pipelines.universal";

        private const string TAG_HDRP = "USING_HDRP";
        private const string TAG_URP = "USING_URP";

        private static ListRequest request;
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            request = Client.List(true);
            EditorApplication.update += ListProgress;
        }

        private static void ListProgress()
        {
            if (request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                {
                    // Find out what packages are installed
                    var packagesList = request.Result.ToList();

                    //Debug.Log("List of offline Unity packages:\n\n" + String.Join("\n", packagesList.Select(x => x.name)) + "\n\n");
                    bool hasHDRP = packagesList.Find(x => x.name.Contains(HDRP_PACKAGE)) != null;
                    bool hasURP = packagesList.Find(x => x.name.Contains(URP_PACKAGE)) != null;

                    DefinePreProcessors(hasHDRP, hasURP);
                }
                else
                {
                    Debug.Log(request.Error.message);
                }

                EditorApplication.update -= ListProgress;
            }
        }

        private static void DefinePreProcessors(bool defineHDRP, bool defineURP)
        {
            string originalDefineSymbols;
            string newDefineSymbols;

            List<string> defined;
            NamedBuildTarget buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            string log = string.Empty;

            originalDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            defined = originalDefineSymbols.Split(';').Where(x => !String.IsNullOrWhiteSpace(x)).ToList();

            Action<bool, string> AppendRemoveTag = (stat, tag) =>
            {
                if (stat && !defined.Contains(tag))
                    defined.Add(tag);
                else if (!stat && defined.Contains(tag))
                    defined.Remove(tag);
            };

            AppendRemoveTag(defineHDRP, TAG_HDRP);
            AppendRemoveTag(defineURP, TAG_URP);

            newDefineSymbols = string.Join(";", defined);
            if (originalDefineSymbols != newDefineSymbols)
                PlayerSettings.SetScriptingDefineSymbols(buildTarget, newDefineSymbols);
        }
    }
}