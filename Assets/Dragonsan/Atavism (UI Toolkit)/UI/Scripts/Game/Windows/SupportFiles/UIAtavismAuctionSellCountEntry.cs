using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{


    public class UIAtavismAuctionSellCountEntry
    {
        private Label m_totalCount;
        private UIAtavismCurrencyDisplay m_currecny;
        public VisualElement m_background;
        public VisualElement m_checkedState;
        private VisualElement m_Root;
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_totalCount = visualElement.Q<Label>("total-count");
            m_background = visualElement.Q<VisualElement>("background");
            m_checkedState = visualElement.Q<VisualElement>("checked-state");
            VisualElement curr = visualElement.Q<VisualElement>("currency-panel");
            m_currecny = new UIAtavismCurrencyDisplay();
            m_currecny.ReverseOrder = true;
            m_currecny.SetVisualElement(curr);
        }

        public void SetData(AuctionCountPrice data)
        {
            obj = data;
            // Debug.LogError("UIAtavismAuctionSellCountEntry SetData "+obj.count+" "+obj.currency+" "+obj.price+" "+obj.selected);
            if (m_totalCount != null)
            {
                m_totalCount.text = data.count.ToString();
            }

            if (m_currecny != null)
            {
                m_currecny.SetData(data.currency, data.price);
            }

            switch (data.selected)
            {
                case 0:
                    Reset();
                    break;
                case 1:
                    setPartial();
                    break;
                case 2:
                    setFull();
                    break;
            }
        }


        AuctionCountPrice obj;

        public void setFull()
        {
            // Reset();
           // Debug.LogError("UIAtavismAuctionSellCountEntry setFull "+obj.count+" "+obj.currency+" "+obj.price +" "+m_Root.parent==null?"null":"parent not null");
            if (m_background != null)
            {
                m_background.AddToClassList("auction-count-list-entry-select");
            }

            if (m_checkedState != null)
            {
                // m_checkedState.RemoveFromClassList("auction-count-list-entry-partial");
                if(m_checkedState.ClassListContains("auction-count-list-entry-partial"))
                    m_checkedState.RemoveFromClassList("auction-count-list-entry-partial");
                m_checkedState.AddToClassList("auction-count-list-entry-full");
            }
            //  Debug.LogError("AuctionCountSlot: setFull "+string.Join(",",m_checkedState.GetClasses())+" "+string.Join(",",));
        }

        public void setPartial()
        {
            // Reset();
         //   Debug.LogError("UIAtavismAuctionSellCountEntry setPartial "+obj.count+" "+obj.currency+" "+obj.price+" "+m_Root.parent==null?"null":"parent not null");
            if (m_background != null)
            {
                m_background.AddToClassList("auction-count-list-entry-select");
                m_background.GetClasses();
            }

            if (m_checkedState != null)
            {
                //  Debug.LogError("AuctionCountSlot: setPartial bef "+string.Join(",",m_checkedState.GetClasses())+" "+string.Join(",",m_background.GetClasses()));
                // m_checkedState.RemoveFromClassList("auction-count-list-entry-full");
                if(m_checkedState.ClassListContains("auction-count-list-entry-full"))
                    m_checkedState.RemoveFromClassList("auction-count-list-entry-full");
                m_checkedState.AddToClassList("auction-count-list-entry-partial");
                //  Debug.LogError("AuctionCountSlot: setPartial aft "+string.Join(",",m_checkedState.GetClasses())+" "+string.Join(",",m_background.GetClasses()));

            }

            //   Debug.LogError("AuctionCountSlot: setPartial "+string.Join(",",m_checkedState.GetClasses())+" "+string.Join(",",m_background.GetClasses()));
        }

        public void Reset()
        {
           // Debug.LogError("UIAtavismAuctionSellCountEntry Reset "+obj.count+" "+obj.currency+" "+obj.price);
            //  Debug.LogError("AuctionCountSlot: Reset "+string.Join(",",m_checkedState.GetClasses())+" "+string.Join(",",m_background.GetClasses()));
            if (m_background != null)
            {
                if(m_background.ClassListContains("auction-count-list-entry-select"))
                      m_background.RemoveFromClassList("auction-count-list-entry-select");
            }

            if (m_checkedState != null)
            {
                if(m_checkedState.ClassListContains("auction-count-list-entry-partial"))
                    m_checkedState.RemoveFromClassList("auction-count-list-entry-partial");
                if(m_checkedState.ClassListContains("auction-count-list-entry-full"))
                     m_checkedState.RemoveFromClassList("auction-count-list-entry-full");
            }
        }

    }
}