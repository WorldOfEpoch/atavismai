using Atavism.UI.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismShopClose : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        Button exitButton;
        OID shop = null;
        bool isshop = false;
        // Start is called before the first frame update
        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            exitButton = uiDocument.rootVisualElement.Q<Button>("ExitButton");
            exitButton.clicked += Click;
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler("playerShop", shopHandle);
            if (ClientAPI.GetPlayerObject().PropertyExists("playerShop"))
            {
                isshop = (bool)ClientAPI.GetPlayerObject().GetProperty("playerShop");
                if (!isshop)
                {
                    exitButton.HideVisualElement();
                }
            }
            else
            {
                exitButton.HideVisualElement();
            }
        }

        private void shopHandle(object sender, PropertyChangeEventArgs args)
        {

            isshop = (bool)ClientAPI.GetPlayerObject().GetProperty("playerShop");
            if (isshop)
            {
                exitButton.ShowVisualElement();
            }
            else
            {
                exitButton.HideVisualElement();
            }
        }

        private void OnDestroy()
        {
            if(ClientAPI.GetPlayerObject()!=null)
                ClientAPI.GetPlayerObject().RemovePropertyChangeHandler("playerShop", shopHandle);
        }
        public void Click()
        {
            if (isshop)
            {
                if (ClientAPI.GetPlayerObject().PropertyExists("plyShopId"))
                {
                    shop = (OID)ClientAPI.GetPlayerObject().GetProperty("plyShopId");

                }
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(AtavismPlayerShop.Instance.CloseShopConfirmMessage, shop, AtavismPlayerShop.Instance.CloseShopConfirmed);
            }
        }
    }
}