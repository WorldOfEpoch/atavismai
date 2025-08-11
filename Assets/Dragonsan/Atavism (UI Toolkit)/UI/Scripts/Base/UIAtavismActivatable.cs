using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using UnityEngine.UIElements;
using Atavism.UI;
using System.Collections.Generic;
using MouseButton = UnityEngine.UIElements.MouseButton;

namespace Atavism
{
    [Serializable]
    public class UIAtavismActivatable //: VisualElement
    {
        public VisualElement m_Root;
        public VisualElement m_Background;
        public VisualElement m_Quality;
        public VisualElement m_Icon;
        public Label m_countText;
        public VisualElement Quality => m_Quality;
        public Label CountText => m_countText;
        public Label m_CooldownText;

        public VisualElement m_CooldownImage;
        public VisualElement m_UiButton;
        [SerializeField] VisualElement m_Overlay;

        public VisualElement hover;
        public GameObject toggleOn;
        IEnumerator cor;
        //[SerializeField]
        public Vector2 defaultSlotSize;
        public bool sizeFromParent;
        protected bool beingDragged;
        public UIAtavismDraggableSlot releaseTarget;
        public UIAtavismDraggableSlot source;
        protected Activatable activatableObject;
        public ActivatableType activatableType;
        protected bool preventDiscard ;
        protected UIAtavismActivatable link;
        protected bool cooldownRun;
        bool corutRuning;
        public VisualTreeAsset thisUIAtavismSlotItem;
        public VisualElement LinkedVisualElement { get; private set; }
        VisualElement m_selected;
        private float _time;
        
 #if AT_MOBILE
        // GameObject itemOption; //PopuGames
        // [SerializeField]
        // VisualElement selected;//PopuGames{}
        // [SerializeField]
        bool isSelected = false;
        private float selectTime = 0f;
        bool isOnActionBar = false;
        public string state = "";
        public AtavismInventoryItem item;
#endif
        
        
         ~UIAtavismActivatable()
        {
            // Assuming the visualElement you registered the callbacks on is named "visualElement"
            if (m_Root != null)
            {
                m_Root.UnregisterCallback<PointerDownEvent>(OnDragStart, TrickleDown.TrickleDown);
                m_Root.UnregisterCallback<PointerMoveEvent>(OnDragUpdated , TrickleDown.TrickleDown);
                m_Root.UnregisterCallback<PointerUpEvent>(OnDragEnd, TrickleDown.TrickleDown);
                m_Root.UnregisterCallback<PointerEnterEvent>(OnMouseEnter, TrickleDown.TrickleDown);
                m_Root.UnregisterCallback<PointerLeaveEvent>(OnMouseLeave, TrickleDown.TrickleDown);
                // m_Root.UnregisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
            }

            AtavismEventSystem.UnregisterEvent("COOLDOWN_UPDATE", OnEvent);
            AtavismEventSystem.UnregisterEvent("ATOGGLE_UPDATE", OnEvent);
            AtavismEventSystem.UnregisterEvent("ITEM_RELOAD", OnEvent);
            AtavismEventSystem.UnregisterEvent("ABILITY_UPDATE", OnEvent);
#if AT_MOBILE
            AtavismEventSystem.UnregisterEvent("ITEM_DESELECT", OnEvent);
#endif
            if (ClientAPI.GetPlayerObject() != null)
                ClientAPI.GetPlayerObject().RemovePropertyChangeHandler("level", LevelHandler);
        }

      

        public UIAtavismActivatable(VisualElement linkedVisualElement)
        {
            
            LinkedVisualElement = linkedVisualElement;
            thisUIAtavismSlotItem = AtavismSettings.Instance.UIActivatableUXML;
            //LinkedVisualElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            m_Root = thisUIAtavismSlotItem.CloneTree();
            LinkedVisualElement.Add(m_Root);

            
            m_Root.pickingMode = PickingMode.Position;  // Important for drag and drop.
            // Dragger thisDragger = new Dragger();
            // thisDragger.clampToParentEdges=false;
            DragManipulator thisDragger = new DragManipulator();
            // thisDragger.
            Clickable thisClickable = new Clickable((v)=>{OnClick(v);});
            thisClickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse, clickCount = 1, modifiers = EventModifiers.Alt });
            thisClickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
            thisClickable.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2 });
            // thisClickable.clickedWithEventInfo += OnClick;
            
             m_Root.AddManipulator(thisClickable);       // Makes the element draggable.

           // m_Root.AddManipulator(thisDragger);       // Makes the element draggable.
            // Register UI Toolkit events
            
            m_Root.RegisterCallback<PointerDownEvent>(OnDragStart, TrickleDown.TrickleDown);
            m_Root.RegisterCallback<PointerUpEvent>(OnDragEnd, TrickleDown.TrickleDown);
            m_Root.RegisterCallback<PointerMoveEvent>(OnDragUpdated, TrickleDown.TrickleDown);
            m_Root.RegisterCallback<PointerEnterEvent>(OnMouseEnter);
            m_Root.RegisterCallback<PointerLeaveEvent>(OnMouseLeave);
            
            
            m_Root.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
            
            //Search the root for the SlotContainer Visual Element
            m_Background = m_Root.Q<VisualElement>("activatable-item-background");
            m_Quality = m_Root.Q<VisualElement>("activatable-item-quality");
            m_Icon = m_Root.Query<VisualElement>("activatable-item-icon");
            m_countText = m_Root.Query<Label>("activatable-item-label");
            m_CooldownImage = m_Root.Query<VisualElement>("activatable-item-cooldown-image");
            m_UiButton = m_Root.Query<VisualElement>("activatable-item-ui-button");
            m_CooldownText = m_Root.Query<Label>("activatable-item-cooldown-label");
            m_Overlay = m_Root.Query<VisualElement>("activatable-item-overlay");
            m_selected = m_Root.Query<VisualElement>("activatable-item-selected");
            defaultSlotSize = new Vector2(48f, 48f);
            sizeFromParent = true;
            beingDragged = false;
            releaseTarget = null;
            source = null;

            preventDiscard = false;
            cooldownRun = false;
            corutRuning = false;
        
            // if (m_CooldownImage != null)
            // {
                AtavismEventSystem.RegisterEvent("COOLDOWN_UPDATE", OnEvent); // HNG 10-1
            // }
            if (m_CooldownImage != null)
            {
                float num = m_Icon.layout.height - 2f;
                float progress = num - Mathf.Max(num * 1, 1f);
                m_CooldownImage.style.top = (StyleLength)progress;
            }

            if (m_CooldownText != null)
                m_CooldownText.text =  "";    

            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler("level", LevelHandler);
            AtavismEventSystem.RegisterEvent("ATOGGLE_UPDATE", OnEvent);// HNG 10-1
            if (toggleOn != null)
            {
                toggleOn.gameObject.SetActive(false);
            }
            AtavismEventSystem.RegisterEvent("ITEM_RELOAD", OnEvent);
            AtavismEventSystem.RegisterEvent("ABILITY_UPDATE", OnEvent);
#if AT_MOBILE
            AtavismEventSystem.RegisterEvent("ITEM_DESELECT", OnEvent);
#endif
            // RunCooldownUpdate();

            m_Root.MarkDirtyRepaint();

        }

    
        private void OnMouseLeave(PointerLeaveEvent pointerLeaveEvent)
        {
#if !AT_MOBILE
            UIAtavismTooltip.Instance.Hide();
#endif            
        }

        private void OnMouseEnter(PointerEnterEvent pointerEnterEvent)
        {
#if !AT_MOBILE
            ShowTooltip(LinkedVisualElement);
#endif
        }

        private bool OverlapsTarget(VisualElement slot)
    {
        return m_UiButton.worldBound.Overlaps(slot.worldBound);
    }

    private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots)
    {
        List<VisualElement> slotsList = slots.ToList();
        float bestDistanceSq = float.MaxValue;
        VisualElement closest = null;
        foreach (VisualElement slot in slotsList)
        {
            Vector3 displacement =
                RootSpaceOfSlot(slot) - m_UiButton.transform.position;
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq)
            {
                bestDistanceSq = distanceSq;
                closest = slot;
            }
        }
        return closest;
    }

    private Vector3 RootSpaceOfSlot(VisualElement slot)
    {
        Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
        return m_Root.WorldToLocal(slotWorldSpace);
    }




        void OnEnableThis()
        {

        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "COOLDOWN_UPDATE")
            {
              //  Debug.LogError("COOLDOWN_UPDATE "+activatableObject!=null? activatableObject.name:"");
                // Update 
                RunCooldownUpdate();
            }
            else if (eData.eventType == "ATOGGLE_UPDATE")
            {
                RunToggleUpdate();
            }
            else if (eData.eventType == "ITEM_RELOAD")
            {
                // Debug.LogError("UGUIAtavismActivatable ITEM_RELOAD");
                if (activatableObject is AtavismInventoryItem)
                {
                    AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;
                    if (item != null)
                        activatableObject = AtavismPrefabManager.Instance.LoadItem(item);
                }
            }
            else if (eData.eventType == "ABILITY_UPDATE")
            {
                //   Debug.LogError("UGUIAtavismActivatable ABILITY_UPDATE");
                if (activatableObject is AtavismAbility)
                {
                    AtavismAbility ability = (AtavismAbility)activatableObject;
                    if (ability != null)
                        activatableObject = AtavismPrefabManager.Instance.LoadAbility(ability);
                }
            }
#if AT_MOBILE
            else if (eData.eventType == "ITEM_DESELECT")
            {
                if(isSelected && selectTime + 0.2f<Time.time)
                    Deselected();
            }
#endif            
        }


        void RunToggleUpdate()
        {
            if (activatableObject is AtavismAbility)
            {
                AtavismAbility ability = (AtavismAbility)activatableObject;
                if (ability.toggle)
                {
                    bool toggle = Abilities.Instance.isToggleActive(ability.id);
                    if (toggleOn != null)
                    {
                        if (toggle)
                            toggleOn.gameObject.SetActive(true);
                        else
                            toggleOn.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (toggleOn != null)
                    {
                        toggleOn.gameObject.SetActive(false);
                    }
                }
            }
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
            sheduler = LinkedVisualElement.schedule.Execute(UpdateCooldown).Every(100);
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

            corutRuning = true;
            //   WaitForSeconds delay = new WaitForSeconds(0.3f);
            Cooldown c = activatableObject.GetLongestActiveCooldown();

            // while ()
            // {
                cooldownRun = true;
                if (c != null)
                {
                  //  Debug.LogError("UpdateCooldown |||");
                    float total = c.length;
                    //            float total = c.expiration - (c.expiration-c.length);
                    float currentTime = c.expiration - Time.time;
                    if (m_CooldownImage != null)
                    {
                        float num = m_Icon.layout.height - 2f;
                        float progress = num - Mathf.Max(num * (currentTime / total), 1f);
                        m_CooldownImage.style.top = (StyleLength)progress;
                    }

                    string cooldownString = "";
                    decimal timeLeft = Math.Round((decimal)currentTime, 0);
                    if (timeLeft > 86400)
                    {
                        int days = (int)timeLeft / 86400;
                        cooldownString = "" + (int)days + "d";
                    }
                    else if (timeLeft > 3600)
                    {
                        int hours = (int)timeLeft / 3600;
                        cooldownString = "" + (int)hours + "h";
                    }
                    else if (timeLeft > 60)
                    {
                        int minutes = (int)timeLeft / 60;
                        cooldownString = "" + (int)minutes + "m";
                    }
                    else if (timeLeft > 0)
                    {
                        cooldownString = "" + (int)timeLeft + "s";
                    }

                    if (m_CooldownText != null)
                    {
                        m_CooldownText.text = cooldownString;
                    }
                }

                if(c != null && Time.time < c.expiration){
                  //  Debug.LogError("UpdateCooldown |V");
                   
             }
             else
             {
                // Debug.LogError("UpdateCooldown End");
                 sheduler.Pause();
                 cooldownRun = false;
                 if (m_CooldownText != null)
                     m_CooldownText.text = "";
                 if (m_CooldownImage != null) m_CooldownImage.style.top = (StyleLength) m_Icon.layout.height;

                 corutRuning = false;
             }

            
        }
 
        private void OnDragStart(PointerDownEvent evt)
        {
            
            _time = Time.time;
          //  Debug.LogError("UIAtavismActivatable.OnDragStart button="+evt.button);
            if (evt.button == (int)MouseButton.RightMouse)
                return;
            if (link != null)
                return;
            
            delayStartDrag();

        }


        void delayStartDrag()
        {
            //   Debug.LogError("OnDragStart |");
            dragged = false;
            beingDragged = true;

            if (activatableObject is AtavismAbility)
            {
                AtavismAbility ability = (AtavismAbility)activatableObject;
                if (ability.passive)
                    return;
            }
           
        }
        protected Vector2 draggingMinValues, draggingMaxValues, draggingMouseOffset;

        public void update()
        {
            // Debug.LogError("UIAtavismActivatable.update "+beingDragged);
            if (Input.GetMouseButtonUp(0))
            {
                // Debug.LogError("UIAtavismActivatable.update button "+beingDragged+" "+dragged);
                if (dragged && beingDragged)
                {
                   // Debug.LogError("UIAtavismActivatable.update End Drag "+beingDragged+" "+dragged);
                    UIAtavismDragManager.Instance.EndDrag();
                }
                beingDragged = false;
            }
#if AT_MOBILE        
                        if (isSelected && (activatableObject is AtavismInventoryItem))
                        {
                            if (UIAtavismTooltip.Instance != null && !UIAtavismTooltip.Instance.IsVisible)
                            {
                                isSelected = false;
                                m_selected.HideVisualElement();
                                // itemOption.GetComponent<UGUITooltip>().Hide();
                            }
                        }
#endif    
            
        }
        bool dragged = false;
        private void OnDragUpdated(PointerMoveEvent evt)
        {
            
            // Debug.LogError("UIAtavismActivatable.OnDragUpdated button="+eventData.button);

            if (beingDragged)
            {
                if (!dragged)
                {
                    dragged = true;
                    UIAtavismTooltip.Instance.Hide();
                    //Debug.LogError("Start Drag ");
                    DragDropManager.CurrentlyDraggedObject = this;
                    UIAtavismDragManager.Instance.StartDrag(activatableObject, source);
                    AtavismCursor.Instance.UguiIconBeingDragged = true;
                }
            }
            
        }

    /// <summary>
    /// Raises the end drag event. This is called after the OnDrop is run for the slot.
    /// </summary>
    /// <param name="eventData">Event data.</param>
    private void OnDragEnd(PointerUpEvent eventData)
    {
      //  Debug.LogError("UIAtavismActivatable.OnDragEnd beingDragged="+beingDragged+ " "+dragged);
        beingDragged = false;
        if (DragDropManager.CurrentlyDraggedObject == null)
            return;
        if (eventData.button == (int)MouseButton.RightMouse)
            return;
        // Debug.LogError("UIAtavismActivatable.OnDragEnd button="+eventData.button);
        UIAtavismDragManager.Instance.EndDrag();
        m_Root.style.opacity = 1;
        m_Root.ShowVisualElement();

        if (link != null && !beingDragged)
                return;
        if (activatableObject is AtavismInventoryItem)
        {
            AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;
            ItemPrefabData ipd = AtavismPrefabManager.Instance.GetItemTemplateByID(item.TemplateId);
            if (ipd != null)
            {
                if (ipd.audioProfile > 0)
                {
                    ItemAudioProfileData iapd =  AtavismPrefabManager.Instance.GetItemAudioProfileByID(ipd.audioProfile);
                    if (iapd != null)
                    {
                        AtavismInventoryAudioManager.Instance.PlayAudio(iapd.drag_end, ClientAPI.GetPlayerObject().GameObject);
                    }
                }
            }
        }
        
        
            //Debug.LogError("Got drag end");
            beingDragged = false;
            AtavismCursor.Instance.UguiIconBeingDragged = false;

            if (releaseTarget == null) {
                source.Discarded();
                return;
            }

            DragDropManager.CurrentlyDraggedObject = null;
            // If the drop target is a reference slot and the source isn't then return a copy back to the source
            if (releaseTarget.SlotBehaviour == DraggableBehaviour.Reference && releaseTarget != source)
            {

                 // this.releaseTarget = source;
               
                return;
            }

            // If the drop target is a temporarty slot and the source isn't then return a copy back to the source
            if (releaseTarget.SlotBehaviour == DraggableBehaviour.Temporary && releaseTarget != source)
            {
                if (source.SlotBehaviour != DraggableBehaviour.Temporary)
                {
                    // check this to put the parent object in the other slot
                    //this.transform.SetParent(source.transform, false);
                }
                else
                {
                    source = releaseTarget;
                }
             
                this.releaseTarget = source;

                return;
            }
            // check this - see above, refactor to single process
            /*
        this.transform.SetParent(releaseTarget.transform);
            */
        if (releaseTarget != source)
        {
            source.ClearChildSlot();

        }
        else
        {
            // check this - see above, refactor to single process
            if (preventDiscard)
            {
                preventDiscard = false;
                return;
            }

            source.Discarded();
        }
#if AT_MOBILE
            if ((activatableObject is AtavismInventoryItem))
            {
                item = (AtavismInventoryItem)activatableObject;
                Selected();
                Clicked();
            }
#endif
        // eventData.StopPropagation();
        }

        public void ApplyTexture(VisualElement thisElement, Texture2D thisTexture)
        {
            thisElement.SetBackgroundImage(thisTexture);
        }

        private float lastClick = 0;
        private void OnClick(EventBase eventBase)
        {
            if (Time.time - lastClick < 0.05f)
                return;
            lastClick = Time.time;
             // Debug.LogError("OnClick " +(eventBase is ClickEvent)+" "+(eventBase is MouseDownEvent)+" "+(eventBase is MouseUpEvent)+" "+(eventBase is PointerDownEvent)+" "+(eventBase is PointerUpEvent));
             int button = 1;
             int clickCount = 1;
             if (eventBase is ClickEvent)
             {
                 ClickEvent evt = (ClickEvent)eventBase;
                 button = evt.button;
                 clickCount = evt.clickCount;
                 evt.StopPropagation();
             }
             else if (eventBase is MouseUpEvent)
             {
                 MouseUpEvent evt = (MouseUpEvent)eventBase;
                 button = evt.button;
                 clickCount = evt.clickCount;
                 evt.StopPropagation();
             }

             // Debug.LogError("OnClick " + evt.button + " " + evt.clickCount);
#if AT_MOBILE
                  Clicked();
        }

        public void Clicked()
        {
        
            if (activatableObject is AtavismInventoryItem && !isOnActionBar)
            {
                item = (AtavismInventoryItem)activatableObject;
                if ( UIAtavismTooltip.Instance != null)
                {
                    UIAtavismTooltip.Instance.uIAtavismActivatable = this;
                }
                else
                {
                    source.Activate();
                }

            }
            else
            {
                source.Activate();
            }
            // UIAtavismTooltip.Instance.Hide();
            if (!isSelected && !(source is UIAtavismActionBarSlot))
            {
                ShowTooltip(m_Root);
                Selected();
                UIAtavismTooltip.Instance.ActivatePanel(state, item);
            }
            else
            {
                UIAtavismTooltip.Instance.Hide();
                Deselected();
            }
        }
      
        public void Selected()
        {
            if (activatableObject is AtavismInventoryItem && !isOnActionBar)
            {
                isSelected = true;
                selectTime = Time.time;
                m_selected.ShowVisualElement();
                AtavismEventSystem.DispatchEvent("ITEM_DESELECT", new String[]{});
            }
        }
        public void Deselected()
        {
            if (activatableObject is AtavismInventoryItem && !isOnActionBar)
            {
                isSelected = false;
                m_selected.HideVisualElement();
                
            }
        }


        public void Divide(int count) //PopuGames
        {
            if (activatableObject is AtavismInventoryItem)
            {
                AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;
                if (item.Count > count)
                {
                    Inventory.Instance.CreateSplitStack(item, count);
                }
            }

        }

        public void OnClick()
        {
            int button = 1;
            int clickCount = 1;
#else

#endif
            
            // if(shedulerDrag!=null)
                // shedulerDrag.Pause();
            _time = Time.time;
          //  Debug.LogError("OnClick");
            beingDragged = false;
            // m_UiButton.CapturePointer(eventData.pointerId);
            //   if (!cooldownRun) {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                if (activatableObject is AtavismInventoryItem && source.SlotBehaviour == DraggableBehaviour.Standard)
                {
#if !AT_MOBILE                         
                    AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;
                    if (button == (int)PointerEventData.InputButton.Right)
                    {
                        Inventory.Instance.CreateSplitStack(item, 1);
                    }
                    else
                    {
                        Inventory.Instance.CreateSplitStack(item, item.Count / 2);
                    }
                    return;
#endif                        
                }
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (activatableObject is AtavismInventoryItem)
                {
                    AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;

#if AT_I2LOC_PRESET
                UIAtavismChatManager.Instance.uiChatTextField.text .Insert(0,"<link=item#" + item.TemplateId + "><" + ColorTypeConverter.ToRGBHex(AtavismSettings.Instance.ItemQualityColor(item.quality)) + ">" + I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name) + "</color></link>");
#else
                    UIAtavismChatManager.Instance.uiChatTextField.text.Insert(0,"<link=item#" + item.TemplateId + "><" + ColorTypeConverter.ToRGBHex(AtavismSettings.Instance.ItemQualityColor(item.quality)) + ">" + item.name + "</color></link>");

#endif
                    //EventSystem.current.SetSelectedGameObject(UIAtavismChatManager.Instance.uiChatTextField.gameObject);
                }
                else if (activatableObject is AtavismAbility)
                {
                    AtavismAbility ability = (AtavismAbility)activatableObject;
#if AT_I2LOC_PRESET
               UIAtavismChatManager.Instance.uiChatTextField.text .Insert(0, "<link=ability#" + ability.id + "><" + ColorTypeConverter.ToRGBHex(Color.white) + ">" + I2.Loc.LocalizationManager.GetTranslation("Ability/" + ability.name) + "</color></link>");
#else
                    UIAtavismChatManager.Instance.uiChatTextField.text.Insert(0, "<link=ability#" + ability.id + "><" + ColorTypeConverter.ToRGBHex(Color.white) + ">" + ability.name + "</color></link>");
#endif
                   // EventSystem.current.SetSelectedGameObject(UIAtavismChatManager.Instance.uiChatTextField.gameObject);
                }
                return;
            }
            UIAtavismTooltip.Instance.Hide();
#if !AT_MOBILE
            if (
                !((
                        (
                            (button == (int)MouseButton.LeftMouse && clickCount == 2) || 
                            button == (int)MouseButton.RightMouse) && !(source is UIAtavismActionBarSlot)
                    ) || (source is UIAtavismActionBarSlot)|| (source is UIAtavismAbilitySlot)
                )
            )
                return;
#endif

            if (DragDropManager.CurrentlyDraggedObject == null &&  !AtavismCursor.Instance.UguiIconBeingDragged)
            {
                //  UnityEngine.Debug.LogError(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt ")+"Got stat update message for nonexistent object: " + oid+" keys="+keys1);
              //   Debug.LogError(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt ")+" Activate "+(eventBase is ClickEvent)+" | "+(eventBase is MouseUpEvent));
                source.Activate();
            }

            // eventData.StopPropagation();
#if AT_MOBILE
            UIAtavismTooltip.Instance.Hide();
#endif
            //   }
        }

        public void LevelHandler(object sender, PropertyChangeEventArgs args)
        {
            if (m_Overlay != null && activatableObject is AtavismInventoryItem)
            {
                AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;
                if (item.ReqLeval > (int)ClientAPI.GetPlayerObject().GetProperty("level"))
                    m_Overlay.SetEnabled(true);
                else
                    m_Overlay.SetEnabled(false);
            }
        }
        public void ApplyText(Label thisLabelElement, string thisText)
        {
            thisLabelElement.text = thisText;
        }

        public void SetActivatable(Activatable obj, ActivatableType activatableType, UIAtavismDraggableSlot parent)
        {
            SetActivatable(obj, activatableType, parent, true);
        }

        public void SetActivatable(Activatable obj, ActivatableType activatableType, UIAtavismDraggableSlot parent, bool showCooldown)
        {
            if (beingDragged)
                return;
            if (this.activatableObject != obj)
            {
                if (cor != null)
                {
                    //StopCoroutine(cor);
                    if (m_CooldownText != null)
                        m_CooldownText.text = "";
                }
            }
            
            this.activatableObject = obj;
            this.activatableType = activatableType;
            this.source = parent;
            this.releaseTarget = parent;
            getState();
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
                if (parent is UIAtavismActionBarSlot)
                {
                    count = Inventory.Instance.GetCountOfItem(item.templateId);
                }
                if (count > 1)
                {
                    ApplyText(m_countText, count.ToString());
                }
                else
                {
                    ApplyText(m_countText,"");
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

            if (showCooldown)
            {
                RunCooldownUpdate();
            }
            else if (m_CooldownImage != null)
            {
                // m_CooldownImage.style.width = new StyleLength(Length.Percent(0));

                //m_CooldownImage.fillAmount = 0;
                //StopCoroutine("UpdateCooldown");
            }
             RunCooldownUpdate();
        }

        /// <summary>
        /// Tells the system to not run the Discard() function
        /// </summary>
        public void PreventDiscard()
        {
            this.preventDiscard = true;
        }

        public void SetDropTarget(UIAtavismDraggableSlot target)
        {
            releaseTarget = target;
        }

        public void SetLink(UIAtavismActivatable link)
        {
            this.link = link;
            if (link != null)
            {
                m_UiButton.SetEnabled(false);
                    m_Icon.AddToClassList("item-icon-link");
                    m_Root.ShowVisualElement();
            }
            else
            {
                m_UiButton.SetEnabled(true);
                    m_Icon.RemoveFromClassList("item-icon-link");
                    m_Root.ShowVisualElement();
            }
        }

        public void ShowTooltip(VisualElement target)
        {
            if (activatableObject is AtavismAbility)
            {
#if !AT_MOBILE
                AtavismAbility ability = (AtavismAbility)activatableObject;
                ability.ShowUITooltip(target);
#endif                
            }
            else if (activatableObject is AtavismInventoryItem)
            {
                AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;
                item.ShowUITooltip(target);
            }
        }

        void getState()
        {
 #if AT_MOBILE

            if (source is UIAtavismActionBarSlot)
            {
                isOnActionBar = true;
            }
            if (source is UIAtavismMerchantItemSlot)
            {
                state = "Merchant";
            }
            else if (source is UIAtavismInventorySlot)
            {
                state = "Inventory";
            }
            else if (source is UIAtavismCharacterEquipSlot)
            {
                state = "Equip";
            }
            else if (source is UIAtavismCraftingSlot)
            {
                state = "Upgrade";
            }
            else if (source is UIAtavismBankSlot)
            {
                state = "Bank";
            }
            else
            {

            }
             if (activatableObject is AtavismInventoryItem && !isOnActionBar)
            {
                item = (AtavismInventoryItem)activatableObject;
            }
#endif  
             
        }

        public void ShowTooltip(GameObject target)
        {
            if (activatableObject is AtavismAbility)
            {
                AtavismAbility ability = (AtavismAbility)activatableObject;
                ability.ShowTooltip(target);
            }
            else if (activatableObject is AtavismInventoryItem)
            {
                AtavismInventoryItem item = (AtavismInventoryItem)activatableObject;
                item.ShowTooltip(target);
            }
        }

        public void Show()
        {
            m_Root.style.opacity = 1;
            m_Root.ShowVisualElement();
        }

        public Activatable ActivatableObject
        {
            get
            {
                return activatableObject;
            }
        }

        public ActivatableType ActivatableType
        {
            get
            {
                return activatableType;
            }
            set
            {
                activatableType = value;
            }
        }

        public UIAtavismDraggableSlot Source
        {
            get
            {
                return source;
            }
        }

        public UIAtavismActivatable Link
        {
            get
            {
                return link;
            }
        }

    }
}
