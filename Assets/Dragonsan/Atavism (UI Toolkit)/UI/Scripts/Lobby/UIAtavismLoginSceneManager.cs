using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Atavism;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    /*
     * :root {
  --atavismDefault-color: color: rgb(#2A2827);
  --atavismTextDefault-color: color: rgb(#9E9A93);
  --atavismTextHighlighted-color: color: rgb(#F6F1E9);
  --atavismTextHint-color: color: rgb(#8B8B8B);
}
     */
    public class UIAtavismLoginSceneManager : MonoBehaviour
    {
        [AtavismSeparator("Configuration")]
        [SerializeField] protected bool useMd5Encryption = false;
        public bool UseMd5Encryption => useMd5Encryption;
        [SerializeField] protected CursorLockMode cursorLockState = CursorLockMode.Confined;

        [AtavismSeparator("UI")]
        [SerializeField] protected UIDocument uiLoginSceneDocument;
        [Space(10)]
        [SerializeField] private string uiLoginPanelName = "panel_login";
        [SerializeField] private string uiRegisterPanelName = "panel_register";
        [Space(10)]
        [SerializeField] private string uiLoginUsernameTextFieldName = "loginUsernameTextField";
        [SerializeField] private string uiLoginPasswordTextFieldName = "loginPasswordTextField";
        [SerializeField] private string uiLoginButtonName = "loginButton";
        [SerializeField] private string uiSaveCredentialsToggleName = "saveCredentialsToggle";
        [Space(10)]
        [SerializeField] private string uiRegisterOpenButtonName = "registerOpenButton";
        [SerializeField] private string uiRegisterCloseButtonName = "registerCloseButton";
        [SerializeField] private string uiRegisterCreateButtonName = "registerCreateButton";
        [SerializeField] private string uiRegisterUsernameTextFieldName = "registerUsername";
        [SerializeField] private string uiRegisterPasswordTextFieldName = "registerPassword";
        [SerializeField] private string uiRegisterPasswordConfirmTextFieldName = "registerPasswordConfirm";
        [SerializeField] private string uiRegisterEmailTextFieldName = "registerEmail";
        [SerializeField] private string uiRegisterEmailConfirmTextFieldName = "registerEmailConfirm";
        [Space(10)]
        [SerializeField] private string uiQuitButtonName = "quitButton";
        // placeholder color --unity-cursor-color: rgba(122, 115, 105, 255);

        protected VisualElement uiScreen, uiLoginPanel, uiRegisterPanel;
        protected TextField uiLoginUsernameTextField;
        protected TextField uiLoginPasswordTextField;
        protected Toggle uiSaveCredentialsToggle;
        protected Button uiLoginButton;
        protected Button uiRegisterOpenButton;
        protected Button uiRegisterCloseButton;
        protected Button uiRegisterCreateButton;
        protected Button uiQuitButton;

        protected VisualElement test;

        protected TextField uiRegisterUsernameTextField;
        protected TextField uiRegisterPasswordTextField;
        protected TextField uiRegisterPasswordConfirmTextField;
        protected TextField uiRegisterEmailTextField;
        protected TextField uiRegisterEmailConfirmTextField;
    
        private LoginState loginState;
        private bool isSettingsLoaded;

        private float loginMultiClickBlockTime;

        #region Initiate
        protected virtual void Reset()
        {
            uiLoginSceneDocument = GetComponent<UIDocument>();
        }

        protected virtual void OnEnable()
        {
            // UnityEngine.Cursor.lockState = cursorLockState;
            // UnityEngine.Cursor.visible = true;
            // UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            // if (AtavismCursor.Instance != null)
                // AtavismCursor.Instance.SetCursor(CursorState.Default);

            registerUI();

            registerEvents();
        }

        protected virtual void OnDisable()
        {
            unregiterEvents();
            unregisterUI();
        }
        #endregion
        #region UI
        protected virtual void registerUI()
        {
            uiScreen = uiLoginSceneDocument.rootVisualElement.Query<VisualElement>("Screen");

            uiLoginPanel = uiLoginSceneDocument.rootVisualElement.Query<VisualElement>(uiLoginPanelName);
            uiRegisterPanel = uiLoginSceneDocument.rootVisualElement.Query<VisualElement>(uiRegisterPanelName);

            uiLoginUsernameTextField = uiLoginSceneDocument.rootVisualElement.Query<TextField>(uiLoginUsernameTextFieldName);
            uiLoginUsernameTextField.RegisterValueChangedCallback(onLoginUsernameTextFieldChanged);

            uiLoginPasswordTextField = uiLoginSceneDocument.rootVisualElement.Query<TextField>(uiLoginPasswordTextFieldName);
            uiLoginPasswordTextField.RegisterValueChangedCallback(onLoginPasswordTextFieldChanged);

            uiSaveCredentialsToggle = uiLoginSceneDocument.rootVisualElement.Query<Toggle>(uiSaveCredentialsToggleName);
            uiSaveCredentialsToggle.RegisterValueChangedCallback(onSaveCredentialsButtonClicked);

            uiLoginButton = uiLoginSceneDocument.rootVisualElement.Query<Button>(uiLoginButtonName);
            uiLoginButton.clicked += onLoginButtonClicked;

            uiRegisterOpenButton = uiLoginSceneDocument.rootVisualElement.Query<Button>(uiRegisterOpenButtonName);
            uiRegisterOpenButton.clicked += onRegisterOpenButtonClicked;

            uiRegisterCloseButton = uiLoginSceneDocument.rootVisualElement.Query<Button>(uiRegisterCloseButtonName);
            uiRegisterCloseButton.clicked += onRegisterCloseButtonClicked;

            uiRegisterCreateButton = uiLoginSceneDocument.rootVisualElement.Query<Button>(uiRegisterCreateButtonName);
            uiRegisterCreateButton.clicked += onRegisterCreateButtonClicked;

            uiQuitButton = uiLoginSceneDocument.rootVisualElement.Query<Button>(uiQuitButtonName);
            uiQuitButton.clicked += onQuitButtonClicked;

            uiRegisterUsernameTextField = uiLoginSceneDocument.rootVisualElement.Query<TextField>(uiRegisterUsernameTextFieldName);
            uiRegisterUsernameTextField.RegisterValueChangedCallback(onLoginUsernameTextFieldChanged);

            uiRegisterPasswordTextField = uiLoginSceneDocument.rootVisualElement.Query<TextField>(uiRegisterPasswordTextFieldName);
            uiRegisterPasswordTextField.RegisterValueChangedCallback(onLoginPasswordTextFieldChanged);

            uiRegisterPasswordConfirmTextField = uiLoginSceneDocument.rootVisualElement.Query<TextField>(uiRegisterPasswordConfirmTextFieldName);
            uiRegisterPasswordConfirmTextField.RegisterValueChangedCallback(onRegisterPasswordConfirmTextFieldChanged);

            uiRegisterEmailTextField = uiLoginSceneDocument.rootVisualElement.Query<TextField>(uiRegisterEmailTextFieldName);
            uiRegisterEmailTextField.RegisterValueChangedCallback(onRegisterEmailTextFieldChanged);

            uiRegisterEmailConfirmTextField = uiLoginSceneDocument.rootVisualElement.Query<TextField>(uiRegisterEmailConfirmTextFieldName);
            uiRegisterEmailConfirmTextField.RegisterValueChangedCallback(onRegisterEmailConfirmTextFieldChanged);

            test = uiLoginSceneDocument.rootVisualElement.Query<VisualElement>("test");
        }

        protected virtual void unregisterUI()
        {
            uiLoginUsernameTextField.UnregisterValueChangedCallback(onLoginUsernameTextFieldChanged);
            uiLoginPasswordTextField.UnregisterValueChangedCallback(onLoginPasswordTextFieldChanged);
            uiSaveCredentialsToggle.UnregisterValueChangedCallback(onSaveCredentialsButtonClicked);
            uiLoginButton.clicked -= onLoginButtonClicked;
            uiRegisterOpenButton.clicked -= onRegisterOpenButtonClicked;
            uiRegisterCloseButton.clicked -= onRegisterCloseButtonClicked;
            uiRegisterCreateButton.clicked -= onRegisterCreateButtonClicked;
            uiQuitButton.clicked -= onQuitButtonClicked;

            uiRegisterUsernameTextField.UnregisterValueChangedCallback(onLoginUsernameTextFieldChanged);
            uiRegisterPasswordTextField.UnregisterValueChangedCallback(onLoginPasswordTextFieldChanged);
            uiRegisterPasswordConfirmTextField.UnregisterValueChangedCallback(onRegisterPasswordConfirmTextFieldChanged);
            uiRegisterEmailTextField.UnregisterValueChangedCallback(onRegisterEmailTextFieldChanged);
            uiRegisterEmailConfirmTextField.UnregisterValueChangedCallback(onRegisterEmailConfirmTextFieldChanged);

            UIAtavismAudioManager.Instance.UnregisterSFX(uiLoginSceneDocument);
        }

        protected virtual void onLoginButtonClicked()
        {
            Login();
        }

        protected virtual void onRegisterOpenButtonClicked()
        {
            ShowRegisterPanel();
        }

        protected virtual void onRegisterCloseButtonClicked()
        {
            clearRegistrationValues();
            ShowLoginPanel();
        }

        protected virtual void onRegisterCreateButtonClicked()
        {
            if (Register())
            {
                // clearRegistrationValues();
                // ShowLoginPanel();
            }
        }

        protected virtual void onQuitButtonClicked()
        {
            ClientAPI.Instance.Quit();
        }

        protected virtual void onLoginUsernameTextFieldChanged(ChangeEvent<string> evt)
        {
        }

        protected virtual void onLoginPasswordTextFieldChanged(ChangeEvent<string> evt)
        {
        }

        protected virtual void onRegisterPasswordConfirmTextFieldChanged(ChangeEvent<string> evt)
        {
        }

        protected virtual void onRegisterEmailTextFieldChanged(ChangeEvent<string> evt)
        {
        }

        protected virtual void onRegisterEmailConfirmTextFieldChanged(ChangeEvent<string> evt)
        {
        }

        protected virtual void onSaveCredentialsButtonClicked(ChangeEvent<bool> evt)
        {
            if (AtavismSettings.Instance != null)
                AtavismSettings.Instance.GetGeneralSettings().saveCredential = evt.newValue;

            if (!AtavismSettings.Instance.GetGeneralSettings().saveCredential)
            {
                AtavismSettings.Instance.GetCredentials().l = "";
                AtavismSettings.Instance.GetCredentials().p = "";
            }
        }
        #endregion
        #region Loop Updates
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (loginState == LoginState.Login)
                    Login();
            }

            // This function is to make sure, that credetials will be loaded when Player was logged out from gameplay.
            if (!isSettingsLoaded)
            {
                if (AtavismSettings.Instance != null)
                {
                    updateDataCredentials();
                    isSettingsLoaded = true;
                }
            }
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        public virtual void Login()
        {
            if (uiLoginUsernameTextField.text == "")
            {
#if AT_I2LOC_PRESET
   			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Please enter a username"));

#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Please enter a username");
#endif
                return;
            }

            if (uiLoginPasswordTextField.text == "")
            {
#if AT_I2LOC_PRESET
   			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Please enter a password"));

#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Please enter a password");
#endif
                return;
            }

            if (loginMultiClickBlockTime > Time.time)
                return;
            loginMultiClickBlockTime = Time.time + 1f;

            Dictionary<string, object> props = new Dictionary<string, object>();
            if (AtavismSettings.Instance.GetGeneralSettings().saveCredential)
            {
                AtavismSettings.Instance.GetCredentials().l = uiLoginUsernameTextField.text;
                AtavismSettings.Instance.GetCredentials().p = uiLoginPasswordTextField.text;
            }

            if (useMd5Encryption)
                AtavismClient.Instance.Login(uiLoginUsernameTextField.text, AtavismEncryption.Md5Sum(uiLoginPasswordTextField.text), props);
            else
                AtavismClient.Instance.Login(uiLoginUsernameTextField.text, uiLoginPasswordTextField.text, props);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Register()
        {
            if (uiRegisterUsernameTextField.text == "")
            {
#if AT_I2LOC_PRESET
			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Please enter a username"));
#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Please enter a username");
#endif
                return false;
            }

            if (uiRegisterUsernameTextField.text.Length < 4)
            {
#if AT_I2LOC_PRESET
			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Your username must be at least 4 characters long"));
#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Your username must be at least 4 characters long");
#endif
                return false;
            }

            foreach (char chr in uiRegisterUsernameTextField.text)
            {
                if ((chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9'))
                {
#if AT_I2LOC_PRESET
				    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Your username can only contain letters and numbers"));
#else
                    UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Your username can only contain letters and numbers");
#endif
                    return false;
                }
            }

            if (uiRegisterPasswordTextField.text == "")
            {
#if AT_I2LOC_PRESET
			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Please enter a password"));
#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Please enter a password");
#endif
                return false;
            }

            foreach (char chr in uiRegisterPasswordTextField.text)
            {
                if (chr == '*' || chr == '\'' || chr == '"' || chr == '/' || chr == '\\' || chr == ' ')
                {
#if AT_I2LOC_PRESET
				    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Your password cannot contain * \' \" / \\ or spaces"));
#else
                    UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Your password cannot contain * \' \" / \\ or spaces");
#endif
                    return false;
                }
            }

            if (uiRegisterPasswordTextField.text.Length < 6)
            {
#if AT_I2LOC_PRESET
			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Your password must be at least 6 characters long"));
#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Your password must be at least 6 characters long");
#endif
                return false;
            }

            if (uiRegisterPasswordTextField.text != uiRegisterPasswordConfirmTextField.text)
            {
#if AT_I2LOC_PRESET
			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Your passwords must match"));
#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Your passwords must match");
#endif
                return false;
            }

            if (uiRegisterEmailTextField.text == "")
            {
#if AT_I2LOC_PRESET
			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Please enter an email address"));
#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Please enter an email address");
#endif
                return false;
            }

            if (!validateEmail(uiRegisterEmailTextField.text))
            {
#if AT_I2LOC_PRESET
			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Please enter a valid email address"));
#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Please enter a valid email address");
#endif
                return false;
            }

            if (uiRegisterEmailTextField.text != uiRegisterEmailConfirmTextField.text)
            {
#if AT_I2LOC_PRESET
			    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Your email addresses must match"));
#else
                UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Your email addresses must match");
#endif
                return false;
            }

            if (useMd5Encryption)
                AtavismClient.Instance.CreateAccount(uiRegisterUsernameTextField.text, AtavismEncryption.Md5Sum(uiRegisterPasswordTextField.text), uiRegisterEmailTextField.text);
            else
                AtavismClient.Instance.CreateAccount(uiRegisterUsernameTextField.text, uiRegisterPasswordTextField.text, uiRegisterEmailTextField.text);

            return true;
        }

        public void ShowLoginPanel()
        {
            loginState = LoginState.Login;

            uiLoginPanel.style.display = DisplayStyle.Flex;
            uiRegisterPanel.style.display = DisplayStyle.None;
        }

        public void ShowRegisterPanel()
        {
            loginState = LoginState.Register;

            uiLoginPanel.style.display = DisplayStyle.None;
            uiRegisterPanel.style.display = DisplayStyle.Flex;
        }
        #endregion
        #region Private Methods
        protected void updateDataCredentials()
        {
            uiSaveCredentialsToggle.SetValueWithoutNotify(AtavismSettings.Instance.GetGeneralSettings().saveCredential);
            if (AtavismSettings.Instance.GetGeneralSettings().saveCredential)
                if (!string.IsNullOrEmpty(AtavismSettings.Instance.GetCredentials().l))
                    uiLoginUsernameTextField.SetValueWithoutNotify(AtavismSettings.Instance.GetCredentials().l);
            if (AtavismSettings.Instance.GetGeneralSettings().saveCredential)
                if (!string.IsNullOrEmpty(AtavismSettings.Instance.GetCredentials().p))
                    uiLoginPasswordTextField.SetValueWithoutNotify(AtavismSettings.Instance.GetCredentials().p);
        }

        protected bool validateEmail(string email)
        {
            // Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$");
            Match match = regex.Match(email);
            if (match.Success)
                return true;
            else
                return false;
        }

        protected void clearRegistrationValues()
        {
            uiRegisterUsernameTextField.SetValueWithoutNotify("");
            uiRegisterEmailTextField.SetValueWithoutNotify("");
            uiRegisterEmailConfirmTextField.SetValueWithoutNotify("");
            uiRegisterPasswordTextField.SetValueWithoutNotify("");
            uiRegisterPasswordConfirmTextField.SetValueWithoutNotify("");
        }
        #endregion
        #region Atavism Events
        protected virtual void registerEvents()
        {
            AtavismEventSystem.RegisterEvent(EVENTS.LOGIN, this);
            AtavismEventSystem.RegisterEvent(EVENTS.REGISTER, this);
            AtavismEventSystem.RegisterEvent(EVENTS.SETTINGS_LOADED, this);
        }

        protected virtual void unregiterEvents()
        {
            AtavismEventSystem.UnregisterEvent(EVENTS.LOGIN, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.REGISTER, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.SETTINGS_LOADED, this);
        }

        protected virtual void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == EVENTS.LOGIN)
            {
                if (eData.eventArgs[0] == "Success")
                {
                    //Application.LoadLevel(characterScene);
                }
                else
                {
                    string errorType = eData.eventArgs[0];
#if AT_I2LOC_PRESET
				string errorMessage = I2.Loc.LocalizationManager.GetTranslation(errorType);
#else
                    string errorMessage = errorType;
#endif
                    if (errorType == "LoginFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Invalid username or password");
#else
                        errorMessage = "Invalid username or password";
#endif
                    }
                    else if (errorType == "NoAccessFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Your account does not have access to log in");
#else
                        errorMessage = "Your account does not have access to log in";
#endif
                    }
                    else if (errorType == "BannedFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Your account has been banned");
#else
                        errorMessage = "Your account has been banned";
#endif
                    }
                    else if (errorType == "SubscriptionExpiredFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Your account does not have an active subscription");
#else
                        errorMessage = "Your account does not have an active subscription";
#endif
                    }
                    else if (errorType == "ServerMaintanance")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("The server is in maintenance mode, please try again later");
#else
                        errorMessage = "The server is in maintenance mode, please try again later";
#endif
                    }

                    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(errorMessage);
                }
            }
            else if (eData.eventType == EVENTS.REGISTER)
            {
                if (eData.eventArgs[0] == "Success")
                {
                    if (uiLoginUsernameTextField != null)
                        uiLoginUsernameTextField.value = uiRegisterUsernameTextField.value;
                    if (uiLoginPasswordTextField != null)
                        uiLoginPasswordTextField.value = uiRegisterPasswordTextField.value;
                    
                    ShowLoginPanel();
#if AT_I2LOC_PRESET
				    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(I2.Loc.LocalizationManager.GetTranslation("Account created. You can now log in"));
#else
                    UIAtavismDialogPopupManager.Instance.ShowDialogPopup("Account created. You can now log in");
#endif
                    clearRegistrationValues();
                }
                else
                {
                    string errorType = eData.eventArgs[0];
                    string errorMessage = errorType;
                    if (errorType == "UsernameUsed")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("An account with that username already exists");
#else
                        errorMessage = "An account with that username already exists";
#endif
                    }
                    else if (errorType == "EmailUsed")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("An account with that email address already exists");
#else
                        errorMessage = "An account with that email address already exists";
#endif
                    }
                    else if (errorType == "Unknown")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Unknown error. Please let the Dragonsan team know");
#else
                        errorMessage = "Unknown error. Please let the Dragonsan team know";
#endif
                    }
                    else if (errorType == "MasterTcpConnectFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Unable to connect to the Authentication Server");
#else
                        errorMessage = "Unable to connect to the Authentication Server";
#endif
                    }
                    else if (errorType == "NoAccessFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Account creation has been disabled on this server");
#else
                        errorMessage = "Account creation has been disabled on this server";
#endif
                    }

                    UIAtavismDialogPopupManager.Instance.ShowDialogPopup(errorMessage);
                }
            }
            else if (eData.eventType == EVENTS.SETTINGS_LOADED)
            {
                isSettingsLoaded = true;
                updateDataCredentials();
            }
        }
        #endregion
    }
}