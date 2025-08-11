using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class RankingMenuTreeEntry
    {
        public string name;
        public string desc = "";
        public int id = 0;
    }
    
    public class RankingListEntry
    {
        public string name;
        public string value = "";
        public int id = 0;
    }

    public class UIAtavismRanking : UIAtavismWindowBase
    {

        public VisualTreeAsset listEntryTemplate;
        public ListView list;
        public List<UIAtavismRankingListEntry> slots = new List<UIAtavismRankingListEntry>();
        public TreeView menuGrid;
        public List<RankingListEntry> playerList = new List<RankingListEntry>();

        void Start()
        {
            base.Start();
            // AtavismEventSystem.RegisterEvent("ACHIEVEMENT_UPDATE", this);
          
           //SelectRanking(1);
            Hide();
         
        }

          protected override void OnEnable()
        {
            base.OnEnable();

            Hide();
        //    Debug.LogError("UIAtavismRanking OnEnable End");
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            NetworkAPI.RegisterExtensionMessageHandler("ao.RANKING_UPDATE", handleRankingUpdate);
            NetworkAPI.RegisterExtensionMessageHandler("ao.RANKING_LIST", handleRankingListUpdate);
            AtavismEventSystem.RegisterEvent("LOADING_SCENE_END", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("LOADING_SCENE_END", this);
            NetworkAPI.RemoveExtensionMessageHandler("ao.RANKING_UPDATE", handleRankingUpdate);
            NetworkAPI.RemoveExtensionMessageHandler("ao.RANKING_LIST", handleRankingListUpdate);
            base.unregisterEvents();
        }

        protected override bool registerUI()
        {
          //  Debug.LogError("UIAtavismRanking registerUI ");
            if (!base.registerUI())
                return false;
            VisualElement innerPanel = uiWindow.Query<VisualElement>("inner-panel");
            list = innerPanel.Query<ListView>("list-grid");
#if UNITY_6000_0_OR_NEWER    
            ScrollView scrollView = list.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            list.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismRankingListEntry newListEntryLogic = new UIAtavismRankingListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = listEntryTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                slots.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            list.bindItem = (item, index) =>
            {
                var v = playerList[index];
                
                (item.userData as UIAtavismRankingListEntry).UpdateInfo(v.id,v.name, v.value);
                   
            };
            list.selectionType = SelectionType.None;
           
            menuGrid = uiDocument.rootVisualElement.Query<TreeView>("menu");

            menuGrid.makeItem = () => new Label();;
            menuGrid.bindItem = (e, i) =>
            {
                var item = menuGrid.GetItemDataForIndex<RankingMenuTreeEntry>(i);
                (e as Label).text = item.name;
            };
            menuGrid.selectedIndicesChanged += TreeMenuSelected;
            
         //   Debug.LogError("UIAtavismRanking registerUI End");
           
            return true;
        }

        private void TreeMenuSelected(IEnumerable<int> obj)
        {
            // Debug.LogError("TreeMenuSelected "+obj.First());
            if (obj.Count() == 0)
                return;
            var data = menuGrid.GetItemDataForIndex<RankingMenuTreeEntry>(obj.First());
             SelectRanking(data.id);
        }

        public void OnEvent(AtavismEventData eData)
        {
           if (eData.eventType == "LOADING_SCENE_END")
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GET_RANKING_LIST", props);
            }

        }
        private void handleRankingListUpdate(Dictionary<string, object> props)
        {
          
            int num = (int)props["num"];
            // Debug.LogError("Rankings menu "+num);
            var items = new List<TreeViewItemData<RankingMenuTreeEntry>>(num);
            if (num > 0)
            {
              
               
                int a = 0;
                for (int i = 0; i < num; i++)
                {
                    string name = (string)props["name" + i];
                    string desc = (string)props["desc" + i];
                    int id = (int)props["id" + i];
#if AT_I2LOC_PRESET
             name = I2.Loc.LocalizationManager.GetTranslation(name);
#endif
#if AT_I2LOC_PRESET
             desc = I2.Loc.LocalizationManager.GetTranslation(desc);
#endif
                    RankingMenuTreeEntry ms = new RankingMenuTreeEntry();
                    ms.name = name;
                    ms.desc = desc;
                    ms.id = id;
                    var treeViewItemData = new TreeViewItemData<RankingMenuTreeEntry>(i, ms);
                    items.Add(treeViewItemData);
                }
            }
            else
            {
                // for (int ii = 0; ii < slots.Count; ii++)
                //     if (menus.Count > 0)
                //         if (menus[ii].gameObject.activeSelf)
                //             menus[ii].gameObject.SetActive(false);
            }
            // if (menus.Count > 0)
            //     menus[0].Select();
          //  Debug.LogError("handleRankingListUpdate EDN");
          
          menuGrid.SetRootItems(items);
          menuGrid.Rebuild();
          menuGrid.selectedIndex = 0;
        }


        private void handleRankingUpdate(Dictionary<string, object> props)
        {
            playerList.Clear();
            int id = (int)props["id"];
            int num = (int)props["num"];
            if (num > 0)
            {
                int a = 0;
                for (int i = 0; i < num; i++)
                {
                    string name = (string)props["name" + i];
                    int value = (int)props["value" + i];
                    int pos = (int)props["pos" + i];
                    var v = new RankingListEntry();
                    v.id = pos;
                    v.name = name;
                    v.value = value.ToString();
                    playerList.Add(v);
                }
            }
            list.itemsSource = playerList;
            list.Rebuild();
            
        }  
        

        void UpdateDetails()
        {
        }
        public void Show()
        {
            base.Show();
            Dictionary<string, object> props = new Dictionary<string, object>();
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GET_RANKING_LIST", props);
            UpdateDetails();
        }

        public void Hide()
        {
            base.Hide();
        }


        public void SelectRanking(int id)
        {
            // Debug.LogError("Select ranking "+id);
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("id", id);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.GET_RANKING", props);
        }
        
        
        
    }
}