using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    [Serializable]
    public class SkillTypeButton
    {
        public int typeId = 0;
        public Button Button;
        public Label ButtonText;

    }
    public class UIAtavismSkillsWindow : UIAtavismWindowBase 
    {
        [SerializeField] bool talents = false;
        [SerializeField] private VisualTreeAsset skillListElementTemplate;
        [SerializeField] private VisualTreeAsset abilityListElementTemplate;
        [SerializeField] private VisualTreeAsset craftListElementTemplate;
        public bool autoFillAbilityList = true;
        public bool onlyShowClassSkills = false;
        public bool onlyShowKnownSkills = false;
        public bool showMaxLevel = true;
        public VisualElement skillIcon;
        public Label skillName;
        public Label skillLevel;
        public Label skillExp;
        public UIProgressBar skillExpSlider;
        public VisualElement skillExpFill;
        public Button increaseButton;
        public Label costText;
        public Label pointsText;
        public List<UIAtavismSkillButton> skillButtons = new List<UIAtavismSkillButton>();
        public List<UIAtavismAbilityEntry> abilitiesList = new List<UIAtavismAbilityEntry>();
        List<Skill> activeSkills = new List<Skill>();
        List<Skill> activeCrftSkills = new List<Skill>();
        int selectedSkill = 0;
        public VisualElement tabSpells;
        public VisualElement tabCraftSpells;


        private UIButtonToggleGroup menu;
        private ListView abilityList;
        private ListView skillList;

          // public Text tabSpellsText;
          // public Text tabCraftSpellsText;
        //public GameObject tabGatheringSpells;
         public List<UIAtavismCraftSkillSlot> craftSlots = new List<UIAtavismCraftSkillSlot>();
         private ListView craftSkillList;
       // [SerializeField] bool skillNoAbilityToCraft = false;
        [SerializeField] List<SkillTypeButton> skillTypeButtons = new List<SkillTypeButton>();
        [SerializeField] Color buttonMenuSelectedColor = Color.green;
        [SerializeField] Color buttonMenuNormalColor = Color.white;
        [SerializeField] Color buttonMenuSelectedTextColor = Color.black;
        [SerializeField] Color buttonMenuNormalTextColor = Color.black;

        int type = -1;
        bool withAbilities = true;
        [SerializeField] int defaultSkillType = 1;
        // Use this for initialization
        // void Start()
        // {
        //     if (titleBar != null)
        //     {
        //         titleBar.SetPanelTitle(ClientAPI.GetPlayerObject().Name);
        //     }
        // }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateSkills();
            
        }

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("ABILITY_UPDATE", OnEvent);
            AtavismEventSystem.RegisterEvent("SKILL_UPDATE", OnEvent);
            AtavismEventSystem.RegisterEvent("SKILL_ICON_UPDATE", OnEvent);
            
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("ABILITY_UPDATE", OnEvent);
            AtavismEventSystem.UnregisterEvent("SKILL_UPDATE", OnEvent);
            AtavismEventSystem.UnregisterEvent("SKILL_ICON_UPDATE", OnEvent);
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            
            skillIcon = uiWindow.Q<VisualElement>("icon");
            skillName = uiWindow.Q<Label>("skill-name");
            skillLevel = uiWindow.Q<Label>("skill-level");
            increaseButton = uiWindow.Q<Button>("levelup-button");
            if (increaseButton != null)
                increaseButton.clicked += IncreaseSkill;
            pointsText = uiWindow.Q<Label>("points-text");
            costText = uiWindow.Q<Label>("cost-text");
           
            skillExpSlider = uiWindow.Q<UIProgressBar>("progress");
            skillList = uiWindow.Query<ListView>("skill-list");
            if (skillList != null)
            {
#if UNITY_6000_0_OR_NEWER    
                ScrollView scrollView = skillList.Q<ScrollView>();
                scrollView.mouseWheelScrollSize = 19;
#endif
                skillList.makeItem = () =>
                {
                    // Instantiate a controller for the data
                    UIAtavismSkillButton newListEntryLogic = new UIAtavismSkillButton();
                    // Instantiate the UXML template for the entry
                    var newListEntry = skillListElementTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = newListEntryLogic;
                    // Initialize the controller script
                    newListEntryLogic.SetVisualElement(newListEntry);
                    skillButtons.Add(newListEntryLogic);
                    // Return the root of the instantiated visual tree
                    return newListEntry;
                };
                skillList.bindItem = (item, index) =>
                {
                    var entry = (item.userData as UIAtavismSkillButton);
                    Skill skill = activeSkills[index];
                    entry.SetSkillData(skill, this, index, selectedSkill);
                };
            }

            abilityList = uiWindow.Query<ListView>("ability-list");
            if (abilityList != null)
            {
#if UNITY_6000_0_OR_NEWER    
                ScrollView scrollView = abilityList.Q<ScrollView>();
                scrollView.mouseWheelScrollSize = 19;
#endif
                abilityList.makeItem = () =>
                {
                    // Instantiate a controller for the data
                    UIAtavismAbilityEntry newListEntryLogic = new UIAtavismAbilityEntry();
                    // Instantiate the UXML template for the entry
                    var newListEntry = abilityListElementTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = newListEntryLogic;
                    // Initialize the controller script
                    newListEntryLogic.SetVisualElement(newListEntry);
                    abilitiesList.Add(newListEntryLogic);
                    // Return the root of the instantiated visual tree
                    return newListEntry;
                };
                abilityList.bindItem = (item, index) =>
                {
                    var entry = (item.userData as UIAtavismAbilityEntry);
                    AtavismAbility ab = Abilities.Instance.GetAbility(activeSkills[selectedSkill].abilities[index]);
                    entry.UpdateAbilityData(ab);
                };
            }

            craftSkillList = uiWindow.Query<ListView>("crafting-list");
            if (craftSkillList != null)
            {
#if UNITY_6000_0_OR_NEWER    
                ScrollView scrollView = craftSkillList.Q<ScrollView>();
                scrollView.mouseWheelScrollSize = 19;
#endif
                craftSkillList.makeItem = () =>
                {
                    // Instantiate a controller for the data
                    UIAtavismCraftSkillSlot newListEntryLogic = new UIAtavismCraftSkillSlot();
                    // Instantiate the UXML template for the entry
                    var newListEntry = craftListElementTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = newListEntryLogic;
                    // Initialize the controller script
                    newListEntryLogic.SetVisualElement(newListEntry);
                    craftSlots.Add(newListEntryLogic);
                    // Return the root of the instantiated visual tree
                    return newListEntry;
                };
                craftSkillList.bindItem = (item, index) =>
                {
                    var entry = (item.userData as UIAtavismCraftSkillSlot);
                    Skill skill = activeCrftSkills[index];
                    entry.UpdateDisplay(skill);
                };
            }
            // skillList.selectionChanged += SelectSkill;
            tabSpells = uiWindow.Query<VisualElement>("skill-tab");
            tabCraftSpells = uiWindow.Query<VisualElement>("other-tab");


            menu = uiWindow.Q<UIButtonToggleGroup>("menu");
            if(menu!=null)
                menu.OnItemIndexChanged += TopMenuChange;
            
            return true;
        }

        private void TopMenuChange(int obj)
        {
            // Debug.LogError("TopMenuChange "+obj);
            switch (obj)
            {
                case 0:
                    ShowSkillsAbility(1);
                    break;
                case 1:
                    ShowSkills(0);
                    break;
                case 2:
                    ShowSkills(2);
                    break;
            }
        }

        void OnDisable()
        {
            // Delete the old list
            // ClearAllCells();
        }

        void OnDestroy()
        {
          
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "ABILITY_UPDATE" || eData.eventType == "SKILL_UPDATE"|| eData.eventType == "SKILL_ICON_UPDATE")
            {
                // Update 
                UpdateSkills();
            }
        }

        public void SelectSkill(int skillPos)
        {
            selectedSkill = skillPos;
            UpdateSkills();
        }

        public void IncreaseSkill()
        {
            Skills.Instance.IncreaseSkill(activeSkills[selectedSkill].id);
        }

        public void ResetSkill()
        {
            Skills.Instance.ResetSkills(talents);
        }

      

        
        
        public void UpdateSkills()
        {
            if (ClientAPI.GetPlayerObject() != null)
                if (ClientAPI.GetPlayerObject().PropertyExists("aspect"))
                {
                    int classID = (int)ClientAPI.GetPlayerObject().GetProperty("aspect");
                    // Get list of skills
                    activeSkills.Clear();
                    activeCrftSkills.Clear();
                    if (onlyShowKnownSkills)
                    {
                        foreach (Skill skill in Skills.Instance.PlayerSkills.Values)
                        {
                            if ((!onlyShowClassSkills && !skill.mainAspectOnly) || skill.mainAspect == classID)
                            {
                               // Debug.LogError("UGUISkillsWindow UpdateSkills skills=" +skill.skillname +  " type=" + skill.type );

                                if (skill.type == type || type==-1)
                                {
                                    if (withAbilities)
                                    {

                                        if (talents && skill.talent)
                                            activeSkills.Add(skill);
                                        else if (!talents && !skill.talent)
                                            activeSkills.Add(skill);
                                    }
                                    else
                                    {
                                     //   Debug.LogError("UGUISkillsWindow UpdateSkills skills=" + skill.skillname + " type=" + skill.type+ " add activeSkills");
                                        activeCrftSkills.Add(skill);
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        foreach (Skill skill in Skills.Instance.SkillsList.Values)
                        {
                            if ((!onlyShowClassSkills && !skill.mainAspectOnly) || skill.mainAspect == classID)
                            {
                                if (skill.type == type || type == -1)
                                {
                                    if (withAbilities)
                                    {
                                        if (talents && skill.talent)
                                            activeSkills.Add(skill);
                                        else if (!talents && !skill.talent)
                                            activeSkills.Add(skill);
                                    }
                                    else
                                    {
                                    //    Debug.LogError("UGUISkillsWindow UpdateSkills skills=" + skill.skillname + " type=" + skill.type + " add activeSkills");
                                        activeCrftSkills.Add(skill);
                                    }
                                }
                            }
                        }
                    }

                //    Debug.LogError("UGUISkillsWindow UpdateSkills skills=" + activeSkills.Count + " craft=" + activeCrftSkills.Count + " type=" + type + " " + withAbilities);

           
                    // Update skill info
                    if (skillIcon != null)
                    {
                        if (activeSkills.Count == 0)
                        {
                            skillIcon.visible = false;
                        }
                        else
                        {
                            skillIcon.visible = true;
                            skillIcon.style.backgroundImage = activeSkills[selectedSkill].Icon.texture;
                        }
                    }
                    if (skillName != null)
                    {
                        if (activeSkills.Count == 0)
                            skillName.text = "";
                        else
                            skillName.text = activeSkills[selectedSkill].skillname;
                    }
                    if (skillLevel != null)
                    {
                        if (activeSkills.Count > selectedSkill && Skills.Instance.PlayerSkills.ContainsKey(activeSkills[selectedSkill].id))
                        {
                            Skill playerSkill = Skills.Instance.PlayerSkills[activeSkills[selectedSkill].id];
                            if (showMaxLevel)
                            {
                                skillLevel.text = playerSkill.CurrentLevel + "/" + playerSkill.MaximumLevel;
                            }
                            else
                            {
                                skillLevel.text = playerSkill.CurrentLevel.ToString();
                            }
                        }
                        else
                        {
                            skillLevel.text = "-";
                        }
                    }
                   
                    if (activeSkills.Count > selectedSkill && Skills.Instance.PlayerSkills.ContainsKey(activeSkills[selectedSkill].id))
                    {
                       // Skill _skill = Skills.Instance.GetSkillByID(activeSkills[selectedSkill].id);
                        Skill _skillPly = Skills.Instance.PlayerSkills[activeSkills[selectedSkill].id];
                        if (skillExpSlider != null)
                        {
                              skillExpSlider.visible = true;
                            if (_skillPly.expMax == 0 && _skillPly.exp == 0)
                            {
                                skillExpSlider.highValue = _skillPly.MaximumLevel;
                                skillExpSlider.value = _skillPly.CurrentLevel;
                            }
                            else
                            {
                                skillExpSlider.highValue = _skillPly.expMax;
                                skillExpSlider.value = _skillPly.exp;
                            }
                        }
                        // if (skillExpFill != null)
                        // {
                        //     skillExpFill.fillAmount = (float)(((float)_skillPly.exp) / (float)_skillPly.expMax);
                        // }

                        if (skillExp != null)
                        {
                            if (_skillPly.expMax == 0 && _skillPly.exp == 0)
                            {
                                skillExp.text = _skillPly.CurrentLevel + "/" + _skillPly.MaximumLevel;
                            }
                            else
                            {
                                skillExp.text = _skillPly.exp + "/" + _skillPly.expMax;
                            }
                        }

                        //  Debug.LogError("updateRecipeList " + _skillPly);
                        //    Debug.LogError("updateRecipeList " + _skillPly.exp + " " + _skillPly.expMax);
                    }
                    else
                    {
                        if (skillExpSlider != null)
                        {
                            skillExpSlider.visible = false;
                        }
                        if (skillExp != null)
                        {
                            skillExp.text = "";
                        }
                    }


                    if (costText != null)
                    {
                        if (activeSkills.Count > selectedSkill)
                        {
                            if (Skills.Instance.PlayerSkills.ContainsKey(activeSkills[selectedSkill].id))
                            {
                                Skill playerSkill = Skills.Instance.PlayerSkills[activeSkills[selectedSkill].id];
                                int cost = playerSkill.pcost;
                                if (playerSkill.mainAspect == classID)
                                    cost = playerSkill.pcost;
                                if (playerSkill.oppositeAspect == classID)
                                    cost = playerSkill.pcost * 2;
                                costText.text = cost.ToString();
                            }
                            else
                            {
                                Skill _skill = Skills.Instance.SkillsList[activeSkills[selectedSkill].id];
                                int cost = _skill.pcost;
                                if (_skill.mainAspect == classID)
                                    cost = _skill.pcost;
                                if (_skill.oppositeAspect == classID)
                                    cost = _skill.pcost * 2;
                                costText.text = cost.ToString();
                            }
                        }
                        else
                        {
                            costText.text = "";
                        }
                    }
                   
                    if (pointsText != null)
                    {
                        if (talents)
                            pointsText.text = Skills.Instance.CurrentTalentPoints.ToString();
                        else
                            pointsText.text = Skills.Instance.CurrentSkillPoints.ToString();
                    }
                  //  Debug.LogError("Skill "+craftSlots.Length+" "+ activeCrftSkills.Count);
                  RefreshCraftSkillList();

                    if (autoFillAbilityList)
                    {
                        // Delete the old list
                        RefreshSkillList();
                    }
                    if (activeSkills.Count > 0)
                    {
                        RefreshAbilityList();
                        // abilitiesList.UpdateAbilities(activeSkills[selectedSkill]);
                    }
                }
        }

        private void RefreshSkillList()
        {
            skillList.itemsSource = activeSkills;
            skillList.Rebuild();
            skillList.selectedIndex = -1;
        }

        private void RefreshAbilityList()
        {
            if (abilityList != null)
            {
                abilityList.itemsSource = activeSkills[selectedSkill].abilities;
                abilityList.Rebuild();
                abilityList.selectedIndex = -1;
            }
        }

        private void RefreshCraftSkillList()
        {
            if (craftSkillList != null)
            {
                craftSkillList.itemsSource = activeCrftSkills;
                craftSkillList.Rebuild();
                craftSkillList.selectedIndex = -1;
            }
        }

        void Update()
        {
            base.Update();
            if ((Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().skills.key) || Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().skills.altKey) )&& !ClientAPI.UIHasFocus())
            {
                if (showing)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }

            if (abilitiesList != null)
            {
                foreach (var ab in abilitiesList)
                {
                    if(ab!=null)
                        ab.Update();
                }
            }
        }
        public override void Show()
        {
            base.Show();
            // AtavismSettings.Instance.OpenWindow(this);
            this.UpdateSkills();
            //   gameObject.SetActive(true);
            AtavismUIUtility.BringToFront(gameObject);
            ShowSkillsAbility(defaultSkillType);
        }
        public override void Hide()
        {
            base.Hide();
           // AtavismSettings.Instance.CloseWindow(this);
        }

        public void ShowSkillsAbility(int type)
        {
            this.type = type;
            withAbilities = true;
            if (tabSpells != null)
                tabSpells.ShowVisualElement();
            if (tabCraftSpells != null)
                tabCraftSpells.HideVisualElement();

            // foreach (SkillTypeButton stb in skillTypeButtons)
            // {
            //     if (stb.typeId == type)
            //     {
            //         if (stb.Button)
            //             stb.Button.targetGraphic.color = buttonMenuSelectedColor;
            //         if (stb.ButtonText)
            //             stb.ButtonText.color = buttonMenuSelectedTextColor;
            //     }
            //     else
            //     {
            //         if(stb.Button)
            //             stb.Button.targetGraphic.color = buttonMenuNormalColor;
            //         if (stb.ButtonText)
            //             stb.ButtonText.color = buttonMenuNormalTextColor;
            //     }
            // }

          /*  if (SkillButton)
                SkillButton.targetGraphic.color = buttonMenuSelectedColor;
            if (SkillButtonText)
                SkillButtonText.color = buttonMenuSelectedTextColor;
            if (CraftSkillButton)
                CraftSkillButton.targetGraphic.color = buttonMenuNormalColor;
            if (CraftSkillButtonText)
                CraftSkillButtonText.color = buttonMenuNormalTextColor;*/
            UpdateSkills();
        }

        public void ShowSkills(int type)
        {
            this.type = type;
            withAbilities = false;

            // foreach (SkillTypeButton stb in skillTypeButtons)
            // {
            //     if (stb.typeId == type)
            //     {
            //         if (stb.Button)
            //             stb.Button.targetGraphic.color = buttonMenuSelectedColor;
            //         if (stb.ButtonText)
            //             stb.ButtonText.color = buttonMenuSelectedTextColor;
            //     }
            //     else
            //     {
            //         if (stb.Button)
            //             stb.Button.targetGraphic.color = buttonMenuNormalColor;
            //         if (stb.ButtonText)
            //             stb.ButtonText.color = buttonMenuNormalTextColor;
            //     }
            // }

            if (tabSpells != null)
                tabSpells.HideVisualElement();
            if (tabCraftSpells != null)
                tabCraftSpells.ShowVisualElement();

         /*   if (SkillButton)
                SkillButton.targetGraphic.color = buttonMenuNormalColor;
            if (SkillButtonText)
                SkillButtonText.color = buttonMenuNormalTextColor;
            if (CraftSkillButton)
                CraftSkillButton.targetGraphic.color = buttonMenuSelectedColor;
            if (CraftSkillButtonText)
                CraftSkillButtonText.color = buttonMenuSelectedTextColor;*/
            UpdateSkills();
        }

      /*  public void ShowGatherSkills()
        {
            if (tabSpells != null)
                tabSpells.SetActive(false);
            if (tabCraftSpells != null)
                tabCraftSpells.SetActive(true);
            if (SkillButton)
                SkillButton.targetGraphic.color = buttonMenuNormalColor;
            if (SkillButtonText)
                SkillButtonText.color = buttonMenuNormalTextColor;
            if (CraftSkillButton)
                CraftSkillButton.targetGraphic.color = buttonMenuSelectedColor;
            if (CraftSkillButtonText)
                CraftSkillButtonText.color = buttonMenuSelectedTextColor;
        }
        */


      

      
    }
}