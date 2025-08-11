using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismGearModification : UIAtavismWindowBase
    {
        [AtavismSeparator("Menu")]
        [SerializeField] Label socketingText;
        [SerializeField] VisualElement socketingImage;
        [SerializeField] Label resetSocketsText;
        [SerializeField] VisualElement resetSocketsImage;
        [SerializeField] Label enchantingText;
        [SerializeField] VisualElement enchantingImage;
        // [SerializeField] Color selectedColor = Color.green;
        // [SerializeField] Color defauiltColor = Color.white;
        // [SerializeField] Color selectedImageColor = Color.green;
        // [SerializeField] Color defauiltImageColor = Color.white;

        [AtavismSeparator("Panels")]
        [SerializeField] VisualElement socketPanel;
        [SerializeField] VisualElement resetSocketPanel;
        [SerializeField] VisualElement enchantPanel;
        [AtavismSeparator("Socketing")]
        [SerializeField] Label itemToSocketName;
        [SerializeField] UIAtavismGearSocketSlot itemSocketingSlot;
        [SerializeField] Label itemToPutInSocketName;
        [SerializeField] UIAtavismGearSocketSlot itemSlot;
        [SerializeField] Label itemToPutInSocketDectiption;
        [SerializeField] Label socketingPriceTitle;
        [SerializeField] UIAtavismCurrencyDisplay currency1;
        [SerializeField] UIProgressBar progressSlider;
        [SerializeField] Button embedButton;
        [AtavismSeparator("Reset Sockets")]
        [SerializeField] Label itemToRecetSocketName;
        [SerializeField] UIAtavismGearSocketSlot itemResetSocketSlot;
        [SerializeField] Label itemResetSocketDectiption;
        [SerializeField] Label resetSocketPriceTitle;
        [SerializeField] UIAtavismCurrencyDisplay currency2;
        [SerializeField] UIProgressBar resetProgressSlider;
        [SerializeField] Button resetButton;
        [AtavismSeparator("Enchanting")]
        [SerializeField] Label itemToEnchantName;
        [SerializeField] UIAtavismGearSocketSlot enchantSlot;
        [SerializeField] Label enchantDectiption;
        [SerializeField] UIProgressBar enchantProgressSlider;
        [SerializeField] Button enchantButton;
        [SerializeField] Label enchantingPriceTitle;
        [SerializeField] UIAtavismCurrencyDisplay currency3;

        private Button socketButton;
        private Button resetSocketButton;
        private Button enchantingButton;

        
        AtavismInventoryItem socketingItem;
        AtavismInventoryItem socketItem;
        AtavismInventoryItem enchantItem;
        float startTime;
        float endTime = -1;
        bool socketing = false;
        // Use this for initialization

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

         // [AtavismSeparator("Menu")]
         socketingText= uiWindow.Query<Label>("menu-socket-text");
         socketingImage= uiWindow.Query<VisualElement>("menu-socket-img");
         resetSocketsText= uiWindow.Query<Label>("menu-reset-socket-text");
         resetSocketsImage= uiWindow.Query<VisualElement>("menu-reset-socket-img");
         enchantingText= uiWindow.Query<Label>("menu-enchant-text");
         enchantingImage= uiWindow.Query<VisualElement>("menu-enchant-img");
         
         socketButton = uiWindow.Query<Button>("menu-socket-button");
         socketButton.clicked += ShowSocketing;
         resetSocketButton = uiWindow.Query<Button>("menu-reset-socket-button");
         resetSocketButton.clicked += ShowResetSockets;
         enchantingButton = uiWindow.Query<Button>("menu-enchant-button");
         enchantingButton.clicked += ShowEnchanting;
         
         
        // Color selectedColor = Color.green;
        // Color defauiltColor = Color.white;
        // Color selectedImageColor = Color.green;
        // Color defauiltImageColor = Color.white;

        // [AtavismSeparator("Panels")]
         socketPanel= uiWindow.Query<VisualElement>("socket-panel");
         resetSocketPanel= uiWindow.Query<VisualElement>("reset-socket-panel");
         enchantPanel= uiWindow.Query<VisualElement>("enchant-panel");
        // [AtavismSeparator("Socketing")]
         itemToSocketName= socketPanel.Query<Label>("socket-name");
         
        // UIAtavismGearSocketSlot
        VisualElement _itemSocketingSlot= socketPanel.Query<VisualElement>("socket-slot");
        itemSocketingSlot = new UIAtavismGearSocketSlot();
        _itemSocketingSlot.Add(itemSocketingSlot);
         
         
         itemToPutInSocketName= socketPanel.Query<Label>("socket-put-name");
         // UIAtavismGearSocketSlot itemSlot;
         VisualElement _itemSlot= socketPanel.Query<VisualElement>("item-slot");
         itemSlot = new UIAtavismGearSocketSlot();
         _itemSlot.Add(itemSlot);
         itemToPutInSocketDectiption= socketPanel.Query<Label>("desc");
         socketingPriceTitle = socketPanel.Query<Label>("price-title");
        VisualElement curr1 = socketPanel.Q<VisualElement>("currency-panel");
        currency1 = new UIAtavismCurrencyDisplay();
        currency1.SetVisualElement(curr1);
        currency1.ReverseOrder = true;
         progressSlider= socketPanel.Query<UIProgressBar>("progress");
         embedButton= socketPanel.Query<Button>("socketing-button");
         embedButton.clicked += ClickEmbed;
         
         
         
         // [AtavismSeparator("Reset Sockets")]
         itemToRecetSocketName= resetSocketPanel.Query<Label>("socket-name");
         // UIAtavismGearSocketSlot itemResetSocketSlot;
         VisualElement _itemResetSocketSlot= resetSocketPanel.Query<VisualElement>("reset-slot");
         itemResetSocketSlot = new UIAtavismGearSocketSlot();
         _itemResetSocketSlot.Add(itemResetSocketSlot);
        
         itemResetSocketDectiption= resetSocketPanel.Query<Label>("desc");
         resetSocketPriceTitle = resetSocketPanel.Query<Label>("price-title");

        VisualElement curr2 = resetSocketPanel.Q<VisualElement>("currency-panel");
        currency2 = new UIAtavismCurrencyDisplay();
        currency2.SetVisualElement(curr2);
        currency2.ReverseOrder = true;
         resetProgressSlider= resetSocketPanel.Query<UIProgressBar>("progress");
         resetButton= resetSocketPanel.Query<Button>("reset-button");
         resetButton.clicked += ClickResetSockets;
         
         
         // [AtavismSeparator("Enchanting")]
         itemToEnchantName= enchantPanel.Query<Label>("socket-name");
       
         VisualElement _enchantSlot= enchantPanel.Query<VisualElement>("enchant-slot");
         enchantSlot = new UIAtavismGearSocketSlot();
         _enchantSlot.Add(enchantSlot);
         enchantDectiption= enchantPanel.Query<Label>("desc");
         enchantProgressSlider= enchantPanel.Query<UIProgressBar>("progress");
         enchantButton= enchantPanel.Query<Button>("enchant-button");
         enchantButton.clicked += ClickEnchant;
         enchantingPriceTitle = enchantPanel.Query<Label>("price-title");

        VisualElement curr3 = enchantPanel.Q<VisualElement>("currency-panel");
        currency3 = new UIAtavismCurrencyDisplay();
        currency3.SetVisualElement(curr3);
        currency3.ReverseOrder = true;
            
            itemSocketingSlot.SetSocket(SetSocketingItem, 1);
            itemSlot.SetSocket(SetSocketItem, 0);
            itemResetSocketSlot.SetSocket(SetResetItem, 1);
            enchantSlot.SetSocket(SetEnchantItem, 2);
            if (itemToPutInSocketName != null)
                itemToPutInSocketName.text = "";
            if (itemToSocketName != null)
                itemToSocketName.text = "";
            if (itemToRecetSocketName != null)
                itemToRecetSocketName.text = "";
            if (itemToEnchantName != null)
                itemToEnchantName.text = "";
            if (progressSlider != null)
                progressSlider.HideVisualElement();
            if (resetProgressSlider != null)
                resetProgressSlider.HideVisualElement();
            if (enchantProgressSlider != null)
                enchantProgressSlider.HideVisualElement();
            // for (int i = 0; i < currency1.Count; i++)
            // {
            //     currency1[i].gameObject.SetActive(false);
            // }
            // for (int i = 0; i < currency2.Count; i++)
            // {
            //     currency2[i].gameObject.SetActive(false);
            // }
            // for (int i = 0; i < currency3.Count; i++)
            // {
            //     currency3[i].gameObject.SetActive(false);
            // }
            currency1.Hide(); 
            currency2.Hide(); 
            currency3.Hide();
            socketingPriceTitle.HiddenVisualElement();
            resetSocketPriceTitle.HiddenVisualElement();
            enchantingPriceTitle.HiddenVisualElement();
            if (itemToPutInSocketDectiption != null)
                itemToPutInSocketDectiption.text = "";
            if (itemResetSocketDectiption != null)
                itemResetSocketDectiption.text = "";
            if (enchantDectiption != null)
                enchantDectiption.text = "";
            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            NetworkAPI.RegisterExtensionMessageHandler("SocketingMsg", HandleSocketingMessage);
            NetworkAPI.RegisterExtensionMessageHandler("EnchantingMsg", HandleEnchantingMessage);
            NetworkAPI.RegisterExtensionMessageHandler("SocketResetMsg", HandleSocketResetMessage);
            AtavismEventSystem.RegisterEvent("GEAR_MODIFICATION_OPEN", this);
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("GEAR_MODIFICATION_OPEN", this);
            NetworkAPI.RemoveExtensionMessageHandler("SocketingMsg", HandleSocketingMessage);
            NetworkAPI.RemoveExtensionMessageHandler("EnchantingMsg", HandleEnchantingMessage);
            NetworkAPI.RemoveExtensionMessageHandler("SocketResetMsg", HandleSocketResetMessage);
        }

        void Start()
        {
           
         
            Hide();
            socketing = false;
            ShowSocketing();
        }



        private void OnDestroy()
        {
           

        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "GEAR_MODIFICATION_OPEN")
            {
                Mailing.Instance.RequestMailList();
                Show();
            }
        }


        private void SetSocketItem(AtavismInventoryItem item)
        {
            socketItem = item;

            if (itemToPutInSocketName != null)
                itemToPutInSocketName.text = item != null ? item.name : "";
            Inventory.Instance.SocketingCost(socketItem, socketingItem);
            if (socketItem == null || socketingItem == null)
            {
                currency1.Hide();
                // for (int i = 0; i < currency1.Count; i++)
                // {
                //     currency1[i].gameObject.SetActive(false);
                // }
                socketingPriceTitle.HiddenVisualElement();

            }
        }

        private void SetSocketingItem(AtavismInventoryItem item)
        {
            socketingItem = item;

            if (itemToSocketName != null)
                itemToSocketName.text = item != null ? item.name : "";
            Inventory.Instance.SocketingCost(socketItem, socketingItem);
            if (socketItem == null || socketingItem == null)
            {
                currency1.Hide();
                socketingPriceTitle.HiddenVisualElement();

            }
        }

        private void SetEnchantItem(AtavismInventoryItem item)
        {
            enchantItem = item;
            if (itemToPutInSocketName != null)
                itemToPutInSocketName.text = item != null ? item.name : "";
            Inventory.Instance.EnchantCost(enchantItem);
            if (enchantItem == null)
            {

                currency3.Hide();
                enchantingPriceTitle.HiddenVisualElement();
                if (enchantDectiption != null)
                    enchantDectiption.text = "";
            }
        }
        private void SetResetItem(AtavismInventoryItem item)
        {
            socketItem = item;
            if (itemToPutInSocketName != null)
                itemToPutInSocketName.text = item != null ? item.name : "";
            Inventory.Instance.ResetSocketsCost(socketItem);
            if (socketItem == null)
            {
                resetSocketPriceTitle.HiddenVisualElement();
                currency2.Hide();

            }
        }

        public void ClickEmbed()
        {
            if (socketItem != null && socketingItem != null && !socketing)
            {
                Inventory.Instance.EmbedInTheSlot(socketItem, socketingItem);
            }
        }
        public void ClickResetSockets()
        {
            if (socketItem != null && !socketing)
            {
                Inventory.Instance.ResetSloctsSlot(socketItem);
            }
        }

        public void ClickEnchant()
        {
            if (enchantItem != null && !socketing)
            {
                Inventory.Instance.EnchantItem(enchantItem);
            }
        }

        public void ShowSocketing()
        {
            if (socketPanel != null)
                socketPanel.ShowVisualElement();
            if (resetSocketPanel != null)
                resetSocketPanel.HideVisualElement();
            if (enchantPanel != null)
                enchantPanel.HideVisualElement();
            if (socketingText != null)
                socketingText.AddToClassList("menu-element-selected");
            if (enchantingText != null)
                enchantingText.RemoveFromClassList("menu-element-selected");
            if (resetSocketsText != null)
                resetSocketsText.RemoveFromClassList("menu-element-selected");
            if (socketingImage != null)
                socketingImage.AddToClassList("menu-element-selected");
            if (enchantingImage != null)
                enchantingImage.RemoveFromClassList("menu-element-selected");
            if (resetSocketsImage != null)
                resetSocketsImage.RemoveFromClassList("menu-element-selected");
            itemSlot.Discarded();
            itemSocketingSlot.Discarded();
            enchantSlot.Discarded();
            itemResetSocketSlot.Discarded();
        }

        public void ShowEnchanting()
        {
            if (socketPanel != null)
                socketPanel.HideVisualElement();
            if (resetSocketPanel != null)
                resetSocketPanel.HideVisualElement();
            if (enchantPanel != null)
                enchantPanel.ShowVisualElement();
            if (socketingText != null)
                socketingText.RemoveFromClassList("menu-element-selected");
            if (enchantingText != null)
                enchantingText.AddToClassList("menu-element-selected");
            if (resetSocketsText != null)
                resetSocketsText.RemoveFromClassList("menu-element-selected");
            if (socketingImage != null)
                socketingImage.RemoveFromClassList("menu-element-selected");
            if (enchantingImage != null)
                enchantingImage.AddToClassList("menu-element-selected");
            if (resetSocketsImage != null)
                resetSocketsImage.RemoveFromClassList("menu-element-selected");
            itemSlot.Discarded();
            itemSocketingSlot.Discarded();
            enchantSlot.Discarded();
            itemResetSocketSlot.Discarded();
        }

        public void ShowResetSockets()
        {
            if (socketPanel != null)
                socketPanel.HideVisualElement();
            if (resetSocketPanel != null)
                resetSocketPanel.ShowVisualElement();
            if (enchantPanel != null)
                enchantPanel.HideVisualElement();
            if (socketingText != null)
                socketingText.RemoveFromClassList("menu-element-selected");
            if (enchantingText != null)
                enchantingText.RemoveFromClassList("menu-element-selected");
            if (resetSocketsText != null)
                resetSocketsText.AddToClassList("menu-element-selected");
            if (socketingImage != null)
                socketingImage.RemoveFromClassList("menu-element-selected");
            if (enchantingImage != null)
                enchantingImage.RemoveFromClassList("menu-element-selected");
            if (resetSocketsImage != null)
                resetSocketsImage.AddToClassList("menu-element-selected");
            itemSlot.Discarded();
            itemSocketingSlot.Discarded();
            enchantSlot.Discarded();
            itemResetSocketSlot.Discarded();
        }


        public override void Show()
        {
            base.Show();
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.SetUIActivatableClickedOverride(PlaceSocketingItem);
            ShowSocketing();
        }

        public override void Hide()
        {
            base.Hide();
            itemSlot.Discarded();
            itemSocketingSlot.Discarded();
            enchantSlot.Discarded();
            itemResetSocketSlot.Discarded();
            if (itemToPutInSocketName != null)
                itemToPutInSocketName.text = "";
            if (itemToSocketName != null)
                itemToSocketName.text = "";
            if (itemToRecetSocketName != null)
                itemToRecetSocketName.text = "";
            if (itemToEnchantName != null)
                itemToEnchantName.text = "";
            if (progressSlider != null)
                progressSlider.HideVisualElement();
            if (resetProgressSlider != null)
                resetProgressSlider.HideVisualElement();
            if (enchantProgressSlider != null)
                enchantProgressSlider.HideVisualElement();
            // for (int i = 0; i < currency1.Count; i++)
            // {
            //     currency1[i].gameObject.SetActive(false);
            // }
            // for (int i = 0; i < currency2.Count; i++)
            // {
            //     currency2[i].gameObject.SetActive(false);
            // }
            // for (int i = 0; i < currency3.Count; i++)
            // {
            //     currency3[i].gameObject.SetActive(false);
            // }
            currency1.Hide(); 
            currency2.Hide(); 
            currency3.Hide();
            socketingPriceTitle.HiddenVisualElement();
            resetSocketPriceTitle.HiddenVisualElement();
            enchantingPriceTitle.HiddenVisualElement();

            if (itemToPutInSocketDectiption != null)
                itemToPutInSocketDectiption.text = "";
            if (itemResetSocketDectiption != null)
                itemResetSocketDectiption.text = "";
            if (enchantDectiption != null)
                enchantDectiption.text = "";
            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(PlaceSocketingItem);
        }

        private void PlaceSocketingItem(UIAtavismActivatable activatable)
        {
          //    Debug.LogError("PlaceSocketingItem " + activatable.Link);

            if (activatable.Link != null)
            {
                //       Debug.LogError("PlaceSocketingItem " + activatable.Link);
                return;
            }
            AtavismInventoryItem item = (AtavismInventoryItem)activatable.ActivatableObject;
            if (item != null)
            {
                if (socketPanel.style.display == DisplayStyle.Flex)
                {
                    if (item.sockettype.Length > 0)
                    {
                        itemSlot.SetActivatable(activatable);
                    }
                    else if (item.itemEffectTypes.Contains("Sockets"))
                    {
                        itemSocketingSlot.SetActivatable(activatable);
                    }
                }
                else if (resetSocketPanel.style.display == DisplayStyle.Flex)
                {
                    itemResetSocketSlot.SetActivatable(activatable);
                }
                else if (enchantPanel.style.display == DisplayStyle.Flex)
                {
                    //   Debug.LogError("item.EnchantId: "+item.EnchantId);
                    if (item.EnchantId > 0)
                    {
                        enchantSlot.SetActivatable(activatable);
                    }
                    else
                    {
                        activatable.PreventDiscard();

                        //     Debug.LogError("Wrong Item");
                        string[] args = new string[1];
#if AT_I2LOC_PRESET
                    args[0] = I2.Loc.LocalizationManager.GetTranslation("Wrong Item");
#else
                        args[0] = "Wrong Item";
#endif
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                    }
                }
            }
        }
        //  enchantSlot.Discarded();
        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }

        void HandleSocketingMessage(Dictionary<string, object> props)
        {
            string msgType = (string)props["PluginMessageType"];
            switch (msgType)
            {

                case "SocketingStarted":
                    {
                        AtavismLogger.LogDebugMessage("SocketingStarted");

                        float creationTime = (float)props["creationTime"];
                        AtavismLogger.LogDebugMessage("SocketingStarted creationTime:" + creationTime);

                        if (creationTime > 0)
                        {
                            progressSlider.ShowVisualElement();
                            startTime = Time.time;
                            endTime =  Time.time + creationTime;
                        }
                        socketing = true;
                        break;
                    }
                case "SocketingCompleted":
                    {
                        AtavismLogger.LogDebugMessage("SocketingCompleted");
                        itemSlot.Discarded();// UpdateAttachmentData(null);
                        itemSocketingSlot.Discarded();
                        socketing = false;
                        break;
                    }
                case "SocketingFailed":
                    {
                        AtavismLogger.LogDebugMessage("SocketingFailed");
                        itemSlot.Discarded();// UpdateAttachmentData(null);
                        itemSocketingSlot.Discarded();
                        string[] args = new string[1];
                        args[0] = (string)props["ErrorMsg"];
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                        socketing = false;
                        break;
                    }
                case "SocketingInterrupted":
                    {
                        AtavismLogger.LogDebugMessage("Socketing was interrupted");
                        // dispatch a ui event to tell the rest of the system
                        ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismMobController>().PlayAnimation("", 0,"" ,1);
                        socketing = false;
                        if (progressSlider != null)
                            progressSlider.HideVisualElement();
                        break;
                    }
                case "SocketingUpdate":
                    {
                        AtavismLogger.LogDebugMessage("SocketingUpdate");
                        long creationCost = (long)props["creationCost"];
                        int creationCurrency = (int)props["creationCurrency"];
                        AtavismLogger.LogDebugMessage("SocketingUpdate creationCost:" + creationCost + " creationCurrency:" + creationCurrency);
                        currency1.SetData(creationCurrency, creationCost);
                        socketingPriceTitle.VisibleVisualElement();

                        break;
                    }
            }
            AtavismLogger.LogDebugMessage("Got A Socketing Message!");
        }
        void HandleSocketResetMessage(Dictionary<string, object> props)
        {
            string msgType = (string)props["PluginMessageType"];
            switch (msgType)
            {

                case "SocketResetStarted":
                    {
                        AtavismLogger.LogDebugMessage("SocketResetStarted");

                        float creationTime = (float)props["creationTime"];
                        AtavismLogger.LogDebugMessage("SocketResetStarted creationTime:" + creationTime);

                        if (creationTime > 0)
                        {
                            if (resetProgressSlider != null)
                                resetProgressSlider.ShowVisualElement();
                            startTime = Time.time;
                            endTime =  Time.time + creationTime;

                        }
                        socketing = true;
                        break;
                    }
                case "SocketResetCompleted":
                    {
                        AtavismLogger.LogDebugMessage("SocketResetCompleted");
                        itemResetSocketSlot.Discarded();// UpdateAttachmentData(null);
                        socketing = false;
                        break;
                    }
                case "SocketResetFailed":
                    {
                        AtavismLogger.LogDebugMessage("SocketingFailed");
                        itemResetSocketSlot.Discarded();// UpdateAttachmentData(null);
                        string[] args = new string[1];
                        args[0] = (string)props["ErrorMsg"];
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                        socketing = false;
                        break;
                    }
                case "SocketResetInterrupted":
                    {
                        AtavismLogger.LogDebugMessage("Socket Reset was interrupted");
                        // dispatch a ui event to tell the rest of the system
                        ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismMobController>().PlayAnimation("", 0,"" ,1);
                        socketing = false;
                        if (resetProgressSlider != null)
                            resetProgressSlider.HideVisualElement();
                        break;
                    }
                case "SocketResetUpdate":
                    {
                        AtavismLogger.LogDebugMessage("SocketResetUpdate");
                        long creationCost = (long)props["creationCost"];
                        int creationCurrency = (int)props["creationCurrency"];
                        AtavismLogger.LogDebugMessage("SocketResetUpdate creationCost:" + creationCost + " creationCurrency:" + creationCurrency);
                        currency2.SetData(creationCurrency, creationCost);
                        resetSocketPriceTitle.VisibleVisualElement();
                        break;
                    }
            }
            AtavismLogger.LogDebugMessage("Got A Socketing Message!");
        }

        void HandleEnchantingMessage(Dictionary<string, object> props)
        {
            string msgType = (string)props["PluginMessageType"];
            switch (msgType)
            {
                case "EnchantingStarted":
                    {
                        if (AtavismLogger.logLevel <= LogLevel.Debug)  AtavismLogger.LogDebugMessage("EnchantingStarted");

                        float creationTime = (float)props["creationTime"];
                        if (AtavismLogger.logLevel <= LogLevel.Debug)  AtavismLogger.LogDebugMessage("EnchantingStarted creationTime:" + creationTime);

                        if (creationTime > 0)
                        {
                            if (enchantProgressSlider != null)
                                enchantProgressSlider.ShowVisualElement();
                            startTime = Time.time;
                            endTime =  Time.time + creationTime;

                        }
                        socketing = true;
                        break;
                    }
                case "EnchantingCompleted":
                    {
                        if (AtavismLogger.logLevel <= LogLevel.Debug)  AtavismLogger.LogDebugMessage("EnchantingCompleted");
                        enchantSlot.Discarded();// UpdateAttachmentData(null);
                        socketing = false;
                        break;
                    }
                case "EnchantingFailed":
                    {
                        if (AtavismLogger.logLevel <= LogLevel.Debug)   AtavismLogger.LogDebugMessage("EnchantingFailed");
                        enchantSlot.Discarded();// UpdateAttachmentData(null);
                        string[] args = new string[1];
                        args[0] = (string)props["ErrorMsg"];
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", args);
                        socketing = false;
                        break;
                    }
                case "EnchantingInterrupted":
                    {
                        if (AtavismLogger.logLevel <= LogLevel.Debug)  AtavismLogger.LogDebugMessage("Enchanting was interrupted");
                        // dispatch a ui event to tell the rest of the system
                        ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismMobController>().PlayAnimation("", 0,"" ,1);
                        socketing = false;
                        if (enchantProgressSlider != null)
                            enchantProgressSlider.HideVisualElement();
                        break;
                    }
                case "EnchantingUpdate":
                    {
                        if (AtavismLogger.logLevel <= LogLevel.Debug)
                        {
                            string keys = " [ ";

                            foreach (var it in props.Keys)
                            {
                                keys += " ; " + it + " => " + props[it];
                            }
                            AtavismLogger.LogDebugMessage("EnchantingUpdate: keys:" + keys);
                            AtavismLogger.LogDebugMessage("EnchantingUpdate");
                        }

                        long creationCost = (long)props["creationCost"];
                        int creationCurrency = (int)props["creationCurrency"];
                        if (AtavismLogger.logLevel <= LogLevel.Debug)
                            AtavismLogger.LogDebugMessage("EnchantingUpdate creationCost:" + creationCost + " creationCurrency:" + creationCurrency);
                        currency3.SetData(creationCurrency, creationCost);
                        enchantingPriceTitle.VisibleVisualElement();

                        string detale = "";
                        int statCount = (int)props["statNumber"];
#if AT_I2LOC_PRESET
                    detale += I2.Loc.LocalizationManager.GetTranslation("Enchant to level") + " " + props["nextLevel"] + "\n";
#else
                        detale += "Enchant to level " + props["nextLevel"] + "\n";
#endif
                        for (int i = 0; i < statCount; i++)
                        {
                            if (!props["stat" + i + "name"].Equals("dmg-base") && !props["stat" + i + "name"].Equals("dmg-max"))
                            {
#if AT_I2LOC_PRESET
                            detale += I2.Loc.LocalizationManager.GetTranslation((string)props["stat" + i + "name"]) + " " + props["stat" + i + "value"] + "\n";
#else
                                detale += props["stat" + i + "name"] + " " + props["stat" + i + "value"] + "\n";
#endif
                            }
                            else if (props["stat" + i + "name"].Equals("dmg-base"))
                            {
#if AT_I2LOC_PRESET
                            detale += I2.Loc.LocalizationManager.GetTranslation("Damage") + " " + props["stat" + i + "value"] + "\n";
#else
                                detale += "Damage " + props["stat" + i + "value"] + "\n";
#endif

                            }
                        }

                        if (enchantDectiption != null)
                            enchantDectiption.text = detale;

                        break;
                    }
            }
            AtavismLogger.LogDebugMessage("Got A Enchanting Message!");
        }


        protected override void Update()
        {
            base.Update();
            if (endTime != -1 && endTime > Time.time)
            {
                float total = endTime - startTime;
                float currentTime = endTime - Time.time;
                if (progressSlider != null)
                {
                    progressSlider.ShowVisualElement();
                    progressSlider.highValue = total;
                    progressSlider.value = currentTime ;
                    // progressSlider.value = 1 - ((float)currentTime / (float)total);
                }

                if (resetProgressSlider != null)
                {
                    resetProgressSlider.ShowVisualElement();
                    resetProgressSlider.highValue = total;
                    resetProgressSlider.value = currentTime ;
                    // resetProgressSlider.value = 1 - ((float)currentTime / (float)total);
                }

                if (enchantProgressSlider != null)
                {
                    enchantProgressSlider.ShowVisualElement();
                    enchantProgressSlider.highValue = total;
                    enchantProgressSlider.value = currentTime ;
                }
            }
            else
            {
                if (progressSlider != null)
                    progressSlider.HideVisualElement();
                if (resetProgressSlider != null)
                    resetProgressSlider.HideVisualElement();
                if (enchantProgressSlider != null)
                    enchantProgressSlider.HideVisualElement();
            }
        }


    }
}