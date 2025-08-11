using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Atavism.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismCircularMenu : MonoBehaviour
    {
        #region Fields

        public UIDocument uiDocument;
        public static UIAtavismCircularMenu Instance;

        [Serializable]
        public class UICustomCategory
        {
            public string Name;
            public string ContentName = "";
            private VisualElement content;

            public VisualElement Content
            {
                get
                {
                    if (content == null)
                        content =
                            UIAtavismCircularMenu.Instance.uiDocument.rootVisualElement.Q<VisualElement>(ContentName);
                    return content;
                }
            }

            public List<UIAtavismCircularButtonData> Buttons = new List<UIAtavismCircularButtonData>();
            public List<UIAtavismCircularButton> InstancedButtons = new List<UIAtavismCircularButton>();
        }

        public enum ControllerType
        {
            KeyboardAndTouch,
            Gamepad
        }

        public ControllerType Controller = ControllerType.KeyboardAndTouch;

        public KeyCode OpenCircularKey = KeyCode.Tab;

        public int DefaultCategoryIndex;

        public List<UICustomCategory> Categories = new List<UICustomCategory>();

        public UIFillerElement Selection;
        public VisualElement SelectionIcon;
        public Sprite PreviousIcon;
        public Sprite NextIcon;
        public Sprite ReturnIcon;
        public Label SelectionText;

        public Label SelectionDescription;
        // public Color ButtonNormalColor;
        // public Color ButtonHoverColor;
        // public Color ButtonPressedColor;

        public VisualTreeAsset CircularButton;
        public float ButtonSpacing = 160f;

        public string GamepadInputOpenName = "Open_Circular";
        public string GamepadInputAxisX = "Mouse X";
        public string GamepadInputAxisY = "Mouse Y";

        // public Animator Animator;
        // public string ShowStateName;
        // public string HideStateName;

        [HideInInspector] public UIAtavismCircularButton SelectedButton;

        [HideInInspector] public UICustomCategory CurrentCategory;

        public bool IsActive = false;

        private readonly List<float> ButtonsRotation = new List<float>();
        private int Elements;
        private float Fill;
        public int DisplayLimit = 10;
        private int page = 0;
        private VisualElement screen;

        #endregion

        #region Methods

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {

            RegisterUI();


            UpdateList();
            AtavismEventSystem.RegisterEvent("CLAIM_CHANGED", this);

            ChangeCategory(Categories[0].Name);
            Hide();
        }

        private void RegisterUI()
        {

            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;

            screen = uiDocument.rootVisualElement.Q<VisualElement>("Screen");
            Selection = uiDocument.rootVisualElement.Q<UIFillerElement>("Circular-Selection");
            SelectionIcon = uiDocument.rootVisualElement.Q<VisualElement>("Circular-Selection-Icon");
            // public VisualElement PreviousIcon = UIDocument.rootVisualElement.Q<VisualElement>("PreviousIcon");
            // public VisualElement NextIcon = 
            // public VisualElement ReturnIcon;
            SelectionText = uiDocument.rootVisualElement.Q<Label>("Circular-Selection-Name");
            SelectionDescription = uiDocument.rootVisualElement.Q<Label>("Circular-Selection-Description");
            Selection.RegisterCallback<GeometryChangedEvent>(onChangeGeometry);
            screen.RegisterCallback<GeometryChangedEvent>(onChangeGeometry);

        }



        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("CLAIM_CHANGED", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
            //  UpdateList();
        }

        private void UpdateList()
        {
            //   Debug.LogError("UpdateList");
            if (BuildManager.Instance != null)
            {
                if (BuildManager.Instance.Pieces != null)
                {
                    //  Debug.LogError("UpdateList | "+Categories[1].Buttons.Count);

                    Categories[1].Buttons.Clear();
                    //  Debug.LogError("UpdateList || "+Categories[1].Buttons.Count);
                    int index = 0;
                    int indexMax = 0;
                    for (int x = 0; x < BuildManager.Instance.Pieces.Count; x++)
                    {

                        if (WorldBuilder.Instance.ActiveClaim != null)
                            if (!WorldBuilder.Instance
                                    .GetBuildObjectTemplate(BuildManager.Instance.Pieces[x].BuildObjDefId)
                                    .onlyAvailableFromItem &&
                                (WorldBuilder.Instance.ActiveClaim.claimType ==
                                 WorldBuilder.Instance.GetAnyClaimType() ||
                                 WorldBuilder.Instance
                                     .GetBuildObjectTemplate(BuildManager.Instance.Pieces[x].BuildObjDefId)
                                     .validClaimTypes.Contains(WorldBuilder.Instance.GetAnyClaimType()) ||
                                 WorldBuilder.Instance
                                     .GetBuildObjectTemplate(BuildManager.Instance.Pieces[x].BuildObjDefId)
                                     .validClaimTypes.Contains(WorldBuilder.Instance.ActiveClaim.claimType)
                                ))
                            {
                                if (index < DisplayLimit && page * DisplayLimit < x)
                                {

                                    UIAtavismCircularButtonData cbd = new UIAtavismCircularButtonData()
                                    {
                                        Icon = BuildManager.Instance.Pieces[x].Icon,
                                        Order = index,
                                        Text = BuildManager.Instance.Pieces[x].Name,
                                        Description = BuildManager.Instance.Pieces[x].Description,
                                        Action = new UnityEvent()
                                    };
                                    string n = BuildManager.Instance.Pieces[x].Name;
                                    cbd.Action.AddListener(delegate { ChangePiece(n); });
                                    Categories[1].Buttons.Add(cbd);
                                    index++;
                                }

                                indexMax++;
                            }
                    }

                    if (page > 0)
                    {
                        UIAtavismCircularButtonData cbd_prew = new UIAtavismCircularButtonData()
                        {
                            Icon = PreviousIcon,
                            Order = BuildManager.Instance.Pieces.Count,
                            Text = "Prev",
                            Description = "Previous page",
                            Action = new UnityEvent()
                        };
                        cbd_prew.Action.AddListener(delegate { ChangePagePrev(); });
                        Categories[1].Buttons.Add(cbd_prew);
                    }

                    if (DisplayLimit == index && page * DisplayLimit + index < indexMax)
                    {
                        UIAtavismCircularButtonData cbd_next = new UIAtavismCircularButtonData()
                        {
                            Icon = NextIcon,
                            Order = BuildManager.Instance.Pieces.Count,
                            Text = "Next",
                            Description = "Next Page",
                            Action = new UnityEvent()
                        };
                        cbd_next.Action.AddListener(delegate { ChangePageNext(); });
                        Categories[1].Buttons.Add(cbd_next);
                    }

                    UIAtavismCircularButtonData cbd_return = new UIAtavismCircularButtonData()
                    {
                        Icon = ReturnIcon,
                        Order = BuildManager.Instance.Pieces.Count,
                        Text = "Back",
                        Description = "Return to menu",
                        Action = new UnityEvent()
                    };
                    cbd_return.Action.AddListener(delegate { ChangeCategory("Main"); });
                    Categories[1].Buttons.Add(cbd_return);

                }
            }
            //  Debug.LogError("UpdateList ||| "+Categories[1].Buttons.Count+" "+ Categories[1].InstancedButtons.Count);

            for (int i = 0; i < Categories.Count; i++)
            {
                if (Categories[i].Content != null)
                {
                    Categories[i].Content.Clear();
                    // foreach (UIAtavismCircularButton go in Categories[i].Content.GetComponentsInChildren<CircularButton>(true).ToList())
                    // {
                    //         GameObject.DestroyImmediate(go.gameObject);
                    // }
                    Categories[i].InstancedButtons.Clear();
                    Categories[i].Buttons = Categories[i].Buttons.OrderBy(o => o.Order).ToList();
                    // Debug.LogError("buttons "+i+" "+Categories[i].Buttons.Count+" "+Categories[i].InstancedButtons.Count);
                    for (int x = 0; x < Categories[i].Buttons.Count; x++)
                    {
                        VisualElement v = CircularButton.CloneTree();
                        UIAtavismCircularButton button = new UIAtavismCircularButton();
                        button.SetVisualElement(v);
                        Categories[i].Content.Add(button.m_Root);
                        //.c, Categories[i].Content.transform);
                        button.Init(Categories[i].Buttons[x].Text, Categories[i].Buttons[x].Description,
                            Categories[i].Buttons[x].Icon, Categories[i].Buttons[x].Action);
                        Categories[i].InstancedButtons.Add(button);
                    }
                    // Debug.LogError("UpdateList |V "+Categories[1].Buttons.Count+" "+ Categories[1].InstancedButtons.Count);
                    // Categories[i].InstancedButtons = Categories[i].Content.GetComponentsInChildren<UIAtavismCircularButton>(true).ToList();
                    // Debug.LogError("UpdateList V "+Categories[1].Buttons.Count+" "+ Categories[1].InstancedButtons.Count);

                }
            }
            //  Debug.LogError("UpdateList V| "+Categories[1].Buttons.Count+" "+ Categories[1].InstancedButtons.Count);

        }

        public void ChangePagePrev()
        {
            page--;
            UpdateList();
            RefreshButtons();
        }

        public void ChangePageNext()
        {
            page++;
            UpdateList();
            RefreshButtons();
        }



        private void Update()
        {
            if (!Application.isPlaying)
                return;

            
            // if (screen != null)
            // {
            //     Vector2 op = screen.resolvedStyle.scale.value;
            //     //  Debug.LogError("Update " + op);
            //     if (IsActive && op.magnitude < 1f)
            //     {
            //         screen.FadeScaleVisualElement(Vector2.one, 15f);
            //
            //     }
            //     else if (!IsActive && op.magnitude > 0f)
            //     {
            //         screen.FadeScaleVisualElement(Vector2.zero, 30f);
            //     }
            // }
          //  Debug.LogError("Game is in play mode "+WorldBuilder.Instance.BuildingState);
            if (WorldBuilder.Instance.BuildingState == WorldBuildingState.None)
                return;
         //   Debug.LogError("Game is in play mode | ");
            if (Input.GetMouseButtonDown(1))
                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
         //   Debug.LogError("Game is in play mode ||");

            if (Controller == ControllerType.KeyboardAndTouch
                    ? Input.GetKeyDown(OpenCircularKey)
                    : Input.GetButtonDown(GamepadInputOpenName))
                Show();
            else if (Controller == ControllerType.KeyboardAndTouch
                         ? Input.GetKeyUp(OpenCircularKey)
                         : Input.GetButtonUp(GamepadInputOpenName))
                Hide();

         //   Debug.LogError("Game is in play mode |||");
          

            if (!IsActive)
                return;

       //     Debug.LogError("Game is in play mode |V");

            Selection.value = Mathf.Lerp(Selection.value, Fill, .2f);

            Vector3 BoundsScreen = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
            Vector3 RelativeBounds = Input.mousePosition - BoundsScreen;

            float CurrentRotation = ((Controller == ControllerType.KeyboardAndTouch)
                ? Mathf.Atan2(RelativeBounds.x, RelativeBounds.y)
                : Mathf.Atan2(UnityEngine.Input.GetAxis(GamepadInputAxisX),
                    UnityEngine.Input.GetAxis(GamepadInputAxisY))) * 57.29578f;

            if (CurrentRotation < 0f)
                CurrentRotation += 360f;

            _ = -(CurrentRotation - Selection.value * 360f / 2f);

            float Average = 9999;

            UIAtavismCircularButton Nearest = null;

            for (int i = 0; i < Elements; i++)
            {
                UIAtavismCircularButton InstancedButton = CurrentCategory.InstancedButtons[i];
                InstancedButton.m_Root.style.scale = Vector2.one;
                float Rotation = Convert.ToSingle(InstancedButton.m_Root.name);

                if (Mathf.Abs(Rotation - CurrentRotation) < Average)
                {
                    Nearest = InstancedButton;
                    Average = Mathf.Abs(Rotation - CurrentRotation);
                }
            }

            SelectedButton = Nearest;
            float CursorRotation = (Convert.ToSingle(SelectedButton.m_Root.name) - Selection.value * 360f / 2f);
            var rot = Selection.style.rotate.value;
            rot.angle = Mathf.SmoothStep(rot.angle.value, CursorRotation, 30f * Time.deltaTime);
            Selection.style.rotate = rot;

            for (int i = 0; i < Elements; i++)
            {
                UIAtavismCircularButton Button = CurrentCategory.InstancedButtons[i];

                if (Button != SelectedButton)
                {
                    if (Button.Icon.ClassListContains("builder-circle-icon-selected"))
                        Button.Icon.RemoveFromClassList("builder-circle-icon-selected");
                }
                else
                {
                    if (!Button.Icon.ClassListContains("builder-circle-icon-selected"))
                        Button.Icon.AddToClassList("builder-circle-icon-selected");
                }
            }

            SelectionIcon.SetBackgroundImage(SelectedButton.Icon.resolvedStyle.backgroundImage.sprite);
            SelectionText.text = SelectedButton.Text;
            SelectionDescription.text = SelectedButton.Description;

            if (Input.GetMouseButtonUp(0))
            {
                if (SelectedButton.Icon.ClassListContains("builder-circle__button-click"))
                    SelectedButton.Icon.RemoveFromClassList("builder-circle__button-click");
                SelectedButton.Icon.RegisterCallback<TransitionEndEvent>(onButtonClicked);
                SelectedButton.Icon.AddToClassList("builder-circle__button-click");
                // if (SelectedButton.GetComponent<CircularButton>().GetComponent<Animator>() != null)
                // SelectedButton.GetComponent<CircularButton>().GetComponent<Animator>().Play("Button Press");
                SelectedButton.Action.Invoke();
            }

            RefreshButtons();
        }

        void onButtonClicked(TransitionEndEvent evt)
        {
        //    Debug.LogError("Button clicked onButtonClicked");
            SelectedButton.Icon.UnregisterCallback<TransitionEndEvent>(onButtonClicked);
            if (SelectedButton.Icon.ClassListContains("builder-circle__button-click"))
                SelectedButton.Icon.RemoveFromClassList("builder-circle__button-click");
        }


        private void onChangeGeometry(GeometryChangedEvent evt)
        {
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            Elements = CurrentCategory.InstancedButtons.Count;
            ButtonsRotation.Clear();
            if (Elements > 0)
            {
                Fill = 1f / (float)Elements;

                float FillRadius = Fill * 360f;
                float LastRotation = 0;

                for (int i = 0; i < Elements; i++)
                {
                    UIAtavismCircularButton Temp = CurrentCategory.InstancedButtons[i];

                    float Rotate = LastRotation + FillRadius / 2;
                    LastRotation = Rotate + FillRadius / 2;
                    var pos = new Vector2(ButtonSpacing * Mathf.Cos((90 - Rotate) * Mathf.Deg2Rad),
                        -ButtonSpacing * Mathf.Sin((90 - Rotate) * Mathf.Deg2Rad));
                    var cWidth = CurrentCategory.Content.resolvedStyle.width;
                    var cHeight = CurrentCategory.Content.resolvedStyle.height;
                    var bWidth = Temp.m_Root.resolvedStyle.width;
                    var bHeight = Temp.m_Root.resolvedStyle.height;
                    pos += new Vector2(cWidth / 2 - bWidth / 2, cHeight / 2 - bHeight / 2);
                    Temp.m_Root.transform.position = pos;
                    Temp.m_Root.style.scale = Vector2.one;

                    if (Rotate > 360)
                        Rotate -= 360;

                    Temp.m_Root.name = Rotate.ToString();

                    ButtonsRotation.Add(Rotate);
                }
            }
        }

        /// <summary>
        /// This method allows to change of category by name.
        /// </summary>
        public void ChangeCategory(string name)
        {
            DefaultCategoryIndex = Categories.ToList().FindIndex(entry => entry.Content.name == name);

            if (DefaultCategoryIndex == -1)
                return;

            CurrentCategory = Categories[DefaultCategoryIndex];

            for (int i = 0; i < Categories.Count; i++)
            {
                if (Categories[i].Content != null)
                {
                    if (i != DefaultCategoryIndex)
                    {
                        Categories[i].Content.UnregisterCallback<GeometryChangedEvent>(onChangeGeometry);
                        foreach (var button in Categories[i].InstancedButtons)
                        {
                            if (button.Icon.ClassListContains("builder-circle__button-click"))
                                button.Icon.RemoveFromClassList("builder-circle__button-click");
                        }
                      
                        Categories[i].Content.HideVisualElement();
                    }
                    else
                    {
                        Categories[i].Content.RegisterCallback<GeometryChangedEvent>(onChangeGeometry);
                        foreach (var button in Categories[i].InstancedButtons)
                        {
                            if (button.Icon.ClassListContains("builder-circle__button-click"))
                                button.Icon.RemoveFromClassList("builder-circle__button-click");
                        }
                        Categories[i].Content.ShowVisualElement();
                    }
                }
            }

            RefreshButtons();
        }

        /// <summary>
        /// This method allows to upgrade the targeted piece.
        /// </summary>
        public void ChangeAppearance(int appearanceIndex)
        {
            PieceBehaviour TargetPiece = BuilderBehaviour.Instance.GetTargetedPart();

            if (TargetPiece != null)
                TargetPiece.ChangeAppearance(appearanceIndex);
        }

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(string modeName)
        {
            BuilderBehaviour.Instance.ChangeMode(modeName);
        }

        /// <summary>
        /// This method allows to pass in placement mode with a piece name.
        /// </summary>
        public void ChangePiece(string name)
        {
            UIAtavismCircularMenu.Instance.Hide();

            //Call none to reset the previews.
            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);

            BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.GetPieceByName(name));
            BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
        }

        /// <summary>
        /// This method allows to show the circular menu.
        /// </summary>
        protected void Show()
        {
            // if (Animator)
                // Animator.CrossFade(ShowStateName, 0.1f);

   // Debug.LogError("Show");
            if (AtavismSettings.Instance && !AtavismSettings.Instance.isWindowOpened())
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            screen.RemoveFromClassList("builder-circle__hide");
            screen.AddToClassList("builder-circle__show");

            // screen.ShowVisualElement();
            IsActive = true;
            UpdateList();
            RefreshButtons();
        }

        /// <summary>
        /// This method allows to close the circular menu.
        /// </summary>
        protected void Hide()
        {
           // Debug.LogError("Hide");

            // if (Animator)
                // Animator.CrossFade(HideStateName, 0.1f);

            // Cursor.lockState = CursorLockMode.Locked;

            screen.RemoveFromClassList("builder-circle__show");
            screen.AddToClassList("builder-circle__hide");
            if (AtavismSettings.Instance && !AtavismSettings.Instance.isWindowOpened())
            {
                Cursor.visible = false;
            }


            // screen.HideVisualElement();
            IsActive = false;
        }

        #endregion
    }
}