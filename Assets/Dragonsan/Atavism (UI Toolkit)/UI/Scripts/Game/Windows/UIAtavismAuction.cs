using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Atavism.UI.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [Serializable]
    public enum searchType
    {
        Type,
        SubType,
        Slot,
    }
    [Serializable]
    public class menuTree
    {
        //[SerializeField] int m_ID;
        public string name;
        public string value;
        public searchType type;
        public List<menuTree> submenu = new List<menuTree>();
    }

    public class MenuTreeEntry
    {
        public string name;
        public string nested = "";
        public int id = 0;
        public string value;
        public searchType type;

        public override string ToString()
        {
            return "name="+name+", id="+id+", nested="+nested+", value="+value+", type="+type;
        }
    }
    
    
    public class UIAtavismAuction : UIAtavismWindowBase
    {

        [AtavismSeparator("Auctions")]
        public VisualElement buyPanel;
        [SerializeField] VisualTreeAsset buyItemEntryTemplate;
        bool auctionlist = true;
        private  ListView _buySellgrid;
        private List<Auction> auctionsList = new List<Auction>();
        bool sortCount = false;
        bool sortName = true;
        bool sortPrice = false;
        string searchRace = "Any";
        string searchClass = "Any";
        string searchText = "";
        string searchCat = "";
        string searchCatType = "";
        Dictionary<string, object> searchCatDic = new Dictionary<string, object>();
        private UIButtonToggleGroup tabs;
        
        
        private TextField searchInput;
        private TextField searchMinLevelInput;
        private TextField searchMaxLevelInput;
        private UIDropdown searchClassDropdown;
        private  UIDropdown searchRaceDropdown;
        // [Tooltip("List of Names Races Defined in Atavism. First must be Any")]
        // [SerializeField] List<string> raceKeys = new List<string>();
        // [Tooltip("List of Names Class Defined in Atavism. First must be Any")]
        // [SerializeField] List<string> classKeys = new List<string>();
        private List<Toggle> qualityList;
        string qualitylevels = "";
        List<int> qualitylevelsList = new List<int>();

        int minLevel = 1;
        int maxLevel = 100;
        public bool showPriceSellItem = false;
        VisualElement searchPanel;
        VisualElement searchPanelMenu;
        private Label countSortButtonText;
        private Label nameSortButtonText;
        private Label priceSortButtonText;
        private VisualElement countSortIcon;
        private VisualElement nameSortIcon;
        private VisualElement priceSortIcon;
        bool sortAsc = true;
        public Label errorText;

        [AtavismSeparator("Inventory")]
        private VisualElement inventoryPanel;
        private ListView inventoryGrid;
        bool showSell = false;

        [AtavismSeparator("Sell/Buy")]
        private VisualElement subWindowSellPanel;
        bool showSubWindowSellPanel=false;
        private VisualElement sellItemIcon;
        private VisualElement sellItemQuality;
        private Label sellItemCount;
        private Label sellItemName;
        UIAtavismCurrencyInputPanel sellCurrencyInputPanel;
        private UIAtavismCurrencyDisplay sellSumaryCurrencies;
        private UITextField sellItemsCount;
        private SliderInt sellItemsCountSlider;
        private Label totalCount;
        private UIAtavismCurrencyDisplay commissionSumaryCurrencies;
        private Label commissionText;

        public VisualTreeAsset auctionCountPrefab;
        private ListView sellCountListGrid;
        private ListView buyCountListGrid;
        
        List<AuctionCountPrice> sellCountList = new List<AuctionCountPrice>();
        List<AuctionCountPrice> buyCountList = new List<AuctionCountPrice>();
        List<UIAtavismAuctionSellCountEntry> sellCountEntryList = new List<UIAtavismAuctionSellCountEntry>();
        List<UIAtavismAuctionSellCountEntry> buyCountEntryList = new List<UIAtavismAuctionSellCountEntry>();
        
        private Button confirmButton;
        private Button cancelButton;

        public string sellButtonText = "Sell Instant";
        public string listSellButtonText = "List Sell";
        public string buyButtonText = "Buy Instant";
        public string orderButtonText = "Place Order";
        bool sellInstant = false;
        bool buyInstant = false;
        int totalAuctionCountItems = 100;
        
        protected Vector2  subDraggingMinValues, subDraggingMaxValues,  subDraggingMouseOffset;
        protected bool isSubDragging;
        
        [AtavismSeparator("Transactions")]
        private VisualElement transactionsPanel;
        private ListView transactionsGrid;
        public List<Auction> ownAuctionsList = new List<Auction>();
        
        bool buying = true; bool selling = false; bool bought = false; bool sold = false; bool expired = false;
        bool showTransactions = false;
        
        public Button takeAllButton;

        [AtavismSeparator("Menu Settings")]
        public List<menuTree> menuFilter = new List<menuTree>();
        private TreeView menuGrid;
        int menuObjectNum = 0;


        protected override void Start()
        {
            base.Start();

        }

        protected override void OnEnable()
        {
            base.OnEnable();
          
            if (searchMinLevelInput != null)
                searchMinLevelInput.value = "1";
            if (searchMaxLevelInput != null)
                searchMaxLevelInput.value = "999";
            
            
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("AUCTION_OPEN", this);
            AtavismEventSystem.RegisterEvent("AUCTION_LIST_UPDATE", this);
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("AUCTION_LIST_FOR_GROUP_UPDATE", this);
            AtavismEventSystem.RegisterEvent("AUCTION_OWN_LIST_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("AUCTION_OPEN", this);
            AtavismEventSystem.UnregisterEvent("AUCTION_LIST_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("AUCTION_LIST_FOR_GROUP_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("AUCTION_OWN_LIST_UPDATE", this);
            
            base.unregisterEvents();
        }


        protected override bool registerUI()
        {
             if (!base.registerUI())
                return false;
            // uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("AuctionWindow");

             tabs = uiWindow.Query<UIButtonToggleGroup>("auction-top-menu");
             tabs.OnItemIndexChanged += TopMenuChange;
             
             buyPanel = uiWindow.Query<VisualElement>("auction-buy-panel");
            _buySellgrid = uiWindow.Query<ListView>("auction-buy-grid");
#if UNITY_6000_0_OR_NEWER                
            ScrollView scrollView = _buySellgrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif

            _buySellgrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismAuctionBuyListEntry newListEntryLogic = new UIAtavismAuctionBuyListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = buyItemEntryTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            _buySellgrid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismAuctionBuyListEntry).SetData(auctionsList[index]);
            };
            _buySellgrid.selectionChanged += SelectAuction;
            // _buySellgrid.fixedItemHeight = 65;
            
          
            
            searchInput = uiWindow.Query<TextField>("auction-search-textfield");
            searchInput.RegisterValueChangedCallback(SearchAuction);
            searchMinLevelInput = uiWindow.Query<TextField>("auction-search-level-min");
            searchMaxLevelInput = uiWindow.Query<TextField>("auction-search-level-max");
            searchMinLevelInput.RegisterValueChangedCallback(SearchMinLevel);
            searchMaxLevelInput.RegisterValueChangedCallback(SearchMaxLevel);

            searchClassDropdown = uiWindow.Query<UIDropdown>("auction-search-class");
            searchClassDropdown.Screen = uiScreen;
            searchClassDropdown.RegisterCallback<ChangeEvent<string>>(SearchClassChange);
            // searchClassDropdown.RegisterValueChangedCallback(SearchClassChange);
            
            searchRaceDropdown = uiWindow.Query<UIDropdown>("auction-search-race");
            searchRaceDropdown.Screen = uiScreen;
            searchRaceDropdown.RegisterCallback<ChangeEvent<string>>(SearchRaceChange);
            // searchRaceDropdown.RegisterValueChangedCallback(SearchRaceChange);

            VisualElement qualityPanel = uiWindow.Query<VisualElement>("quality");
            qualityList = qualityPanel.Query<Toggle>().ToList();
            foreach (var v in qualityList)
            {
                v.RegisterValueChangedCallback(SearchQuality);
            }
            
            searchPanel = uiWindow.Query<VisualElement>("auction-search-panel");
            searchPanelMenu = uiWindow.Query<VisualElement>("auction-search-menu-panel");

            countSortButtonText = uiWindow.Query<Label>("buy-count-header");
            nameSortButtonText = uiWindow.Query<Label>("buy-name-header");
            priceSortButtonText = uiWindow.Query<Label>("buy-price-header");
            countSortButtonText.RegisterCallback<MouseDownEvent>(SortCount);
            nameSortButtonText.RegisterCallback<MouseDownEvent>(SortItemName);
            priceSortButtonText.RegisterCallback<MouseDownEvent>(SortPrice);
            countSortIcon = uiWindow.Query<VisualElement>("buy-count-header-icon");
            nameSortIcon = uiWindow.Query<VisualElement>("buy-name-header-icon");
            priceSortIcon = uiWindow.Query<VisualElement>("buy-price-header-icon");

            
            
            errorText = uiWindow.Query<Label>("error-message");

//        [AtavismSeparator("Inventory")]
            inventoryPanel = uiWindow.Query<VisualElement>("auction-inventory-panel");
            inventoryGrid = uiWindow.Query<ListView>("inventory-item-list");
#if UNITY_6000_0_OR_NEWER                
             scrollView = inventoryGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            inventoryGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismAuctionBuyListEntry newListEntryLogic = new UIAtavismAuctionBuyListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = buyItemEntryTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            inventoryGrid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismAuctionBuyListEntry).SetData(auctionsList[index]);
            };
            inventoryGrid.selectionChanged += SelectItem;
            // inventoryGrid.fixedItemHeight = 65;
            
            
            //      Sell/Buy
            subWindowSellPanel = uiDocument.rootVisualElement.Query<VisualElement>("auction-sub-window");
            //Item
            sellItemName = subWindowSellPanel.Query<Label>("item-name");
            VisualElement slot = subWindowSellPanel.Q<VisualElement>("slot-container");
            sellItemIcon = slot.Q<VisualElement>("icon");
            sellItemQuality = slot.Q<VisualElement>("quality");
            sellItemCount = slot.Q<Label>("count");
            // sellslot = 
            //Quantity
            sellItemsCount = subWindowSellPanel.Query<UITextField>("quantity-text-field");
            sellItemsCount.RegisterValueChangedCallback(changeQuantity);
            sellItemsCountSlider = subWindowSellPanel.Query<SliderInt>("quantity-slider");
            sellItemsCountSlider.RegisterValueChangedCallback(changeQuantitySlider);
            // sellItemsCountSlider.lowValue = 1;
            totalCount = subWindowSellPanel.Query<Label>("quantity-label");

            VisualElement createCurrency = subWindowSellPanel.Q<VisualElement>("sell-currency-input");
            sellCurrencyInputPanel = new UIAtavismCurrencyInputPanel();
            sellCurrencyInputPanel.SetVisualElement(createCurrency);
            sellCurrencyInputPanel.SetOnChange(changeSellPrice);
            sellCurrencyInputPanel.SetCurrencyReverseOrder = true;

            
            subWindowSellPanel.RegisterCallback<GeometryChangedEvent>(onGeometryShowSellPanel);
            
            
            // // sellCurrencyInputPanel.SetCurrencies(Inventory.Instance.GetMainCurrencies());
            // //Price
            // for (int i = 1; i <= 5; i++)
            // {
            //     UITextField value = sellPanel.Q<UITextField>("currency-input-"+i);
            //     VisualElement icon = sellPanel.Q<VisualElement>("currency-icon-"+i);
            //     if (value != null)
            //     {
            //         sellCurrencyInput.Add(value);
            //         //TODO: To add RegisterValueChangedCallback to input field
            //         // TextField aa = new TextField();
            //         // sellCurrencyInput
            //         // aa.RegisterValueChangedCallback()
            //         // sellCurrencyInput.RegisterCallback<MouseDownEvent>(SortCount);
            //         sellCurrencyInputImage.Add(icon);
            //     }
            //
            //     
            // }
          //  Debug.LogError("sellCurrencyInput "+sellCurrencyInput.Count);
            //Commission
            VisualElement commissionPanel = subWindowSellPanel.Query<VisualElement>("commission");
            VisualElement curr = commissionPanel.Q<VisualElement>("currency-panel");
            commissionSumaryCurrencies = new UIAtavismCurrencyDisplay();
            commissionSumaryCurrencies.SetVisualElement(curr);
            commissionSumaryCurrencies.ReverseOrder = true;
            
            //Total Price
            VisualElement totalPanel = subWindowSellPanel.Query<VisualElement>("total");
            VisualElement totalCurr = totalPanel.Q<VisualElement>("currency-panel");
            sellSumaryCurrencies = new UIAtavismCurrencyDisplay();
            sellSumaryCurrencies.SetVisualElement(totalCurr);
            sellSumaryCurrencies.ReverseOrder = true;
            confirmButton = subWindowSellPanel.Query<Button>("confirm-button");
            confirmButton.clicked += ClickSell;
            cancelButton = subWindowSellPanel.Query<Button>("cancel-button");
            cancelButton.clicked += CancelSell;
            // confirmButtonText = uiWindow.Query<Label>("auction-sell-confirm-button");
             // commissionText = uiWindow.Query<Label>("auction-sell-commission-text");

             
             Label subWindowtitle = subWindowSellPanel.Q<Label>("Window-title");
             subWindowtitle.text = "";
             Button closeButton = subWindowSellPanel.Q<Button>("Window-close-button");
             closeButton.clicked += CloseSellWindow; 
             VisualElement subWindowDraggableTrigger = subWindowSellPanel.Query<VisualElement>("Window-Draggable-Trigger");
             if (subWindowDraggableTrigger != null)
                 subWindowDraggableTrigger.RegisterCallback<MouseDownEvent>(onSubDraggableTriggerMouseDown, TrickleDown.TrickleDown);
             
             
             
            //public AuctionCountSlot 
            sellCountListGrid = subWindowSellPanel.Query<ListView>("sellers-list");
#if UNITY_6000_0_OR_NEWER    
             scrollView = sellCountListGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            sellCountListGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismAuctionSellCountEntry newListEntryLogic = new UIAtavismAuctionSellCountEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = auctionCountPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                sellCountEntryList.Add(newListEntryLogic);
                return newListEntry;
            };
            sellCountListGrid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismAuctionSellCountEntry).SetData(sellCountList[index]);
            };
            // sellCountListGrid.selec
            // inventoryGrid.selectionChanged += SelectAuction;
            // sellCountListGrid.fixedItemHeight = 65;
            
            buyCountListGrid = subWindowSellPanel.Query<ListView>("buyers-list");
#if UNITY_6000_0_OR_NEWER    
             scrollView = buyCountListGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            buyCountListGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismAuctionSellCountEntry newListEntryLogic = new UIAtavismAuctionSellCountEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = auctionCountPrefab.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                buyCountEntryList.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            buyCountListGrid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismAuctionSellCountEntry).SetData(buyCountList[index]);
            };
            // inventoryGrid.selectionChanged += SelectAuction;
            // buyCountListGrid.fixedItemHeight = 65;
             
            // [AtavismSeparator("Transactions")]
            transactionsPanel = uiWindow.Query<VisualElement>("auction-transaction-panel");
            // public List<UGUICurrency> seledCurrencies ;
            transactionsGrid = uiWindow.Query<ListView>("auction-transaction-list");
#if UNITY_6000_0_OR_NEWER                
             scrollView = transactionsGrid.Q<ScrollView>();
            scrollView.mouseWheelScrollSize = 19;
#endif
            transactionsGrid.makeItem = () =>
            {
                // Instantiate a controller for the data
                UIAtavismAuctionBuyListEntry newListEntryLogic = new UIAtavismAuctionBuyListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = buyItemEntryTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                // Return the root of the instantiated visual tree
                return newListEntry;
            };
            transactionsGrid.bindItem = (item, index) =>
            {
                (item.userData as UIAtavismAuctionBuyListEntry).SetData(ownAuctionsList[index]);
            };
            
            
            // transactionsGrid.selectionChanged += CancelAuction;
            // transactionsGrid.fixedItemHeight = 65;
            UIButtonToggleGroup menuButton = uiWindow.Query<UIButtonToggleGroup>("auction-transaction-menu-button");
            menuButton.OnItemIndexChanged += showTransactionList;

            takeAllButton = uiWindow.Query<Button>("take-all-button");
            takeAllButton.clicked += TakeAll;

            menuGrid = uiDocument.rootVisualElement.Query<TreeView>("auction-buy-left-menu-grid");
            var items = GenerateMenu(menuFilter, ""); 
            menuGrid.SetRootItems(items);
            menuGrid.makeItem = () =>
            {
                var element = new Label();
                element.RegisterCallback<ClickEvent>((e) => {
                    var el = e.target as Label;
                    // Debug.LogError("Clicked "+el.userData+" expanded "+menuGrid.viewController.IsExpanded((int)el.userData));
                    if (menuGrid.viewController.IsExpandedByIndex((int)el.userData))
                    {
                        menuGrid.viewController.CollapseItemByIndex((int)el.userData, true);  
                    }
                    else
                    {
                        menuGrid.viewController.ExpandItemByIndex((int)el.userData, false);
                    }
                });
                return element;
            };;
            menuGrid.bindItem = (e, i) =>
            {
                var item = menuGrid.GetItemDataForIndex<MenuTreeEntry>(i);
                (e as Label).text = item.name;
                (e as Label).userData = i;
            };
            menuGrid.Rebuild();
            menuGrid.selectedIndicesChanged += TreeMenuSelected;
            // menuGrid.selec

            // Hide();

            if (searchMinLevelInput != null)
                searchMinLevelInput.value = "1";
            if (searchMaxLevelInput != null)
                searchMaxLevelInput.value = "99";

         //   Debug.LogError("UIAtavismAuction registerUI End");
            return true;
        }

       

        private void onSubDraggableTriggerMouseDown(MouseDownEvent evt)
        {
            if (evt.button == (int)DraggingMouseButton)
                subDraggingBegin();
        }
       
        private void subDraggingBegin()
        {
            float width = subWindowSellPanel.resolvedStyle.width;
            float height = subWindowSellPanel.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            subDraggingMinValues.x = 0f;
            subDraggingMinValues.y = 0f;
            subDraggingMaxValues.x = canvasWidth - width;
            subDraggingMaxValues.y = canvasHeight - height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            
            subDraggingMouseOffset.x = scaledMousePosition.x - subWindowSellPanel.resolvedStyle.left;
            subDraggingMouseOffset.y = (canvasHeight - scaledMousePosition.y) - subWindowSellPanel.resolvedStyle.top;
            
            
            isSubDragging = true;
        }
        protected virtual void subDragging()
        {
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
            Vector2 position = new Vector2(scaledMousePosition.x - subDraggingMouseOffset.x, (canvasHeight - scaledMousePosition.y) - subDraggingMouseOffset.y);
            
            subWindowSellPanel.style.left = Mathf.Clamp(position.x,subDraggingMinValues.x, subDraggingMaxValues.x);
            subWindowSellPanel.style.top = Mathf.Clamp(position.y, subDraggingMinValues.y, subDraggingMaxValues.y);
        }
        protected virtual void subDraggingEnd()
        {
            //   Debug.LogError("draggingEnd");
            isSubDragging = false;
        }

        private void changeQuantity(ChangeEvent<string> evt)
        {
            changeQuantity();
        }

        private void showTransactionList(int obj)
        {
            switch (obj)
            {
                case 0:
                    ShowBuying();
                    break;
                case 1:
                    ShowSelling();
                    break;
                case 2:
                    ShowBought();
                    break;
                case 3:
                    ShowSold();
                    break;
                case 4:
                    ShowExpired();
                    break;
            }
        }

        private void TopMenuChange(int obj)
        {
            switch (obj)
            {
                case 0:
                   ShowAuctionList();
                    break;
                case 1:
                    ShowSellList();
                    break;
                case 2:
                    ShowTransactions();
                    break;
            }
        }

        private void TreeMenuSelected(IEnumerable<int> obj)
        {
            if (obj.Count() == 0)
                return;
            var data = menuGrid.GetItemDataForIndex<MenuTreeEntry>(obj.First());
            // Debug.LogError("TreeMenuSelected "+data.ToString()+" "+obj.First());
            // if (menuGrid.IsExpanded(obj.First()))
            // {
            //     menuGrid.CollapseItem(obj.First(), true);
            // }
            // else
            // {
            //     menuGrid.ExpandItem(obj.First());    
            // }
            Search(data.type.ToString(), data.value, data.nested);
        }
        /// <summary>
        /// Function to populate TreeView element 
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="nested"></param>
        /// <returns></returns>

        List<TreeViewItemData<MenuTreeEntry>> GenerateMenu(List<menuTree> menu, string nested)
        {
            var treeViewSubItemsData = new List<TreeViewItemData<MenuTreeEntry>>(menu.Count);

            for (int iii = 0; iii < menu.Count; iii++)
            {
                menuTree m = menu[iii];
                string nested1 = nested;
                if (nested.Length > 0)
                    nested1 += ";" + m.type + "|" + m.value;
                else
                    nested1 = m.type + "|" + m.value;

                menuObjectNum++;
                MenuTreeEntry ms = new MenuTreeEntry();
                ms.name = m.name;
                ms.value = m.value;
                ms.type = m.type;
                ms.nested = nested;
                ms.id = menuObjectNum;
                List<TreeViewItemData<MenuTreeEntry>> subdata = null;
                if (m.submenu.Count > 0)
                {
                    subdata = GenerateMenu(m.submenu, nested1);
                }

                if (subdata == null)
                {
                    treeViewSubItemsData.Add( new TreeViewItemData<MenuTreeEntry>(ms.id, ms));
                }
                else
                {
                    treeViewSubItemsData.Add(new TreeViewItemData<MenuTreeEntry>(ms.id, ms, subdata));
                }
              
            }

            return treeViewSubItemsData;
        }

 /// <summary>
        /// 
        /// </summary>
        public override void Show()
        {
            base.Show();
          //  AtavismSettings.Instance.OpenWindow(this);
            // uiWindow.ShowVisualElement();
            // AtavismUIUtility.BringToFront(this.gameObject);
            sellCurrencyInputPanel.SetCurrencies(AtavismAuction.Instance.GetCurrencyType);
            AtavismAuction.Instance.GetAuctionList();
            // showing = true;
            tabs.Set(0);
            // ShowAuctionList();
        }
        /// <summary>
        /// 
        /// </summary>
        void OnlyShow()
        {
            
            if(!showing)
            base.Show();
            // uiWindow.ShowVisualElement();
            // AtavismUIUtility.BringToFront(this.gameObject);
            // showing = true;
            tabs.Set(0);
            ShowAuctionList();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            // AtavismSettings.Instance.CloseWindow(this);
            
            // uiWindow.HideVisualElement();
            // showing = false;
            AtavismAuction.Instance.Auctions.Clear();
            AtavismAuction.Instance.OwnAuctions.Clear();
            AtavismAuction.Instance.AuctionsForGroupOrder.Clear();
            AtavismAuction.Instance.AuctionsForGroupSell.Clear();

            HideSellWindow();
        }

        /// <summary>
        /// Function called by AtavismEventSystem
        /// </summary>
        /// <param name="eData"></param>
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "AUCTION_OPEN")
            {
                OnlyShow();
            }
            else if (eData.eventType == "AUCTION_LIST_UPDATE")
                {
                    //  Debug.LogError("OnEvent AUCTION_LIST_UPDATE");
                    
                    if (_buySellgrid!=null)
                {
                    _buySellgrid.Clear();
                    auctionsList = AtavismAuction.Instance.Auctions.Values.ToList();
                    int i = 1;
                
                    _buySellgrid.itemsSource = auctionsList;
                    _buySellgrid.Rebuild();
                    _buySellgrid.selectedIndex = -1;
                    
                    if (auctionsList.Count >= AtavismAuction.Instance.AuctionsLimit)
                    {
                        if (errorText != null)
#if AT_I2LOC_PRESET
                        errorText.text = I2.Loc.LocalizationManager.GetTranslation("List of displayed auctions has been limited, please use filters to narrow your search");
#else
                            errorText.text = "List of displayed auctions has been limited, please use filters to narrow your search";
#endif
                    }
                    else
                    {
                        if (errorText != null)
                        errorText.text = "";
                    }

                }

                //  Debug.LogError("OnEvent AUCTION_LIST_UPDATE End");


            }
            else if (eData.eventType == "INVENTORY_UPDATE")
            {
                if (showSell)
                    ShowSellList();
            }
            else if (eData.eventType == "AUCTION_LIST_FOR_GROUP_UPDATE")
            {
                // Debug.LogError("OnEvent AUCTION_LIST_FOR_GROUP_UPDATE");
                buyCountListGrid.Clear();
                sellCountListGrid.Clear();
                buyCountEntryList.Clear();
                sellCountEntryList.Clear();
                Dictionary<long, AuctionCountPrice> auctionsForGroupOrder = AtavismAuction.Instance.AuctionsForGroupOrder;
                Dictionary<long, AuctionCountPrice> auctionsForGroupSell = AtavismAuction.Instance.AuctionsForGroupSell;
                sellCountList = auctionsForGroupSell.Values.ToList();
                buyCountList = auctionsForGroupOrder.Values.ToList();
                
                buyCountListGrid.itemsSource = buyCountList;
                buyCountListGrid.Rebuild();
                buyCountListGrid.selectedIndex = -1;
                sellCountListGrid.itemsSource = sellCountList;
                sellCountListGrid.Rebuild();
                sellCountListGrid.selectedIndex = -1;
                // buyCountListGrid.RefreshItems();
               // sellCountListGrid.RefreshItems();
                
                int i = 1;
                //   Debug.LogError(auctions.Keys);
                foreach (long aucid in auctionsForGroupOrder.Keys)
                {
                    //    Debug.LogError(aucid + " " + auctionsForGroupOrder.Count);
                    if (i == 1)
                    {
                        if (showSell)
                        {
                               sellCurrencyInputPanel.SetCurrencyAmounts(auctionsForGroupOrder[aucid].currency, auctionsForGroupOrder[aucid].price);
                            
                           // changeSellPrice();
                        }
                    }
                    i++;
                }
                int j = 1;
                foreach (long aucid in auctionsForGroupSell.Keys)
                {
                   // Debug.LogError(aucid + " auctionsForGroupSell=" + auctionsForGroupSell.Count+" j="+j+ " sellCountList="+ sellCountList.Count);
                    if (j == 1)
                    {
                        if (auctionlist)
                        {
                            sellCurrencyInputPanel.SetCurrencyAmounts(auctionsForGroupSell[aucid].currency, auctionsForGroupSell[aucid].price);
                           
                        }
                      //  changeSellPrice();
                    }
                    j++;
                }
                totalAuctionCountItems = 0;
                foreach (long aucid in auctionsForGroupSell.Keys)
                {
                    totalAuctionCountItems += auctionsForGroupSell[aucid].count;
                }
                changeSellPrice();

            }
            else if (eData.eventType == "AUCTION_OWN_LIST_UPDATE")
            {
                transactionsGrid.Clear();
                Dictionary<int, Auction> auctionsForGroupOrder = AtavismAuction.Instance.OwnAuctions;
                ownAuctionsList = auctionsForGroupOrder.Values.ToList();
                
                transactionsGrid.itemsSource = ownAuctionsList;
                transactionsGrid.Rebuild();
                transactionsGrid.selectedIndex = -1;
                if (selling || buying)
                {
                    transactionsGrid.selectionChanged += CancelAuction;
                }
                else
                {
                    transactionsGrid.selectionChanged -= CancelAuction;
                }

            }
        }

        private void CancelAuction(IEnumerable<object> enumerable)
        {
            if (selling || buying)
            {
                if (enumerable.Count() == 0)
                    return;

                Auction auction = (Auction)enumerable.First();
#if AT_I2LOC_PRESET
        UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Do you really want to cancel the auctions") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/"+auction.item.BaseName) + " ?", auction, cancelAuction);
#else
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(
                    "Do you really want to cancel the auctions " + auction.item.BaseName + "?", auction, cancelAuction);
#endif
            }
        }

        void cancelAuction(object auction, bool accepted)
        {
            if (accepted)
                AtavismAuction.Instance.CancelAuction((Auction)auction, selling, buying);

        }


        /// <summary>
        /// 
        /// </summary>
        public void ShowAuctionList()
        {
            auctionlist = true;
            showSell = false;
            showTransactions = false;
            // string qualitylevels = "";
            qualitylevels = "";
            int k = 0;
            qualitylevelsList.Clear();
            foreach (Toggle t in qualityList)
            {
                k++;
                if (t != null)
                {
                    if (t.value)
                    {
                        qualitylevels += k;
                        qualitylevelsList.Add(k);
                    }
                }
                if (k < qualityList.Count)
                    qualitylevels += ";";
            }
            List<object> list = new List<object>();
            foreach (int elm in qualitylevelsList)
                list.Add(elm);
            AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);

            if (buyPanel!=null)
                buyPanel.ShowVisualElement();
            if (searchPanel!=null)
                searchPanel.ShowVisualElement();
            if (searchPanelMenu != null)
                searchPanelMenu.ShowVisualElement();
            if (inventoryPanel!=null)
                inventoryPanel.HideVisualElement();
            HideSellWindow();

            if (transactionsPanel != null)
                transactionsPanel.HideVisualElement();
            if (_buySellgrid != null)
            {
                _buySellgrid.Clear();
                auctionsList = AtavismAuction.Instance.Auctions.Values.ToList();
                _buySellgrid.itemsSource = auctionsList;
                _buySellgrid.Rebuild();
                _buySellgrid.selectedIndex = -1;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public void ShowSellList()
        {
            auctionlist = false;
            showSell = true;
            showTransactions = false;
           
            HideSellWindow();

            if (buyPanel!=null)
                buyPanel.HideVisualElement();
            if (searchPanel!=null)
                searchPanel.HideVisualElement();
            if (searchPanelMenu != null)
                    searchPanelMenu.HideVisualElement();
           
            if (transactionsPanel!=null)
                transactionsPanel.HideVisualElement();
            if (inventoryPanel!=null)
                inventoryPanel.ShowVisualElement();



            inventoryGrid.Clear();
           
            Dictionary<int, Bag> bags = Inventory.Instance.Bags;
            // Dictionary<int, AtavismInventoryItem> aii = new Dictionary<int, AtavismInventoryItem>();
            auctionsList.Clear();
            int it = 0;
            List<string> itemGroups = new List<string>();
            for (int i = 0; i < bags.Count; i++)
            {
                for (int k = 0; k < bags[i].numSlots; k++)
                {
                    if (bags[i].items.ContainsKey(k))
                    {
                        if (Inventory.Instance.GetCurrencyGroup(bags[i].items[k].CurrencyType) == Inventory.Instance.GetCurrencyGroup(AtavismAuction.Instance.GetCurrencyType))
                        if (!bags[i].items[k].isBound && bags[i].items[k].auctionHouse)
                        {

                            String itemGroup = bags[i].items[k].templateId.ToString();
                            if (bags[i].items[k].enchantLeval > 0)
                                itemGroup += "_E" + bags[i].items[k].enchantLeval;
                            if (bags[i].items[k].SocketSlotsOid.Count > 0)
                            {
                                List<long> socketItems = new List<long>();
                                //       HashMap<Integer, SocketInfo> itemSockets = (HashMap<Integer, SocketInfo>)Item.getProperty("sockets");
                                //    ArrayList<Long> socketItems = new ArrayList<Long>();
                                foreach (String sType in bags[i].items[k].SocketSlotsOid.Keys)
                                {
                                    foreach (int sId in bags[i].items[k].SocketSlotsOid[sType].Keys)
                                    {
                                        //  if (itemSockets.get(sId).GetItemOid() != null)
                                        //   {
                                        socketItems.Add(bags[i].items[k].SocketSlotsOid[sType][sId]);
                                        //     }
                                    }
                                }
                                socketItems.Sort();


                                //  Collections.sort(socketItems);
                                foreach (long l in socketItems)
                                {
                                    itemGroup += "_S" + l;
                                }
                              
                            }


                            if (!itemGroups.Contains(itemGroup) || bags[i].items[k].StackLimit == 1)
                            {
                                itemGroups.Add(itemGroup);
                                Auction auction = new Auction();
                                if (bags[i].items[k].StackLimit > 1)
                                    auction.count = Inventory.Instance.GetCountOfItem(bags[i].items[k].templateId);
                                auction.item = bags[i].items[k];
                                int currencyId = 0;
                                long currencyAmount = 0;
                                Inventory.Instance.ConvertCurrencyToBaseCurrency(bags[i].items[k].currencyType,
                                    bags[i].items[k].cost, out currencyId, out currencyAmount);
                                auction.buyout = currencyAmount;
                                auction.currency = currencyId;

                                if (bags[i].items[k].StackLimit > 1)
                                    auction.groupId = itemGroup;
                                auctionsList.Add(auction);
                                //    Debug.LogWarning("Auction item for sell bn:" + bags[i].items[k].BaseName + " el:" + bags[i].items[k].enchantLeval+" c:"+ bags[i].items[k].Count);
                                it++;
                            }
                        }

                    }

                }

            }
            
                
            inventoryGrid.itemsSource = auctionsList;
            inventoryGrid.Rebuild();
            inventoryGrid.selectedIndex = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ShowTransactions()
        {
            auctionlist = false;
            showSell = false;
            showTransactions = true;
          
            if (buyPanel!=null)
                buyPanel.HideVisualElement();
            if (transactionsPanel!=null)
                transactionsPanel.ShowVisualElement();
            if (inventoryPanel!=null)
                inventoryPanel.HideVisualElement();
            ShowBuying();
        }
        /// <summary>
        /// Sort List of the auction by Count
        /// </summary>

        public void SortCount(MouseDownEvent evt)
        {
            if(sortCount)
                sortAsc = !sortAsc;
            sortCount = true;
            sortName = false;
            sortPrice = false;
            if (countSortIcon != null)
            {
                countSortIcon.RemoveFromClassList("tooltip-attribute-negative");
                countSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                if (sortAsc)
                    countSortIcon.AddToClassList("tooltip-attribute-positive");
                else
                    countSortIcon.AddToClassList("tooltip-attribute-negative");
            }

            if (priceSortIcon != null)
            {
                priceSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                priceSortIcon.RemoveFromClassList("tooltip-attribute-negative");
            }
            if (nameSortIcon != null)
            {
                nameSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                nameSortIcon.RemoveFromClassList("tooltip-attribute-negative");
            }
            if (auctionlist)
            {
                List<object> list = new List<object>();
                foreach (int elm in qualitylevelsList)
                    list.Add(elm);
                AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);
            }

        }
        /// <summary>
        /// Sort List of the auction by Item Name
        /// </summary>
        public void SortItemName(MouseDownEvent evt)
        {
            if(sortName)
                sortAsc = !sortAsc;
            sortName = true;
            sortCount = false;
            sortPrice = false;
          
            if (nameSortIcon != null)
            {
                nameSortIcon.RemoveFromClassList("tooltip-attribute-negative");
                nameSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                if (sortAsc)
                    nameSortIcon.AddToClassList("tooltip-attribute-positive");
                else
                    nameSortIcon.AddToClassList("tooltip-attribute-negative");
            }

            if (priceSortIcon != null)
            {
                priceSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                priceSortIcon.RemoveFromClassList("tooltip-attribute-negative");
            }
            if (countSortIcon != null)
            {
                countSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                countSortIcon.RemoveFromClassList("tooltip-attribute-negative");
            }
            if (auctionlist)
            {
                List<object> list = new List<object>();
                foreach (int elm in qualitylevelsList)
                    list.Add(elm);
                AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);
            }
        }
        /// <summary>
        /// Sort List of the auction by Price
        /// </summary>
        public void SortPrice(MouseDownEvent evt)
        {
            if(sortPrice)
                sortAsc = !sortAsc;
            sortCount = false;
            sortName = false;
            sortPrice = true;
            if (priceSortIcon != null)
            {
                priceSortIcon.RemoveFromClassList("tooltip-attribute-negative");
                priceSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                if (sortAsc)
                {
                    priceSortIcon.AddToClassList("tooltip-attribute-positive");
                }
                else
                {
                    priceSortIcon.AddToClassList("tooltip-attribute-negative");
                }
            }

            if (nameSortIcon != null)
            {
                nameSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                nameSortIcon.RemoveFromClassList("tooltip-attribute-negative");
            }
            if (countSortIcon != null)
            {
                countSortIcon.RemoveFromClassList("tooltip-attribute-positive");
                countSortIcon.RemoveFromClassList("tooltip-attribute-negative");
            }
            if (auctionlist)
            {
                List<object> list = new List<object>();
                foreach (int elm in qualitylevelsList)
                    list.Add(elm);
                AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);

            }
            else if (showSell)
            {

            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ShowBuying()
        {
            buying = true;
            selling = false;
            bought = false;
            sold = false;
            expired = false;
            AtavismAuction.Instance.GetOwnAuctionList(buying, selling, bought, sold, expired);
        }
        /// <summary>
        /// 
        /// </summary>
        public void ShowSelling()
        {
            buying = false;
            selling = true;
            bought = false;
            sold = false;
            expired = false;

            AtavismAuction.Instance.GetOwnAuctionList(buying, selling, bought, sold, expired);

        }
        /// <summary>
        /// 
        /// </summary>
        public void ShowBought()
        {
            buying = false;
            selling = false;
            bought = true;
            sold = false;
            expired = false;

            AtavismAuction.Instance.GetOwnAuctionList(buying, selling, bought, sold, expired);

        }
        /// <summary>
        /// 
        /// </summary>
        public void ShowSold()
        {
            buying = false;
            selling = false;
            bought = false;
            sold = true;
            expired = false;

            AtavismAuction.Instance.GetOwnAuctionList(buying, selling, bought, sold, expired);
        }
        /// <summary>
        /// 
        /// </summary>
        public void ShowExpired()
        {
            buying = false;
            selling = false;
            bought = false;
            sold = false;
            expired = true;

            AtavismAuction.Instance.GetOwnAuctionList(buying, selling, bought, sold, expired);

        }

        /// <summary>
        /// 
        /// </summary>
        public void SearchAuction(ChangeEvent<string> evt)
        {
            searchText = searchInput.text;
            List<object> list = new List<object>();
            foreach (int elm in qualitylevelsList)
                list.Add(elm);
            AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);
        }

        /// <summary>
        /// 
        /// </summary>
        public void TakeAll()
        {
            AtavismAuction.Instance.TakeReward(buying, selling, bought, sold, expired);
        }

        // private void SearchClassChange(ChangeEvent<int> evt)
        // {
        //     searchClassDropdown.Options();
        // }
        /// <summary>
        ///  
        /// </summary>
        public void SearchClassChange(ChangeEvent<string> evt)
        {
             // Debug.LogError("searchClassChange " + searchClassDropdown.value + " " + searchClassDropdown.options[searchClassDropdown.value].text);
            searchClass = evt.newValue; 
        //    Debug.LogError("searchClassChange " +searchClass);
                //classKeys[searchClassDropdown.Index];
            List<object> list = new List<object>();
            foreach (int elm in qualitylevelsList)
                list.Add(elm);
            AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SearchRaceChange(ChangeEvent<string> evt)
        {
            
            // Debug.LogError("searchClassChange " + searchRaceDropdown.value + " " + searchRaceDropdown.options[searchRaceDropdown.value].text);
            searchRace = evt.newValue;
          // Debug.LogError("SearchRaceChange " +searchRace);
            List<object> list = new List<object>();
            foreach (int elm in qualitylevelsList)
                list.Add(elm);
            AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SearchQuality(ChangeEvent<bool> evt)
        {
            qualitylevels = "";
            int k = 0;
            qualitylevelsList.Clear();
            foreach (Toggle t in qualityList)
            {
                k++;
                if (t != null)
                {
                    if (t.value)
                    {
                        qualitylevels += k;
                        qualitylevelsList.Add(k);
                    }
                }
                if (k < qualityList.Count)
                    qualitylevels += ";";
            }
            List<object> list = new List<object>();
            foreach (int elm in qualitylevelsList)
                list.Add(elm);
            AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SearchMinLevel(ChangeEvent<string> evt)
        {
            if (searchMinLevelInput.value == "" || searchMinLevelInput.value == " ")
                searchMinLevelInput.value = "1";
            minLevel = int.Parse(searchMinLevelInput.value);
            if (maxLevel < minLevel)
                searchMinLevelInput.value = maxLevel.ToString();
            List<object> list = new List<object>();
            foreach (int elm in qualitylevelsList)
                list.Add(elm);
            AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SearchMaxLevel(ChangeEvent<string> evt)
        {
            if (searchMaxLevelInput.value == "" || searchMaxLevelInput.value == " ")
                searchMaxLevelInput.value = "999";
            maxLevel = int.Parse(searchMaxLevelInput.value);
            if (maxLevel < minLevel)
                searchMaxLevelInput.value = minLevel.ToString();
            List<object> list = new List<object>();
            foreach (int elm in qualitylevelsList)
                list.Add(elm);
            AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseSellWindow()
        {
            showSubWindowSellPanel = false;
            itemtosell = null;
            auctionGroupId = "";
            transactionsGrid.selectedIndex = -1;
            _buySellgrid.selectedIndex = -1;
        }

        public void HideSellWindow()
        {
            showSubWindowSellPanel = false;
        }

        public void ShowSellWindow()
        {
            showSubWindowSellPanel=true;
            subWindowSellPanel.ShowVisualElement();
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        public void ClickSell()
        {
            int currencyId = 0;
            long cost = 0;
            sellCurrencyInputPanel.GetCurrencyAmount(out currencyId, out cost);
            Dictionary<string, object> currencies = new Dictionary<string, object>();
            currencies.Add(currencyId.ToString(), cost);
            if (showSell)
            {
                AtavismAuction.Instance.CreateAuction(itemtosell, currencies, (int)sellItemsCountSlider.value, auctionGroupId);
            }
            else
            {
                if (buyInstant)
                {
                    List<Vector2> curr = new List<Vector2>();
                    curr.Add(new Vector2(currencyId, cost));
                    if (Inventory.Instance.DoesPlayerHaveEnoughCurrency(curr))
                    {
                        AtavismAuction.Instance.BuyAuction(auctionGroupId, currencies, (int)sellItemsCountSlider.value);
                    }
                    else
                    {
                        AtavismEventSystem.DispatchEvent("ERROR_MESSAGE", new String[]{"You do not have enough currency to buy this item"});
                        return;
                    }
                }
                else
                    AtavismAuction.Instance.OrderAuction(auctionGroupId, currencies, (int)sellItemsCountSlider.value);
            }
            CloseSellWindow();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelSell()
        {
            CloseSellWindow();
            itemtosell = null;
            auctionGroupId = "";
            inventoryGrid.selectedIndex = -1;
        }

        

        /// <summary>
        ///  function search parent currency and return it
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lc"></param>
        /// <returns></returns>
        Currency GetChildCurrency(int id, List<Currency> lc)
        {
            foreach (Currency c in lc)
            {
                if (c.convertsTo.Equals(id))
                    return c;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        void changeSellPrice()
        {
            
            int currencyType = 0;
            long currencyAmount = 0;
            sellCurrencyInputPanel.GetCurrencyAmount(out currencyType, out currencyAmount);
            
       
          //  Debug.LogError("currencyAmount=" + currencyAmount+" ");
            if (String.IsNullOrEmpty(sellItemsCount.value) || sellItemsCount.value == "0")
                sellItemsCount.value = "1";
            int number = int.Parse(sellItemsCount.value);
            //   Debug.LogError("changeSellPrice: currencyAmount:" + currencyAmount + " number:" + number);
            Dictionary<long, AuctionCountPrice> auctionsForGroupOrder = AtavismAuction.Instance.AuctionsForGroupOrder;
            Dictionary<long, AuctionCountPrice> auctionsForGroupSell = AtavismAuction.Instance.AuctionsForGroupSell;
            sellInstant = false;
            buyInstant = false;
            long pricetotal = 0;
            if (showSell)
            {
                int ii = 1;
                bool setPrice = false;
                foreach (long aucid in auctionsForGroupOrder.Keys)
                {
                    if (auctionsForGroupOrder[aucid].price == currencyAmount)
                    {
                        if (auctionsForGroupOrder[aucid].count > number)
                        {
                            auctionsForGroupOrder[aucid].selected = 1;
                            // if (buyCountEntryList.Count >= ii)
                            // {
                            //     buyCountEntryList[ii - 1].setPartial();
                            //     //    pricetotal += (auctionsForGroupOrder[aucid].count - number) * auctionsForGroupOrder[aucid].price;
                            // }
                            // else
                            // {
                            //     //     Debug.LogError("changeSellPrice: buyCountList.Count:" + buyCountList.Count + " ii-1:" + (ii - 1));
                            //     //     numberslected += auctionsForGroupOrder[aucid].count;
                            // }
                        }
                        else if (auctionsForGroupOrder[aucid].count == number)
                        {
                            auctionsForGroupOrder[aucid].selected = 2;
                            // if (buyCountEntryList.Count >= ii)
                            // {
                            //     //      pricetotal += auctionsForGroupOrder[aucid].count * auctionsForGroupOrder[aucid].price;
                            //     buyCountEntryList[ii - 1].setFull();
                            // }
                            // else
                            // {
                            //     //   Debug.LogError("changeSellPrice: buyCountList.Count:" + buyCountList.Count + " ii-1:" + (ii - 1));
                            //     //  numberslected += auctionsForGroupOrder[aucid].count;
                            // }
                        }
                        else if (auctionsForGroupOrder[aucid].count < number)
                        {
                            auctionsForGroupOrder[aucid].selected = 2;
                            // if (buyCountEntryList.Count >= ii)
                            // {
                            //     //     pricetotal += auctionsForGroupOrder[aucid].count * auctionsForGroupOrder[aucid].price;
                            //     buyCountEntryList[ii - 1].setFull();
                            // }
                            // else
                            // {
                            //     //    Debug.LogError("changeSellPrice: buyCountList.Count:" + buyCountList.Count + " ii-1:" + (ii - 1));
                            // }
                            sellItemsCount.value = auctionsForGroupOrder[aucid].count.ToString();
                           // Debug.Log("sellItemsCountSlider.highValue set to "+auctionsForGroupOrder[aucid].count);
                            sellItemsCountSlider.highValue = auctionsForGroupOrder[aucid].count;
                           // Debug.LogError("Set slider value to "+auctionsForGroupOrder[aucid].count);
                            sellItemsCountSlider.value = auctionsForGroupOrder[aucid].count;
                            if( sellItemsCountSlider.highValue==1)
                                sellItemsCountSlider.SetEnabled(false);
                            else
                                sellItemsCountSlider.SetEnabled(true);
                            totalCount.text = sellItemsCount.text + "/" + auctionsForGroupOrder[aucid].count;
                        }
                        totalCount.text = sellItemsCount.text + "/" + auctionsForGroupOrder[aucid].count;
                        sellInstant = true;
                    }
                    else
                    {
                        auctionsForGroupOrder[aucid].selected = 0;
                        if (!setPrice && auctionsForGroupOrder[aucid].price > currencyAmount)
                        {
                            setPrice = true;

                            currencyAmount = auctionsForGroupOrder[aucid].price;

                            sellCurrencyInputPanel.SetCurrencyAmounts(itemtosell.currencyType,auctionsForGroupOrder[aucid].price);
                         
                            changeSellPrice();
                        }
                        if (buyCountList.Count >= ii)
                        {
                            // buyCountList[ii - 1].Reset();
                        }
                    }
                    ii++;
                }
                
                buyCountListGrid.RefreshItems();
                
                ii = 1;
                int numberslected = 0;
                // if (!sellInstant)
                foreach (long aucid in auctionsForGroupSell.Keys)
                {
                    if (auctionsForGroupSell[aucid].price == currencyAmount)
                    {
                        if (auctionsForGroupSell[aucid].count > number - numberslected)
                        {
                            auctionsForGroupSell[aucid].selected = 1;
                            // if (sellCountEntryList.Count >= ii)
                            // {
                            //     sellCountEntryList[ii - 1].setPartial();
                            // }
                            numberslected += auctionsForGroupSell[aucid].count;
                        }
                        else if (auctionsForGroupSell[aucid].count <= number - numberslected)
                        {
                            auctionsForGroupSell[aucid].selected = 2;
                            // if (sellCountEntryList.Count >= ii)
                            // {
                            //     sellCountEntryList[ii - 1].setFull();
                            // }
                            numberslected += auctionsForGroupSell[aucid].count;
                        }
                        else if (number - numberslected <= 0)
                        {
                            auctionsForGroupSell[aucid].selected = 0;
                            // if (sellCountEntryList.Count >= ii)
                            // {
                            //     sellCountEntryList[ii - 1].Reset();
                            // }
                        }
                    }
                    else
                    {
                        auctionsForGroupSell[aucid].selected = 0;
                        // if (sellCountEntryList.Count >= ii)
                        // {
                        //     sellCountEntryList[ii - 1].Reset();
                        // }
                    }
                    ii++;
                }

                buyCountListGrid.RefreshItems();
                
                
                if (!sellInstant)
                {
                    int value = Inventory.Instance.GetCountOfItem(itemtosell.templateId);
                    if (value > itemtosell.StackLimit)
                        value = itemtosell.StackLimit;
                    if (value < int.Parse(sellItemsCount.value))
                        sellItemsCount.value = itemtosell.StackLimit.ToString();
                   // Debug.Log("sellItemsCountSlider.highValue set to "+value);
                    sellItemsCountSlider.highValue = value;
                    totalCount.text = sellItemsCount.text + "/" + value;
                }

                if (setPrice)
                    changeSellPrice();
            }
            //    Debug.LogError("Total ptzrd auction list" + pricetotal);

            if (auctionlist)
            {
                // bool isin = false;
                int ii = 1;
                int numberslected = 0;
                bool setPrice = false;
                if(auctionsForGroupSell!=null && auctionsForGroupSell.Count>0)
                foreach (long aucid in auctionsForGroupSell.Keys)
                {
                    if (auctionsForGroupSell[aucid].price <= currencyAmount)
                    {
                        if (auctionsForGroupSell[aucid].count > number - numberslected && number - numberslected > 0)
                        {
                            auctionsForGroupSell[aucid].selected = 1;
                            if (sellCountEntryList.Count >= ii)
                            {
                                // sellCountEntryList[ii - 1].setPartial();
                              //     Debug.LogError("changeSellPrice: auctionsForGroupSell.Count:" + auctionsForGroupSell[aucid].count + " number:" + number+ " numberslected:"+ numberslected+" price:"+ auctionsForGroupSell[aucid].price);
                              //        Debug.LogError("Total " + pricetotal+" add "+ (( (number - numberslected)) * auctionsForGroupSell[aucid].price));
                                pricetotal += ((number - numberslected)) * auctionsForGroupSell[aucid].price;
                            }
                            else
                            {
                               //  Debug.LogError("changeSellPrice: buyCountList.Count:" + sellCountList.Count + " ii-1:" + (ii - 1));
                            }
                            numberslected += auctionsForGroupSell[aucid].count;
                        }
                        else if (auctionsForGroupSell[aucid].count <= number - numberslected)
                        {
                            auctionsForGroupSell[aucid].selected = 2;
                            if (sellCountEntryList.Count >= ii)
                            {
                                // sellCountEntryList[ii - 1].setFull();
                               //         Debug.LogError("changeSellPrice: auctionsForGroupSell.Count:" + auctionsForGroupSell[aucid].count + " number:" + number + " numberslected:" + numberslected + " price:" + auctionsForGroupSell[aucid].price);
                               //         Debug.LogError("Total " + pricetotal+" add:"+((auctionsForGroupSell[aucid].count) * auctionsForGroupSell[aucid].price));
                                pricetotal += (auctionsForGroupSell[aucid].count) * auctionsForGroupSell[aucid].price;
                            }
                            else
                            {
                                //   Debug.LogError("changeSellPrice: buyCountList.Count:" + sellCountList.Count + " ii-1:" + (ii - 1));
                            }
                            numberslected += auctionsForGroupSell[aucid].count;
                        }
                        else if (number - numberslected <= 0)
                        {
                            auctionsForGroupSell[aucid].selected = 0;

                            if (sellCountEntryList.Count >= ii)
                            {
                                // sellCountEntryList[ii - 1].Reset();
                                //  pricetotal += (auctionsForGroupOrder[aucid].count - number) * auctionsForGroupOrder[aucid].price;
                
                            }
                            else
                            {
                                //       Debug.LogError("changeSellPrice: buyCountList.Count:" + sellCountList.Count + " ii-1:" + (ii - 1));
                            }
                        }
                        buyInstant = true;
                
                      //  Debug.LogError("changeSellPrice: currencyAmount:" + currencyAmount + " number:" + number+ " aucid:"+ aucid+" price:"+ auctionsForGroupSell[aucid].price+" count: "+ auctionsForGroupSell[aucid].count+ " numberslected:"+ numberslected);
                
                    }
                    else
                    {
                         //   Debug.LogError("changeSellPrice: currencyAmount:" + currencyAmount + " number:" + number + " aucid:" + aucid + " price:" + auctionsForGroupSell[aucid].price + " count: " + auctionsForGroupSell[aucid].count + " numberslected:" + numberslected+ " setPrice:"+ setPrice);
                         auctionsForGroupSell[aucid].selected = 0;
                        if (!setPrice && buyInstant && number - numberslected > 0)
                        {
                            setPrice = true;
                           
                          
                                sellCurrencyInputPanel.SetCurrencyAmounts(itemtosell.currencyType,auctionsForGroupSell[aucid].price);
                                
                                
                        }
                     
                        if (sellCountEntryList.Count >= ii)
                        {
                            // sellCountEntryList[ii - 1].Reset();
                        }
                        else
                        {
                            //       Debug.LogError("changeSellPrice: buyCountList.Count:" + sellCountList.Count + " ii-1:" + (ii - 1));
                        }
                    }
                    ii++;
                }
                sellCountListGrid.RefreshItems();
                ii = 1;
                //     if (!buyInstant)
                if (auctionsForGroupOrder != null && auctionsForGroupOrder.Count > 0)
                    foreach (long aucid in auctionsForGroupOrder.Keys)
                {
                    if (auctionsForGroupOrder[aucid].price == currencyAmount)
                    {
                        if (auctionsForGroupOrder[aucid].count > number - numberslected)
                        {
                            auctionsForGroupOrder[aucid].selected = 1;
                            if (buyCountEntryList.Count >= ii)
                            {
                                // buyCountEntryList[ii - 1].setPartial();
                            }
                            else
                            {
                                //    Debug.LogError("changeSellPrice: buyCountList.Count:" + buyCountList.Count + " ii-1:" + (ii - 1));
                            }
                            numberslected += auctionsForGroupOrder[aucid].count;
                        }
                        else if (auctionsForGroupOrder[aucid].count <= number - numberslected)
                        {
                            auctionsForGroupOrder[aucid].selected = 2;
                            if (buyCountEntryList.Count >= ii)
                            {
                                // buyCountEntryList[ii - 1].setFull();
                            }
                            else
                            {
                                //   Debug.LogError("changeSellPrice: buyCountList.Count:" + buyCountList.Count + " ii-1:" + (ii - 1));
                            }
                            numberslected += auctionsForGroupOrder[aucid].count;
                        }
                        else if (number - numberslected <= 0)
                        {
                            if (buyCountEntryList.Count >= ii)
                            {
                                auctionsForGroupOrder[aucid].selected = 0;
                                // buyCountEntryList[ii - 1].Reset();
                            }
                            else
                            {
                                //       Debug.LogError("changeSellPrice: buyCountList.Count:" + buyCountList.Count + " ii-1:" + (ii - 1));
                            }
                        }
                    }
                    else
                    {
                        if (buyCountEntryList.Count >= ii)
                        {
                            auctionsForGroupOrder[aucid].selected = 0;
                            // buyCountEntryList[ii - 1].Reset();
                        }
                        else
                        {
                            //       Debug.LogError("changeSellPrice: buyCountList.Count:" + buyCountList.Count + " ii-1:" + (ii - 1));
                        }
                    }
                    ii++;
                }
            }
            buyCountListGrid.RefreshItems();

            //   currencyAmount *= int.Parse(sellItemsCount.text);
            // Debug.LogError("Total " + pricetotal);
            if (auctionlist)
            {
                if (pricetotal > 0)
                {
                    currencyAmount = pricetotal;
                }
                else
                {
                    currencyAmount *= int.Parse(sellItemsCount.text);
                }

                //    if (auctionlist)
                //     {
                if (selectedAuction.countSell > 0 && totalAuctionCountItems < itemtosell.StackLimit && buyInstant)
                {
                    //Debug.Log("sellItemsCountSlider.highValue set to "+totalAuctionCountItems);
                    sellItemsCountSlider.highValue = totalAuctionCountItems;
                    totalCount.text = sellItemsCount.text + "/" + totalAuctionCountItems;
                }
                else
                {
                   // Debug.Log("sellItemsCountSlider.highValue set to "+itemtosell.StackLimit);
                    sellItemsCountSlider.highValue = itemtosell.StackLimit;
                    totalCount.text = sellItemsCount.text + "/" + itemtosell.StackLimit;
                }
                //   }

            }
            else
            {
                currencyAmount *= int.Parse(sellItemsCount.text);
            }
            long costSell = 0;
            if (showSell)
            {
                if (commissionText!=null)
                    commissionText.ShowVisualElement();

              /*  int basecost = 0;
                int basecostCur = AtavismAuction.Instance.GetCurrencyType;
                Inventory.Instance.ConvertCurrencyToBaseCurrency(itemtosell.cost, itemtosell.currencyType,out basecost,out basecostCur);
                */
                costSell = AtavismAuction.Instance.CalcCost(currencyAmount);
             //   Debug.LogError("Auction  ca=" + currencyAmount + " cost=" + costSell);
                List<CurrencyDisplay> currencyDisplayList1 = Inventory.Instance.GenerateCurrencyListFromAmount(AtavismAuction.Instance.GetCurrencyType/* itemtosell.currencyType*/, costSell);
                commissionSumaryCurrencies.SetData(AtavismAuction.Instance.GetCurrencyType,costSell);
               
            }
            else
            {
                if (commissionText!=null)
                    commissionText.HideVisualElement();
                // for (int i = 0; i < commissionSumaryCurrencies.Count; i++)
                // {
                //     commissionSumaryCurrencies[i].gameObject.SetActive(false);
                // }
            }
            //  Debug.LogError(" showSell:" + showSell + " sellInstant:" + sellInstant + " auctionlist:" + auctionlist + " buyInstant:" + buyInstant+ " currencyAmount:"+ currencyAmount);
            List<CurrencyDisplay> currencyDisplayList = Inventory.Instance.GenerateCurrencyListFromAmount(currencyType, currencyAmount);
            sellSumaryCurrencies.SetData(currencyType,currencyAmount);
           


            if (showSell)
            {
                if (sellInstant)
                {
#if AT_I2LOC_PRESET
           if (confirmButton != null) confirmButton.text = I2.Loc.LocalizationManager.GetTranslation(sellButtonText).ToUpper();
#else
                    if (confirmButton != null)
                        confirmButton.text = sellButtonText.ToUpper();
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
           if (confirmButton != null) confirmButton.text = I2.Loc.LocalizationManager.GetTranslation(listSellButtonText).ToUpper();
#else
                    if (confirmButton != null)
                        confirmButton.text = listSellButtonText.ToUpper();
#endif
                }
            }
            if (auctionlist)
            {
                if (buyInstant)
                {
#if AT_I2LOC_PRESET
           if (confirmButton != null) confirmButton.text = I2.Loc.LocalizationManager.GetTranslation(buyButtonText).ToUpper();
#else
                    if (confirmButton != null)
                        confirmButton.text = buyButtonText.ToUpper();
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
           if (confirmButton != null) confirmButton.text = I2.Loc.LocalizationManager.GetTranslation(orderButtonText).ToUpper();
#else
                    if (confirmButton != null)
                        confirmButton.text = orderButtonText.ToUpper();
#endif
                }
            }


            if( sellItemsCountSlider.highValue==1)
                sellItemsCountSlider.SetEnabled(false);
            else
                sellItemsCountSlider.SetEnabled(true);
        }

        AtavismInventoryItem itemtosell;
        Auction selectedAuction;
        string auctionGroupId = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="auction"></param>
        private void SelectItem(IEnumerable<object> enumerable)
        {
            if (enumerable.Count() == 0)
                return;

            Auction auction = (Auction)enumerable.First();
            //  Debug.LogError("selectItem id" + auction.item.ItemId);
            itemtosell = auction.item;
            selectedAuction = auction;
            if (auction.groupId.Length > 0)
                auctionGroupId = auction.groupId;
#if AT_I2LOC_PRESET
           if (confirmButton != null) confirmButton.text = I2.Loc.LocalizationManager.GetTranslation(sellButtonText).ToUpper();
#else
            if (confirmButton != null)
                confirmButton.text = sellButtonText.ToUpper();
#endif
            sellItemsCountSlider.lowValue = 1;
            if (subWindowSellPanel != null)
            {
              
             ShowSellWindow();
                
                sellCurrencyInputPanel.SetCurrencies(AtavismAuction.Instance.GetCurrencyType);
                // sellPanel.transform.position = new Vector3((Screen.width / 2), (Screen.height / 2), 0);
                // AtavismUIUtility.BringToFront(sellPanel);

            }

            
           
            if (sellItemName != null)
            {
                if (itemtosell.enchantLeval > 0)
#if AT_I2LOC_PRESET
                 sellItemName.text = "+" + itemtosell.enchantLeval + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + itemtosell.BaseName);
#else
                    sellItemName.text = "+" + itemtosell.enchantLeval + " " + itemtosell.BaseName;
#endif
                else
#if AT_I2LOC_PRESET
                 sellItemName.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + itemtosell.BaseName);
#else
                    sellItemName.text = itemtosell.BaseName;
#endif
            }
            int value = Inventory.Instance.GetCountOfItem(itemtosell.templateId);
            //int stack = itemtosell.StackLimit;
            //  Debug.LogError(" Item Sell val:" + value + " ss:" + itemtosell.StackLimit + " st:" + itemtosell.Count + " cost:" + itemtosell.cost + " cur:" + itemtosell.currencyType);
            if (value > itemtosell.StackLimit)
                value = itemtosell.StackLimit;
            // if (value<)


            sellItemsCount.SetValueWithoutNotify(value.ToString());
           // Debug.Log("sellItemsCountSlider.highValue set to "+value);
            sellItemsCountSlider.highValue = value;
            sellItemsCountSlider.SetValueWithoutNotify(value);
            totalCount.text = value + "/" + value;
            sellItemQuality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(itemtosell.quality);
            sellItemIcon.style.backgroundImage = itemtosell.Icon.texture;
            sellItemCount.text = "";    
            // sellslot.SetItemData(itemtosell, null);
            if (commissionText!=null)
                commissionText.ShowVisualElement();

            long basecost = 0;
            int basecostCur = AtavismAuction.Instance.GetCurrencyType;
            Inventory.Instance.ConvertCurrencyToBaseCurrency(itemtosell.currencyType, itemtosell.cost, out basecostCur ,out basecost);

            long costSell = AtavismAuction.Instance.CalcCost(basecost * value);
            //   Debug.LogError("CalcCost: c:"+ itemtosell.cost+" count:"+ value+" sum:"+ (itemtosell.cost * value)+" costSell:"+ costSell);


            commissionSumaryCurrencies.SetData(AtavismAuction.Instance.GetCurrencyType,costSell);
            sellCurrencyInputPanel.SetCurrencyAmounts(AtavismAuction.Instance.GetCurrencyType, basecost);
          

            if (auctionlist)
                AtavismAuction.Instance.GetAuctionsForGroup(auctionGroupId, 0L);
            else
                AtavismAuction.Instance.GetAuctionsForGroup("", itemtosell.ItemId.ToLong());
            changeSellPrice();
        }

        private void onGeometryShowSellPanel(GeometryChangedEvent evt)
        {
          //  Debug.LogError("onGeometryShowSellPanel");
            float width = subWindowSellPanel.resolvedStyle.width;
            float height = subWindowSellPanel.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
           
            subWindowSellPanel.style.left = canvasWidth * 0.5f - (width * 0.5f);
            subWindowSellPanel.style.top = canvasHeight * 0.5f - (height * 0.5f);
            subWindowSellPanel.UnregisterCallback<GeometryChangedEvent>(onGeometryShowSellPanel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auction"></param>
        private void SelectAuction(IEnumerable<object> enumerable)
        {
            if (enumerable.Count() == 0)
                return;
            Auction auction = (Auction)enumerable.First();
            HideSellWindow();
            auctionGroupId = auction.groupId;
            selectedAuction = auction;
            //  Debug.LogError("selectItem id" + auction.item.ItemId);
            itemtosell = auction.item;
#if AT_I2LOC_PRESET
           if (confirmButton != null) confirmButton.text = I2.Loc.LocalizationManager.GetTranslation(buyButtonText).ToUpper();
#else
            if (confirmButton != null)
                confirmButton.text = buyButtonText.ToUpper();
#endif
            ShowSellWindow();
            sellCurrencyInputPanel.SetCurrencies(AtavismAuction.Instance.GetCurrencyType);
            if (sellItemName != null)
            {
                if (itemtosell.enchantLeval > 0)
#if AT_I2LOC_PRESET
                sellItemName.text = "+" + itemtosell.enchantLeval + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + itemtosell.BaseName);
#else
                    sellItemName.text = "+" + itemtosell.enchantLeval + " " + itemtosell.BaseName;
#endif
                else
#if AT_I2LOC_PRESET
                 sellItemName.text = I2.Loc.LocalizationManager.GetTranslation("Items/" + itemtosell.BaseName);
#else
                    sellItemName.text = itemtosell.BaseName;
#endif
            }
            int value = 1;
            //int stack = itemtosell.StackLimit;
            //   Debug.LogError(" Item Sell val:" + itemtosell.Count + " ss:" + itemtosell.StackLimit + " st:" + itemtosell.Count+ " cost:"+itemtosell.cost+" cur:"+ itemtosell.currencyType);
            if (value > itemtosell.StackLimit)
                value = itemtosell.StackLimit;
            // if (value<)

            sellCurrencyInputPanel.SetCurrencyAmounts(itemtosell.currencyType, itemtosell.cost);

        
            sellItemsCount.value = value.ToString();
            // Debug.LogWarning("selectedAuction.countSell:" + selectedAuction.countSell + " itemtosell.StackLimit:" + itemtosell.StackLimit);
            if (selectedAuction.countSell > 0 && totalAuctionCountItems < itemtosell.StackLimit)
            {
               // Debug.Log("sellItemsCountSlider.highValue set to "+totalAuctionCountItems);
                sellItemsCountSlider.highValue = totalAuctionCountItems;
                totalCount.text = value + "/" + totalAuctionCountItems;
            }
            else
            {
               // Debug.Log("sellItemsCountSlider.highValue set to "+itemtosell.StackLimit);
                sellItemsCountSlider.highValue = itemtosell.StackLimit;
                totalCount.text = value + "/" + itemtosell.StackLimit;
            }
            sellItemsCountSlider.SetValueWithoutNotify(value);
            sellItemQuality.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(itemtosell.quality);
            sellItemIcon.style.backgroundImage = itemtosell.Icon.texture;
            sellItemCount.text = "";    
            // sellslot.SetItemData(itemtosell, null);

            changeSellPrice();

            if (auctionlist)
                AtavismAuction.Instance.GetAuctionsForGroup(auctionGroupId, 0L);
            else
                AtavismAuction.Instance.GetAuctionsForGroup("", itemtosell.ItemId.ToLong());

        }
        /// <summary>
        /// 
        /// </summary>
        public void changeQuantitySlider(ChangeEvent<int> evt)
        {
           // Debug.Log("changeQuantitySlider "+evt.newValue+" "+evt.previousValue);
            //  int value = Inventory.Instance.GetCountOfItem(itemtosell.templateId);
            //  if (value > itemtosell.StackLimit) value = itemtosell.StackLimit;
          //    Debug.LogWarning("selectedAuction.countSell:" + selectedAuction.countSell + " itemtosell.StackLimit:" + itemtosell.StackLimit+ " auctionlist:"+ auctionlist+ " buyInstant:"+ buyInstant+ " showSell:"+ showSell+ " sellInstant:"+ sellInstant);

            if (auctionlist)
            {
                if (buyInstant)
                {


                    if (selectedAuction.countSell > 0 && totalAuctionCountItems < itemtosell.StackLimit)
                    {
                       // Debug.Log("sellItemsCountSlider.highValue set to "+totalAuctionCountItems);
                        sellItemsCountSlider.highValue = totalAuctionCountItems;
                        totalCount.text = sellItemsCountSlider.value + "/" + totalAuctionCountItems;
                    }
                    else
                    {
                       // Debug.Log("sellItemsCountSlider.highValue set to "+itemtosell.StackLimit);
                        sellItemsCountSlider.highValue = itemtosell.StackLimit;
                        totalCount.text = sellItemsCountSlider.value + "/" + itemtosell.StackLimit;
                    }
                    //  int value = Inventory.Instance.GetCountOfItem(itemtosell.templateId);
                    //    if (value > itemtosell.StackLimit) value = itemtosell.StackLimit;
                    // asdasd
                    //   totalCount.text = sellItemsCountSlider.value + "/" + itemtosell.StackLimit;

                }
            }
            if (showSell)
            {
                if (sellInstant)
                {
                    //   totalCount.text = sellItemsCountSlider.value + "/" + value;
                }
                else
                {
                    int value = Inventory.Instance.GetCountOfItem(itemtosell.templateId);
                    if (value > itemtosell.StackLimit)
                        value = itemtosell.StackLimit;
                    totalCount.text = sellItemsCountSlider.value + "/" + value;
                }
            }
            sellItemsCount.value = sellItemsCountSlider.value.ToString();
            changeSellPrice();
        }
        /// <summary>
        /// 
        /// </summary>
        public void changeQuantity()
        {

            if (String.IsNullOrEmpty(sellItemsCount.value) || sellItemsCount.value == "0" || sellItemsCount.value == "" || sellItemsCount.value.Length == 0)
                sellItemsCount.SetValueWithoutNotify("1");
            
            if(sellItemsCountSlider.lowValue==0)
                sellItemsCountSlider.lowValue = 1;
            if (auctionlist)
            {
                totalCount.text = sellItemsCount.value + "/" + itemtosell.StackLimit;
                if (selectedAuction.countSell > 0 && totalAuctionCountItems < itemtosell.StackLimit)
                {
                   Debug.Log("sellItemsCountSlider.highValue set to "+selectedAuction.countSell);
                 //   sellItemsCountSlider.highValue = selectedAuction.countSell;
                  if(int.Parse(sellItemsCount.value)>totalAuctionCountItems)
                     sellItemsCount.SetValueWithoutNotify(totalAuctionCountItems.ToString());
                    totalCount.text = sellItemsCount.value + "/" + totalAuctionCountItems;
                }
                else
                {
                  //  Debug.Log("sellItemsCountSlider.highValue set to "+itemtosell.StackLimit);
                    sellItemsCountSlider.highValue = itemtosell.StackLimit;
                    if(int.Parse(sellItemsCount.value)>itemtosell.StackLimit)
                        sellItemsCount.SetValueWithoutNotify(itemtosell.StackLimit.ToString());
                    totalCount.text = sellItemsCount.value + "/" + itemtosell.StackLimit;
                }   //  Dictionary<int, AuctionCountPrice> auctionsForGroupSell = AtavismAuction.Instance.AuctionsForGroupSell;

            }
            if (showSell)
            {
                if (sellInstant)
                {
                    //   totalCount.text = sellItemsCountSlider.value + "/" + value;
                }
                else
                {
                    int value = Inventory.Instance.GetCountOfItem(itemtosell.templateId);
                    if (value > itemtosell.StackLimit)
                        value = itemtosell.StackLimit;
                    if (value < int.Parse(sellItemsCount.value))
                        sellItemsCount.value = itemtosell.StackLimit.ToString();
                        
                    if(int.Parse(sellItemsCount.value) > value)
                        sellItemsCount.SetValueWithoutNotify(value.ToString());
                    
                    totalCount.text = sellItemsCount.value + "/" + value;
                }
            }
            sellItemsCountSlider.SetValueWithoutNotify(int.Parse(sellItemsCount.value));
            changeSellPrice();
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="value"></param>
        /// <param name="nested"></param>
        void Search(string category, string value, string nested)
        {
            searchCatType = category;
            searchCat = value;
            searchCatDic.Clear();
            string[] nes = nested.Split(';');
            //searchCatDic
            foreach (string s in nes)
            {
                if (s.Length > 0)
                {
                    string[] param = s.Split('|');
                    // int id = int.Parse(s);
                    // var data = menuGrid.GetItemDataForIndex<MenuTreeEntry>(id);
                    // Debug.LogError("UI Auction Search name="+data.name+" value=" + data.value + " type=" + data.type+" id="+data.id);
                    // searchCatDic.Add(data.type.ToString(), data.value);
                    searchCatDic.Add(param[0], param[1]);
                    // foreach (UGUIMenuSlot ms in Atavism.menuTree)
                    // {
                    //     if (id.Equals(ms.id))
                    //     {
                    //         searchCatDic.Add(ms.category, ms.value);
                    //         break;
                    //     }
                    // }
                }
            }
            if(searchCat.Length>0)
                searchCatDic.Add(searchCatType,searchCat);
            List<object> list = new List<object>();
            foreach (int elm in qualitylevelsList)
                list.Add(elm);
            AtavismAuction.Instance.SearchAuction(sortCount, sortName, sortPrice, qualitylevels, searchRace, searchClass, minLevel, maxLevel, searchCatType, searchCat, searchText, list, sortAsc, searchCatDic);

        }
        // Use this for initialization
     
        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
           
            base.OnDisable();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            
            if (isDraggable)
            {
                if (isSubDragging)
                {
                    if (Input.GetMouseButtonUp((int)DraggingMouseButton))
                    {
                        subDraggingEnd();
                    }
                    else
                    {
                        subDragging();
                    }
                }
            }
            
            if (showUseFade)
            {
                if (subWindowSellPanel != null)
                {
                    var op = subWindowSellPanel.resolvedStyle.opacity;
                    //  Debug.LogError("Update " + op);
                    if (showSubWindowSellPanel && op < 1f)
                    {
                        subWindowSellPanel.FadeInVisualElement();
                        
                    }
                    else if (!showSubWindowSellPanel && op > 0f)
                    {
                        subWindowSellPanel.FadeOutVisualElement();
                    }
                }
            }
            
            
        }
    }
}