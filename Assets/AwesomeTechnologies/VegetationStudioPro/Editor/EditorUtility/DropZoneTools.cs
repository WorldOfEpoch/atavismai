using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationSystem;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AwesomeTechnologies.Utility
{
    public enum DropZoneType
    {
        GrassPrefab,
        PlantPrefab,
        ObjectPrefab,
        LargeObjectPrefab,
        TreePrefab,
        GrassTexture,
        PlantTexture,
        MeshRenderer
    }

    public class DropZoneTools
    {
        private static Type GetDropZoneSystemType(DropZoneType _dropZoneType)
        {
            return _dropZoneType switch
            {
                DropZoneType.GrassPrefab => typeof(GameObject),
                DropZoneType.PlantPrefab => typeof(GameObject),
                DropZoneType.ObjectPrefab => typeof(GameObject),
                DropZoneType.LargeObjectPrefab => typeof(GameObject),
                DropZoneType.TreePrefab => typeof(GameObject),
                DropZoneType.GrassTexture => typeof(Texture2D),
                DropZoneType.PlantTexture => typeof(Texture2D),
                DropZoneType.MeshRenderer => typeof(MeshRenderer),
                _ => typeof(GameObject),
            };
        }

        public static void DrawVegetationItemDropZone(DropZoneType _dropZoneType, VegetationPackagePro _vegetationPackage, ref bool _addedItem)
        {
            Event evt = Event.current;

            Type selectedType = GetDropZoneSystemType(_dropZoneType);
            Texture2D iconTexture = GetDropZoneIconTexture(_dropZoneType);

            Rect dropArea = GUILayoutUtility.GetRect(iconTexture.width, iconTexture.height, GUILayout.ExpandWidth(false));
            GUILayoutUtility.GetRect(5, iconTexture.height, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(dropArea, iconTexture);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    bool hasType = HasDropType(DragAndDrop.objectReferences, selectedType);
                    if (!hasType) return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                            if (draggedObject.GetType() == selectedType)
                                switch (_dropZoneType)
                                {
                                    case DropZoneType.GrassPrefab:
                                        _vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.Grass);
                                        break;
                                    case DropZoneType.PlantPrefab:
                                        _vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.Plant);
                                        break;
                                    case DropZoneType.ObjectPrefab:
                                        _vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.Objects);
                                        break;
                                    case DropZoneType.LargeObjectPrefab:
                                        _vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.LargeObjects);
                                        break;
                                    case DropZoneType.TreePrefab:
                                        _vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.Tree);
                                        break;
                                    case DropZoneType.GrassTexture:
                                        _vegetationPackage.AddVegetationItem(draggedObject as Texture2D, VegetationType.Grass);
                                        break;
                                    case DropZoneType.PlantTexture:
                                        _vegetationPackage.AddVegetationItem(draggedObject as Texture2D, VegetationType.Plant);
                                        break;
                                }
                        _addedItem = true;
                    }
                    break;
            }
        }

        public static void DrawMeshTerrainDropZone(DropZoneType _dropZoneType, MeshTerrain _meshTerrain, ref bool _addedItem)
        {
            Event evt = Event.current;
            Texture2D iconTexture = GetDropZoneIconTexture(_dropZoneType);

            Rect dropArea = GUILayoutUtility.GetRect(iconTexture.width, iconTexture.height, GUILayout.ExpandWidth(false));
            GUILayoutUtility.GetRect(5, iconTexture.height, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(dropArea, iconTexture);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    bool hasType = HasDropComponentType(DragAndDrop.objectReferences, _dropZoneType);
                    if (!hasType) return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            GameObject droppedgo;
                            switch (_dropZoneType)
                            {
                                case DropZoneType.MeshRenderer:
                                    droppedgo = draggedObject as GameObject;
                                    if (!droppedgo) break;
                                    _addedItem = true;
                                    _meshTerrain.AddMeshRenderer(droppedgo, TerrainSourceID.TerrainSourceID1);
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        private static Texture2D GetDropZoneIconTexture(DropZoneType _dropZoneType)
        {
            return _dropZoneType switch
            {
                DropZoneType.GrassPrefab => Resources.Load<Texture2D>("DropZoneIcons/GrassPrefab"),
                DropZoneType.PlantPrefab => Resources.Load<Texture2D>("DropZoneIcons/PlantPrefab"),
                DropZoneType.ObjectPrefab => Resources.Load<Texture2D>("DropZoneIcons/ObjectPrefab"),
                DropZoneType.LargeObjectPrefab => Resources.Load<Texture2D>("DropZoneIcons/LargeObjectPrefab"),
                DropZoneType.TreePrefab => Resources.Load<Texture2D>("DropZoneIcons/TreePrefab"),
                DropZoneType.GrassTexture => Resources.Load<Texture2D>("DropZoneIcons/GrassTexture"),
                DropZoneType.PlantTexture => Resources.Load<Texture2D>("DropZoneIcons/PlantTexture"),
                DropZoneType.MeshRenderer => Resources.Load<Texture2D>("DropZoneIcons/MeshDropZone"),
                _ => Resources.Load<Texture2D>("DropZoneIcons/GrassPrefab"),
            };
        }

        private static bool HasDropType(Object[] _dragObjects, Type _type)
        {
            foreach (Object draggedObject in _dragObjects)
            {
                if (draggedObject.GetType() != _type)
                    continue;
                return true;
            }

            return false;
        }

        private static bool HasDropComponentType(Object[] _dragObjects, DropZoneType _dropZoneType)
        {
            foreach (Object draggedObject in _dragObjects)
            {
                GameObject draggedGO = draggedObject as GameObject;
                if (draggedGO == null)
                    continue;

                if (_dropZoneType == DropZoneType.MeshRenderer)
                    if (draggedGO.GetComponentInChildren<MeshRenderer>() != null)
                        return true;
            }

            return false;
        }
    }
}