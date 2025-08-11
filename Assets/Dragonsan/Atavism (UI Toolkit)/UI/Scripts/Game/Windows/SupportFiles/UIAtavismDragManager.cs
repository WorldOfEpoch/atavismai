using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismDragManager : MonoBehaviour
    {
        [SerializeField] public UIDocument uiDocument;
        private static UIAtavismDragManager instance;
        
        // public VisualTreeAsset dragTemplate;
        
        public VisualElement m_Root;
        public VisualElement m_Background;
        public VisualElement m_Quality;
        public VisualElement m_Icon;
        public Label m_Text;
        public Label m_CooldownText;

        public VisualElement m_CooldownImage;
        public VisualElement m_UiButton;
        [SerializeField] VisualElement m_Overlay;
        public VisualElement hover;
        
        public UIAtavismDraggableSlot source;
        protected Activatable activatableObject;
        public ActivatableType activatableType;
        
        protected Vector2 draggingMinValues, draggingMaxValues, draggingMouseOffset;
        private bool beingDragged;
        protected bool cooldownRun;
        
        protected virtual void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        // Start is called before the first frame update
        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            if (instance != null)
                Destroy(instance);
            instance = this;

            uiDocument.enabled = true;

            m_Root = uiDocument.rootVisualElement.Q<VisualElement>("root-item");
            //Search the root for the SlotContainer Visual Element
            m_Background = m_Root.Q<VisualElement>("activatable-item-background");
            m_Quality = m_Root.Q<VisualElement>("activatable-item-quality");
            m_Icon = m_Root.Query<VisualElement>("activatable-item-icon");
            m_Text = m_Root.Query<Label>("activatable-item-label");
            m_CooldownImage = m_Root.Query<VisualElement>("activatable-item-cooldown-image");
            m_UiButton = m_Root.Query<VisualElement>("activatable-item-ui-button");
            m_CooldownText = m_Root.Query<Label>("activatable-item-cooldown-label");
            m_Overlay = m_Root.Query<VisualElement>("activatable-item-overlay");
            if (m_CooldownImage != null)
            {
                float num = m_Icon.layout.height - 2f;
                float progress = num - Mathf.Max(num * 1, 1f);
                m_CooldownImage.style.top = (StyleLength)progress;
            }

            if (m_CooldownText != null)
                m_CooldownText.text =  "";    
            m_Root.HideVisualElement();
           // Debug.LogError("UIAtavismDragManager.OnEnable() End");
        }


        public void StartDrag(Activatable obj,UIAtavismDraggableSlot source )
        {
       //     Debug.LogError("UIAtavismDragManager.StartDrag() called "+obj+" "+ source);
            if(DragDropManager.CurrentlyDraggedObject!=null)
                DragDropManager.CurrentlyDraggedObject.m_Root.ShowVisualElement();
            
            float width = m_Root.resolvedStyle.width;
            float height = m_Root.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            draggingMinValues.x = 0f;
            draggingMinValues.y = 0f;
            draggingMaxValues.x = canvasWidth - width;
            draggingMaxValues.y = canvasHeight - height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            oldPoition = scaledMousePosition;
            draggingMouseOffset.x = scaledMousePosition.x - m_Root.resolvedStyle.left;
            draggingMouseOffset.y = (canvasHeight - scaledMousePosition.y) - m_Root.resolvedStyle.top;
            
            
            
            this.source = source;
            activatableObject = obj;
           
              if (this.m_Icon == null && m_Root != null)
                this.m_Icon = m_Root.Query<VisualElement>("activatable-item-icon");
            if (this.m_Icon != null)
            {
                this.m_Icon.SetEnabled(true);
                
                if (activatableObject is AtavismInventoryItem)
                {
                    AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;

                    if (item != null && item.Icon != null)
                        this.m_Icon.SetBackgroundImage(item.Icon);
                    else
                        this.m_Icon.SetBackgroundImage(AtavismSettings.Instance.defaultItemIcon);

                }
                else if (activatableObject is AtavismAbility)
                {
                    AtavismAbility ability = (AtavismAbility)activatableObject;
                    if (ability != null && ability.Icon != null)
                        this.m_Icon.SetBackgroundImage(ability.Icon);
                    else
                        this.m_Icon.SetBackgroundImage(AtavismSettings.Instance.defaultItemIcon);

                }
            }
            //this.GetComponent<Image>().sprite = obj.icon;
            if (hover != null)
                hover.SetEnabled(false);
            if (m_Overlay != null && obj is AtavismInventoryItem)
            {
                AtavismInventoryItem item = (AtavismInventoryItem)obj;
                //int l = ClientAPI.GetPlayerObject.
                if (item.ReqLeval > (int)ClientAPI.GetPlayerObject().GetProperty("level"))
                    m_Overlay.SetEnabled(true);
                else
                    m_Overlay.SetEnabled(false);
            }

            if (obj is AtavismInventoryItem)
            {
                AtavismInventoryItem item = (AtavismInventoryItem)obj;
                int count = item.Count;
                if (source is UIAtavismActionBarSlot)
                {
                    count = Inventory.Instance.GetCountOfItem(item.templateId);
                }
                if (count > 1)
                {
                    ApplyText(m_Text, count.ToString());
                }
                else
                {
                    ApplyText(m_Text,"");
                }
                if (m_Quality != null)
                {
                    m_Quality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                    m_Quality.style.borderBottomColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                    m_Quality.style.borderTopColor   = AtavismSettings.Instance.ItemQualityColor(item.quality);
                    m_Quality.style.borderLeftColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                    m_Quality.style.borderRightColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                }
            }
            else
            {
                m_Quality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.abilityQualityColor;
                m_Quality.style.borderBottomColor = AtavismSettings.Instance.abilityQualityColor;
                m_Quality.style.borderTopColor   = AtavismSettings.Instance.abilityQualityColor;
                m_Quality.style.borderLeftColor = AtavismSettings.Instance.abilityQualityColor;
                m_Quality.style.borderRightColor = AtavismSettings.Instance.abilityQualityColor;
            }

        
            // m_Root.ShowVisualElement();
            beingDragged = true;
            RunCooldownUpdate();
        }

     Vector2 oldPoition = Vector2.zero;

        // Update is called once per frame
        void Update()
        {
            if (beingDragged )
            {
                float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
                float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
                float widthScaleFactor = Screen.width / canvasWidth;
                float heightScaleFactor = Screen.height / canvasHeight;
                Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
                Vector2 position = new Vector2(scaledMousePosition.x - m_Root.resolvedStyle.width * 0.5f, (canvasHeight - scaledMousePosition.y) - m_Root.resolvedStyle.height * 0.5f);
       //         Debug.LogError("UIAtavismDragManager.Update() beingDragged "+(oldPoition != position));

                if (oldPoition != position)
                {
                    m_Root.ShowVisualElement();
                    m_Root.style.left = Mathf.Clamp(position.x, draggingMinValues.x, draggingMaxValues.x);
                    m_Root.style.top = Mathf.Clamp(position.y, draggingMinValues.y, draggingMaxValues.y);
                }

                if (Input.GetMouseButtonUp(0))
                    EndDrag();
            }
        }

        public UIAtavismDraggableSlot EndDrag()
        {
            if(!beingDragged)
                return null;
            
             // Debug.LogError("UIAtavismDragManager.EndDrag() called");
            if(DragDropManager.CurrentlyDraggedObject!=null)
                DragDropManager.CurrentlyDraggedObject.Show();
          
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            Vector2 position = new Vector2(scaledMousePosition.x , (canvasHeight - scaledMousePosition.y) );
            
            List<VisualElement> list = AtavismSettings.Instance.getAllElement(position);
            bool found = false;
            List<UIAtavismDraggableSlot> ListOfSend = new List<UIAtavismDraggableSlot>();
            foreach (var v in list)
            {
                if (v.userData is UIAtavismDraggableSlot)
                {
                    // Debug.LogError("UIAtavismDragManager -=>"+v.name+" |"+(v.userData is UIAtavismDraggableSlot)+" slotNum="+((v.userData is UIAtavismDraggableSlot)?(v.userData as UIAtavismDraggableSlot).slotNum+"":""));
                    UIAtavismDraggableSlot target = (UIAtavismDraggableSlot)v.userData;
                    if (!ListOfSend.Contains(target))
                    {
                        ListOfSend.Add(target);
                        target.OnDrop(null);
                        found = true;
                    }
                }
            }
            m_Root.HideVisualElement();
            if(sheduler!=null)
                sheduler.Pause();
            if(!found && source!=null )
                source.Discarded();
            this.source = null;
            beingDragged = false;
            // Debug.LogError("Zerowanie drag");
            DragDropManager.CurrentlyDraggedObject = null;
            return null;
        }


        void RunCooldownUpdate()
        {
            // Check if this is on cooldown
            if (m_CooldownImage == null)
                return;
            if (activatableObject == null)
                return;

            if (activatableObject.GetLongestActiveCooldown() == null)
                return;

            if (sheduler != null)
                sheduler.Pause();
            sheduler = m_Root.schedule.Execute(UpdateCooldown).Every(100);
        }

        public IVisualElementScheduledItem sheduler { get; set; }


        void UpdateCooldown()
        {
            //   Debug.LogError("UpdateCooldown | m_CooldownImage="+m_CooldownImage+" activatableObject="+activatableObject);

            if (m_CooldownImage == null || activatableObject == null)
            {
                sheduler.Pause();
                cooldownRun = false;
                return;
            }

            //   Debug.LogError("UpdateCooldown ||");
            Cooldown c = activatableObject.GetLongestActiveCooldown();

            // while ()
            // {
            cooldownRun = true;
            if (c != null)
            {
                float total = c.length;
                //            float total = c.expiration - (c.expiration-c.length);
                float currentTime = c.expiration - Time.time;
                if (m_CooldownImage != null)
                {
                    float num = m_Icon.layout.height - 2f;
                    float progress = num - Mathf.Max(num * (currentTime / total), 1f);
                    m_CooldownImage.style.top = (StyleLength)progress;
                }

                if (m_CooldownText != null)
                    m_CooldownText.text = Math.Round((decimal)currentTime, 0) > 0
                        ? Math.Round((decimal)currentTime, 0).ToString()
                        : "";
            }

            if (c != null && Time.time < c.expiration)
            {

            }
            else
            {
                sheduler.Pause();
                cooldownRun = false;
                if (m_CooldownText != null)
                    m_CooldownText.text = "";
                if (m_CooldownImage != null) m_CooldownImage.style.top = (StyleLength)m_Icon.layout.height;
            }
        }


        public void ApplyText(Label thisLabelElement, string thisText)
        {
            thisLabelElement.text = thisText;
        }

        public static UIAtavismDragManager Instance
        {
            get { return instance; }
        }
    }
}