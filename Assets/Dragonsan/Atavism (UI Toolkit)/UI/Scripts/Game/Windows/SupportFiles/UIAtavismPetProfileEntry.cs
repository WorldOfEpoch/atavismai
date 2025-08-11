using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismPetProfileEntry
    {
        private VisualElement m_icon;
        private Label m_info;
        private VisualElement m_Root;
        private long petOid = -1;
        public void SetVisualElement(VisualElement root)
        {
            m_Root = root;
            m_icon = root.Q<VisualElement>("icon");
            m_icon.RegisterCallback<MouseUpEvent>(onButtonMouseUp);
            m_info = root.Q<Label>("info");
        }

        private void onButtonMouseUp(MouseUpEvent evt)
        {
           UIAtavismPetInventoryPanel.Instance.UpdateCharacterData(petOid);
        }

        public void UpdateData(Texture2D icon, string info, long pet )
        {
            petOid = pet;
#if AT_I2LOC_PRESET
            if(info.Length>0)
            info = I2.Loc.LocalizationManager.GetTranslation(info);
            
#endif
            m_info.text = info;
            m_icon.SetBackgroundImage(icon); 
        }

        public void Hide()
        {
            m_Root.HideVisualElement();
        }

        public void Show()
        {
            m_Root.ShowVisualElement();
        }
    }
}