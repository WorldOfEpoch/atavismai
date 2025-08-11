using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Atavism.UI
{
    public class UIAttributeInfo
    {
        public string value;
        public string text;
        public int compare1=99;
        public int compare2=99;
        public Color textColour = UIAtavismTooltip.Instance.defaultTextColour;
        public bool singleColumnRow;
        public RectOffset margin;
        public bool separator = false;
        public bool title = false;
        public bool socket = false;
        public bool resource = false;
        public Sprite socketIcon;
    }
    public class UIAtavismTooltip : MonoBehaviour
    {
        static UIAtavismTooltip instance;

        public UIDocument uiDocument;
        
        [SerializeField] private VisualTreeAsset attributeRowTemplate;
        [SerializeField] private VisualTreeAsset resourceRowTemplate;
        [SerializeField] private VisualTreeAsset socketRowTemplate;
        [SerializeField] private VisualTreeAsset titleRowTemplate;
        [SerializeField] private VisualTreeAsset separatorRowTemplate;

        private VisualElement root;
        private VisualElement tooltipContainer;


        // Converted to UI Toolkit components
        public VisualElement tooltip;
        public Label title;
        public Label type;
        public Label weight;
        public Label description;
        public VisualElement contentPanel;
        public VisualElement attributeRow;
        public VisualElement resourceRow;
        public VisualElement attributeTitle;
        public VisualElement socketRow;
        public VisualElement separator;
        // public int greaterSpriteId = 0;
        // public int lowerSpriteId = 1;
        // public int equalSpriteId = 2;
        // public int newSpriteId = 3;
        	// public List<Color> itemGradeColors; 
         public Color itemTypeColour = Color.white;
         public Color itemStatColour = Color.green;
         public Color abilityRangeColour = Color.white;
         public Color abilityCostColour = Color.white;
         public Color abilityCastTimeColour = Color.white;
         public Color defaultTextColour = Color.black;
         public Color itemStatLowerColour = Color.red;
         //public Color itemStatHigherColour = Color.green;
         public Color itemSectionTitleColour = Color.yellow;
         public Color itemSetColour = Color.yellow;
         public Color itemInactiveSetColour = Color.gray;
        public VisualElement overlayIcon;
        public VisualElement itemIcon;
        public VisualElement iconPanel;
        public VisualElement anchorTR;
        public VisualElement anchorTL;
        public VisualElement anchorBR;
        public VisualElement anchorBL;

        private VisualElement attributesContainer;
        // Other variables remain unchanged
        CanvasGroup canvasGroup = null;
        [SerializeField] Color defaultTitleColor = Color.white;
        List<UIAttributeInfo> attributes = new List<UIAttributeInfo>();
        private List<VisualElement> attributesRows = new List<VisualElement>();

        GameObject target;
        bool showing = false;
        bool showing1 = false;
        bool showing2 = false;
        public bool IsVisible => showing;
        
        // Additional tooltips (converted to UI Toolkit)
        public VisualElement additionalTooltip;
        public Label additionalTooltipTitle;
        public Label additionalTooltipType;
        public Label additionalTooltipWeight;
        public Label additionalTooltipDescription;
        public VisualElement additionalTooltipOverlayIcon;
        public VisualElement additionalTooltipItemIcon;
        public VisualElement additionalTooltipItemIconPanel;
        public VisualElement additionalTooltipAttributesContainer;

        // Additional tooltips (converted to UI Toolkit)
        public VisualElement additionalTooltip2;
        public Label additionalTooltipTitle2;
        public Label additionalTooltipType2;
        public Label additionalTooltipWeight2;
        public Label additionalTooltipDescription2;
        public VisualElement additionalTooltipOverlayIcon2;
        public VisualElement additionalTooltipItemIcon2;
        public VisualElement additionalTooltipItemIconPanel2;
        public VisualElement additionalTooltipAttributesContainer2;

        List<UIAttributeInfo> additionalAttributes = new List<UIAttributeInfo>();
        // private List<VisualElement> additionalAttributesRows = new List<VisualElement>();

        List<UIAttributeInfo> additionalAttributes2 = new List<UIAttributeInfo>();
        // private List<VisualElement> additionalAttributesRows2 = new List<VisualElement>();
        protected Vector2 draggingMinValues, draggingMaxValues, draggingMouseOffset;
        private float initialWidth;
        private float initialHeight;
        private IVisualElementScheduledItem showScheduler;
        private IVisualElementScheduledItem hideScheduler;

#if AT_MOBILE
        private VisualElement uiDivideSection, uiEquipSection, uiUnequipSection, uiUpgradeSection, uiUseSection, uiCleanSection, uiMoveSection, uiSellSection, uiBuySection;
        private UITextField uiDivideInput;
        private Button uiDivideButton, uiEquipButton, uiUnequipeButton, uiUpgradeButton, uiUseButton, uiCleanButton, uiMoveButton, uiSellButton, uiBuyButton, uiIncreaseButton, uiDecreaseButton;
        int count = 1;
        public UIAtavismActivatable uIAtavismActivatable;
        [SerializeField] UIAtavismWindowBase MerchantWindow;
        [SerializeField] UIAtavismWindowBase UpgradeWindow;
        [SerializeField] UIAtavismWindowBase BankWindow;
#endif        
        
        void OnEnable()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            initialWidth = Screen.width;
            if (uiDocument != null)
            {
                uiDocument.enabled = true;
                root = uiDocument.rootVisualElement;
                // Assume 'tooltipContainer' is the name of the main container in your UXML
                tooltipContainer = root.Q("tooltipContainer");
            }
            else
            {
                Debug.LogError("UIDocument is not assigned to UIAtavismTooltip");
            }
            // Query UI Toolkit components
            //Main Tooltip
            tooltip = root.Q<VisualElement>("MainTooltip");
            title = tooltip.Q<Label>("Title");
            type = tooltip.Q<Label>("Type");
            weight = tooltip.Q<Label>("Weight");
            description = tooltip.Q<Label>("Description");
            contentPanel = tooltip.Q<VisualElement>("ContentPanel");
         //  Debug.LogError("UIAtavismTooltip.OnEnable title1 "+title+"|"+type+"|"+weight+"|"+description+"|"+contentPanel);
            iconPanel = tooltip.Q<VisualElement>("IconPanel");
            overlayIcon = iconPanel.Q<VisualElement>("quality");
            itemIcon = iconPanel.Q<VisualElement>("icon");
            Label itemCount = iconPanel.Q<Label>("count");
            if(itemCount!=null)  itemCount.text = "";

            
            
        //    Debug.LogError("UIAtavismTooltip.OnEnable icon "+iconPanel+"|"+overlayIcon+"|"+itemIcon);
            anchorTR = tooltip.Q<VisualElement>("AnchorTR");
            anchorTL = tooltip.Q<VisualElement>("AnchorTL");
            anchorBR = tooltip.Q<VisualElement>("AnchorBR");
            anchorBL = tooltip.Q<VisualElement>("AnchorBL");
        //    Debug.LogError("UIAtavismTooltip.OnEnable anchor "+anchorTR+"|"+anchorTL+"|"+anchorBR+"|"+anchorBL);
            attributesContainer = tooltip.Q<VisualElement>("attributesContainer");
            // Query additionalTooltip components
             additionalTooltip = root.Q<VisualElement>("AdditionalTooltip");
             additionalTooltipTitle = additionalTooltip.Q<Label>("Title");
             additionalTooltipType = additionalTooltip.Q<Label>("Type");
             additionalTooltipWeight = additionalTooltip.Q<Label>("Weight");
             additionalTooltipDescription = additionalTooltip.Q<Label>("Description");
             additionalTooltipItemIconPanel = additionalTooltip.Q<VisualElement>("IconPanel");

             additionalTooltipOverlayIcon = additionalTooltipItemIconPanel.Q<VisualElement>("quality");
             additionalTooltipItemIcon = additionalTooltipItemIconPanel.Q<VisualElement>("icon");
             additionalTooltipAttributesContainer = additionalTooltip.Q<VisualElement>("attributesContainer");
             Label aitemCount = additionalTooltipItemIconPanel.Q<Label>("count");
             if(aitemCount!=null)aitemCount.text = "";
            // Query additionalTooltip2 components
            additionalTooltip2 = root.Q<VisualElement>("AdditionalTooltip2");
            additionalTooltipTitle2 = additionalTooltip2.Q<Label>("Title");
            additionalTooltipType2 = additionalTooltip2.Q<Label>("Type");
            additionalTooltipWeight2 = additionalTooltip2.Q<Label>("Weight");
            additionalTooltipDescription2 = additionalTooltip2.Q<Label>("Description");
            additionalTooltipItemIconPanel2 = additionalTooltip2.Q<VisualElement>("IconPanel");

            additionalTooltipOverlayIcon2 = additionalTooltipItemIconPanel2.Q<VisualElement>("quality");
            additionalTooltipItemIcon2 = additionalTooltipItemIconPanel2.Q<VisualElement>("icon");
            Label a2itemCount = additionalTooltipItemIconPanel2.Q<Label>("count");
            if(a2itemCount!=null)a2itemCount.text = "";
            additionalTooltipAttributesContainer2 = additionalTooltip2.Q<VisualElement>("attributesContainer");
//            Debug.LogError("UIAtavismTooltip.OnEnable End");
            additionalTooltip.HideVisualElement();
            additionalTooltip2.HideVisualElement();
            tooltipContainer.style.opacity = 0f;
            tooltipContainer.HideVisualElement();
#if AT_MOBILE
            uiDivideSection = tooltip.Q<VisualElement>("divide-section");
            uiDivideInput = uiDivideSection.Q<UITextField>("divide-input");
            uiIncreaseButton = uiDivideSection.Q<Button>("increase-button");
            uiDecreaseButton = uiDivideSection.Q<Button>("decrease-button");
            uiDivideButton = uiDivideSection.Q<Button>("divide-button");
            
            uiEquipSection = tooltip.Q<VisualElement>("equip-section");
            uiEquipButton = uiEquipSection.Q<Button>("equip-button");
            
            uiUnequipSection = tooltip.Q<VisualElement>("unequip-section");
            uiUnequipeButton = uiUnequipSection.Q<Button>("unequip-button");
            
            uiUpgradeSection = tooltip.Q<VisualElement>("upgrade-section");
            uiUpgradeButton = uiUpgradeSection.Q<Button>("upgrade-button");
            
            uiUseSection = tooltip.Q<VisualElement>("use-section");
            uiUseButton = uiUseSection.Q<Button>("use-button");
            
            uiCleanSection = tooltip.Q<VisualElement>("clean-section");
            uiCleanButton = uiCleanSection.Q<Button>("clean-button");
            
            uiMoveSection = tooltip.Q<VisualElement>("move-section");
            uiMoveButton = uiMoveSection.Q<Button>("move-button");
            
            uiSellSection = tooltip.Q<VisualElement>("sell-section");
            uiSellButton = uiSellSection.Q<Button>("sell-button");
            
            uiBuySection = tooltip.Q<VisualElement>("buy-section");
            uiBuyButton = uiBuySection.Q<Button>("buy-button");
            
            uiDivideButton.clicked += Divide;
            uiDivideInput.value = count.ToString();
            uiDivideInput.RegisterValueChangedCallback(ChangeValue);
            uiIncreaseButton.clicked += IncrementCount;
            uiDecreaseButton.clicked += DecrementCount;

            uiEquipButton.clicked += Equip;
            uiUnequipeButton.clicked += Equip;

            uiUpgradeButton.clicked += Equip;
            uiUseButton.clicked += Equip;
            uiCleanButton.clicked += Cleanup;
            uiMoveButton.clicked += Equip;

            uiSellButton.clicked += Equip;
            uiBuyButton.clicked += Equip;

            tooltipContainer.RegisterCallback<PointerDownEvent>(Close);
            
#endif

        }

        private void Close(PointerDownEvent evt)
        {
            Hide();
        }
        
#if AT_MOBILE
        public void SellOFF()
        {
            uiSellSection.HideVisualElement();
        }

        public void Refresh()
        {
            if (uIAtavismActivatable != null)
            {
                // uGUIAtavismActivatable.Clicked();
                ActivatePanel(uIAtavismActivatable.state, uIAtavismActivatable.item);
            }
        }

        public void Equip()
        {
            if (uIAtavismActivatable != null)
            {
                uIAtavismActivatable.OnClick();
                uIAtavismActivatable = null;
            }
        }


        public void Divide()
        {

            if (uiDivideInput.value.Length > 0)
            {
                count = int.Parse(uiDivideInput.value);
            }

            if (uIAtavismActivatable != null && count > 0)
            {
                uIAtavismActivatable.Divide(count);
            }
        }

        public void IncrementCount()
        {
            if (count < 199)
            {
                count++;
                uiDivideInput.value = count.ToString();
            }
        }

        public void DecrementCount()
        {
            if (count > 1)
            {
                count--;
                uiDivideInput.value = count.ToString();
            }
        }

        public void ChangeValue(ChangeEvent<string> evt)
        {
            if (int.Parse(uiDivideInput.value) > 199)
            {
                uiDivideInput.value = "199";
            }

            if (int.Parse(uiDivideInput.value) < 1)
            {
                uiDivideInput.value = "1";
            }

            if (uiDivideInput.value.Length == 0)
            {
                uiDivideInput.value = "1";
            }

            count = int.Parse(uiDivideInput.value);
        }
        public void TurnOffAll()
        {
            uiDivideSection.HideVisualElement();
            uiEquipSection.HideVisualElement();
            uiUnequipSection.HideVisualElement();
            uiUpgradeSection.HideVisualElement();
            uiUseSection.HideVisualElement();
            uiCleanSection.HideVisualElement();
            uiMoveSection.HideVisualElement();
            uiSellSection.HideVisualElement();
            uiBuySection.HideVisualElement();
        }

        public void ActivatePanel(string state, AtavismInventoryItem item)
        {
            TurnOffAll();
            switch (state)
            {
                case "Upgrade":
                    uiCleanSection.ShowVisualElement();
                    break;
                case "Bank":

                    uiMoveSection.ShowVisualElement();
                    if (BankWindow!=null && !BankWindow.IsVisible)
                    {
                        Hide();
                    }

                    break;
                case "Merchant":

                    uiBuySection.ShowVisualElement();
                    if (MerchantWindow!=null && !MerchantWindow.IsVisible)
                    {
                        Hide();
                    }

                    break;
                case "Inventory":
                    if (item.itemType.ToString() == "Weapon" || item.itemType.ToString() == "Armor")
                    {
                        if (UpgradeWindow!=null && UpgradeWindow.IsVisible) // if Upgrade is open
                        {
                            uiUpgradeSection.ShowVisualElement();
                        }
                        else if (MerchantWindow!=null && MerchantWindow.IsVisible) // if Merchant is open
                        {
                            uiSellSection.ShowVisualElement();
                        }
                        else if (BankWindow!=null && BankWindow.IsVisible) // if Bank is open
                        {
                            uiMoveSection.ShowVisualElement();
                        }
                        else
                        {
                            uiEquipSection.ShowVisualElement();
                        }

                    }
                    else
                    {
                        if (UpgradeWindow !=null && UpgradeWindow.IsVisible) // if Upgrade is open
                        {
                            uiUseSection.ShowVisualElement();
                        }
                        else if (MerchantWindow.IsVisible) // if Merchant is open
                        {
                            uiSellSection.ShowVisualElement();
                        }
                        else if (BankWindow!=null && BankWindow.IsVisible) // if Bank is open
                        {
                            uiMoveSection.ShowVisualElement();
                        }
                        else
                        {
                            if (item.itemType.ToString() == "Consumable")
                            {
                                uiUseSection.ShowVisualElement();
                            }

                            if(item.Count > 1)
                            uiDivideSection.ShowVisualElement();
                        }
                    }

                    break;

                case "Equip":

                    if (MerchantWindow.IsVisible) // if Merchant is open
                    {
                        uiUnequipSection.ShowVisualElement();
                    }
                    else
                    {
                        uiUnequipSection.ShowVisualElement();
                    }

                    break;
            }

            // uiEquipSection.transform.SetAsLastSibling();
            // uiUnequipSection.transform.SetAsLastSibling();
            // uiUpgradeSection.transform.SetAsLastSibling();
            // uiUseSection.transform.SetAsLastSibling();
            // uiCleanSection.transform.SetAsLastSibling();
            // uiMoveSection.transform.SetAsLastSibling();
            // uiDivideSection.transform.SetAsLastSibling();
            // uiSellSection.transform.SetAsLastSibling();
            // uiBuySection.transform.SetAsLastSibling();

        }
#endif        
        
        
        
        
        void Update()
        {
            if (tooltipContainer != null)
            {
                var op = tooltipContainer.resolvedStyle.opacity;
                //  Debug.LogError("Update " + op);
                if (showing && op < 1f)
                {
                    tooltipContainer.FadeInVisualElement();

                }
                else if (!showing && op > 0f)
                {
                    tooltipContainer.FadeOutVisualElement();
                }
            }

            if (additionalTooltip != null)
            {
                var op1 = additionalTooltip.resolvedStyle.opacity;
                if (showing1 && op1 < 1f)
                {
                    additionalTooltip.FadeInVisualElement();

                }
                else if (!showing1 && op1 > 0f)
                {
                    additionalTooltip.FadeOutVisualElement();
                }
            }

            if (additionalTooltip2 != null)
            {
                var op2 = additionalTooltip2.resolvedStyle.opacity;
                if (showing2 && op2 < 1f)
                {
                    additionalTooltip2.FadeInVisualElement();

                }
                else if (!showing2 && op2 > 0f)
                {
                    additionalTooltip2.FadeOutVisualElement();
                }

            }


            if (!showing)
                return;
#if !AT_MOBILE               
            CalculatePosition();
#endif
        }

        void CalculatePosition(){

            float width = tooltipContainer.resolvedStyle.width;
            float height = tooltipContainer.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            draggingMinValues.x = 0f;
            draggingMinValues.y = 0f;
            draggingMaxValues.x = canvasWidth - width;
            draggingMaxValues.y = canvasHeight - height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 tooltipPosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            if (anchorBL != null)
                anchorBL.HideVisualElement();
            if (anchorTL != null)
                anchorTL.HideVisualElement();
            if (anchorBR != null)
                anchorBR.HideVisualElement();
            if (anchorTR != null)
                anchorTR.HideVisualElement();
            bool left = false;
            bool top = false;
            tooltipPosition += new Vector2(5, -5);

            if ((tooltipPosition.x + width + 10) * widthScaleFactor > Screen.width)
            {
                if ((tooltipPosition.x - width + 10) * widthScaleFactor >= 0)
                    tooltipPosition.x -= (width + 10) ;
                else
                    tooltipPosition.x = 1;

                left = true;
            }
            else
                tooltipPosition.x += 10;

            if ((tooltipPosition.y + height + 5) * heightScaleFactor > Screen.height)
            {
                if ((tooltipPosition.y - height + 5) * heightScaleFactor >= 0)
                    tooltipPosition.y -= (height + 5) ;
                else
                    tooltipPosition.y = 0;
                top = true;
            }
            else
                tooltipPosition.y += 5;
    // Debug.LogWarning("Tooltip "+tooltipPosition+" top="+top+" left="+left+" Screen="+Screen.width+"x"+Screen.height+" initialWidth="+initialWidth+" scale="+widthScaleFactor+"x"+heightScaleFactor+" uiScale="+uiDocument.panelSettings.scale+" "+uiDocument.rootVisualElement.resolvedStyle.width+"x"+uiDocument.rootVisualElement.resolvedStyle.height);
             tooltipContainer.style.left = Mathf.Clamp(tooltipPosition.x, draggingMinValues.x, draggingMaxValues.x);
             tooltipContainer.style.top = Mathf.Clamp(canvasHeight-tooltipPosition.y-height, draggingMinValues.y, draggingMaxValues.y);
            if (!top && left)
                if (anchorBR != null)
                    anchorBR.ShowVisualElement();
            if (top && !left)
                if (anchorTL != null)
                    anchorTL.ShowVisualElement();
            if (!top && !left)
                if (anchorBL != null)
                    anchorBL.ShowVisualElement();
            if (top && left)
                if (anchorTR != null)
                    anchorTR.ShowVisualElement();
            
        }
        
        
        
        /// <summary>
        /// Set Title 
        /// </summary>
        /// <param name="titleText"></param>
        public void SetTitle(string titleText)
        {
            if (title != null)
            {
                title.text = titleText;
                title.style.color = defaultTitleColor;
            }
        }

        /// <summary>
        ///  Set title color
        /// </summary>
        /// <param name="color"></param>
        public void SetTitleColour(Color color)
        {
            if (title != null)
                title.style.color = color;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeText"></param>
        public void SetType(string typeText)
        {
            if (type != null)
                type.text = typeText;

        }

        public void HideType(bool b)
        {
            if (type != null)
                type.style.display = b ? DisplayStyle.None : DisplayStyle.Flex;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        public void SetTypeColour(Color color)
        {
            if (type != null)
                type.style.color = color;

        }

        /// <summary>
        /// Set weight
        /// </summary>
        /// <param name="weightText"></param>
        public void SetWeight(string weightText)
        {
            if (weight != null)
            {
                weight.text = weightText;
            }

            HideWeight(weightText.Length == 0);

        }

        public void HideWeight(bool b)
        {
            if (weight != null)
                weight.style.display = b ? DisplayStyle.None : DisplayStyle.Flex;

        }
        /// <summary>
        /// Set description
        /// </summary>
        /// <param name="descriptionText"></param>
        public void SetDescription(string descriptionText)
        {
            if (description != null)
            {
                description.text = descriptionText;
                if (string.IsNullOrEmpty(descriptionText))
                {
                    description.style.display = DisplayStyle.None;
                }
                else
                {
                    description.style.display = DisplayStyle.Flex;
                }
            }

        }

        /// <summary>
        /// Set icon
        /// </summary>
        /// <param name="icon"></param>
        public void SetIcon(Sprite icon)
        {
            if (icon != null)
            {
                EnableIcon(true);
                itemIcon.SetBackgroundImage(icon);
            }
            else
            {
                EnableIcon(false);
            }
        }

        /// <summary>
        /// Function enable/disable icon panel
        /// </summary>
        /// <param name="i"></param>
        public void EnableIcon(bool i)
        {
            if (iconPanel != null)
            {
                if (i)
                {
                    iconPanel.ShowVisualElement();
                }
                else
                {
                    iconPanel.HideVisualElement();
                }
            }

        }

        /// <summary>
        /// Function to set quality color icon overlay and title
        /// </summary>
        /// <param name="quality"></param>
        public void SetQuality(int quality)
        {
            overlayIcon.style.color = AtavismSettings.Instance.ItemQualityColor(quality);
            if (title != null)
                title.style.color = AtavismSettings.Instance.ItemQualityColor(quality);

        }

        /// <summary>
        /// Function to set quality color icon overlay and title
        /// </summary>
        /// <param name="quality"></param>
        public void SetQualityColor(Color quality)
        {
            overlayIcon.style.color = quality;
            if (title != null)
                title.style.color = quality;

        }

        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAttributeTitle(string text)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.title = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.attributes.Add(info);
        }
        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAttributeTitle(string text, Color colour)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.textColour = colour;
            info.title = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.attributes.Add(info);
        }


        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAttributeSeperator()
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.separator = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.attributes.Add(info);
        }


        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAttributeSocket(string text, Sprite socketIcon, bool singleColumn)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.singleColumnRow = singleColumn;
            info.socket = true;
            info.socketIcon = socketIcon;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.attributes.Add(info);
        }
        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAttributeResource(string text, string value, Sprite socketIcon, bool singleColumn)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.value = value;
            info.singleColumnRow = singleColumn;
            info.resource = true;
            info.socketIcon = socketIcon;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.attributes.Add(info);
        }

        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAttribute(string text, string value, bool singleColumn, int compare1=99, int compare2=99)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.value = value;
            info.text = text;
            info.compare1 = compare1;
            info.compare2 = compare2;
            info.singleColumnRow = singleColumn;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.attributes.Add(info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        /// <param name="colour"></param>
        public void AddAttribute(string text, string value, bool singleColumn, Color colour, int compare1=99, int compare2=99)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            //string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(colour.r), ToByte(colour.g), ToByte(colour.b));
            info.value = /*"<color=" + colourText + ">" + */value;
            info.text = text /*+ "</color>"*/;
            info.compare1 = compare1;
            info.compare2 = compare2;
            info.textColour = colour;
            info.singleColumnRow = singleColumn;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.attributes.Add(info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }


        public void Show(VisualElement targetElement)
        {
           // Debug.LogError("Show");
            tooltipContainer.RegisterCallback<GeometryChangedEvent>(onGeometryShow);
            tooltipContainer.style.opacity = 0.01f;
            // if (hideScheduler != null)
            // {
            //    // Debug.LogError("Pause Hide Scheduler");
            //     hideScheduler.Pause();
            // }
            // Update tooltip content based on the targetElement or other data
            UpdateTooltipContent();
            showing = true;
            // Show the tooltip
            tooltipContainer.ShowVisualElement();
            // showScheduler = tooltipContainer.schedule.Execute(FadeIn).Every(20);//.Until(() => showing);
            uiDocument.sortingOrder = 60;
        }

        private void onGeometryShow(GeometryChangedEvent evt)
        {
           // Debug.LogError("onGeometryShow");
#if !AT_MOBILE           
            CalculatePosition();
#endif
            tooltipContainer.UnregisterCallback<GeometryChangedEvent>(onGeometryShow);
           
          
        }

        public void Hide()
        {
          //  Debug.LogError("Hide");
            // if (showScheduler != null)
            // {
            //     Debug.LogError("Pause Show Scheduler");
            //     showScheduler.Pause();
            // }  
            showing = false;
            showing1 = false;
            showing2 = false;
            // hideScheduler = tooltipContainer.schedule.Execute(FadeOut).Every(20);//.Until(() => !showing);
         
           
            // tooltipContainer.style.opacity = 0f;
        }

       
        private Vector2 CalculateTooltipPosition(VisualElement targetElement)
        {
            // Calculate the tooltip position based on the target element
            // This is placeholder logic
            return new Vector2(100, 100);
        }

        private void UpdateTooltipContent()
        {
            Cleanup();
            if (instance.attributes.Count > 0 && attributeRowTemplate != null)
            {
                bool isLeft = true;
                UIAtavismTooltipAttributeRow lastRow = null;
                UIAtavismTooltipSocketRow lastSocketRow = null;
                UIAtavismTooltipResourceRow lastResourceRow = null;
                // Loop the attributes
                foreach (UIAttributeInfo info in instance.attributes)
                {
                    if (info.separator)
                    {
                        var newListEntry = separatorRowTemplate.Instantiate();
                        attributesContainer.Add(newListEntry);
                        isLeft = true;
                    }
                    else if (info.title)
                    {
                        var newListEntry = titleRowTemplate.Instantiate();
                        Label t = newListEntry.Q<Label>();
                        if (t != null)
                        {
                            t.text = info.text;
                            t.style.color = info.textColour;
                        }
                        attributesContainer.Add(newListEntry);
                        isLeft = true;
                    }
                    else
                    {
                        // Force left column in case it's a single column row
                        if (info.singleColumnRow)
                            isLeft = true;
                        if (isLeft)
                        {
                            // Instantiate a prefab

                            GameObject obj;
                            if (info.socket)
                            {
                                // Instantiate a controller for the data
                                lastSocketRow = new UIAtavismTooltipSocketRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = socketRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastSocketRow;
                                // Initialize the controller script
                                lastSocketRow.SetVisualElement(newListEntry);
                                lastRow = null;
                                lastResourceRow = null;
                                attributesContainer.Add(newListEntry);
                            }
                            else if (info.resource)
                            {
                                // Instantiate a controller for the data
                                lastResourceRow = new UIAtavismTooltipResourceRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = resourceRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastResourceRow;
                                // Initialize the controller script
                                lastResourceRow.SetVisualElement(newListEntry);
                                lastRow = null;
                                lastSocketRow = null;
                                attributesContainer.Add(newListEntry);
                            }
                            else
                            {
                                // Instantiate a controller for the data
                                lastRow = new UIAtavismTooltipAttributeRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = attributeRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastRow;
                                // Initialize the controller script
                                lastRow.SetVisualElement(newListEntry);
                                lastResourceRow = null;
                                lastSocketRow = null;
                                attributesContainer.Add(newListEntry);
                            }
                            // Make some changes if it's a single column row
                            if (info.singleColumnRow)
                            {
                                
                                // Destroy the right column
                                /*   if (lastRow.rightText != null)
                                       Destroy(lastRow.rightText.gameObject);
                                   if (lastRow.rightTextValue != null)
                                       Destroy(lastRow.rightTextValue.gameObject);
                                   if (lastRow.TMPRightText != null)
                                       Destroy(lastRow.TMPRightText.gameObject);
                                   if (lastRow.TMPRightTextValue != null)
                                       Destroy(lastRow.TMPRightTextValue.gameObject);*/
                            }

                        }

                        // Check if we have a row object to work with
                        if (lastRow != null)
                        {
                            Label text = null;
                            Label textValue = null;
                               VisualElement compare1 = null;
                            VisualElement compare2 = null;
                            if (isLeft)
                            {
                                if (lastRow.LeftText != null)
                                    text = lastRow.LeftText;
                                if (lastRow.LeftTextValue != null)
                                    textValue = lastRow.LeftTextValue;
                                if (lastRow.LeftCompare1 != null)
                                    compare1 = lastRow.LeftCompare1;
                                if (lastRow.LeftCompare2 != null)
                                    compare2 = lastRow.LeftCompare2;
                            }
                            else
                            {
                                if (lastRow.RightText != null)
                                    text = lastRow.RightText;
                                if (lastRow.RightTextValue != null)
                                    textValue = lastRow.RightTextValue;
                                if (lastRow.RightCompare1 != null)
                                    compare1 = lastRow.RightCompare1;
                                if (lastRow.RightCompare2 != null)
                                    compare2 = lastRow.RightCompare2;
                            }

                            if (compare1 != null)
                            {
                                if (info.compare1 == 99)
                                {
                                    compare1.HideVisualElement();
                                }
                                else
                                {
                                    switch (info.compare1)
                                    {
                                        case -1:
                                            compare1.AddToClassList("tooltip-attribute-negative");
                                            break;
                                        case 0:
                                            compare1.AddToClassList("tooltip-attribute-neutral");
                                            break;
                                        case 1:
                                            compare1.AddToClassList("tooltip-attribute-positive");
                                            break;
                                        case 10:
                                            compare1.AddToClassList("tooltip-attribute-new");
                                            break;
                                    }
                                    compare1.ShowVisualElement();
                                }
                            }
                            if (compare2 != null)
                            {
                                if (info.compare2 == 99)
                                {
                                    compare2.HideVisualElement();
                                }
                                else
                                {
                                    switch (info.compare2)
                                    {
                                        case -1:
                                            compare2.AddToClassList("tooltip-attribute-negative");
                                            break;
                                        case 0:
                                            compare2.AddToClassList("tooltip-attribute-neutral");
                                            break;
                                        case 1:
                                            compare2.AddToClassList("tooltip-attribute-positive");
                                            break;
                                        case 10:
                                            compare1.AddToClassList("tooltip-attribute-new");
                                            break;
                                    }
                                    compare2.ShowVisualElement();
                                }
                            }

                            // Check if we have the label
                            
                            if (text != null)
                            {
                                if (textValue == null)
                                {
                                    // Set the label text
                                    text.text = info.value + info.text;
                                    text.style.color = info.textColour;
                                }
                                else
                                {
                                    text.text = info.text;
                                    textValue.text = info.value;
                                    text.style.color = info.textColour;
                                    textValue.style.color = info.textColour;
                                }
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                        }
                        //Sockets
                        // Check if we have a row object to work with
                        if (lastSocketRow != null)
                        {
                            Label textRow = null;
                            VisualElement iconRow = null;
                            if (isLeft)
                            {
                                if (lastSocketRow.LeftText != null)
                                    textRow = lastSocketRow.LeftText;
                                if (lastSocketRow.LeftIcon != null)
                                    iconRow = lastSocketRow.LeftIcon;
                            }
                            else
                            {
                                if (lastSocketRow.RightText != null)
                                    textRow = lastSocketRow.RightText;
                                if (lastSocketRow.RightIcon != null)
                                    iconRow = lastSocketRow.RightIcon;
                            }

                            // Check if we have the label
                            if (textRow != null)
                            {
                                // Set the label text
                                textRow.text = info.text;
                                textRow.style.color = info.textColour;
                                // iconRow.enabled = true;
                                if (info.socketIcon != null)
                                    iconRow.SetBackgroundImage(info.socketIcon);
                              if(iconRow!=null)  iconRow.ShowVisualElement();
                                //  if (info.socketIcon == null)
                                //      iconRow.enabled = false;
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                            else
                                Debug.LogWarning("Tooltip Socket row text is null isLeft:" + isLeft);


                        }
                        //Resource
                        // Check if we have a row object to work with
                        if (lastResourceRow != null)
                        {
                            Label textRow = null;
                            Label valueRow = null;
                            VisualElement iconRow = null;
                            if (isLeft)
                            {
                                if (lastResourceRow.LeftText != null)
                                    textRow = lastResourceRow.LeftText;
                                if (lastResourceRow.LeftTextValue != null)
                                    valueRow = lastResourceRow.LeftTextValue;
                                if (lastResourceRow.LeftIcon != null)
                                    iconRow = lastResourceRow.LeftIcon;
                            }
                            else
                            {
                                if (lastResourceRow.RightText != null)
                                    textRow = lastResourceRow.RightText;
                                if (lastResourceRow.RightTextValue != null)
                                    valueRow = lastResourceRow.RightTextValue;
                                if (lastResourceRow.RightIcon != null)
                                    iconRow = lastResourceRow.RightIcon;
                            }

                            // Check if we have the label
                            if (textRow != null)
                            {
                                // Set the label text
                                textRow.text = info.text;
                                    textRow.style.color = info.textColour;
                                if (valueRow != null)
                                {
                                    valueRow.style.color = info.textColour;
                                    valueRow.text = info.value;
                                }
                                else
                                    Debug.LogWarning("Tooltip Resource row value text is null isLeft:" + isLeft);

                                // iconRow.enabled = true;
                                if (info.socketIcon != null)
                                    iconRow.SetBackgroundImage(info.socketIcon);
                                if(iconRow!=null)  iconRow.ShowVisualElement();
                                //  if (info.socketIcon == null)
                                //      iconRow.enabled = false;
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                            else
                                Debug.LogWarning("Tooltip Resource row text is null isLeft:" + isLeft);
                        }
                    }
                }
                // Clear the attributes list, we no longer need it
                attributes.Clear();
                
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void Cleanup()
        {
            // Assuming 'attributesContainer' is a VisualElement that holds your attribute rows
           

            if (attributesContainer != null)
            {
                // Remove all children from the attributes container
                attributesContainer.Clear();
            }
            else
            {
                Debug.LogError("Attributes container not found in the UI Document");
            }

            // Since we're using VisualElements, we don't need to explicitly destroy them
            // as they are managed by the UI Toolkit system
        }

        /// <summary>
        /// 
        /// </summary>
        public static UIAtavismTooltip Instance
        {
            get
            {
                return instance;
            }

        }
        #region Additional Tooltip Functions

        public void SetAdditionalTitle(string titleText)
        {
            if (additionalTooltipTitle != null)
            {
                additionalTooltipTitle.text = titleText;
                additionalTooltipTitle.style.color = defaultTitleColor;
            }
            HideAdditionalTitle(false);
        }

        public void HideAdditionalTitle(bool b)
        {
            if (additionalTooltipTitle != null)
                additionalTooltipTitle.style.display = b ? DisplayStyle.None : DisplayStyle.Flex;

        }

        public void SetAdditionalTitleColour(Color color)
        {
            if (additionalTooltipTitle != null)
                additionalTooltipTitle.style.color = color;
        }

        public void SetAdditionalType(string typeText)
        {
            if (additionalTooltipType != null)
                additionalTooltipType.text = typeText;
            HideAdditionalType(false);
        }

        public void HideAdditionalType(bool b)
        {
            if (additionalTooltipType != null)
                additionalTooltipType.style.display = b ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void SetAdditionalTypeColour(Color color)
        {
            if (additionalTooltipType != null)
                additionalTooltipType.style.color = color;
        }

        public void SetAdditionalWeight(string weightText)
        {
            if (additionalTooltipWeight != null)
            {
                additionalTooltipWeight.text = weightText;
            }
            HideAdditionalWeight(false);

        }

        public void HideAdditionalWeight(bool b)
        {
            if (additionalTooltipWeight != null)
                additionalTooltipWeight.style.display = b ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void SetAdditionalDescription(string descriptionText)
        {
            if (additionalTooltipDescription != null)
            {
                additionalTooltipDescription.text = descriptionText;
                if (string.IsNullOrEmpty(descriptionText))
                {
                    additionalTooltipDescription.style.display = DisplayStyle.None;
                }
                else
                {
                    additionalTooltipDescription.style.display = DisplayStyle.Flex;
                }
            }
        }

        public void SetAdditionalIcon(Sprite icon)
        {
            if (icon != null)
            {
                EnableAdditionalIcon(true);
                additionalTooltipItemIcon.SetBackgroundImage(icon);
            }
            else
            {
                EnableAdditionalIcon(false);
            }
        }

        public void EnableAdditionalIcon(bool i)
        {
            if (i)
            {
                additionalTooltipItemIconPanel.style.display = DisplayStyle.Flex;
            }
            else
            {
                iconPanel.style.display = DisplayStyle.None;
            }
        }

        public void SetAdditionalQuality(int quality)
        {
            additionalTooltipOverlayIcon.style.color = AtavismSettings.Instance.ItemQualityColor(quality);
            if (additionalTooltipTitle != null)
                additionalTooltipTitle.style.color = AtavismSettings.Instance.ItemQualityColor(quality);
        }

        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeTitle(string text)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.title = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes.Add(info);
        }
        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeTitle(string text, Color colour)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.textColour = colour;
            info.title = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes.Add(info);
        }


        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeSeperator()
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.separator = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes.Add(info);
        }


        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeSocket(string text, Sprite socketIcon, bool singleColumn)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.singleColumnRow = singleColumn;
            info.socket = true;
            info.socketIcon = socketIcon;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes.Add(info);
        }
        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeResource(string text, string value, Sprite socketIcon, bool singleColumn)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.value = value;
            info.singleColumnRow = singleColumn;
            info.resource = true;
            info.socketIcon = socketIcon;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes.Add(info);
        }
        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttribute(string text, string value, bool singleColumn, int compare1=99, int compare2=99)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.value = value;
            info.text = text;
            info.compare1 = compare1;
            info.compare2 = compare2;
            info.singleColumnRow = singleColumn;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes.Add(info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        /// <param name="colour"></param>
        public void AddAdditionalAttribute(string text, string value, bool singleColumn, Color colour, int compare1=99, int compare2=99)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            //  string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(colour.r), ToByte(colour.g), ToByte(colour.b));
            info.value = value;
            info.text = text;
            info.compare1 = compare1;
            info.compare2 = compare2;
            info.textColour = colour;
            info.singleColumnRow = singleColumn;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes.Add(info);
        }

        public void ShowAdditionalTooltip()
        {
#if AT_MOBILE
            TurnOffAll();
#endif            
            // Assuming 'additionalTooltipContainer' is your main container for the additional tooltip
            // VisualElement additionalTooltipContainer = root.Q<VisualElement>("additionalTooltipContainer");

            if (additionalTooltip != null)
            {
                additionalTooltip.style.opacity = 1;
                showing1 = true;
                // Show the tooltip container
                additionalTooltip.ShowVisualElement();

                // Cleanup any existing elements
                CleanupAdditional();
                UpdateAdditionalTooltipContent();
                // Check if there are attributes to display
                // if (additionalAttributes.Count > 0)
                // {
                //     // Loop through the attributes and create elements
                //     foreach (UIAttributeInfo info in additionalAttributes)
                //     {
                //         VisualElement newElement = CreateAttributeElement(info); // You need to implement this method
                //         additionalTooltipContainer.Add(newElement);
                //     }
                //
                //     // Clear the attributes list as they are now added to the tooltip
                //     additionalAttributes.Clear();
                // }

                // Bring description to front if needed
                // ... Additional logic for description ...
            }
            else
            {
                Debug.LogError("Additional tooltip container not found in the UI Document");
            }
        }

        
        // Method to create a new attribute element based on UIAttributeInfo
        // private VisualElement CreateAttributeElement(UIAttributeInfo info)
        // {
        //     // Create and configure your VisualElement here based on the info
        //     // For example, create a Label or a custom VisualElement
        //     Label label = new Label(info.text);
        //     label.style.color = info.textColour; // Convert Color to StyleColor
        //     return label;
        // }

        private void UpdateAdditionalTooltipContent()
        {
            CleanupAdditional();
            if (instance.additionalAttributes.Count > 0 && attributeRowTemplate != null)
            {
                bool isLeft = true;
                UIAtavismTooltipAttributeRow lastRow = null;
                UIAtavismTooltipSocketRow lastSocketRow = null;
                UIAtavismTooltipResourceRow lastResourceRow = null;
                // Loop the attributes
                foreach (UIAttributeInfo info in instance.additionalAttributes)
                {
                    if (info.separator)
                    {
                        var newListEntry = separatorRowTemplate.Instantiate();
                        additionalTooltipAttributesContainer.Add(newListEntry);
                        isLeft = true;
                    }
                    else if (info.title)
                    {
                        var newListEntry = titleRowTemplate.Instantiate();
                        Label t = newListEntry.Q<Label>();
                        if (t != null)
                        {
                            t.text = info.text;
                            t.style.color = info.textColour;
                        }
                        additionalTooltipAttributesContainer.Add(newListEntry);
                        isLeft = true;
                    }
                    else
                    {
                        // Force left column in case it's a single column row
                        if (info.singleColumnRow)
                            isLeft = true;
                        if (isLeft)
                        {
                            // Instantiate a prefab

                            GameObject obj;
                            if (info.socket)
                            {
                                // Instantiate a controller for the data
                                lastSocketRow = new UIAtavismTooltipSocketRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = socketRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastSocketRow;
                                // Initialize the controller script
                                lastSocketRow.SetVisualElement(newListEntry);
                                lastResourceRow = null;
                                lastRow = null;
                                additionalTooltipAttributesContainer.Add(newListEntry);
                            }
                            else if (info.resource)
                            {
                                // Instantiate a controller for the data
                                lastResourceRow = new UIAtavismTooltipResourceRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = resourceRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastResourceRow;
                                // Initialize the controller script
                                lastResourceRow.SetVisualElement(newListEntry);
                                lastRow = null;
                                lastSocketRow = null;
                                additionalTooltipAttributesContainer.Add(newListEntry);
                            }
                            else
                            {
                                // Instantiate a controller for the data
                                lastRow = new UIAtavismTooltipAttributeRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = attributeRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastRow;
                                // Initialize the controller script
                                lastRow.SetVisualElement(newListEntry);
                                lastResourceRow = null;
                                lastSocketRow = null;
                                additionalTooltipAttributesContainer.Add(newListEntry);
                            }
                            // Make some changes if it's a single column row
                            if (info.singleColumnRow)
                            {
                                
                                // Destroy the right column
                                /*   if (lastRow.rightText != null)
                                       Destroy(lastRow.rightText.gameObject);
                                   if (lastRow.rightTextValue != null)
                                       Destroy(lastRow.rightTextValue.gameObject);
                                   if (lastRow.TMPRightText != null)
                                       Destroy(lastRow.TMPRightText.gameObject);
                                   if (lastRow.TMPRightTextValue != null)
                                       Destroy(lastRow.TMPRightTextValue.gameObject);*/
                            }

                        }

                        // Check if we have a row object to work with
                        if (lastRow != null)
                        {
                            Label text = null;
                            Label textValue = null;
                            VisualElement compare1 = null;
                            VisualElement compare2 = null;
                            if (isLeft)
                            {
                                if (lastRow.LeftText != null)
                                    text = lastRow.LeftText;
                                if (lastRow.LeftTextValue != null)
                                    textValue = lastRow.LeftTextValue;
                                if (lastRow.LeftCompare1 != null)
                                    compare1 = lastRow.LeftCompare1;
                                if (lastRow.LeftCompare2 != null)
                                    compare2 = lastRow.LeftCompare2;
                            }
                            else
                            {
                                if (lastRow.RightText != null)
                                    text = lastRow.RightText;
                                if (lastRow.RightTextValue != null)
                                    textValue = lastRow.RightTextValue;
                                if (lastRow.RightCompare1 != null)
                                    compare1 = lastRow.RightCompare1;
                                if (lastRow.RightCompare2 != null)
                                    compare2 = lastRow.RightCompare2;
                            }

                            if (compare1 != null)
                            {
                                if (info.compare1 == 99)
                                {
                                    compare1.HideVisualElement();
                                }
                                else
                                {
                                    switch (info.compare1)
                                    {
                                        case -1:
                                            compare1.AddToClassList("tooltip-attribute-negative");
                                            break;
                                        case 0:
                                            compare1.AddToClassList("tooltip-attribute-neutral");
                                            break;
                                        case 1:
                                            compare1.AddToClassList("tooltip-attribute-positive");
                                            break;
                                        case 10:
                                            compare1.AddToClassList("tooltip-attribute-new");
                                            break;
                                    }
                                    compare1.ShowVisualElement();
                                }
                            }
                            if (compare2 != null)
                            {
                                if (info.compare2 == 99)
                                {
                                    compare2.HideVisualElement();
                                }
                                else
                                {
                                    switch (info.compare2)
                                    {
                                        case -1:
                                            compare2.AddToClassList("tooltip-attribute-negative");
                                            break;
                                        case 0:
                                            compare2.AddToClassList("tooltip-attribute-neutral");
                                            break;
                                        case 1:
                                            compare2.AddToClassList("tooltip-attribute-positive");
                                            break;
                                        case 10:
                                            compare1.AddToClassList("tooltip-attribute-new");
                                            break;
                                    }
                                    compare2.ShowVisualElement();
                                }
                            }
                            // Check if we have the label
                            
                            if (text != null)
                            {
                                if (textValue == null)
                                {
                                    // Set the label text
                                    text.text = info.value + info.text;
                                    text.style.color = info.textColour;
                                }
                                else
                                {
                                    text.text = info.text;
                                    textValue.text = info.value;
                                    text.style.color = info.textColour;
                                    textValue.style.color = info.textColour;
                                }
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                        }
                        //Sockets
                        // Check if we have a row object to work with
                        if (lastSocketRow != null)
                        {
                            Label textRow = null;
                            VisualElement iconRow = null;
                            if (isLeft)
                            {
                                if (lastSocketRow.LeftText != null)
                                    textRow = lastSocketRow.LeftText;
                                if (lastSocketRow.LeftIcon != null)
                                    iconRow = lastSocketRow.LeftIcon;
                            }
                            else
                            {
                                if (lastSocketRow.RightText != null)
                                    textRow = lastSocketRow.RightText;
                                if (lastSocketRow.RightIcon != null)
                                    iconRow = lastSocketRow.RightIcon;
                            }

                            // Check if we have the label
                            if (textRow != null)
                            {
                                // Set the label text
                                textRow.text = info.text;
                                textRow.style.color = info.textColour;
                                // iconRow.enabled = true;
                                if (info.socketIcon != null)
                                    iconRow.style.backgroundImage = info.socketIcon.texture;
                                if(iconRow!=null)  iconRow.ShowVisualElement();
                                //  if (info.socketIcon == null)
                                //      iconRow.enabled = false;
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                            else
                                Debug.LogWarning("Tooltip Socket row text is null isLeft:" + isLeft);


                        }
                        //Resource
                        // Check if we have a row object to work with
                        if (lastResourceRow != null)
                        {
                            Label textRow = null;
                            Label valueRow = null;
                            VisualElement iconRow = null;
                            if (isLeft)
                            {
                                if (lastResourceRow.LeftText != null)
                                    textRow = lastResourceRow.LeftText;
                                if (lastResourceRow.LeftTextValue != null)
                                    valueRow = lastResourceRow.LeftTextValue;
                                if (lastResourceRow.LeftIcon != null)
                                    iconRow = lastResourceRow.LeftIcon;
                            }
                            else
                            {
                                if (lastResourceRow.RightText != null)
                                    textRow = lastResourceRow.RightText;
                                if (lastResourceRow.RightTextValue != null)
                                    valueRow = lastResourceRow.RightTextValue;
                                if (lastResourceRow.RightIcon != null)
                                    iconRow = lastResourceRow.RightIcon;
                            }

                            // Check if we have the label
                            if (textRow != null)
                            {
                                // Set the label text
                                textRow.text = info.text;
                                    textRow.style.color = info.textColour;
                                if (valueRow != null)
                                {
                                    valueRow.style.color = info.textColour;
                                    valueRow.text = info.value;
                                }
                                else
                                    Debug.LogWarning("Tooltip Resource row value text is null isLeft:" + isLeft);

                                // iconRow.enabled = true;
                                if (info.socketIcon != null)
                                    iconRow.SetBackgroundImage(info.socketIcon);
                                if(iconRow!=null)  iconRow.ShowVisualElement();
                                //  if (info.socketIcon == null)
                                //      iconRow.enabled = false;
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                            else
                                Debug.LogWarning("Tooltip Resource row text is null isLeft:" + isLeft);
                        }
                    }
                }
                // Clear the attributes list, we no longer need it
                additionalAttributes.Clear();
                
            }
        }
        
        
        private void CleanupAdditional()
        {
            // Assuming 'attributesContainer' is a VisualElement that holds your attribute rows
            // VisualElement additionalAttributesRows = root.Q<VisualElement>("additionalAttributesRows");

            if (additionalTooltipAttributesContainer != null)
            {
                // Remove all children from the attributes container
                additionalTooltipAttributesContainer.Clear();
            }
            else
            {
                Debug.LogError("Attributes container not found in the UI Document");
            }

            // Since we're using VisualElements, we don't need to explicitly destroy them
            // as they are managed by the UI Toolkit system
        }

        #endregion Additional Tooltip Functions

        #region Additional 2 Tooltip Functions

        public void SetAdditionalTitle2(string titleText)
        {
            if (additionalTooltipTitle2 != null)
            {
                additionalTooltipTitle2.text = titleText;
                additionalTooltipTitle2.style.color = defaultTitleColor;
            }
            HideAdditionalTitle2(false);
        }

        public void HideAdditionalTitle2(bool b)
        {
            if (additionalTooltipTitle2 != null)
                additionalTooltipTitle2.style.display = b ? DisplayStyle.None : DisplayStyle.Flex;


        }

        public void SetAdditionalTitleColour2(Color color)
        {
            if (additionalTooltipTitle2 != null)
                additionalTooltipTitle2.style.color = color;
        }

        public void SetAdditionalType2(string typeText)
        {
            if (additionalTooltipType2 != null)
                additionalTooltipType2.text = typeText;
            HideAdditionalType2(false);
        }

        public void HideAdditionalType2(bool b)
        {
            if (additionalTooltipType2 != null)
                additionalTooltipType2.style.display = b ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void SetAdditionalTypeColour2(Color color)
        {
            if (additionalTooltipType2 != null)
                additionalTooltipType2.style.color = color;

        }

        public void SetAdditionalWeight2(string weightText)
        {
            if (additionalTooltipWeight2 != null)
            {
                additionalTooltipWeight2.text = weightText;
            }

            HideAdditionalWeight2(false);

        }

        public void HideAdditionalWeight2(bool b)
        {
            if (additionalTooltipWeight2 != null)
                additionalTooltipWeight2.style.display = b ? DisplayStyle.None : DisplayStyle.Flex;

        }

        public void SetAdditionalDescription2(string descriptionText)
        {
            if (additionalTooltipDescription2 != null)
            {
                additionalTooltipDescription2.text = descriptionText;
                if (string.IsNullOrEmpty(descriptionText))
                {
                    additionalTooltipDescription2.HideVisualElement();
                }
                else
                {
                    additionalTooltipDescription2.ShowVisualElement();
                }
            }
        }

        public void SetAdditionalIcon2(Sprite icon)
        {
            if (icon != null)
            {
                EnableAdditionalIcon2(true);
                additionalTooltipItemIcon2.style.backgroundImage = icon.texture;
            }
            else
            {
                EnableAdditionalIcon2(false);
            }
        }

        public void EnableAdditionalIcon2(bool i)
        {
            if (i)
            {
                additionalTooltipItemIconPanel2.ShowVisualElement();
            }
            else
            {
                additionalTooltipItemIconPanel2.HideVisualElement();
            }
        }

        public void SetAdditionalQuality2(int quality)
        {
            additionalTooltipOverlayIcon2.style.color = AtavismSettings.Instance.ItemQualityColor(quality);
            if (additionalTooltipTitle2 != null)
                additionalTooltipTitle2.style.color = AtavismSettings.Instance.ItemQualityColor(quality);
        }

        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeTitle2(string text)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.title = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes2.Add(info);
        }
        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeTitle2(string text, Color colour)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.textColour = colour;
            info.title = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes2.Add(info);
        }


        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeSeperator2()
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.separator = true;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes2.Add(info);
        }


        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeSocket2(string text, Sprite socketIcon, bool singleColumn)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.singleColumnRow = singleColumn;
            info.socket = true;
            info.socketIcon = socketIcon;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes2.Add(info);
        }
        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttributeResource2(string text, string value, Sprite socketIcon, bool singleColumn)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.text = text;
            info.value = value;
            info.singleColumnRow = singleColumn;
            info.resource = true;
            info.socketIcon = socketIcon;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes2.Add(info);
        }
        /// <summary>
        /// Set Attribute Row of tooltip
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        public void AddAdditionalAttribute2(string text, string value, bool singleColumn, int compare1=99, int compare2=99)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            info.value = value;
            info.text = text;
            info.compare1 = compare1;
            info.compare2 = compare2;
            info.singleColumnRow = singleColumn;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes2.Add(info);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        /// <param name="singleColumn"></param>
        /// <param name="colour"></param>
        public void AddAdditionalAttribute2(string text, string value, bool singleColumn, Color colour, int compare1=99, int compare2=99)
        {
            // Create new attribute info
            UIAttributeInfo info = new UIAttributeInfo();
            //    string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(colour.r), ToByte(colour.g), ToByte(colour.b));
            info.value = value;
            info.text = text;
            info.compare1 = compare1;
            info.compare2 = compare2;
            info.textColour = colour;
            info.singleColumnRow = singleColumn;
            info.margin = new RectOffset();

            // Add it to the attribute list
            instance.additionalAttributes2.Add(info);
        }

        public void ShowAdditionalTooltip2()
        {
            // Assuming 'additionalTooltipContainer2' is your main container for the second tooltip
            VisualElement additionalTooltipContainer2 = root.Q<VisualElement>("additionalTooltipContainer2");

            if (additionalTooltipContainer2 != null)
            {
                // Show the tooltip container
                additionalTooltip.style.opacity = 1;
                showing2 = true;
                additionalTooltipContainer2.ShowVisualElement();

                // Cleanup any existing elements
                CleanupAdditional2();
                UpdateAdditionalTooltipContent2();
                // // Check if there are attributes to display
                // if (additionalAttributes2.Count > 0)
                // {
                //     // Loop through the attributes and create elements
                //     foreach (UIAttributeInfo info in additionalAttributes2)
                //     {
                //         VisualElement newElement = CreateAttributeElement2(info); // You need to implement this method
                //         additionalTooltipContainer2.Add(newElement);
                //     }
                //
                //     // Clear the attributes list as they are now added to the tooltip
                //     additionalAttributes2.Clear();
                // }

                // Bring description to front if needed
                // ... Additional logic for description ...
            }
            else
            {
                Debug.LogError("Second tooltip container not found in the UI Document");
            }
        }

        // Method to create a new attribute element based on UIAttributeInfo for the second tooltip
        // private VisualElement CreateAttributeElement2(UIAttributeInfo info)
        // {
        //     // Create and configure your VisualElement here based on the info
        //     // For example, create a Label or a custom VisualElement
        //     Label label = new Label(info.text);
        //     label.style.color = info.textColour; // Convert Color to StyleColor
        //     return label;
        // }

    private void UpdateAdditionalTooltipContent2()
        {
            // CleanupAdditional();
            if (instance.additionalAttributes2.Count > 0 && attributeRowTemplate != null)
            {
                bool isLeft = true;
                UIAtavismTooltipAttributeRow lastRow = null;
                UIAtavismTooltipSocketRow lastSocketRow = null;
                UIAtavismTooltipResourceRow lastResourceRow = null;
                // Loop the attributes
                foreach (UIAttributeInfo info in instance.additionalAttributes2)
                {
                    if (info.separator)
                    {
                        var newListEntry = separatorRowTemplate.Instantiate();
                        additionalTooltipAttributesContainer2.Add(newListEntry);
                        isLeft = true;
                    }
                    else if (info.title)
                    {
                        var newListEntry = titleRowTemplate.Instantiate();
                        Label t = newListEntry.Q<Label>();
                        if (t != null)
                        {
                            t.text = info.text;
                            t.style.color = info.textColour;
                        }
                        additionalTooltipAttributesContainer2.Add(newListEntry);
                        isLeft = true;
                    }
                    else
                    {
                        // Force left column in case it's a single column row
                        if (info.singleColumnRow)
                            isLeft = true;
                        if (isLeft)
                        {
                            // Instantiate a prefab

                            GameObject obj;
                            if (info.socket)
                            {
                                // Instantiate a controller for the data
                                lastSocketRow = new UIAtavismTooltipSocketRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = socketRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastSocketRow;
                                // Initialize the controller script
                                lastSocketRow.SetVisualElement(newListEntry);
                                lastResourceRow = null;
                                lastRow = null;
                                additionalTooltipAttributesContainer2.Add(newListEntry);
                            }
                            else if (info.resource)
                            {
                                // Instantiate a controller for the data
                                lastResourceRow = new UIAtavismTooltipResourceRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = resourceRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastResourceRow;
                                // Initialize the controller script
                                lastResourceRow.SetVisualElement(newListEntry);
                                lastRow = null;
                                lastSocketRow = null;
                                additionalTooltipAttributesContainer2.Add(newListEntry);
                            }
                            else
                            {
                                // Instantiate a controller for the data
                                lastRow = new UIAtavismTooltipAttributeRow();
                                // Instantiate the UXML template for the entry
                                var newListEntry = attributeRowTemplate.Instantiate();
                                // Assign the controller script to the visual element
                                newListEntry.userData = lastRow;
                                // Initialize the controller script
                                lastRow.SetVisualElement(newListEntry);
                                lastResourceRow = null;
                                lastSocketRow = null;
                                additionalTooltipAttributesContainer2.Add(newListEntry);
                            }
                            // Make some changes if it's a single column row
                            if (info.singleColumnRow)
                            {
                                
                                // Destroy the right column
                                   if (lastRow.RightText != null)
                                       lastRow.RightText.text = "";
                                   if (lastRow.RightTextValue != null)
                                       lastRow.RightTextValue.text = "";
                            }

                        }

                        // Check if we have a row object to work with
                        if (lastRow != null)
                        {
                            Label text = null;
                            Label textValue = null;
                             VisualElement compare1 = null;
                            VisualElement compare2 = null;
                            if (isLeft)
                            {
                                if (lastRow.LeftText != null)
                                    text = lastRow.LeftText;
                                if (lastRow.LeftTextValue != null)
                                    textValue = lastRow.LeftTextValue;
                                if (lastRow.LeftCompare1 != null)
                                    compare1 = lastRow.LeftCompare1;
                                if (lastRow.LeftCompare2 != null)
                                    compare2 = lastRow.LeftCompare2;
                            }
                            else
                            {
                                if (lastRow.RightText != null)
                                    text = lastRow.RightText;
                                if (lastRow.RightTextValue != null)
                                    textValue = lastRow.RightTextValue;
                                if (lastRow.RightCompare1 != null)
                                    compare1 = lastRow.RightCompare1;
                                if (lastRow.RightCompare2 != null)
                                    compare2 = lastRow.RightCompare2;
                            }

                            if (compare1 != null)
                            {
                                if (info.compare1 == 99)
                                {
                                    compare1.HideVisualElement();
                                }
                                else
                                {
                                    switch (info.compare1)
                                    {
                                        case -1:
                                            compare1.AddToClassList("tooltip-attribute-negative");
                                            break;
                                        case 0:
                                            compare1.AddToClassList("tooltip-attribute-neutral");
                                            break;
                                        case 1:
                                            compare1.AddToClassList("tooltip-attribute-positive");
                                            break;
                                        case 10:
                                            compare1.AddToClassList("tooltip-attribute-new");
                                            break;
                                    }
                                    compare1.ShowVisualElement();
                                }
                            }
                            if (compare2 != null)
                            {
                                if (info.compare2 == 99)
                                {
                                    compare2.HideVisualElement();
                                }
                                else
                                {
                                    switch (info.compare2)
                                    {
                                        case -1:
                                            compare2.AddToClassList("tooltip-attribute-negative");
                                            break;
                                        case 0:
                                            compare2.AddToClassList("tooltip-attribute-neutral");
                                            break;
                                        case 1:
                                            compare2.AddToClassList("tooltip-attribute-positive");
                                            break;
                                        case 10:
                                            compare1.AddToClassList("tooltip-attribute-new");
                                            break;
                                    }
                                    compare2.ShowVisualElement();
                                }
                            }
                            // Check if we have the label
                            
                            if (text != null)
                            {
                                if (textValue == null)
                                {
                                    // Set the label text
                                    text.text = info.value + info.text;
                                    text.style.color = info.textColour;
                                }
                                else
                                {
                                    text.text = info.text;
                                    textValue.text = info.value;
                                    text.style.color = info.textColour;
                                    textValue.style.color = info.textColour;
                                }
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                        }
                        //Sockets
                        // Check if we have a row object to work with
                        if (lastSocketRow != null)
                        {
                            Label textRow = null;
                            VisualElement iconRow = null;
                            if (isLeft)
                            {
                                if (lastSocketRow.LeftText != null)
                                    textRow = lastSocketRow.LeftText;
                                if (lastSocketRow.LeftIcon != null)
                                    iconRow = lastSocketRow.LeftIcon;
                            }
                            else
                            {
                                if (lastSocketRow.RightText != null)
                                    textRow = lastSocketRow.RightText;
                                if (lastSocketRow.RightIcon != null)
                                    iconRow = lastSocketRow.RightIcon;
                            }

                            // Check if we have the label
                            if (textRow != null)
                            {
                                // Set the label text
                                textRow.text = info.text;
                                textRow.style.color = info.textColour;
                                // iconRow.enabled = true;
                                if (info.socketIcon != null)
                                    iconRow.style.backgroundImage = info.socketIcon.texture;
                                if(iconRow!=null)  iconRow.ShowVisualElement();
                                //  if (info.socketIcon == null)
                                //      iconRow.enabled = false;
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                            else
                                Debug.LogWarning("Tooltip Socket row text is null isLeft:" + isLeft);


                        }
                        //Resource
                        // Check if we have a row object to work with
                        if (lastResourceRow != null)
                        {
                            Label textRow = null;
                            Label valueRow = null;
                            VisualElement iconRow = null;
                            if (isLeft)
                            {
                                if (lastResourceRow.LeftText != null)
                                    textRow = lastResourceRow.LeftText;
                                if (lastResourceRow.LeftTextValue != null)
                                    valueRow = lastResourceRow.LeftTextValue;
                                if (lastResourceRow.LeftIcon != null)
                                    iconRow = lastResourceRow.LeftIcon;
                            }
                            else
                            {
                                if (lastResourceRow.RightText != null)
                                    textRow = lastResourceRow.RightText;
                                if (lastResourceRow.RightTextValue != null)
                                    valueRow = lastResourceRow.RightTextValue;
                                if (lastResourceRow.RightIcon != null)
                                    iconRow = lastResourceRow.RightIcon;
                            }

                            // Check if we have the label
                            if (textRow != null)
                            {
                                // Set the label text
                                textRow.text = info.text;
                                    textRow.style.color = info.textColour;
                                if (valueRow != null)
                                {
                                    valueRow.style.color = info.textColour;
                                    valueRow.text = info.value;
                                }
                                else
                                    Debug.LogWarning("Tooltip Resource row value text is null isLeft:" + isLeft);

                                // iconRow.enabled = true;
                                if (info.socketIcon != null)
                                    iconRow.style.backgroundImage = info.socketIcon.texture;
                                if(iconRow!=null)  iconRow.ShowVisualElement();
                                //  if (info.socketIcon == null)
                                //      iconRow.enabled = false;
                                // Flip is left
                                if (!info.singleColumnRow)
                                    isLeft = !isLeft;
                            }
                            else
                                Debug.LogWarning("Tooltip Resource row text is null isLeft:" + isLeft);
                        }
                    }
                }
                // Clear the attributes list, we no longer need it
                additionalAttributes2.Clear();
                
            }
        }

        private void CleanupAdditional2()
        {
            if (additionalTooltipAttributesContainer2 != null)
            {
                // Remove all children from the attributes container
                additionalTooltipAttributesContainer2.Clear();
            }
            else
            {
                Debug.LogError("Attributes container 2 not found in the UI Document");
            }

            // Since we're using VisualElements, we don't need to explicitly destroy them
            // as they are managed by the UI Toolkit system
        }

        #endregion Additional 2 Tooltip Functions
    }
}
