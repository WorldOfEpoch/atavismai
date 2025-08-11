using UnityEngine;
using System.Collections.Generic;

namespace Atavism.UI
{

    /*
public enum BagOverflowExpandDirection
{
    Left,
    Right
}*/

    public class UIAtavismBagBar : MonoBehaviour
    {

        public List<UIAtavismBagSlot> bagButtons;
        public List<UIAtavismBag> bagPanels;
       public BagOverflowExpandDirection overflowDirection = BagOverflowExpandDirection.Left;

        // Use this for initialization
        void Start()
        {
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
            Dictionary<int, Bag> bags = Inventory.Instance.Bags;
            ProcessBagInventoryChange(bags);
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
        }

        // Update is called once per frame
        void Update()
        {
            foreach (UIAtavismBag bag in bagPanels)
            {
                if (!bag.gameObject.activeSelf)
                    continue;
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "INVENTORY_UPDATE")
            {
                // Update 
                Dictionary<int, Bag> bags = Inventory.Instance.Bags;
                ProcessBagInventoryChange(bags);
            }
        }

        void ProcessBagInventoryChange(Dictionary<int, Bag> bags)
        {
            for (int i = 0; i < bagButtons.Count; i++)
            {
                if (bags.ContainsKey(i) && bags[i].isActive)
                {
                    //bagButtons[i].gameObject.SetActive(true);
                    // Set icon
                    bagButtons[i].UpdateBagData(bags[i], bagPanels[i], null);
                }
                else
                {
                    //bagButtons[i].gameObject.SetActive(false);
                    bagButtons[i].UpdateBagData(null, bagPanels[i],null);
                    bagPanels[i].gameObject.SetActive(false);
                }
            }
        }

    }
}