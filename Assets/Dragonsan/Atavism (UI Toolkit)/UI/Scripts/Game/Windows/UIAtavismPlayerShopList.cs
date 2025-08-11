using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class ShopListData : ScriptableObject
    {
        public string desc;
        public OID oid;
    }
    public class UIAtavismPlayerShopList : UIAtavismWindowBase
    {
        [SerializeField] VisualTreeAsset listElementTemplate;
        [SerializeField] List<UIAtavismShopListEntry> shops = new List<UIAtavismShopListEntry>();
        private List<ShopListData> shopsData = new List<ShopListData>();
        [SerializeField] bool autoShowHide = false;
        [SerializeField] VisualElement grid;
        // Start is called before the first frame update
        protected override void OnEnable()
        {
            base.OnEnable();
            // uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("ClaimPanelWindow");
            if (AtavismPlayerShop.Instance.ShopList.Count == 0)
            {
                Hide();
            }
            else
            {
                if (autoShowHide)
                {
                    Show();
                }
                UpdateDisplay();
            }
          //  Debug.LogError("UIAtavismClaimPanel OnEnable End");
            // Hide();
        }
        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("SHOP_LIST_UPDATE", this);
            // UpdateDisplay();
            // Hide();
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("SHOP_LIST_UPDATE", this);
        }
       
        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            // uiWindow = uiDocument.rootVisualElement.Query<VisualElement>("ClaimPanelWindow");

          
            grid = uiWindow.Query<VisualElement>("shop-list");
           
            // grid.makeItem = () =>
            // {
            //     // Instantiate a controller for the data
            //     UIAtavismShopListEntry newListEntryLogic = new UIAtavismShopListEntry();
            //     // Instantiate the UXML template for the entry
            //     var newListEntry = listElementTemplate.Instantiate();
            //     // Assign the controller script to the visual element
            //     newListEntry.userData = newListEntryLogic;
            //     // Initialize the controller script
            //     newListEntryLogic.SetVisualElement(newListEntry);
            //     shops.Add(newListEntryLogic);
            //     // Return the root of the instantiated visual tree
            //     return newListEntry;
            // };
            // grid.bindItem = (item, index) =>
            // {
            //     var entry = (item.userData as UIAtavismShopListEntry);
            //     ShopListData sld = shopsData[index]; 
            //     entry.SetData(sld.desc, sld.oid);
            // };
            //
            // grid.selectionChanged += SelectEntry;
            // grid.fixedItemHeight = 65;
            
            
            Hide();
         //   Debug.LogError("UIAtavismClaimPanel registerUI End");
            return true;
        }

        private void SelectEntry(IEnumerable<object> obj)
        {
            throw new System.NotImplementedException();
        }


        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "SHOP_LIST_UPDATE")
            {

                if (!showing && autoShowHide)
                    Show();

                UpdateDisplay();
            } else if (eData.eventType == "CLOSE_SHOP")
            {
                Hide();
            }
        }

        void UpdateDisplay()
        {
            // shopsData.Clear();
            StopAllCoroutines();
            grid.Clear();
            foreach (OID oid in AtavismPlayerShop.Instance.ShopList.Keys)
            {
                // ShopListData sld = new ShopListData();
                // sld.oid = oid;
                // sld.desc = AtavismPlayerShop.Instance.ShopList[oid];
                // Instantiate a controller for the data
                UIAtavismShopListEntry newListEntryLogic = new UIAtavismShopListEntry();
                // Instantiate the UXML template for the entry
                var newListEntry = listElementTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                shops.Add(newListEntryLogic);
                newListEntryLogic.SetData(AtavismPlayerShop.Instance.ShopList[oid], oid);
                // Return the root of the instantiated visual tree
                grid.Add(newListEntry);
                // shopsData.Add(sld);
            }

          
            
            
            
            
            // grid.itemsSource = shopsData;
            // grid.Rebuild();
            // grid.selectedIndex = -1;
            // int i = 0;
            // foreach (OID oid in AtavismPlayerShop.Instance.ShopList.Keys)
            // {
            //     if (i >= shops.Count)
            //         shops.Add(Instantiate(prefab, transform));
            //     shops[i].gameObject.SetActive(true);
            //     shops[i].UpdateDisplay(AtavismPlayerShop.Instance.ShopList[oid], oid);
            //     i++;
            // }
            // for (int j = i; j < shops.Count; j++)
            //     shops[i].gameObject.SetActive(false);

            if (AtavismPlayerShop.Instance.ShopList.Count == 0 && autoShowHide)
            {
                Hide();
                return;
            }
        }

        public override void Show()
        {
            base.Show();
            // AtavismSettings.Instance.OpenWindow(this);
            // AtavismUIUtility.BringToFront(gameObject);
            UpdateDisplay();
            // GetComponent<CanvasGroup>().alpha = 1f;
            // GetComponent<CanvasGroup>().blocksRaycasts = true;
            // GetComponent<CanvasGroup>().interactable = true;
            showing = true;

        }

        public override void Hide()
        {
            base.Hide();
          //  AtavismSettings.Instance.CloseWindow(this);
            // GetComponent<CanvasGroup>().alpha = 0f;
            // GetComponent<CanvasGroup>().blocksRaycasts = false;
            // showing = false;

        }
        public void Toggle()
        {
            if (showing)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}