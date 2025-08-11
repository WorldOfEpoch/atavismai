using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismClaimPermission : MonoBehaviour
    {

        Label permissionText;
        ClaimPermission permission;

        private Button deletePermission;

        private VisualElement m_Root;
        // Use this for initialization
        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement;
            permissionText = m_Root.Q<Label>("text");
            deletePermission = m_Root.Q<Button>("button");
            deletePermission.clicked += RemoveClaimPermission;
        }


        public void SetPermissionDetails(ClaimPermission permission)
        {
            string[] levels = new string[] { "Interaction", "Add Objects", "Edit Objects", "Add Users", "Manage Users" };
            this.permission = permission;
            if (permissionText != null)
                this.permissionText.text = permission.playerName + " (" + levels[permission.permissionLevel - 1] + ")";
        }

        public void RemoveClaimPermission()
        {
            WorldBuilder.Instance.RemovePermission(permission.playerName);
        }

    }
}