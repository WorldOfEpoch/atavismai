using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismBuildObjectsList //:  AtUIList<UIAtavismBuildObject>
    {
        private int category = -2;
        private ListView list;
        private VisualTreeAsset listElementTemplate;
        private List<AtavismBuildObjectTemplate> templates = new List<AtavismBuildObjectTemplate>();
        public void SetVisualElement(VisualElement visualElement, VisualTreeAsset template)
        {
            this.listElementTemplate = template;
            list = visualElement.Query<ListView>("list-grid");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = list.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            list.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismBuildObject newListEntryLogic = new UIAtavismBuildObject();
                // Instantiate the UXML template for the entry
                var newListEntry = listElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // slots.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            list.bindItem = (item, index) =>
            {
                // List<AtavismBuildObjectTemplate> templates = WorldBuilder.Instance.GetBuildObjectsOfCategory(category, true);
                (item.userData as UIAtavismBuildObject).UpdateBuildObjectInfo(templates[index]);
            };
            list.selectionChanged += buildingSelected;
            
        }

        private void buildingSelected(IEnumerable<object> obj)
        {
            if (list.selectedItem == null)
                return;
            var selected = list.selectedItem as AtavismBuildObjectTemplate; 
           
                WorldBuilder.Instance.BuildingState = WorldBuildingState.SelectItem;

                if (BuildManager.Instance.Pieces.Count != 0)
                {
                    if (BuilderBehaviour.Instance != null)
                    {
                        BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                        BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
                        bool found = false;
                        foreach (var piece in BuildManager.Instance.Pieces)
                        {
                            if (piece != null && piece.BuildObjDefId == selected.id)
                            {
                                AtavismLogger.LogDebugMessage($"BuildObjectClicked piece {piece.Id} {selected.id} piece.BuildObjDefId={piece.BuildObjDefId} {piece.name}");
                                BuilderBehaviour.Instance.SelectPrefab(piece);
                                found = true;
                                break; // Stop iterating as we found the relevant piece
                            }
                        }

                        AtavismLogger.LogDebugMessage($"BuildObjectClicked {selected.id} found={found}");
                        if (!found)
                        {
                            BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
                        }
                    }
                    else
                    {
                        WorldBuilder.Instance.StartPlaceBuildObject(selected.id);
                    }
                }
            
        }

        public void changeCategory(int category)
        {
            this.category = category;
            Refresh();
        }

        private void Refresh()
        {
            templates = WorldBuilder.Instance.GetBuildObjectsOfObjectCategory(category, true);

            list.itemsSource = templates;
            list.Rebuild();
            list.selectedIndex = -1;
        }

    }
}