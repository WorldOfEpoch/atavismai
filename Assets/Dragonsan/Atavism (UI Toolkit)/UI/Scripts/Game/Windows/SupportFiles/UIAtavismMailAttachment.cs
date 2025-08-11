using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismMailAttachment //: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        public Label m_countText;
        public VisualElement m_iconImage;
        public VisualElement m_qualityImage;
        public GameObject stackBox;
        AtavismInventoryItem item;
        int slotNum;
        bool mouseEntered = false;
        private VisualElement m_Root;
        
        // Use this for initialization
        void Start()
        {
            // if (iconImage == null)
            //     if (GetComponent<Button>() != null)
            //         iconImage = GetComponent<Button>().image;
        }

        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
            m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
            m_Root.RegisterCallback<MouseUpEvent>(TakeMailAttachment);
            m_countText = visualElement.Q<Label>("count");
            m_iconImage = visualElement.Q<VisualElement>("icon");
            m_qualityImage = visualElement.Q<VisualElement>("quality");

        }
        
        public void OnPointerEnter(MouseEnterEvent evt)
        {
            MouseEntered = true;
        }

        public void OnPointerExit(MouseLeaveEvent evt)
        {
            MouseEntered = false;
        }

        public void SetMailAttachmentData(AtavismInventoryItem item, int count, int slot)
        {
            this.item = item;
            if (m_iconImage != null)
            {
                if (item.Icon != null)
                    m_iconImage.style.backgroundImage = item.Icon.texture;
                else
                    m_iconImage.style.backgroundImage = AtavismSettings.Instance.defaultItemIcon.texture;
           }
            if (m_qualityImage != null)
            {
                if (item != null)
                {
                    m_qualityImage.style.unityBackgroundImageTintColor = AtavismSettings.Instance.ItemQualityColor(item.quality);
                    m_qualityImage.visible = true;
                }
                else
                {
                    m_qualityImage.visible = false;
                }
            }

            if (m_countText != null)
                m_countText.text = count.ToString();
            if (stackBox != null)
                stackBox.SetActive(count > 0);

            this.slotNum = slot;
        }

        public void TakeMailAttachment(MouseUpEvent evt)
        {
            Mailing.Instance.TakeMailItem(slotNum);
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
        }

        public bool MouseEntered
        {
            get
            {
                return mouseEntered;
            }
            set
            {
                mouseEntered = value;
                if (mouseEntered && item != null)
                {
                    item.ShowUITooltip(m_Root);
                }
                else
                {
                    HideTooltip();
                }
            }
        }

        public bool IsVisible()
        {
            return m_Root.visible;
        }

        public void Hide()
        {
            m_Root.HideVisualElement();
            m_Root.visible = false;
        }

        public void Show()
        {
            m_Root.ShowVisualElement();
            m_Root.visible = true;
        }
    }
}