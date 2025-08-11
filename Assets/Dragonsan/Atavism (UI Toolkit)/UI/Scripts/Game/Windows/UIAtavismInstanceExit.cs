using System.Collections.Generic;
using Atavism.UI.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Atavism.UI
{
[RequireComponent(typeof(UIDocument))]
    public class UIAtavismInstanceExit : MonoBehaviour
    {

        [SerializeField] UIDocument uiDocument;
        Button exitButton;

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if(uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            exitButton = uiDocument.rootVisualElement.Q<Button>("ExitButton");
            exitButton.clicked += LeaveInstance;
            
            
            if (AtavismSettings.Instance.ArenaInstances.Contains(SceneManager.GetActiveScene().name) || AtavismSettings.Instance.DungeonInstances.Contains(SceneManager.GetActiveScene().name))
            {
                if (exitButton != null)
                    exitButton.ShowVisualElement();
            }
            else
            {
                if (exitButton != null)
                    exitButton.HideVisualElement();
            }

        }

        public void LeaveInstance()
        {
            if (AtavismSettings.Instance.ArenaInstances.Contains(SceneManager.GetActiveScene().name))
            {
#if AT_I2LOC_PRESET
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want exit arena") + " " + "?", null, SendLeaveArena);
#else
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want exit arena ?", null, SendLeaveArena);
#endif
            }

            if (AtavismSettings.Instance.DungeonInstances.Contains(SceneManager.GetActiveScene().name))
            {
#if AT_I2LOC_PRESET
            UIAtavismConfirmationPanel.Instance.ShowConfirmationBox(I2.Loc.LocalizationManager.GetTranslation("Are you sure you want exit instance") + " " + "?", null, SendLeaveInstance);
#else
                UIAtavismConfirmationPanel.Instance.ShowConfirmationBox("Are you sure you want exit instance ?", null, SendLeaveInstance);
#endif
            }
        }

        void SendLeaveArena(object item, bool accepted)
        {
            if (accepted)
            {
                //    Debug.LogError("Leave Arena");
                Dictionary<string, object> props = new Dictionary<string, object>();
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "arena.leaveArena", props);
            }
        }

        void SendLeaveInstance(object item, bool accepted)
        {
            if (accepted)
            {
                //    Debug.LogError("Leave Instance");
                Dictionary<string, object> props = new Dictionary<string, object>();
                NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.leaveInstance", props);
            }
        }

    }
}