using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismAchievement : UIAtavismWindowBase
    {
        public VisualTreeAsset listElementTemplate;
        public ListView grid;
        public List<UIAtavismAchievementSlot> achievementList = new List<UIAtavismAchievementSlot>();
        CanvasGroup _canvasGroup;
        // Start is called before the first frame update
        void Start()
        {
            base.Start();
            AtavismAchievements.Instance.GetAchievementStatus();
            if (ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()) != null)
            {
                ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).RegisterPropertyChangeHandler("title", titleHandler);
            }
                Hide();
        }
        

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("ACHIEVEMENT_UPDATE", this);
            if (ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()) != null)
            {
                ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).RegisterPropertyChangeHandler("title", titleHandler);
            }
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("ACHIEVEMENT_UPDATE", this);
            if (ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()) != null)
            {
                ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).RemovePropertyChangeHandler("title", titleHandler);
            }
            base.unregisterEvents();
        }


        protected override bool registerUI()
        {
             if (!base.registerUI())
                return false;
           
            grid = uiWindow.Query<ListView>("list");
#if UNITY_6000_0_OR_NEWER                
            ScrollView scrollView = grid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            grid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismAchievementSlot newListEntryLogic = new UIAtavismAchievementSlot();
                // Instantiate the UXML template for the entry
                var newListEntry = listElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                achievementList.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            grid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismAchievementSlot).SetData(AtavismAchievements.Instance.achivments[index]);
            };
            grid.selectionChanged += SelectEntry;
            return true;
        }

        private void SelectEntry(IEnumerable<object> obj)
        {
            
            
            if (obj.Count() == 0)
                return;
            AtavismAchievement achievement = (AtavismAchievement)obj.First();
            if (achievement != null && achievement.id > 0)
            {
                // Debug.LogError("OnPointerClick " + achievement.id);
                ClientAPI.GetPlayerObject();
                Dictionary<string, object> props = new Dictionary<string, object>();
                if (ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).PropertyExists("title") && ((string)ClientAPI.GetObjectNode(ClientAPI.GetPlayerOid()).GetProperty("title")).Equals(achievement.name))
                    props.Add("id", 0);
                else
                    props.Add("id", achievement.id);
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.SET_ACHIEVEMENTS_TITLE", props);
            }
        }

        public void titleHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateDetails();
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "ACHIEVEMENT_UPDATE")
            {
                UpdateDetails();
            }
        }

        void UpdateDetails()
        {
            grid.itemsSource = AtavismAchievements.Instance.achivments;
            grid.Rebuild();
            grid.selectedIndex = -1;
        }

        public override void Show()
        {
            base.Show();
            AtavismUIUtility.BringToFront(gameObject);
            UpdateDetails();
           
            showing = true;
        }

        public override void Hide()
        {
          base.Hide();
            showing = false;
        }
     
    }
}