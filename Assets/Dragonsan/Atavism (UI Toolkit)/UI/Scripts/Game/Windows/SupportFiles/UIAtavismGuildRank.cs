using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismGuildRank : MonoBehaviour
    {
        Label m_rankText;
        public Label RankText => m_rankText;
        UITextField m_input;
        public UITextField Input => m_input;
        Button m_deleteButton;
        public Button DeleteButton => m_deleteButton;
        private VisualElement m_Root;
        public void SetVisualElement(VisualElement root)
        {
            m_Root = root;

            // backgroud.RegisterCallback<MouseOverEvent>(e => OnPointerEnter());
            // backgroud.RegisterCallback<MouseOutEvent>(e => OnPointerExit());
            m_rankText = root.Q<Label>("rank-id");
            m_input = root.Q<UITextField>("rank-name");
            m_input.RegisterValueChangedCallback(UpdateRankName);
            m_deleteButton = root.Q<Button>("delete-button");
            m_deleteButton.clicked += DeleteRank;
        }
        int rankId = -1;
        public void UpdateRankName(ChangeEvent<string> evt)
        {
            if (m_input != null && m_input.text.Length > 0)
                if (rankId >= 0)
                    AtavismGuild.Instance.SendGuildCommand("editRank", null, rankId + ";rename;" + m_input.value);
                else
                    AtavismGuild.Instance.SendGuildCommand("addRank", null, m_input.value);

        }
        public void DeleteRank()
        {
            AtavismGuild.Instance.SendGuildCommand("delRank", null, rankId.ToString());
        }
        public int setRankId
        {
            set
            {
                this.rankId = value;
            }
        }

        public void Hide()
        {
            m_Root.HideVisualElement();
        }

        public void Show()
        {m_Root.ShowVisualElement();
        }
    }
}