using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.Utility.Extentions
{
    public static class TextureExtension
    {
        public static void SaveTextureToFile(Texture2D _texture, string _fileName)
        {
#if UNITY_EDITOR
            File.WriteAllBytes(_fileName + ".png", _texture.EncodeToPNG());
#endif
        }

        public static void ImportTexture(string _path, int _textureType, int _textureResolution)    // manually import textures into the project w/ given parameters based on type IDs
        {
#if UNITY_EDITOR
            if (_path == "")
                return;

            AssetDatabase.Refresh();    // refresh project files to get (newest) data of saved on-disk files -- mandatory to save/write certain info

            TextureImporter tImporter = AssetImporter.GetAtPath(_path) as TextureImporter;
            if (tImporter != null)
            {
                switch (_textureType)
                {
                    case 0:
                        tImporter.textureType = TextureImporterType.Default;
                        tImporter.alphaIsTransparency = true;
                        tImporter.maxTextureSize = _textureResolution;
                        break;
                    case 1: // normal textures
                        tImporter.textureType = TextureImporterType.Default;  // don't use "NormalMap" here as in the billboard shader we don't use unwrapping for normals as well
                        tImporter.alphaIsTransparency = true;
                        tImporter.maxTextureSize = _textureResolution;
                        break;
                    case 2: // texture masks
                        tImporter.SetPlatformTextureSettings(new() { format = TextureImporterFormat.RGBA32 });  // called first due to overrides
                        tImporter.filterMode = FilterMode.Point;
                        tImporter.isReadable = true;
                        tImporter.mipmapEnabled = false;
                        tImporter.maxTextureSize = _textureResolution;
                        tImporter.textureType = TextureImporterType.Default;
                        break;
                }

                tImporter.SaveAndReimport();
            }
#endif
        }
    }
}