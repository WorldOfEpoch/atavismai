using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismInventory : UIAtavismWindowBase, IPointerDownHandler
    {
        [AtavismSeparator("Base Screen")] public string uiScreenstring = "Screen";
        private VisualElement m_Root;
        private VisualElement m_SlotContainer;
        private VisualElement m_BagContainer;
        private VisualElement m_BagGold;
        private VisualElement m_BagSilver;
        private VisualElement m_BagCopper;

        private static VisualElement m_GhostIcon;

        public List<UIAtavismInventorySlot> bagButtons;
        public List<UIAtavismBag> bagPanels;
        [SerializeField] List<UIAtavismInventorySlot> inventorySlots = new List<UIAtavismInventorySlot>();
        [SerializeField] List<UIAtavismBagSlot> bagSlots = new List<UIAtavismBagSlot>();
        private UIAtavismCurrencyDisplay currencyDisplays;
        Dictionary<int, Bag> bags;
        private static bool m_IsDragging;
        private static UIAtavismInventorySlot m_OriginalSlot;

        [AtavismSeparator("UI")] [SerializeField]
        protected VisualTreeAsset uiInventorySlotUXML;

        [SerializeField] protected VisualTreeAsset uiBagSlotUXML;
        [SerializeField] protected VisualTreeAsset uiActivatableSlotUXML;

        private Vector2 targetStartPosition; // { get; set; }

        private Vector3 pointerStartPosition; // { get; set; }

        private Vector2 targetEndtPosition; // { get; set; }

        private Vector3 pointerEndPosition; // { get; set; }

        //private bool enabled;// { get; set; }

        // Start is called before the first frame update
        new void Start()
        {
            base.Start();
        }

        void OnDestroy()
        {

        }

        new void OnEnable()
        {
            base.OnEnable();
            registerUI();
            bags = Inventory.Instance.Bags;
            ProcessBagInventoryChange(Inventory.Instance.Bags);
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CURRENCY_ICON_UPDATE", this);
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CURRENCY_ICON_UPDATE", this);
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            //Store the root from the UI Document component
            m_Root = GetComponent<UIDocument>().rootVisualElement;
            // Add the Draggable behavior
            //m_Root.AddManipulator(new Draggable());
            //Search the root for the SlotContainer Visual Element
            m_SlotContainer = m_Root.Q<VisualElement>("bag-items");
            m_BagContainer = m_Root.Q<VisualElement>("bags");
            VisualElement currency = m_Root.Q<VisualElement>("currency");
            currencyDisplays = new UIAtavismCurrencyDisplay();
            currencyDisplays.SetVisualElement(currency);
            currencyDisplays.ReverseOrder = true;
            isVisible = false;
            return true;
        }

        // Update is called once per frame
        protected override void Update()
        {
            // Debug.LogError("UIAtavismInventory::Update");
            base.Update();
            if ((Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().inventory.key) ||
                 Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().inventory.altKey)) &&
                !ClientAPI.UIHasFocus())
            {
                if (!isVisible)
                {
                    Show();
                    isVisible = true;
                }
                else
                {
                    Hide();
                    isVisible = false;

                }
            }

            foreach (var slot in bagSlots)
            {
                if (slot != null)
                    slot.Update();
            }

            foreach (var slot in inventorySlots)
            {
                if (slot != null)
                    slot.Update();
            }
        }

        protected void RegisterCallbacksFromInventoryTarget(VisualElement thisobject)
        {
            // thisobject.RegisterCallback<PointerDownEvent>(OnInventoryPointerDown);

        }

        protected void UnregisterCallbacksFromInventoryTarget(VisualElement thisobject)
        {

        }

   
        public void OnEvent(AtavismEventData eData)
        {
            base.OnEvent(eData);
            if (eData.eventType == "INVENTORY_UPDATE")
            {
                // Update 
                Dictionary<int, Bag>
                    bags = Inventory.Instance.Bags; //ClientAPI.ScriptObject.GetComponent<Inventory>().Bags;
                //   Debug.LogError("INVENTORY_UPDATE",gameObject);
                ProcessBagInventoryChange(bags);
            }

            if (eData.eventType == "CURRENCY_UPDATE" || eData.eventType == "CURRENCY_ICON_UPDATE")
            {
                UpdateCurrencies();
            }
        }

        void UpdateCurrencies()
        {
            currencyDisplays.MainCurrency();
            currencyDisplays.Show();
        }

        public static void StartDrag(Vector2 position, UIAtavismInventorySlot originalSlot)
        {
         //   Debug.LogError("StartDrag");
            //Set tracking variables
            m_IsDragging = true;
            m_OriginalSlot = originalSlot;
            //Set the new position
            m_GhostIcon.style.top = position.y - m_GhostIcon.layout.height / 2;
            m_GhostIcon.style.left = position.x - m_GhostIcon.layout.width / 2;
            //Set the image
            // m_GhostIcon.style.backgroundImage = Inventory.Instance.GetInventoryItem(originalSlot.).icon.texture;
            //Flip the visibility on
            m_GhostIcon.style.visibility = Visibility.Visible;
        }

        void ProcessBagInventoryChange(Dictionary<int, Bag> bags)
        {
            int numBags = 0;
            int numBagSlots = 0;
            int numBagHaveSlots = 0;
            GameObject objToSpawn = null;
            for (int iii = 0; iii < bags.Count; iii++)
            {
                numBagSlots += bags[iii].numSlots;
                numBagHaveSlots++;
            }

            var tmpbagSlots = new List<UIAtavismBagSlot>(bagSlots);

            foreach (var item in tmpbagSlots)
            {
                if (item != null)
                {
                    bagSlots.Remove(item);
                    item.Clear();
                }
            }

            var tmpinventorySlots = new List<UIAtavismInventorySlot>(inventorySlots);

            foreach (var item in tmpinventorySlots)
            {
                if (item != null)
                {
                    inventorySlots.Remove(item);
                    item.Clear();
                }
            }

            if (numBags != numBagHaveSlots || bagSlots.Count != numBagHaveSlots)
            {
                numBags = numBagHaveSlots;
                m_BagContainer.Clear();
                m_SlotContainer.Clear();

                for (int i = 0; i < bags.Count; i++)
                {

                    UIAtavismBagSlot bag = new UIAtavismBagSlot();
                    //UIAtavismActivatable itemUIAtavismActivatable = new UIAtavismActivatable(bag.thisBagSlotItem);
                    bag.slotNum = i;
                    bag.Startup();
                    bag.name = "bag" + i;
                    bag.allowOverwrite = i > 0;
                    bagSlots.Add(bag);
                    m_BagContainer.Add(bag.thisBagSlotItem);

                }

            }

            int ii = 0;
            for (int i = 0; i < bags.Count; i++)
            {
                for (int k = 0; k < bags[i].numSlots; k++)
                {
                    if (ii <= inventorySlots.Count)
                    {
                        UIAtavismInventorySlot item = new UIAtavismInventorySlot();
                        //UIAtavismActivatable itemUIAtavismActivatable = new UIAtavismActivatable(item.thisInventorySlotItem);

                        item.slotNum = k;
                        item.bagNum = i;
                        item.slotName = "item" + k;

                        inventorySlots.Add(item);
                        m_SlotContainer.Add(item.thisInventorySlotItem);
                        RegisterCallbacksFromInventoryTarget(item);

                    }

                    ii++;
                }
            }

            //  }
            int it = 0;
            for (int i = 0; i < bags.Count; i++)
            {
                for (int k = 0; k < bags[i].numSlots; k++)
                {
                    if (bags[i].items.ContainsKey(k))
                    {
                        inventorySlots[it + k].UpdateInventoryItemData(bags[i].items[k], this);

                    }
                    else
                    {
                        inventorySlots[it + k].UpdateInventoryItemData(null, this);
                    }
                }

                it += bags[i].numSlots;
                if (bags.ContainsKey(i) && bags[i].isActive)
                {
                    bagSlots[i].UpdateBagData(bags[i], null /*bagPanels[i]*/, this);

                }
                else
                {
                    bagSlots[i].UpdateBagData(null, null /*bagPanels[i]*/, null);
                }
            }
        }


        private void OnPointerMove(PointerMoveEvent evt)
        {
            //Only take action if the player is dragging an item around the screen
            if (!m_IsDragging)
            {
                return;
            }

            //Set the new position
            m_GhostIcon.style.top = evt.position.y - m_GhostIcon.layout.height / 2;
            m_GhostIcon.style.left = evt.position.x - m_GhostIcon.layout.width / 2;
        }

        private void OnActivatablePointerDown(PointerDownEvent evt)
        {
            if (m_IsDragging)
            {
                return;
            }

            // Focus the window
            AtavismUIUtility.BringToFront(this.gameObject);

        }

        private void OnActivatablePointerUp(PointerUpEvent evt)
        {
            if (!m_IsDragging)
            {
                return;
            }

            //Check to see if they are dropping the ghost icon over any inventory slots.
            IEnumerable<UIAtavismInventorySlot> slots =
                inventorySlots.Where(x => x.thisInventorySlotItem.worldBound.Overlaps(m_GhostIcon.worldBound));
            //Found at least one
            if (slots.Count() != 0)
            {
                UIAtavismInventorySlot closestSlot = slots.OrderBy(x =>
                        Vector2.Distance(x.thisInventorySlotItem.worldBound.position, m_GhostIcon.worldBound.position))
                    .First();

                //Set the new inventory slot with the data
                //closestSlot.HoldItem(Inventory.Instance.GetInventoryItem(m_OriginalSlot));

                //Clear the original slot
                m_OriginalSlot.DropItem();
            }
            //Didn't find any (dragged off the window)
            else
            {
                m_OriginalSlot.m_itemIcon.style.backgroundImage =
                    Inventory.Instance.GetInventoryItem(m_OriginalSlot.ItemGuid).Icon.texture;
            }

            //Clear dragging related visuals and data
            m_IsDragging = false;
            m_OriginalSlot = null;
            m_GhostIcon.style.visibility = Visibility.Hidden;
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            // Focus the window
            AtavismUIUtility.BringToFront(this.gameObject);
        }

        public override void Show()
        {
            base.Show();
            UpdateCurrencies();
        }
    }
}