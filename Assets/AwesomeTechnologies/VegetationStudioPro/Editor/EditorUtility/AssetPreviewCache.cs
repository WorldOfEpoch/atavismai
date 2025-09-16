using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public class AssetPreviewCache
    {
        public static AssetPreviewCache instance;
        public Dictionary<Object, Texture2D> textureCache = new Dictionary<Object, Texture2D>();

        AssetPreviewCache()
        {
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
        }

        private void OnPlaymodeStateChanged(PlayModeStateChange _playModeStateChange)
        {
            textureCache.Clear();
        }

        public static Texture2D GetCachedAssetPreview(Object _asset)
        {
            if (_asset is Texture2D)
            {
                ValidateInstance();

                if (instance.textureCache.ContainsKey(_asset))
                {
                    Texture2D previewTexture = instance.textureCache[_asset];
                    if (previewTexture)
                        if (previewTexture.width > 2)
                            return previewTexture;

                    instance.textureCache.Remove(_asset);
                    previewTexture = CreateAssetPreview(_asset);
                    instance.textureCache.Add(_asset, previewTexture);
                    return previewTexture;
                }
                else
                {
                    Texture2D previewTexture = CreateAssetPreview(_asset);
                    instance.textureCache.Add(_asset, previewTexture);
                    return previewTexture;
                }
            }
            else
            {
                ValidateInstance();

                if (instance.textureCache.ContainsKey(_asset))
                {
                    Texture2D previewTexture = instance.textureCache[_asset];
                    if (previewTexture)
                        if (previewTexture.width > 2)
                            return previewTexture;

                    instance.textureCache.Remove(_asset);
                    previewTexture = CreateAssetPreview(_asset);
                    instance.textureCache.Add(_asset, previewTexture);
                    return previewTexture;
                }
                else
                {
                    Texture2D previewTexture = CreateAssetPreview(_asset);
                    instance.textureCache.Add(_asset, previewTexture);
                    return previewTexture;
                }
            }
        }

        public static Texture2D GetAssetPreview(Object _asset)
        {
            return AssetPreview.GetAssetPreview(_asset);
        }

        static Texture2D CreateAssetPreview(Object _asset)
        {
            Texture2D previewTexture = AssetPreview.GetAssetPreview(_asset);
            Texture2D convertedTexture = new(2, 2, TextureFormat.ARGB32, true, true);
            if (previewTexture)
                convertedTexture.LoadImage(previewTexture.EncodeToPNG());
            return convertedTexture;
        }

        private static void ValidateInstance()
        {
            if (instance == null)
                instance = new AssetPreviewCache();
        }
    }
}