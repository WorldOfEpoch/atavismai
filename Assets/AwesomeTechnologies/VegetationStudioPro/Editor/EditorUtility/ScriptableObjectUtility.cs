using UnityEngine;
using UnityEditor;

namespace AwesomeTechnologies.Utility
{
    public static class ScriptableObjectUtility
    {
        /// <summary>
        /// Create a new asset from the <see cref="ScriptableObject"/> type with a unique name in the selected folder of the project window
        /// Asset creation can be cancelled by pressing the escape key when assets are initially being named
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            ProjectWindowUtil.CreateAsset(asset, "New " + typeof(T).Name + ".asset");
        }

        public static T CreateAndReturnAsset<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            ProjectWindowUtil.CreateAsset(asset, "New " + typeof(T).Name + ".asset");
            return asset;
        }
    }
}