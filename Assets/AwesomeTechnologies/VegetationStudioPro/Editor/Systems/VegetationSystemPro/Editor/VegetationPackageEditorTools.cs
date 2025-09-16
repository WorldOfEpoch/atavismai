using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class VegetationPackageEditorTools
{
    public enum VegetationItemTypeSelection
    {
        AllVegetationItems,
        LargeItems,
        Grass,
        Plants,
        Trees,
        Objects,
        LargeObjects
    }

    public static List<string> CreateVegetationInfoIdList(VegetationPackagePro _vegetationPackage)
    {
        List<string> resultList = new();
        for (int i = 0; i < _vegetationPackage.VegetationInfoList.Count; i++)
            resultList.Add(_vegetationPackage.VegetationInfoList[i].VegetationItemID);
        return resultList;
    }

    public static VegetationItemTypeSelection GetVegetationItemTypeSelection(int _index)
    {
        return _index switch
        {
            0 => VegetationItemTypeSelection.AllVegetationItems,
            1 => VegetationItemTypeSelection.Trees,
            2 => VegetationItemTypeSelection.LargeObjects,
            3 => VegetationItemTypeSelection.Objects,
            4 => VegetationItemTypeSelection.Plants,
            5 => VegetationItemTypeSelection.Grass,
            _ => VegetationItemTypeSelection.AllVegetationItems,
        };
    }

    public static List<string> CreateVegetationInfoIDList(VegetationPackagePro _vegetationPackage, VegetationType[] _vegetationTypes)
    {
        List<string> resultList = new();
        for (int i = 0; i < _vegetationPackage.VegetationInfoList.Count; i++)
            if (_vegetationTypes.Contains(_vegetationPackage.VegetationInfoList[i].VegetationType))
                resultList.Add(_vegetationPackage.VegetationInfoList[i].VegetationItemID);
        return resultList;
    }

    public static void DrawVegetationItemSelector(VegetationPackagePro _vegetationPackage, List<string> _vegetationInfoIdList, int _imageSize, ref string _selectedVegetationItemId)
    {
        AssetPreview.SetPreviewTextureCacheSize(100 + _vegetationPackage.VegetationInfoList.Count);

        VegetationInfoIDComparer vIc = new() { vegetationInfoList = _vegetationPackage.VegetationInfoList };
        _vegetationInfoIdList.Sort(vIc.Compare);

        GUIContent[] imageButtons = new GUIContent[_vegetationInfoIdList.Count];
        for (int i = 0; i < _vegetationInfoIdList.Count; i++)
        {
            VegetationItemInfoPro vegetationItemInfo = _vegetationPackage.GetVegetationInfo(_vegetationInfoIdList[i]);
            if (vegetationItemInfo == null)
                imageButtons[i] = new GUIContent { image = AssetPreviewCache.GetAssetPreview(null) };
            else
            {
                if (vegetationItemInfo.PrefabType == VegetationPrefabType.Mesh)
                    imageButtons[i] = new GUIContent { image = AssetPreviewCache.GetAssetPreview(vegetationItemInfo.VegetationPrefab) };
                else
                    imageButtons[i] = new GUIContent { image = AssetPreviewCache.GetAssetPreview(vegetationItemInfo.VegetationTexture) };
            }
        }

        int imageWidth = _imageSize;
        int columns = (int)math.floor((EditorGUIUtility.currentViewWidth - imageWidth / 2f) / imageWidth);
        int rows = (int)math.ceil((float)imageButtons.Length / columns);
        int gridHeight = (rows) * imageWidth;

        int selectedGridIndex = _vegetationInfoIdList.IndexOf(_selectedVegetationItemId);
        if (selectedGridIndex < 0) selectedGridIndex = 0;

        if (imageButtons.Length > 0 && columns > 0)
            selectedGridIndex = GUILayout.SelectionGrid(selectedGridIndex, imageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

        _selectedVegetationItemId = _vegetationInfoIdList.Count > selectedGridIndex ? _vegetationInfoIdList[selectedGridIndex] : "";
        if (_selectedVegetationItemId != "")
            EditorGUILayout.LabelField("Selected item", _vegetationPackage.GetVegetationInfo(_selectedVegetationItemId).Name);
    }

    public static void DrawVegetationItemSelector(VegetationSystemPro _vegetationSystemPro, VegetationPackagePro _vegetationPackage, ref int _selectedGridIndex, ref int _selectedVegetationItemIndex, ref int _selectionCount, VegetationItemTypeSelection _vegetationItemTypeSelection, int _imageSize)
    {
        if (_vegetationPackage == null)
            return;

        AssetPreview.SetPreviewTextureCacheSize(100 + _vegetationSystemPro.GetMaxVegetationPackageItemCount());

        List<int> vegetationItemIndexList = new(); for (int i = 0; i < _vegetationPackage.VegetationInfoList.Count; i++)
        {
            VegetationItemInfoPro vegetationItemInfo = _vegetationPackage.VegetationInfoList[i];
            switch (_vegetationItemTypeSelection)
            {
                case VegetationItemTypeSelection.AllVegetationItems:
                    vegetationItemIndexList.Add(i);
                    break;
                case VegetationItemTypeSelection.LargeItems:
                    if (vegetationItemInfo.VegetationType == VegetationType.Objects || vegetationItemInfo.VegetationType == VegetationType.LargeObjects || vegetationItemInfo.VegetationType == VegetationType.Tree)
                        vegetationItemIndexList.Add(i);
                    break;
                case VegetationItemTypeSelection.Grass:
                    if (vegetationItemInfo.VegetationType == VegetationType.Grass)
                        vegetationItemIndexList.Add(i);
                    break;
                case VegetationItemTypeSelection.Plants:
                    if (vegetationItemInfo.VegetationType == VegetationType.Plant)
                        vegetationItemIndexList.Add(i);
                    break;
                case VegetationItemTypeSelection.Trees:
                    if (vegetationItemInfo.VegetationType == VegetationType.Tree)
                        vegetationItemIndexList.Add(i);
                    break;
                case VegetationItemTypeSelection.Objects:
                    if (vegetationItemInfo.VegetationType == VegetationType.Objects)
                        vegetationItemIndexList.Add(i);
                    break;
                case VegetationItemTypeSelection.LargeObjects:
                    if (vegetationItemInfo.VegetationType == VegetationType.LargeObjects)
                        vegetationItemIndexList.Add(i);
                    break;
            }
        }

        _selectionCount = vegetationItemIndexList.Count;

        VegetationInfoComparer vIc = new() { vegetationInfoList = _vegetationPackage.VegetationInfoList };
        vegetationItemIndexList.Sort(vIc.Compare);

        GUIContent[] imageButtons = new GUIContent[vegetationItemIndexList.Count];

        for (int i = 0; i < vegetationItemIndexList.Count; i++)
            if (_vegetationPackage.VegetationInfoList[vegetationItemIndexList[i]].PrefabType == VegetationPrefabType.Mesh)
                imageButtons[i] = new GUIContent { image = AssetPreviewCache.GetAssetPreview(_vegetationPackage.VegetationInfoList[vegetationItemIndexList[i]].VegetationPrefab) };
            else
                imageButtons[i] = new GUIContent { image = AssetPreviewCache.GetAssetPreview(_vegetationPackage.VegetationInfoList[vegetationItemIndexList[i]].VegetationTexture) };

        int imageWidth = _imageSize;
        int columns = (int)math.floor((EditorGUIUtility.currentViewWidth - 50) / imageWidth);
        int rows = (int)math.ceil((float)imageButtons.Length / columns);
        int gridHeight = (rows) * imageWidth;

        if (_selectedGridIndex > imageButtons.Length - 1) _selectedGridIndex = 0;
        if (imageButtons.Length > 0 && columns > 0)
            _selectedGridIndex = GUILayout.SelectionGrid(_selectedGridIndex, imageButtons, columns, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

        _selectedVegetationItemIndex = vegetationItemIndexList.Count > _selectedGridIndex ? vegetationItemIndexList[_selectedGridIndex] : 0;
    }

    public static void DrawLODRanges(int _lodCount, float _lod0To1Distance, float _lod1To2Distance, float _lod2To3Distance, float _billboardDistance)
    {
        _lod0To1Distance = math.clamp(_lod0To1Distance, 0, _billboardDistance);
        _lod1To2Distance = math.clamp(_lod1To2Distance, 0, _billboardDistance);
        _lod2To3Distance = math.clamp(_lod2To3Distance, 0, _billboardDistance);

        GUILayout.BeginVertical("box");
        Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 100, EditorGUIUtility.currentViewWidth - 100, 30, 30);

        float width = rect.width;

        rect.width = width / 2f;

        Color lod0Color = new Color(60 / 255f, 70 / 255f, 26 / 255f, 1);
        Color lod1Color = new Color(46 / 255f, 55 / 255f, 67 / 255f, 1);
        Color lod2Color = new Color(64 / 255f, 37 / 255f, 27 / 255f, 1);
        Color lod3Color = new Color(40 / 255f, 64 / 255f, 73 / 255f, 1);

        float lod0Width = width * (_lod0To1Distance / _billboardDistance);
        float lod1Width = width * ((_lodCount > 2 ? (_lod1To2Distance - _lod0To1Distance) : (_billboardDistance - _lod0To1Distance)) / _billboardDistance);
        float lod2Width = width * ((_lodCount > 3 ? (_lod2To3Distance - _lod1To2Distance) : (_billboardDistance - _lod1To2Distance)) / _billboardDistance);
        float lod3Width = width * ((_billboardDistance - _lod2To3Distance) / _billboardDistance);

        _lod1To2Distance -= _lod0To1Distance;
        _lod2To3Distance -= _lod0To1Distance + _lod1To2Distance;
        _billboardDistance -= _lod0To1Distance + _lod1To2Distance + _lod2To3Distance;

        rect.width = lod0Width;
        GUIContent lod0GUIContent = lod0Width < 100 ? new GUIContent { text = _lod0To1Distance.ToString("F0") + "m" } : new GUIContent { text = "LOD 0 - " + _lod0To1Distance.ToString("F0") + "m" };
        if (_lodCount > 0) DrawRect(rect, lod0Color, lod0GUIContent);

        rect.xMin += lod0Width;
        rect.width = lod1Width;
        GUIContent lod1GUIContent = lod1Width < 100 ? new GUIContent { text = _lod1To2Distance.ToString("F0") + "m" } : new GUIContent { text = "LOD 1 - " + _lod1To2Distance.ToString("F0") + "m" };
        if (_lodCount > 1) DrawRect(rect, lod1Color, lod1GUIContent);

        rect.xMin += lod1Width;
        rect.width = lod2Width;
        GUIContent lod2GUIContent = lod2Width < 100 ? new GUIContent { text = _lod2To3Distance.ToString("F0") + "m" } : new GUIContent { text = "LOD 2 - " + _lod2To3Distance.ToString("F0") + "m" };
        if (_lodCount > 2) DrawRect(rect, lod2Color, lod2GUIContent);

        rect.xMin += lod2Width;
        rect.width = lod3Width;
        GUIContent lod3GUIContent = lod3Width < 100 ? new GUIContent { text = _billboardDistance.ToString("F0") + "m" } : new GUIContent { text = "LOD 3 - " + _billboardDistance.ToString("F0") + "m" };
        if (_lodCount > 3) DrawRect(rect, lod3Color, lod3GUIContent);
        //if (lod3Width > 5)
        //{
        //}

        EditorGUILayout.HelpBox("Active \"LOD bias\" is " + QualitySettings.lodBias.ToString("F2") + ", distances are adjusted accordingly", MessageType.Warning);
        EditorGUILayout.HelpBox("For optimal crossfade transitions allow enough space between each LOD\nUse the \"Debug\" tab to help visualize LODs" +
            "\nThe cull/billboard distance is set in the \"Vegetation\" tab", MessageType.Info);

        GUILayout.EndVertical();
    }

    private static readonly GUIStyle LabelStyle = new("Label")
    {
        normal = new GUIStyleState { textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f) : new Color(0f, 0f, 0f) },
    };

    private static readonly Texture2D BackgroundTexture = Texture2D.whiteTexture;
    private static readonly GUIStyle LabelStyleLOD = new("Label")
    {
        normal = new GUIStyleState { background = BackgroundTexture, textColor = Color.white },
        fontStyle = FontStyle.Italic
    };

    public static void DrawRect(Rect position, Color color, GUIContent content = null)
    {
        LabelStyleLOD.alignment = TextAnchor.MiddleCenter;
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUI.Box(position, content ?? GUIContent.none, LabelStyleLOD);
        GUI.backgroundColor = backgroundColor;
    }

    public static void LayoutBox(Color color, GUIContent content = null)
    {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUILayout.Box(content ?? GUIContent.none, LabelStyleLOD);
        GUI.backgroundColor = backgroundColor;
    }

    public static bool DrawHeader(string title, bool state)
    {
        GUILayoutUtility.GetRect(2, 2, 2, 2);

        Rect backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

        Rect labelRect = backgroundRect;
        labelRect.xMin += 16f;
        labelRect.xMax -= 20f;

        Rect foldoutRect = backgroundRect;
        foldoutRect.y += 1f;
        foldoutRect.width = 13f;
        foldoutRect.height = 13f;

        backgroundRect.xMin = 13f;
        //backgroundRect.width += 4f;

        // Background
        float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
        EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

        // Title
        EditorGUI.LabelField(labelRect, GetContent(title), EditorStyles.boldLabel);

        // Active checkbox
        state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

        Event e = Event.current;
        if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
        {
            state = !state;
            e.Use();
        }

        return state;
    }

    public static GUIContent GetContent(string _textAndTooltip)
    {
        if (string.IsNullOrEmpty(_textAndTooltip))
            return GUIContent.none;

        string[] s = _textAndTooltip.Split('|');
        GUIContent content = new(s[0]);

        if (s.Length > 1 && !string.IsNullOrEmpty(s[1]))
            content.tooltip = s[1];

        return content;
    }
}