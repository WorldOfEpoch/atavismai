using Atavism.UI.Game;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismShopListEntry
    {
        Label name;
        private Button close;

        OID shop = null;

        // public UIAtavisMiniTooltipEvent tooltip;
        private VisualElement uiRoot;

        public void SetVisualElement(VisualElement visualElement)
        {
            uiRoot = visualElement;
            close = visualElement.Q<Button>("close-button");
            close.clicked += Click; 
            name = visualElement.Q<Label>("name");
        }

        // Update is called once per frame
        public void SetData(string msg, OID shop)
        {
            this.shop = shop;
            if (name != null)
                name.text = msg;
            // if (tooltip != null)
            //     tooltip.dectName = msg;

            uiRoot.tooltip = msg;
        }

        public void Click()
        {
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(AtavismPlayerShop.Instance.CloseShopConfirmMessage, shop,
                AtavismPlayerShop.Instance.CloseShopConfirmed);
        }

    }
}