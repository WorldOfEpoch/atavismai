using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    /// <summary>
    /// Bugs: 
    /// 1) ListView - multi-lines not implemented - hopefull Unity will make new ListView update or has to create some work-arround
    /// 2) ListView - Scroll speed not implemented
    /// ToDo: optional - Reordable chatPanels
    /// </summary>
    public class UIAtavismChatManager : UIAtavismWindowBase
    {
        public enum ChatWindowViewEnum { Chat = 2, Group = 4, Guild = 8, CombatLog = 16, EventLog = 32, Whisper = 64 }

        [System.Serializable]
        public class ChatPanelItem
        {
            public string PanelName;
            public ChatWindowViewEnum ChatType;
            public Color TextColor;
            public string Placeholder;
            public string ChatLabelClassStyle = "chat-label__item";
            public int ItemHeight = 16;

            [System.NonSerialized]
            public List<string> ChatLog;
            [System.NonSerialized]
            public ListView ListView;
            [System.NonSerialized]
            public ScrollView ScrollView;

            public Action<IEnumerable<object>> OnItemsChosen;
            public Action<IEnumerable<object>> OnSelectionChange;
            public void Activate() => groupPanel.SelectPanel(panelIndex);

            public bool IsInitialized { get; private set; }
            private Func<VisualElement> makeItem;
            private UIButtonToggleGroupPanel groupPanel;
            private int panelIndex;
            
         
            public ChatPanelItem(string name, ChatWindowViewEnum type, Color color, string placeholder)
            {
                this.PanelName = name;
                this.ChatType = type;
                this.TextColor = color;
                this.Placeholder = placeholder;
            }

            /// <summary>
            /// The "makeItem" function is called when the ListView needs more items to render.
            /// As the user scrolls through the list, the ListView object recycles elements created by the "makeItem" function,
            /// and invoke the "bindItem" callback to associate the element with the matching data item (specified as an index in the list).
            /// </summary>
            /// <returns></returns>
            public void Initialize(UIButtonToggleGroupPanel group, int panelIndex)
            {
                if (string.IsNullOrEmpty(PanelName))
                    return;

                if (ChatLog == null)
                    ChatLog = new List<string>();

                if (makeItem == null)
                    makeItem = () =>
                    {
                        Label label = new Label();
                        label.style.color = TextColor;
                        label.AddToClassList(ChatLabelClassStyle);

                        return label;
                    };

                this.groupPanel = group;
                this.panelIndex = panelIndex;

                // Provide the list view with an explict height for every row so it can calculate how many items to actually display
                if (ListView == null)
                {
                    ListView = new ListView(ChatLog, ItemHeight, makeItem, (e, i) => (e as Label).text = ChatLog[i]);

                    ListView.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                    ListView.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                    ListView.selectionType = SelectionType.None;
                    ListView.style.flexGrow = 1.0f;
                    ListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                    ListView.AddToClassList("chat-ListView");

                    groupPanel.AddItem(ListView, panelIndex);
                }

                if (ScrollView == null)
                {
                    ScrollView = ListView.Query<ScrollView>();
                    ScrollView.AddToClassList("chat-ScrollView");
#if UNITY_6000_0_OR_NEWER    
                ScrollView.mouseWheelScrollSize = 19;
#endif
                }

                VisualElement container = ListView.Query<VisualElement>("unity-content-container");
                container.AddToClassList("chat-ScrollView__container");

                ListView.itemsChosen -= OnItemsChosen;
                ListView.selectionChanged -= OnSelectionChange;
                ListView.itemsChosen += OnItemsChosen;
                ListView.selectionChanged += OnSelectionChange;
                ListView.RegisterCallback<GeometryChangedEvent>(onGeometryChangedEvent);
                IsInitialized = true;
            }

            private void onGeometryChangedEvent(GeometryChangedEvent evt)
            {
                bool doScroll = false;
                if (ScrollView.verticalScroller.value == 0 || ScrollView.verticalScroller.highValue - ScrollView.verticalScroller.value < 50 )
                    doScroll = true;
                ListView.RefreshItems();
              //  Debug.LogError("Chat onGeometryChangedEvent do Scroll? "+doScroll+" "+ScrollView.verticalScroller.value+" "+ScrollView.verticalScroller.highValue);
                // ScrollView.verticalScroller.
                if (doScroll)
                    ListView.ScrollToItem(ChatLog.Count - 1);
            }

            /// <summary>
            /// 
            /// </summary>
            public void Deinitialize()
            {
                if (ListView != null)
                {
                    ListView.itemsChosen -= OnItemsChosen;
                    ListView.selectionChanged -= OnSelectionChange;
                }

                IsInitialized = false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="text"></param>
            public void AddChatMessage(string text, int maxHistory = 100)
            {
                if (string.IsNullOrEmpty(text))
                    return;

                if (text.Contains("<link=item#"))
                {
                    string s = text.Substring(text.IndexOf("<link=item#") + 11);
                    s = s.Substring(0, s.IndexOf(">"));
                    int id = Int32.Parse(s);
                    Inventory.Instance.GetItemByTemplateID(id);
                }
                if (text.Contains("<link=ability#"))
                {
                    string s = text.Substring(text.IndexOf("<link=ability#") + 14);
                    s = s.Substring(0, s.IndexOf(">"));
                    int id = Int32.Parse(s);
                    Abilities.Instance.GetAbility(id);
                }
                if (text.Contains("<size="))
                {
                    while (text.Contains("<size="))
                    {
                        string fs = text.Substring(0, text.IndexOf("<size"));
                        string s = text.Substring(text.IndexOf("<size"));
                        s = s.Substring(s.IndexOf(">") + 1);
                        text = fs + s;
                    }
                }

                bool doScroll = false;
                if (ScrollView.verticalScroller.value == 0 || ScrollView.verticalScroller.highValue - ScrollView.verticalScroller.value < 50 )
                    doScroll = true;

                ChatLog.Add(text);
                if (ChatLog.Count >= maxHistory)
                    ChatLog.RemoveAt(0);
                ListView.RefreshItems();
              //  Debug.LogError("Chat AddChatMessage do Scroll? "+doScroll+" "+ScrollView.verticalScroller.value+" "+ScrollView.verticalScroller.highValue);
                if (doScroll)
                    ListView.ScrollToItem(ChatLog.Count - 1);
            }
        }

        private static UIAtavismChatManager instance;
        public static UIAtavismChatManager Instance => instance;
        public static ChatWindowViewEnum WindowView { get; private set; }
        public static string ChatChannelCommand { get; private set; }
        public static string ChatPlaceholder { get; private set; }

        [AtavismSeparator("Settings")]
        [SerializeField] private ChatPanelItem[] chatPanels;
        [SerializeField] private int maxHistory = 100;

        [SerializeField] private bool canResizing = true;
        [SerializeField] private Vector2 resizingMinValues = new Vector2(570, 240);
        [SerializeField] private Vector2 resizingMaxValues = new Vector2(1920, 1080);
        
        VisualElement uiResizeContainer;
        VisualElement uiResizeDrag;
        bool isResizing = false;
        
        [AtavismSeparator("Text Colors Settings")]
        public Color announcementColor = Color.green;
        public Color globalColor = new Color(1f, 0.5f, 0f);
        public Color instanceColor = new Color(.5f, 0f, 1f);
        public Color currentChannelCommandColor = Color.white;
        public Color chatColor => listofChatPanels[ChatWindowViewEnum.Chat].TextColor;
        public Color groupColor => listofChatPanels[ChatWindowViewEnum.Group].TextColor;
        public Color guildColor => listofChatPanels[ChatWindowViewEnum.Guild].TextColor;
        public Color eventColor => listofChatPanels[ChatWindowViewEnum.EventLog].TextColor;
        public Color combatColor => listofChatPanels[ChatWindowViewEnum.CombatLog].TextColor;
        public Color whisperColor => listofChatPanels[ChatWindowViewEnum.Whisper].TextColor;

        private UIButtonToggleGroupPanel uiGroupPanel;
        public  UITextField uiChatTextField;
        private Button uiHelpButton;
        private Button uiSubmitButton;

       

        private Dictionary<ChatWindowViewEnum, ChatPanelItem> listofChatPanels;
        public List<string> GetChatLog(ChatWindowViewEnum chat) => listofChatPanels[chat].ChatLog;

        private bool isChatSubmited;
        private Vector2 Offset;
        private float starWidth;
        private float starHeight;
        private Vector2 startPosition;
        private float lastTimeReturnKeyPressed;
        public bool IsFocused => uiChatTextField.IsFocused;

        #region Initiate
        protected override void Reset()
        {
            base.Reset();

            chatPanels = new ChatPanelItem[5];
            chatPanels[0] = new ChatPanelItem("Chat", ChatWindowViewEnum.Chat, Color.white, "Say:");
            chatPanels[1] = new ChatPanelItem("Group", ChatWindowViewEnum.Group, Color.cyan, "Group:");
            chatPanels[2] = new ChatPanelItem("Guild", ChatWindowViewEnum.Guild, Color.green, "Guild:");
            chatPanels[3] = new ChatPanelItem("Events", ChatWindowViewEnum.EventLog, Color.yellow, "Events:");
            chatPanels[4] = new ChatPanelItem("Combat", ChatWindowViewEnum.CombatLog, Color.red, "Combat:");
        }

        private void Awake()
        {
            instance = this;
        }

        protected override void Start()
        {
            base.Start();

            Show();
            uiGroupPanel.SelectPanel(0);
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;

            uiGroupPanel = uiDocument.rootVisualElement.Query<UIButtonToggleGroupPanel>("group-panel");
            string panelsNames = "";
            for (int n = 0; n < chatPanels.Length; n++)
            {
                if (string.IsNullOrEmpty(chatPanels[n].PanelName))
                    continue;

                if (n == 0)
                    panelsNames = chatPanels[n].PanelName;
                else
                    panelsNames += "," + chatPanels[n].PanelName;
            }
            uiGroupPanel.CreateButtons(panelsNames);
            uiGroupPanel.OnItemIndexChanged += onGroupPanelChanged;

            uiGroupPanel.RegisterCallback<PointerDownEvent>(OnPanelPointerDown);
            uiGroupPanel.RegisterCallback<PointerUpEvent>(OnPanelPointerUp);
            uiChatTextField = uiDocument.rootVisualElement.Query<UITextField>("chat-TextField");
            uiChatTextField.RegisterCallback<KeyDownEvent>(onChatSubmitEvent);
            uiChatTextField.RegisterCallback<FocusOutEvent>(onInputFokusOutEvent);

            uiHelpButton = uiDocument.rootVisualElement.Query<Button>("help-Button");
            uiHelpButton.clicked += CommandHelp;

            uiSubmitButton = uiDocument.rootVisualElement.Query<Button>("submit-Button");
            if (uiSubmitButton != null)
                uiSubmitButton.clicked += SubmitMessage;
            if (canResizing)
            {
                uiResizeContainer = uiDocument.rootVisualElement.Query<VisualElement>("frame");
                uiResizeDrag = uiDocument.rootVisualElement.Query<VisualElement>("resize");

                uiResizeDrag.RegisterCallback<MouseDownEvent>(StartDrag);
                uiResizeDrag.RegisterCallback<MouseUpEvent>(EndDrag);
            }

            // Initialize Chat panels
            if (listofChatPanels == null)
                listofChatPanels = new Dictionary<ChatWindowViewEnum, ChatPanelItem>();
            else listofChatPanels.Clear();
            for (int n = 0; n < chatPanels.Length; n++)
            {
                chatPanels[n].Initialize(uiGroupPanel, n);
                listofChatPanels[chatPanels[n].ChatType] = chatPanels[n];

                if (chatPanels[n].ChatType == ChatWindowViewEnum.Chat)
                {
                    chatPanels[n].OnItemsChosen += onGeneralChatItemChosen;
                    chatPanels[n].OnSelectionChange += onGeneralChatSelectionChange;
                }
            }

            return true;
        }

        private void onInputFokusOutEvent(FocusOutEvent evt)
        {
            if(!isChatSubmited && Time.time - lastTimeReturnKeyPressed < 0.2f)
                FocusOn();
        }

        private void OnPanelPointerUp(PointerUpEvent evt)
        {
            draggingEnd();
        }

        private void OnPanelPointerDown(PointerDownEvent evt)
        {
            uiWindow.style.bottom = new StyleLength(StyleKeyword.Auto);
            draggingBegin();

        }

        private void StartDrag(MouseDownEvent mouseDownEvent)
        {
            float width = uiWindow.resolvedStyle.width;
            float height = uiWindow.resolvedStyle.height;
            float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
            float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
            float widthScaleFactor = Screen.width / canvasWidth;
            float heightScaleFactor = Screen.height / canvasHeight;
            startPosition = new Vector2(uiWindow.resolvedStyle.left, uiWindow.resolvedStyle.top);

            draggingMinValues.x = 0f;
            draggingMinValues.y = 0f;
            draggingMaxValues.x = canvasWidth - width;
            draggingMaxValues.y = canvasHeight - height;
            Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,
                Input.mousePosition.y / heightScaleFactor);
            Offset.x = scaledMousePosition.x - uiWindow.resolvedStyle.left;
            Offset.y = (canvasHeight - scaledMousePosition.y) - uiWindow.resolvedStyle.top;
            starWidth = uiWindow.resolvedStyle.width;
            starHeight = uiWindow.resolvedStyle.height;
            isResizing = true;
        }

        private void EndDrag(MouseUpEvent mouseUpEvent)
        {
         //   Debug.LogError("EndDrag");
            isResizing = false;
        }

        protected override bool unregisterUI()
        {
            if (!base.unregisterUI())
                return false;

            uiGroupPanel.OnItemIndexChanged -= onGroupPanelChanged;
            uiChatTextField.UnregisterCallback<KeyDownEvent>(onChatSubmitEvent);
            
            
            uiHelpButton.clicked -= CommandHelp;

            for (int n = 0; n < chatPanels.Length; n++)
            {
                if (chatPanels[n].ListView != null)
                {
                    chatPanels[n].ListView.onItemsChosen -= chatPanels[n].OnItemsChosen;
                    chatPanels[n].ListView.onSelectionChange -= chatPanels[n].OnSelectionChange;
                }
            }

            
            
            return true;
        }

        protected override void registerEvents()
        {
            base.registerEvents();

            AtavismEventSystem.RegisterEvent(EVENTS.CHAT_MSG_SERVER, this);
            AtavismEventSystem.RegisterEvent(EVENTS.CHAT_MSG_SAY, this);
            AtavismEventSystem.RegisterEvent(EVENTS.CHAT_MSG_SYSTEM, this);
            AtavismEventSystem.RegisterEvent(EVENTS.ADMIN_MESSAGE, this);

            AtavismEventSystem.RegisterEvent(EVENTS.INVENTORY, this);
            AtavismEventSystem.RegisterEvent(EVENTS.COMBAT, this);
            AtavismEventSystem.RegisterEvent(EVENTS.UPDATE_LANGUAGE, this);
        }

        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent(EVENTS.CHAT_MSG_SERVER, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.CHAT_MSG_SAY, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.CHAT_MSG_SYSTEM, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.ADMIN_MESSAGE, this);

            AtavismEventSystem.UnregisterEvent(EVENTS.INVENTORY, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.COMBAT, this);
            AtavismEventSystem.UnregisterEvent(EVENTS.UPDATE_LANGUAGE, this);

            base.unregisterEvents();
        }

        protected override void registerExtensionMessages()
        {
            base.registerExtensionMessages();

            NetworkAPI.RegisterExtensionMessageHandler(EXT_MSGS.ANNOUNCEMENT, HandleAnnouncementMessage);
        }

        protected override void unregisterExtensionMessages()
        {
            NetworkAPI.RemoveExtensionMessageHandler(EXT_MSGS.ANNOUNCEMENT, HandleAnnouncementMessage);

            base.unregisterExtensionMessages();
        }
        #endregion
        #region UI - events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newValue"></param>
        protected virtual void onGroupPanelChanged(int newValue)
        {
            WindowView = chatPanels[newValue].ChatType;

            switch(WindowView)
            {
                case ChatWindowViewEnum.Chat:
                    ChatChannelCommand = "/say";
                    break;
                case ChatWindowViewEnum.Group:
                    ChatChannelCommand = "/group";
                    break;
                case ChatWindowViewEnum.Guild:
                    ChatChannelCommand = "/guild";
                    break;
            }
            

            if (WindowView == ChatWindowViewEnum.EventLog || WindowView == ChatWindowViewEnum.CombatLog)
            {
                uiChatTextField.visible = false;
            }
            else
            {
                uiChatTextField.visible = true;
                setChatColor(chatPanels[newValue].TextColor);

                string placeholder = chatPanels[newValue].Placeholder;
#if AT_I2LOC_PRESET
                placeholder = I2.Loc.LocalizationManager.GetTranslation(placeholder);
#endif
                setChatPlaceholder(placeholder);
            }
            
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        private void onChatSubmitEvent(KeyDownEvent evt)
        {

            if (evt.keyCode == KeyCode.Return /*|| evt.character == '/n'*/)
            {
                SubmitMessage();
            }
        }

        void SubmitMessage()
        {
            string text = uiChatTextField.text;
            isChatSubmited = true;
            EventSystem.current.SetSelectedGameObject(null); 
            proccessChatSubmit(uiChatTextField.text);
            uiChatTextField.SetValueWithoutNotify("");
        }

        protected virtual void onGeneralChatItemChosen(IEnumerable<object> obj)
        {
        }

        protected virtual void onGeneralChatSelectionChange(IEnumerable<object> obj)
        {
        }
        #endregion
        #region Atavism Events
        protected override void OnEvent(AtavismEventData eData)
        {
            base.OnEvent(eData);

            //   Debug.LogError(eData.eventType + " >" + eData.eventArgs[0] + "<");
            if (eData.eventType == EVENTS.CHAT_MSG_SERVER)
            {
                AddChatMessage("(" + getTime() + ") " + eData.eventArgs[0]);
            }
            else if (eData.eventType == EVENTS.CHAT_MSG_SAY)
            {
                AtavismLogger.LogDebugMessage("Got chat say event with numargs: " + eData.eventArgs.Length);

                proccessChatMessage(eData.eventArgs);
            }
            else if (eData.eventType == EVENTS.CHAT_MSG_SYSTEM)
            {
                AtavismLogger.LogDebugMessage("Got system event with numargs: " + eData.eventArgs.Length);

#if AT_I2LOC_PRESET
                AddChatMessage(getLine("(" + getTime() + ") [" + I2.Loc.LocalizationManager.GetTranslation("Event") + "]: " + eData.eventArgs[0], announcementColor)); //System Event
#else
                AddChatMessage(getLine("(" + getTime() + ")[Event]: " + eData.eventArgs[0], announcementColor)); //System Event
#endif
            }
            else if (eData.eventType == EVENTS.ADMIN_MESSAGE)
            {
                AtavismLogger.LogDebugMessage("Got system event with numargs: " + eData.eventArgs.Length);

#if AT_I2LOC_PRESET
                AddChatMessage(getLine("(" + getTime() + ") [" + I2.Loc.LocalizationManager.GetTranslation("Admin") + "]: " + eData.eventArgs[0], eventColor)); //System Event
                AddGroupMessage(getLine("(" + getTime() + ") [" + I2.Loc.LocalizationManager.GetTranslation("Admin") + "]: " + eData.eventArgs[0], eventColor)); //System Event
                AddGuildMessage(getLine("(" + getTime() + ") [" + I2.Loc.LocalizationManager.GetTranslation("Admin") + "]: " + eData.eventArgs[0], eventColor)); //System Event
#else
                AddChatMessage(getLine("(" + getTime() + ") [Admin]: " + eData.eventArgs[0], eventColor)); //System Event
                AddGroupMessage(getLine("(" + getTime() + ") [Admin]: " + eData.eventArgs[0], eventColor)); //System Event
                AddGuildMessage(getLine("(" + getTime() + ") [Admin]: " + eData.eventArgs[0], eventColor)); //System Event
#endif

            }
            else if (eData.eventType == EVENTS.INVENTORY)
            {
                AtavismLogger.LogDebugMessage("Got inventory event with numargs: " + eData.eventArgs.Length);

                processInventoryEvent(eData.eventArgs);
            }
            else if (eData.eventType == EVENTS.COMBAT)
            {
                AtavismLogger.LogDebugMessage("Got combat event with numargs: " + eData.eventArgs.Length);

                processCombatEvent(eData.eventArgs);
            }
            else if (eData.eventType == EVENTS.UPDATE_LANGUAGE)
            {
                string placeholderText = "";

#if AT_I2LOC_PRESET
            switch (ChatChannelCommand)
            {
                case "/say":
                    placeholderText = I2.Loc.LocalizationManager.GetTranslation("Say") + ":";
                    break;
                case "/group":
                    placeholderText = I2.Loc.LocalizationManager.GetTranslation("Group") + ":";
                    break;
                case "/guild":
                    placeholderText = I2.Loc.LocalizationManager.GetTranslation("Guild") + ":";
                    break;

                case "/1":
                    placeholderText = I2.Loc.LocalizationManager.GetTranslation("Instance General") + ":";
                    break;
                case "/2":
                    placeholderText = I2.Loc.LocalizationManager.GetTranslation("Global") + ":";
                    break;
                case "/admininfo":
                    placeholderText = I2.Loc.LocalizationManager.GetTranslation("Admin") + ":";
                    break;
                default:
                    if (ChatChannelCommand.IndexOf("/whisper") > -1)
                    {
                        string[] splitMessage = ChatChannelCommand.Split(' ');
                        placeholderText = I2.Loc.LocalizationManager.GetTranslation("Whisper") + " " + splitMessage[1] + ":";
                    }
                    break;
            }
#endif
                uiChatTextField.SetPlaceholder(placeholderText);
            }
        }

        /// <summary>
        /// Override text color class style
        /// </summary>
        /// <param name="color"></param>
        private void setChatColor(Color color)
        {
            uiChatTextField.TextInputLabel.style.color = color;
        }

        private void setChatPlaceholder(string placeholder)
        {
            ChatPlaceholder = placeholder;

#if AT_I2LOC_PRESET
            placeholder = I2.Loc.LocalizationManager.GetTranslation(placeholder);
#endif
            uiChatTextField.SetPlaceholder(ChatPlaceholder);
        }
        #endregion
        #region Handlers
        public virtual void HandleAnnouncementMessage(Dictionary<string, object> props)
        {
            string message = "(" + getTime() + ")" + (string)props["AnnouncementText"];
            string line = getLine(message, eventColor);

            if (line.Contains("<link=item#"))
            {
                string s = line.Substring(line.IndexOf("<link=item#") + 11);
                s = s.Substring(0, s.IndexOf(">"));
                int id = Int32.Parse(s);
                Inventory.Instance.GetItemByTemplateID(id);
            }
            if (line.Contains("<link=ability#"))
            {
                string s = line.Substring(line.IndexOf("<link=ability#") + 14);
                s = s.Substring(0, s.IndexOf(">"));
                int id = Int32.Parse(s);
                Abilities.Instance.GetAbility(id);
            }
            if (line.Contains("<size="))
            {
                while (line.Contains("<size="))
                {
                    string fs = line.Substring(0, line.IndexOf("<size"));
                    string s = line.Substring(line.IndexOf("<size"));
                    s = s.Substring(s.IndexOf(">") + 1);
                    line = fs + s;
                }
            }
        }
        #endregion
        #region Loop Updates
        protected override void Update()
        {
            base.Update();

            if (!IsFocused)
            {
                if (Input.GetKeyUp(KeyCode.Return) && !ClientAPI.UIHasFocus())
                {
                    if (!isChatSubmited)
                        FocusOn();
                    isChatSubmited = false;
                }
            }

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                isResizing = false;
            }

            if (isResizing)
            {
                if (uiResizeDrag !=null && uiResizeContainer != null)
                {
                    float canvasWidth = uiDocument.rootVisualElement.resolvedStyle.width;
                    float canvasHeight = uiDocument.rootVisualElement.resolvedStyle.height;
                    float widthScaleFactor = Screen.width / canvasWidth;
                    float heightScaleFactor = Screen.height / canvasHeight;
                    Vector2 scaledMousePosition = new Vector2(Input.mousePosition.x / widthScaleFactor,Input.mousePosition.y/heightScaleFactor );
             //  Debug.LogWarning("Chat resize scaledMousePosition="+scaledMousePosition);
                    Vector2 position = new Vector2(scaledMousePosition.x - Offset.x, (canvasHeight - scaledMousePosition.y) - Offset.y);
                    Vector2 newPosition = new Vector2(Mathf.Clamp(position.x, draggingMinValues.x-starWidth, draggingMaxValues.x),Mathf.Clamp(position.y, draggingMinValues.y, draggingMaxValues.y+starHeight));
                    Vector2 difPosition = (newPosition - startPosition);
                    float width = Mathf.Clamp( starWidth + difPosition.x, resizingMinValues.x, resizingMaxValues.x);
                    float height = Mathf.Clamp( starHeight - difPosition.y, resizingMinValues.y, resizingMaxValues.y);
                //    Debug.Log("Chat resize position="+position+" draggingMinValues="+draggingMinValues+" draggingMaxValues="+draggingMaxValues+" resizingMinValues="+resizingMinValues+" resizingMaxValues="+resizingMaxValues+" width="+width+" height="+height);
                    if(width != resizingMinValues.x || height != resizingMinValues.y)
                        uiWindow.style.top = Mathf.Clamp(position.y, startPosition.y+starHeight-resizingMaxValues.y, startPosition.y+starHeight-resizingMinValues.y);
                    
                   uiWindow.style.width = width;
                   uiWindow.style.height = height;
                   
                       
                       

                }
            }
        }
        #endregion
        #region Public Methods
        public void AddChatMessage(string message) => listofChatPanels[ChatWindowViewEnum.Chat].AddChatMessage(message);
        public void AddGroupMessage(string text) => listofChatPanels[ChatWindowViewEnum.Group].AddChatMessage(text);
        public void AddGuildMessage(string text) => listofChatPanels[ChatWindowViewEnum.Guild].AddChatMessage(text);
        public void AddEventMessage(string text) => listofChatPanels[ChatWindowViewEnum.EventLog].AddChatMessage(text);
        public void AddCombatMessage(string text) => listofChatPanels[ChatWindowViewEnum.CombatLog].AddChatMessage(text);
        public void AddWhisperMessage(string text) => listofChatPanels[ChatWindowViewEnum.Whisper].AddChatMessage(text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetName"></param>
        public void StartWhisper(string targetName)
        { 
            uiChatTextField.Focus();

            string[] splitMessage = targetName.Split(' ');
            if (splitMessage.Length > 1)
                ChatChannelCommand = "/whisper \"" + targetName + "\"";
            else
                ChatChannelCommand = "/whisper " + targetName;

            setChatPlaceholder(listofChatPanels[ChatWindowViewEnum.Whisper].Placeholder + " " + targetName + ":");
            setChatColor(listofChatPanels[ChatWindowViewEnum.Whisper].TextColor);
        }

        /// <summary>
        /// 
        /// </summary>
        public void FocusOn()
        {
          //  Debug.LogError("Chat Focus On");
            StartCoroutine(setFocus());
        }


        IEnumerator setFocus()
        {
            yield return new WaitForSeconds(0.1f);
            uiChatTextField.Focus();
        }
        
        public void FocusOff()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        #endregion
        #region Public Methods - Commands
        /// <summary>
        /// 
        /// </summary>
        public void CommandTest()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("coordEffect", "Emote_wave"); // Put name of Coord Effect Prefab to play here
            props.Add("hasTarget", false);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.PLAY_COORD_EFFECT", props);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CommandNavmeshTest()
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("add", 1);
            //  props.Add("state", MoveToNextState());
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.ADD_NM_OBJECT", props);
        }

/// <summary>
/// 
/// </summary>
/// <param name="on"></param>
        public void commandAbilityDebug(bool on)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("mode", on?1:0);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "combat.DEBUG_ABILITY", props);
            Debug.LogWarning("abilityDebug send");
        }

        public void commandMobDebug(bool on)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("mode", on?1:0);
            NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "ao.DEBUG_MOB", props);
            Debug.LogWarning("combatMobDebug send");
        }
        /// <summary>
        /// 
        /// </summary>
        public void CommandClearChat()
        {
            switch (WindowView)
            {
                case ChatWindowViewEnum.Chat:
                    GetChatLog(ChatWindowViewEnum.Chat).Clear();
                    break;
                case ChatWindowViewEnum.Group:
                    GetChatLog(ChatWindowViewEnum.Group).Clear();
                    break;
                case ChatWindowViewEnum.Guild:
                    GetChatLog(ChatWindowViewEnum.Guild).Clear();
                    break;
                case ChatWindowViewEnum.EventLog:
                    GetChatLog(ChatWindowViewEnum.EventLog).Clear();
                    break;
                case ChatWindowViewEnum.CombatLog:
                    GetChatLog(ChatWindowViewEnum.CombatLog).Clear();
                    break;

                case ChatWindowViewEnum.Whisper:
                    GetChatLog(ChatWindowViewEnum.Whisper).Clear();
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CommandHelp()
        {
#if AT_I2LOC_PRESET
        string[] helps = { I2.Loc.LocalizationManager.GetTranslation("Chat commands:"),
            "/help "+I2.Loc.LocalizationManager.GetTranslation("or")+" /h" + I2.Loc.LocalizationManager.GetTranslation("HelpHelp"),
            "/say "+I2.Loc.LocalizationManager.GetTranslation("or")+" /s " + I2.Loc.LocalizationManager.GetTranslation("HelpSay"),
            "/group "+I2.Loc.LocalizationManager.GetTranslation("or")+" /p " + I2.Loc.LocalizationManager.GetTranslation("HelpGroup"),
            "/guild "+I2.Loc.LocalizationManager.GetTranslation("or")+" /g " + I2.Loc.LocalizationManager.GetTranslation("HelpGuild"),
            "/whisper "+I2.Loc.LocalizationManager.GetTranslation("or")+" /w " + I2.Loc.LocalizationManager.GetTranslation("HelpWhisper"),
            "/1 "+I2.Loc.LocalizationManager.GetTranslation("HelpInstanceGeneral"),
            "/2 "+I2.Loc.LocalizationManager.GetTranslation("HelpGlobal"),
        };
#else
            string[] helps = { "Chat commands:",
            "/help or /h This help",
            "/say or /s Send message localy",
            "/group or /p Send message to Group",
            "/guild or /g Send message to Guild",
            "/whisper or /w \"Player Name\" Send private mesage",
            "/1 Send message to whole instance",
            "/2 Send message Globaly",
        };

#endif
            foreach (string message in helps)
            {
                AddChatMessage(getLine(message, chatColor));
            }
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void proccessChatSubmit(string message)
        {
            string text = message;

            if (!string.IsNullOrEmpty(text))
            {
                if (text.ToLower().Equals("/test"))
                    CommandTest();
                else if (text.ToLower().Equals("/navmeshtest"))
                    CommandNavmeshTest();

                else if (text.Equals("/abilityDebug"))
                {
                    commandAbilityDebug(true);
                    return;
                }else if (text.Equals("/abilityDebugOff"))
                {
                    commandAbilityDebug(false);
                     return;
                } else if (text.Equals("/combatMobDebug"))
                {
                    commandMobDebug(true);
                    return;
                }else if (text.Equals("/combatMobDebugOff"))
                {
                    commandMobDebug(false);
                    return;
                }else
                
                if (text.ToLower().Equals("/help") || text.ToLower().Equals("/help ") || text.ToLower().Equals("/h") || text.ToLower().Equals("/h "))
                    CommandHelp();
                else if (text.StartsWith("/clearChat"))
                    CommandClearChat();
                else if (text.StartsWith("/say ") || text.StartsWith("/s "))
                {
                    listofChatPanels[ChatWindowViewEnum.Chat].Activate();
                }
                else if (text.StartsWith("/group ") || text.StartsWith("/p "))
                {
                    listofChatPanels[ChatWindowViewEnum.Group].Activate();
                }
                else if (text.StartsWith("/guild ") || text.StartsWith("/g "))
                {
                    listofChatPanels[ChatWindowViewEnum.Guild].Activate();
                }
                else if (text.StartsWith("/whisper ") || text.StartsWith("/w "))
                {
                    //listofChatPanels[ChatWindowViewEnum.Whisper].Activate();

                    string[] splitMessage = message.Split(' ');
                    if (splitMessage[1].StartsWith("\""))
                    {
                        splitMessage = message.Split('"');
                        ChatChannelCommand = "/whisper \"" + splitMessage[1] + "\"";
                    }
                    else
                    {
                        ChatChannelCommand = "/whisper " + splitMessage[1];
                    }

                    string placeholderText;
#if AT_I2LOC_PRESET
                    placeholderText = I2.Loc.LocalizationManager.GetTranslation("Whisper") + " " + splitMessage[1] + ":";
#else
                    placeholderText = "Whisper " + splitMessage[1] + ":";
#endif
                    setChatPlaceholder(placeholderText);
                }
                else if (!text.StartsWith("/"))
                {
                    text = ChatChannelCommand + " " + text;
                }

                // Intercept global chat channels here
                if (text.StartsWith("/1 "))
                {
                    ChatChannelCommand = "/1";
                    setChatPlaceholder("Instance General:");
                    currentChannelCommandColor = chatColor;

                    Dictionary<string, object> props = new Dictionary<string, object>();
                    props.Add("channel", 10); // Channels 1-5 are already used, lets try 10+
                    message = text.Substring(text.IndexOf(' '));
                    props.Add("message", message);
                    NetworkAPI.SendExtensionMessage(0, false, "ao.GLOBAL_CHAT", props);

                    return;
                }
                else if (text.StartsWith("/2 "))
                {
                    ChatChannelCommand = "/2";
                    setChatPlaceholder("Global:");
                    currentChannelCommandColor = chatColor;

                    Dictionary<string, object> props = new Dictionary<string, object>();
                    props.Add("channel", -1); // Use -1 for global
                    message = text.Substring(text.IndexOf(' '));
                    props.Add("message", message);
                    NetworkAPI.SendExtensionMessage(0, false, "ao.GLOBAL_CHAT", props);

                    return;
                }
                else if (text.StartsWith("/admininfo "))
                {
                    ChatChannelCommand = "/admininfo";
                    setChatPlaceholder("Admin:");
                    currentChannelCommandColor = chatColor;

                    Dictionary<string, object> props = new Dictionary<string, object>();
                    props.Add("channel", -2); // Use -1 for global
                    message = text.Substring(text.IndexOf(' '));
                    props.Add("message", message);
                    NetworkAPI.SendExtensionMessage(0, false, "ao.GLOBAL_CHAT", props);

                    return;
                } 
                

                // Send to the Atavism Command class
                AtavismCommand.HandleCommand(text);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private void proccessChatMessage(string[] args)
        {
            //Debug.Log("oid of chat sender: " + args[3]);
            if (args[2] == "4")
            {
#if AT_I2LOC_PRESET
            AddChatMessage(getLine("(" + getTime() + ") [" + I2.Loc.LocalizationManager.GetTranslation("Group") + "]: " + args[0], groupColor)); //Group Message
            AddGroupMessage(getLine("(" + getTime() + ") [" + I2.Loc.LocalizationManager.GetTranslation("Group") + "]: " + args[0], groupColor)); //Group Message
#else
                AddChatMessage(getLine("(" + getTime() + ") [Group] " + args[1] + ": " + args[0], groupColor)); //Group Message
                AddGroupMessage(getLine("(" + getTime() + ") [Group] " + args[1] + ": " + args[0], groupColor)); //Group Message
#endif
            }
            else if (args[2] == "5")
            {
#if AT_I2LOC_PRESET
            AddChatMessage(getLine("(" + getTime() + ") [" + I2.Loc.LocalizationManager.GetTranslation("Guild") + "] " + args[1] + ": " + args[0], guildColor)); //Guild Message
            AddGuildMessage(getLine("(" + getTime() + ") [" + I2.Loc.LocalizationManager.GetTranslation("Guild") + "] " + args[1] + ": " + args[0], guildColor)); //Guild Message
#else
                AddChatMessage(getLine("(" + getTime() + ") [Guild] " + args[1] + ": " + args[0], guildColor)); //Guild Message
                AddGuildMessage(getLine("(" + getTime() + ") [Guild] " + args[1] + ": " + args[0], guildColor)); //Guild Message
#endif
            }
            else if (args[2] == "1")
            {
                AddChatMessage(getLine("(" + getTime() + ") <link=user#" + args[1] + ">[" + args[1] + "]</link>: " + args[0], chatColor)); //Chat Message
            }
            else if (args[2] == "6")
            {
                if (listofChatPanels.ContainsKey(ChatWindowViewEnum.Whisper))
                    if (listofChatPanels[ChatWindowViewEnum.Whisper].IsInitialized)
                        listofChatPanels[ChatWindowViewEnum.Whisper].AddChatMessage(getLine("(" + getTime() + ") <link=user#" + args[1] + ">[" + args[1] + "]</link>: " + args[0], whisperColor), maxHistory);
                AddChatMessage(getLine("(" + getTime() + ") <link=user#" + args[1] + ">[" + args[1] + "]</link>: " + args[0], whisperColor)); //Whisper Message
            }
            else if (int.Parse(args[2]) >= 10)
            {
                AddChatMessage(getLine("(" + getTime() + ") <link=user#" + args[1] + ">[" + args[1] + "]</link>:" + args[0], instanceColor)); //Some Other Message
            }
            else if (int.Parse(args[2]) == -1)
            {
                AddChatMessage(getLine("(" + getTime() + ") <link=user#" + args[1] + ">[" + args[1] + "]</link>:" + args[0], globalColor)); //Global chat Message
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private void processInventoryEvent(string[] args)
        {
            string message = "";

            if (args[0] == "ItemHarvested")
            {
                AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(int.Parse(args[1]));
#if AT_I2LOC_PRESET
                message = getLine("(" + getTime() + ")" + I2.Loc.LocalizationManager.GetTranslation("Received") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name) + " x" + args[2],announcementColor);
#else
                message = getLine("(" + getTime() + ")" + "Received " + item.name + " x" + args[2], announcementColor);
#endif
                AddEventMessage(message);
            }
            else if (args[0] == "ItemLooted")
            {
                AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(int.Parse(args[1]));

#if AT_I2LOC_PRESET
                message = getLine(I2.Loc.LocalizationManager.GetTranslation("Received") + " " + I2.Loc.LocalizationManager.GetTranslation("Items/" + item.name) + " x" + args[2], announcementColor);
#else
                message = getLine("Received " + item.name + " x" + args[2], announcementColor);
#endif
                AddEventMessage(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private void processCombatEvent(string[] args)
        {
            string casterName = getObjectName(args[1]);
            string targetName = getObjectName(args[2]);

#if AT_I2LOC_PRESET
        if (I2.Loc.LocalizationManager.GetTranslation("Mobs/" + casterName) != "") casterName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + casterName);
        if (I2.Loc.LocalizationManager.GetTranslation("Mobs/" + targetName) != "") targetName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + targetName);
#endif
            string message = "(" + getTime() + ") ";
            if (args[0] == "CombatDamage")
            {
#if AT_I2LOC_PRESET
            message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("hit") + " " + targetName + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + args[3] + " " + I2.Loc.LocalizationManager.GetTranslation(args[8])+ " " + I2.Loc.LocalizationManager.GetTranslation("damage") + ".";
#else
                message = casterName + " hit " + targetName + " for " + args[3] + " " + args[8] + " damage.";
#endif
            }
            /*  else if (args[0] == "CombatMagicalDamage")
              {
  #if AT_I2LOC_PRESET
              message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("hit") + " " + targetName + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + args[3] + " " + I2.Loc.LocalizationManager.GetTranslation("magical damage") + ".";
  #else
                  message = casterName + " hit " + targetName + " for " + args[3] + " magical damage.";
  #endif
              }*/
            else if (args[0] == "CombatDamageCritical")
            {
#if AT_I2LOC_PRESET
            message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("hit") + " " + targetName + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + args[3] + " " + I2.Loc.LocalizationManager.GetTranslation(args[8])+ " " + I2.Loc.LocalizationManager.GetTranslation("critical damage") + ".";
#else
                message = casterName + " hit " + targetName + " for " + args[3] + " " + args[8] + " critical damage.";
#endif
            }
            /* else if (args[0] == "CombatMagicalCritical")
             {
 #if AT_I2LOC_PRESET
             message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("hit") + " " + targetName + " " + I2.Loc.LocalizationManager.GetTranslation("for") + " " + args[3] + " " + I2.Loc.LocalizationManager.GetTranslation("magical damage") + ".";
 #else
                 message = casterName + " hit " + targetName + " for " + args[3] + " critical magical damage.";
 #endif
             }*/
            else if (args[0] == "CombatBuffGained")
            {
#if AT_I2LOC_PRESET
            message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("gained") + " " + args[3] + ".";
#else
                message = targetName + " gained " + args[3] + ".";
#endif
            }
            else if (args[0] == "CombatBuffLost")
            {
                //   string effectName = "effect";
#if AT_I2LOC_PRESET
            message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("lost") + " " + args[3] + ".";
#else
                message = targetName + " lost " + args[3] + ".";
#endif
            }
            else if (args[0] == "CombatHeal")
            {
                //    string effectName = "effect";
                if (args[7] == AtavismCombat.Instance.HealthStat)
                {

#if AT_I2LOC_PRESET
                    message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("healed by") + " " + args[3] + ".";
#else
                    message = casterName + " healed by " + args[3] + ".";
#endif
                }
                else
                {

#if AT_I2LOC_PRESET
                    message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("restore")+ " " + I2.Loc.LocalizationManager.GetTranslation(args[7])+ " " + I2.Loc.LocalizationManager.GetTranslation("by") + " " + args[3] + ".";
#else
                    message = casterName + " restore " + args[7] + " by " + args[3] + ".";
#endif
                }

            }
            else if (args[0] == "CombatHealCritical")
            {
                //    string effectName = "effect";
                if (args[7] == AtavismCombat.Instance.HealthStat)
                {

#if AT_I2LOC_PRESET
                    message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("healed by") + " " + args[3] + ".";
#else
                    message = casterName + " healed by " + args[3] + ".";
#endif
                }
                else
                {

#if AT_I2LOC_PRESET
                    message = casterName + " " + I2.Loc.LocalizationManager.GetTranslation("restore")+ " " + I2.Loc.LocalizationManager.GetTranslation(args[7])+ " " + I2.Loc.LocalizationManager.GetTranslation("by") + " " + args[3] + ".";
#else
                    message = casterName + " restore " + args[7] + " by " + args[3] + ".";
#endif
                }

            }
            else if (args[0] == "CombatExpGained")
            {
                //    string effectName = "effect";
#if AT_I2LOC_PRESET
            message = I2.Loc.LocalizationManager.GetTranslation("You have received") + " " + args[3] +" "+ I2.Loc.LocalizationManager.GetTranslation("experience points") + ".";
#else
                message = "You have received " + args[3] + " experience points.";
#endif
            }
            else if (args[0] == "CombatMissed")
            {
#if AT_I2LOC_PRESET
              
                message =casterName+" "+ I2.Loc.LocalizationManager.GetTranslation("Missed");
#else
                message = casterName + " has Missed";
#endif
            }
            else if (args[0] == "CombatDodged")
            {
#if AT_I2LOC_PRESET
            
                message = targetName+" "+I2.Loc.LocalizationManager.GetTranslation("have Dodged");
#else
                message = targetName + " have Dodged";
#endif
            }
            else if (args[0] == "CombatBlocked")
            {
#if AT_I2LOC_PRESET
            
                message = targetName+" "+I2.Loc.LocalizationManager.GetTranslation("have Blocked");

#else
                message = targetName + " have Blocked";
#endif
            }
            else if (args[0] == "CombatParried")
            {
#if AT_I2LOC_PRESET
            
                message = targetName+" "+I2.Loc.LocalizationManager.GetTranslation("have Parried");

#else
                message = targetName + " have Parried";
#endif
            }
            else if (args[0] == "CombatEvaded")
            {
#if AT_I2LOC_PRESET
           
                message =targetName+" "+ I2.Loc.LocalizationManager.GetTranslation("have Evaded");
#else
                message = targetName + " have Evaded";
#endif
            }
            else if (args[0] == "CombatImmune")
            {
#if AT_I2LOC_PRESET
            
                message = targetName+" "+I2.Loc.LocalizationManager.GetTranslation("is Immune");
#else
                message = targetName + " is Immune";
#endif
            }
            else
            {
                Debug.Log("no message type " + args[0]);
            }

            AddCombatMessage(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        private string getObjectName(string oid)
        {
            if (oid.Length > 0)
                if (OID.fromString(oid).ToLong() == ClientAPI.GetPlayerOid())
                {
#if AT_I2LOC_PRESET
            return I2.Loc.LocalizationManager.GetTranslation("You");
#else
                    return "You";
#endif
                }
                else
                {
                    AtavismObjectNode caster = ClientAPI.GetObjectNode(OID.fromString(oid).ToLong());
                    if (caster != null)
                        return caster.Name;
                }
            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private string getLine(string message, Color color)
        {
            return "<color=" + ColorTypeConverter.ToRGBHex(color) + ">" + message + "</color>";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string getTime()
        {
            string hour = DateTime.Now.Hour.ToString(); //= TimeManager.Instance.Hour.ToString();
            if (DateTime.Now.Hour < 10)
            {
                hour = "0" + hour;
            }
            string minute = DateTime.Now.Minute.ToString();//= TimeManager.Instance.Minute.ToString();
            if (DateTime.Now.Minute < 10)
            {
                minute = "0" + minute;
            }
            return hour + ":" + minute;
        }
        #endregion
    }
}