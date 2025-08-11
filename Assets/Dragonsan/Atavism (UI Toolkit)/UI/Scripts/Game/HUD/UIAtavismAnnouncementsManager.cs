using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismAnnouncementsManager : MonoBehaviour
    {
        [SerializeField] protected UIDocument uiDocument;
        [AtavismSeparator("Settings")]
        [Range(0f, 10f)]
        [SerializeField] private float adminMessageDuration = 5f;
        [Range(0f, 10f)]
        [SerializeField] private float errorMessageDuration = 2f;
        [Range(0f, 10f)]
        [SerializeField] private float regionMessageDuration = 1.5f;
        [Range(0f, 10f)]
        [SerializeField] private float announcementMessageDuration = 3.5f;
        [AtavismSeparator("UI")]
        [SerializeField] private string adminMessageName = "admin-message";
        [SerializeField] private string errorMessageName = "error-message";
        [SerializeField] private string regionMessageName = "region-message";
        [SerializeField] private string announcementMessageName = "announcement-message";
        [SerializeField] private string messageHolderName = "message-holder";

        private Label uiAdminMessage, uiErrorMessage, uiRegionMessage, uiAnnouncementMessage;
        private VisualElement uiMessageHolder;
        #region Initiate
        protected  void OnEnable()
        {
           // base.OnEnable();
           registerUI();
           registerEvents();
           registerExtensionMessages();
            HideAllMessages();
        }

        protected  void Start()
        {
        }

        private void OnDestroy()
        {
            unregisterUI();
            unregisterEvents();
            unregisterExtensionMessages();
        }

        protected  bool registerUI()
        {
            uiMessageHolder = uiDocument.rootVisualElement.Query<VisualElement>(messageHolderName); 
            uiAdminMessage = uiDocument.rootVisualElement.Query<Label>(adminMessageName);
            uiErrorMessage = uiDocument.rootVisualElement.Query<Label>(errorMessageName);
            uiRegionMessage = uiDocument.rootVisualElement.Query<Label>(regionMessageName);
            uiAnnouncementMessage = uiDocument.rootVisualElement.Query<Label>(announcementMessageName);

            return true;
        }

        
        protected  bool unregisterUI()
        {
            return true;
        }

        protected void registerEvents()
        {
            AtavismEventSystem.RegisterEvent(EVENTS.ADMIN_MESSAGE, this);
            AtavismEventSystem.RegisterEvent(EVENTS.ERROR_MESSAGE, this);
            AtavismEventSystem.RegisterEvent(EVENTS.REGION_MESSAGE, this);
            AtavismEventSystem.RegisterEvent(EVENTS.ANNOUNCEMENT_MESSAGE, this);
            AtavismEventSystem.RegisterEvent(EVENTS.ANNOUNCEMENT, this);
        }

        protected void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent(EVENTS.ADMIN_MESSAGE, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.ERROR_MESSAGE, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.REGION_MESSAGE, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.ANNOUNCEMENT_MESSAGE, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.ANNOUNCEMENT, this);
        }

        protected void registerExtensionMessages()
        {
            NetworkAPI.RegisterExtensionMessageHandler(EXT_MSGS.ERROR_MESSAGE, HandleErrorMessage);
            NetworkAPI.RegisterExtensionMessageHandler(EXT_MSGS.ERROR_ABILITY, HandleAbilityErrorMessage);
            NetworkAPI.RegisterExtensionMessageHandler(EXT_MSGS.ANNOUNCEMENT_SPECIAL, HandleAnnouncementMessage);
            NetworkAPI.RegisterExtensionMessageHandler(EXT_MSGS.ANNOUNCEMENT, HandleAnnouncementMessage);
        }

        protected void unregisterExtensionMessages()
        {
            NetworkAPI.RemoveExtensionMessageHandler(EXT_MSGS.ERROR_MESSAGE, HandleErrorMessage);
            NetworkAPI.RemoveExtensionMessageHandler(EXT_MSGS.ERROR_ABILITY, HandleAbilityErrorMessage);
            NetworkAPI.RemoveExtensionMessageHandler(EXT_MSGS.ANNOUNCEMENT_SPECIAL, HandleAnnouncementMessage);
            NetworkAPI.RemoveExtensionMessageHandler(EXT_MSGS.ANNOUNCEMENT, HandleAnnouncementMessage);
        }
        #endregion
        #region Atavism Events
        protected void OnEvent(AtavismEventData eData)
        {
            
         //   Debug.LogError("OnEvent "+eData.eventType);
         if (eData.eventArgs[0].Length > 0)
         {
             if (eData.eventType == EVENTS.ADMIN_MESSAGE)
                 ShowAdminMessage(eData.eventArgs[0]);
             if (eData.eventType == EVENTS.ERROR_MESSAGE)
                 ShowErrorMessage(eData.eventArgs[0]);
             if (eData.eventType == EVENTS.REGION_MESSAGE)
                 ShowRegionMessage(eData.eventArgs[0]);
             if (eData.eventType == EVENTS.ANNOUNCEMENT_MESSAGE)
                 ShowAnnouncementMessage(eData.eventArgs[0]);
             if (eData.eventType == EVENTS.ANNOUNCEMENT)
                 ShowAnnouncementMessage(eData.eventArgs[0]);
         }
        }
        #endregion
        #region Admin Message
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void ShowAdminMessage(string message)
        {
            if (!gameObject.activeInHierarchy)
                return;
            if (uiAdminMessage == null)
                return;

            StartCoroutine(nameof(showAdminMessageAsync), message);
        }

        private IEnumerator showAdminMessageAsync(string message)
        {
            Label newMessageElement = new(message);
            newMessageElement.AddToClassList(adminMessageName);
            uiMessageHolder.Add(newMessageElement);
            yield return new WaitForSecondsRealtime(adminMessageDuration);
            uiMessageHolder.Remove(newMessageElement);
        }
        #endregion
        #region Error Message
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorMessage(string message)
        {
            if (!gameObject.activeInHierarchy)
                return;
            if (uiErrorMessage == null)
                return;

            StartCoroutine(nameof(showErrorMessageAsync), message);
        }

        private IEnumerator showErrorMessageAsync(string message)
        {
            Label newMessageElement = new(message);
            newMessageElement.AddToClassList(errorMessageName);
            uiMessageHolder.Add(newMessageElement);
            yield return new WaitForSecondsRealtime(errorMessageDuration);
            uiMessageHolder.Remove(newMessageElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="props"></param>
        private void HandleErrorMessage(Dictionary<string, object> props)
        {
            string errorMessage = (string)props["ErrorText"];

            if (errorMessage == "NotEnoughCurrency")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("NotEnoughCurrency");
#else
                errorMessage = "You do not have enough currency to perform that action";
#endif
            }
            else if (errorMessage == "cooldownNoEnd")
            {
#if AT_I2LOC_PRESET
             errorMessage =  I2.Loc.LocalizationManager.GetTranslation("cooldownNoEnd");
#else
                errorMessage = "Cooldown has not finished yet";
#endif
            }
            else if (errorMessage == "SocialPlayerOffline")
            {
#if AT_I2LOC_PRESET
             errorMessage =  I2.Loc.LocalizationManager.GetTranslation("Can not add Friend because is offline");
#else
                errorMessage = "Can not add Friend because is offline";
#endif
            }
            else if (errorMessage == "InstanceRequiresGuild")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must be in a Guild to enter this Instance");
#else
                errorMessage = "You must be in a Guild to enter this Instance";
#endif
            }
            else if (errorMessage == "PET_GLOBAL_LIMIT")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Pet limit has been reached");
#else
                errorMessage = "Pet limit has been reached";
#endif
            } else if (errorMessage == "PET_TYPE_LIMIT")
            {
#if AT_I2LOC_PRESET
            errorMessage = I2.Loc.LocalizationManager.GetTranslation("Pet limit has been reached");
#else
                errorMessage = "Pet limit has been reached";
#endif
            }
            
#if AT_I2LOC_PRESET
            ShowErrorMessage(I2.Loc.LocalizationManager.GetTranslation(errorMessage));
#else
            ShowErrorMessage(errorMessage);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="props"></param>
        private void HandleAbilityErrorMessage(Dictionary<string, object> props)
        {
            string errorMessage = "";

            int messageType = (int)props["ErrorText"];
#if AT_I2LOC_PRESET
        		if (messageType == 1) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("Invalid target");
		} else if (messageType == 2) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("Target is too far away");
		} else if ( messageType == 3) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("Target is too close");
		} else if ( messageType == 4) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You cannot perform that action yet");
		} else if ( messageType == 5) {
             string data = (string)props["data"];
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("Not enough")+" "+I2.Loc.LocalizationManager.GetTranslation(data);
			
		} else if ( messageType == 6) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have the required reagent");
		} else if ( messageType == 7) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have the required tool");
		} else if ( messageType == 8) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have the required ammo equipped");
		} else if ( messageType == 9) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You are not in the correct stance");
		} else if ( messageType == 10) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have the required weapon equipped");
		} else if ( messageType == 11) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have a shield equipped");
		} else if ( messageType == 12) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("Not Enough Vigor");
		} else if ( messageType == 13) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have the required effect");
		} else if ( messageType == 14) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You have no target");
		} else if ( messageType == 15) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You do not have the required weapon type equipped");
		} else if ( messageType == 16) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You cannot activate a passive ability");
		} else if ( messageType == 17) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("Interrupted");
		} else if ( messageType == 18) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You cannot do that while you are dead");
		}else if ( messageType == 19) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must be facing your target to use that ability");
		} else if ( messageType == 20) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must be begind your target to use that ability");
		} else if ( messageType == 21) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must see your target to use that ability");
		}else if ( messageType == 22) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must be dead to use that ability");
		}else if ( messageType == 23) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must be in spirit to use that ability");
		}else if ( messageType == 24) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must be in combat to use that ability");
		}else if ( messageType == 25) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You must not be in combat to use that ability");
		}else if ( messageType == 26) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You have reached your pet limit");
		}else if ( messageType == 27) {
			errorMessage = I2.Loc.LocalizationManager.GetTranslation("You have reached your pet limit");
		}
        

#else
            if (messageType == 1)
            {
                errorMessage = "Invalid target";
            }
            else if (messageType == 2)
            {
                errorMessage = "Target is too far away";
            }
            else if (messageType == 3)
            {
                errorMessage = "Target is too close";
            }
            else if (messageType == 4)
            {
                errorMessage = "You cannot perform that action yet";
            }
            else if (messageType == 5)
            {
                string data = (string)props["data"];
                errorMessage = "Not enough " + data;

            }
            else if (messageType == 6)
            {
                errorMessage = "You do not have the required reagent";
            }
            else if (messageType == 7)
            {
                errorMessage = "You do not have the required tool";
            }
            else if (messageType == 8)
            {
                errorMessage = "You do not have the required ammo equipped";
            }
            else if (messageType == 9)
            {
                errorMessage = "You are not in the correct stance";
            }
            else if (messageType == 10)
            {
                errorMessage = "You do not have the required weapon equipped";
            }
            else if (messageType == 11)
            {
                errorMessage = "You do not have a shield equipped";
            }
            else if (messageType == 12)
            {
                errorMessage = "Not Enough Vigor";
            }
            else if (messageType == 13)
            {
                errorMessage = "You do not have the required effect";
            }
            else if (messageType == 14)
            {
                errorMessage = "You have no target";
            }
            else if (messageType == 15)
            {
                errorMessage = "You do not have the required weapon type equipped";
            }
            else if (messageType == 16)
            {
                errorMessage = "You cannot activate a passive ability";
            }
            else if (messageType == 17)
            {
                errorMessage = "Interrupted";
            }
            else if (messageType == 18)
            {
                errorMessage = "You cannot do that while you are dead";
            }
            else if (messageType == 19)
            {
                errorMessage = "You must be facing your target to use that ability";
            } else if (messageType == 20)
            {
                errorMessage = "You must be behind your target to use that ability";
            } else if (messageType == 21)
            {
                errorMessage = "You must see your target to use that ability";
            } else if (messageType == 22)
            {
                errorMessage = "You must be dead to use that ability";
            } else if (messageType == 23)
            {
                errorMessage = "You must be in spirit to use that ability";
            }else if (messageType == 24)
            {
                errorMessage = "You must be in combat to use that ability";
            }else if (messageType == 25)
            {
                errorMessage = "You must not be in combat to use that ability";
            }else if (messageType == 26)
            {
                errorMessage = "You have reached your pet limit";
            }else if (messageType == 27)
            {
                errorMessage = "You have reached your pet limit";
            }
#endif
            ShowErrorMessage(errorMessage);
        }
        #endregion
        #region Region Message
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void ShowRegionMessage(string message)
        {
            if (!gameObject.activeInHierarchy)
                return;
            if (uiRegionMessage == null)
                return;

            StartCoroutine(nameof(showRegionMessageAsync), message);
        }

        private IEnumerator showRegionMessageAsync(string message)
        {
            Label newMessageElement = new(message);
            newMessageElement.AddToClassList(regionMessageName);
            uiMessageHolder.Add(newMessageElement);
            yield return new WaitForSecondsRealtime(regionMessageDuration);
            uiMessageHolder.Remove(newMessageElement);
        }
        #endregion
        #region Announcement Message
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void ShowAnnouncementMessage(string message)
        {
            if (!gameObject.activeInHierarchy)
                return;
            if (uiAnnouncementMessage == null)
                return;

            StartCoroutine(nameof(showAnnouncementMessageAsync), message);
        }

        private IEnumerator showAnnouncementMessageAsync(string message)
        {
            Label newMessageElement = new(message);
            newMessageElement.AddToClassList(announcementMessageName);
            uiMessageHolder.Add(newMessageElement);
            yield return new WaitForSecondsRealtime(announcementMessageDuration);
            uiMessageHolder.Remove(newMessageElement);
        }

        public void HandleAnnouncementMessage(Dictionary<string, object> props)
        {
            ShowAnnouncementMessage((string)props["AnnouncementText"]);
        }
        #endregion

        public void HideAllMessages()
        {
            uiAdminMessage.HideVisualElement();
            uiErrorMessage.HideVisualElement();
            uiRegionMessage.HideVisualElement();
            uiAnnouncementMessage.HideVisualElement();
        }
    }
}