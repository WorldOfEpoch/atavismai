using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismFactionStances : UIAtavismWindowBase
    {

        [SerializeField] List<UIAtavismFactionStancesListEntry> factions = new List<UIAtavismFactionStancesListEntry>();
        List<string> dKeys = new List<string>();
        List<string> dKeysRegistered = new List<string>();
        [SerializeField] VisualTreeAsset listElementTemplate;
        [SerializeField] VisualElement grid;
        [SerializeField] ListView list;
        AtavismObjectNode node;
   protected override void OnEnable()
        {
            base.OnEnable();
            // uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("faction-panel-container");
            // if (AtavismPlayerShop.Instance.ShopList.Count == 0)
            // {
            //     Hide();
            // }
            // else
            // {
            //     if (autoShowHide)
            //     {
            //         Show();
            //     }
            //     UpdateDisplay();
            // }
        //    Debug.LogError("UIAtavismClaimPanel OnEnable End");
            // Hide();
        }
        protected override void registerEvents()
        {
            // AtavismEventSystem.RegisterEvent("SHOP_LIST_UPDATE", this);
            // UpdateDisplay();
            // Hide();
        }

        protected override void unregisterEvents()
        {
            // AtavismEventSystem.UnregisterEvent("SHOP_LIST_UPDATE", this);
        }
       
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            // uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("faction-panel-container");

            //  tabs = uiWindow.Query<UIButtonToggleGroup>("auction-top-menu");
            //  tabs.OnItemIndexChanged += TopMenuChange;
            //  
            grid = uiWindow.Query<VisualElement>("faction-list");
            list = uiWindow.Query<ListView>("faction-list");
             if (list != null)
             {
#if UNITY_6000_0_OR_NEWER    
                 ScrollView scrollView = list.Q<ScrollView>();
                 scrollView.mouseWheelScrollSize = 19;
#endif
                 list.makeItem = () =>
                 {
                     // Instantiate a controller for the data
                     UIAtavismFactionStancesListEntry newListEntryLogic = new UIAtavismFactionStancesListEntry();
                     // Instantiate the UXML template for the entry
                     var newListEntry = listElementTemplate.Instantiate();
                     // Assign the controller script to the visual element
                     newListEntry.userData = newListEntryLogic;
                     // Initialize the controller script
                     newListEntryLogic.SetVisualElement(newListEntry);
                     factions.Add(newListEntryLogic);
                     // Return the root of the instantiated visual tree
                     return newListEntry;
                 };
                 list.bindItem = (item, index) =>
                 {
                     var entry = (item.userData as UIAtavismFactionStancesListEntry);
                     string faction = (string)ClientAPI.GetPlayerObject().GetProperty(dKeys[index]);
                     string[] fac = faction.Split(new Char[] { ' ' });
                     if (fac.Length == 3)
                     {
                         entry.UpdateDisplay(fac[1], int.Parse(fac[fac.Length - 1]));
                     }
                     else
                     {
                         string facName = "";
                         for (int j = 1; j < fac.Length - 2; j++)
                         {
                             facName += fac[j];
                             if (j < fac.Length - 3)
                                 facName += " ";
                         }

                         entry.UpdateDisplay(facName, int.Parse(fac[fac.Length - 1]));
                     }
                     // ShopListData sld = shopsData[index]; 
                     // entry.SetData(sld.desc, sld.oid);
                 };
             }
             // grid.selectionChanged += SelectEntry;
            // grid.fixedItemHeight = 65;
            
            
            Hide();
       //     Debug.LogError("UIAtavismClaimPanel registerUI End");
            return true;
        }
        // Use this for initialization
        void Start()
        {
            if (ClientAPI.GetPlayerObject() != null)
            {
                node = ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid());
                foreach (string key in ClientAPI.GetPlayerObject().Properties.Keys)
                {
                    if (key.Contains("Reputation"))
                    {
                        if (!key.EndsWith("_t"))
                            if (!dKeys.Contains(key))
                                dKeys.Add(key);
                        //    Debug.LogError(key);
                    }

                }
                if (node != null)
                {
                    foreach (string s in dKeys)
                    {
                        node.RegisterPropertyChangeHandler(s, reputationHandler);
                        dKeysRegistered.Add(s);
                    }
                }

                UpdateDisplay();
            }
            Hide();
        }

        private void reputationHandler(object sender, PropertyChangeEventArgs args)
        {

            UpdateDisplay();
        }

        // Update is called once per frame
        void UpdateDisplay()
        {
            if (list != null)
            {
                list.itemsSource = dKeys;
                list.Rebuild();
                list.selectedIndex = -1;
            }

            if (grid != null)
            {
                grid.Clear();

                // int i = 0;
                foreach (string s in dKeys)
                {
                    // if (i >= factions.Count)
                    //     factions.Add(Instantiate(prefab, transform));
                    // factions[i].gameObject.SetActive(true);
                    string faction = (string)ClientAPI.GetPlayerObject().GetProperty(s);
                    string[] fac = faction.Split(new Char[] { ' ' });
                    UIAtavismFactionStancesListEntry row = new UIAtavismFactionStancesListEntry();
                    // Instantiate the UXML template for the entry
                    var newListEntry = listElementTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = row;
                    // Initialize the controller script
                    row.SetVisualElement(newListEntry);
                    grid.Add(newListEntry);
                    if (fac.Length == 3)
                    {
                        row.UpdateDisplay(fac[1], int.Parse(fac[fac.Length - 1]));
                    }
                    else
                    {
                        string facName = "";
                        for (int j = 1; j < fac.Length - 2; j++)
                        {
                            facName += fac[j];
                            if (j < fac.Length - 3)
                                facName += " ";
                        }

                        row.UpdateDisplay(facName, int.Parse(fac[fac.Length - 1]));
                    }
                    // i++;
                }
                // for (int j = i; j < factions.Count; j++)
                //     factions[i].gameObject.SetActive(false);
            }
        }
        private void OnDestroy()
        {
            if (node != null)
            {
                foreach (string s in dKeysRegistered)
                {
                    node.RemovePropertyChangeHandler(s, reputationHandler);
                }
                dKeysRegistered.Clear();
            }
        }


        public override void Show()
        {
            base.Show();
           
            // AtavismUIUtility.BringToFront(gameObject);
            if (ClientAPI.GetPlayerObject() != null)
            {
                node = ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid());
                foreach (string key in ClientAPI.GetPlayerObject().Properties.Keys)
                {
                    if (key.Contains("Reputation"))
                    {
                        if (!key.EndsWith("_t"))
                            if (!dKeys.Contains(key))
                                dKeys.Add(key);
                        //    Debug.LogError(key);
                    }

                }
                if (node != null)
                {
                    foreach (string s in dKeys)
                    {
                        node.RegisterPropertyChangeHandler(s, reputationHandler);
                        dKeysRegistered.Add(s);
                    }
                }

                UpdateDisplay();
            }
            else
            {
                Hide();
            }
        }

        public override void Hide()
        {
            base.Hide();
           
        }

    }
}