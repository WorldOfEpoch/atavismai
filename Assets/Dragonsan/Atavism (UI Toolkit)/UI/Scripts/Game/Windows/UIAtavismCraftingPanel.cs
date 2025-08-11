using UnityEngine;
using System.Collections.Generic;
using Atavism.UI.Game;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismCraftingPanel : UIAtavismWindowBase, IPointerDownHandler
    {
        [SerializeField] VisualTreeAsset recipeListTemplate;
        [SerializeField] VisualTreeAsset slotTemplate;

        [SerializeField] private int numberRequireSlots = 14;
        [SerializeField] private int numberResultSlotsPerRow = 4;

        public List<UIAtavismCraftingSlot> craftingSlots = new List<UIAtavismCraftingSlot>();
        public List<UIAtavismItemDisplay> resultSlots = new List<UIAtavismItemDisplay>();
        public Button craftButton;
        
        // [SerializeField] GameObject panel;
        [AtavismSeparator("Craft Book")]
        [SerializeField]bool newCraft = true;
        // [SerializeField] VisualTreeAsset craftPrefab;
        private VisualElement requireList;
        private VisualElement resultList;
        private VisualElement craftingSlotsList1;
        private VisualElement craftingSlotsList2;
        private VisualElement craftingSlotsList3;
        private VisualElement craftingSlotsList4;
        
        // ScrollView recipeGrid;
        List<UIAtavismCraftRecipeSlot> recipeSlots = new List<UIAtavismCraftRecipeSlot>();
        List<int> recipies = new List<int>();
        List<UIAtavismCraftingSlot> requiredItemSlots = new List<UIAtavismCraftingSlot>();
        List<UIAtavismCraftingSlot> craftingSlots1= new List<UIAtavismCraftingSlot>();
        List<UIAtavismCraftingSlot> craftingSlots2= new List<UIAtavismCraftingSlot>();
        List<UIAtavismCraftingSlot> craftingSlots3= new List<UIAtavismCraftingSlot>();
        List<UIAtavismCraftingSlot> craftingSlots4= new List<UIAtavismCraftingSlot>();
        UITextField inputCount;
        private Button minButton;
        private Button maxButton;
        private Button increseButton;
        private Button decreseButton;
        Label stationName;
        Label skillName;
        Label skillExp;
        UIProgressBar skillExpSlider;
        Toggle availableToggle;
        Toggle onlyBackpackToggle;
        UIDropdown skillList;

        // private UIProgressBar levelBar;
        private VisualElement panelToHide;
        private ListView recipeGrid;
        // [SerializeField] List<VisualElement> hideObjects;
        AtavismCraftingRecipe recipeSelected;
        int selectSkill = 0;

        // Use this for initialization
        protected override void Start()
        {
          base.Start();
            if (inputCount != null)
                inputCount.value = 1 + "";
           
            Hide();
        }

        protected override bool registerUI()
        { 
            // Debug.LogError("registerUI ");
            if (!base.registerUI())
            return false;
            // Debug.LogError("registerUI |");
            if (newCraft)
            {
                // Debug.LogError("registerUI ||");
                skillList = uiWindow.Query<UIDropdown>("skills");
                // skillList.RegisterCallback<ChangeEvent<int>>(fffff);
                if (skillList != null) skillList.Screen = uiScreen;
                skillExpSlider = uiWindow.Query<UIProgressBar>("skill-level-bar");
                recipeGrid = uiWindow.Query<ListView>("recipe-list");
#if UNITY_6000_0_OR_NEWER    
                ScrollView scrollView = recipeGrid.Q<ScrollView>();
                scrollView.mouseWheelScrollSize = 19;
#endif
                availableToggle = uiWindow.Query<Toggle>("toggle-only-available");
                if (availableToggle != null) availableToggle.RegisterValueChangedCallback(ShowAvailable);
                onlyBackpackToggle = uiWindow.Query<Toggle>("toggle-only-backpack");
                // onlyBackpackToggle.RegisterValueChangedCallback(only);
                requireList = uiWindow.Q<VisualElement>("require-items-grid");

                for (int i = 0; i < numberRequireSlots; i++)
                {
                    UIAtavismCraftingSlot script = new UIAtavismCraftingSlot();
                    // Instantiate the UXML template for the entry
                    var newListEntry = slotTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = script;
                    // Initialize the controller script
                    script.SetVisualElement(newListEntry);
                    script.allowOverwrite =  false;
                    requiredItemSlots.Add(script);
                    requireList.Add(newListEntry);
                }

                craftingSlotsList1 = uiWindow.Q<VisualElement>("highest-grid");
                //craftingSlots1
                for (int i = 0; i < numberResultSlotsPerRow; i++)
                {
                    UIAtavismCraftingSlot script = new UIAtavismCraftingSlot();
                    // Instantiate the UXML template for the entry
                    var newListEntry = slotTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = script;
                    // Initialize the controller script
                    script.SetVisualElement(newListEntry);
                    script.allowOverwrite =  false;
                    craftingSlots1.Add(script);
                    craftingSlotsList1.Add(newListEntry);
                }

                craftingSlotsList2 = uiWindow.Q<VisualElement>("high-grid");
                //craftingSlots2
                for (int i = 0; i < numberResultSlotsPerRow; i++)
                {
                    UIAtavismCraftingSlot script = new UIAtavismCraftingSlot();
                    // Instantiate the UXML template for the entry
                    var newListEntry = slotTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = script;
                    // Initialize the controller script
                    script.SetVisualElement(newListEntry);
                    script.allowOverwrite =  false;
                    craftingSlots2.Add(script);
                    craftingSlotsList2.Add(newListEntry);
                }

                craftingSlotsList3 = uiWindow.Q<VisualElement>("low-grid");
                //craftingSlots3
                for (int i = 0; i < numberResultSlotsPerRow; i++)
                {
                    UIAtavismCraftingSlot script = new UIAtavismCraftingSlot();
                    // Instantiate the UXML template for the entry
                    var newListEntry = slotTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = script;
                    // Initialize the controller script
                    script.SetVisualElement(newListEntry);
                    script.allowOverwrite =  false;
                    craftingSlots3.Add(script);
                    craftingSlotsList3.Add(newListEntry);
                }
                Debug.LogError("registerUI ||");
                craftingSlotsList4 = uiWindow.Q<VisualElement>("lowest-grid");
                //craftingSlots4
                for (int i = 0; i < numberResultSlotsPerRow; i++)
                {
                    UIAtavismCraftingSlot script = new UIAtavismCraftingSlot();
                    // Instantiate the UXML template for the entry
                    var newListEntry = slotTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = script;
                    // Initialize the controller script
                    script.SetVisualElement(newListEntry);
                    script.allowOverwrite =  false;
                    craftingSlots4.Add(script);
                    craftingSlotsList4.Add(newListEntry);
                }

                Debug.LogError("Craft ");
                minButton = uiWindow.Q<Button>("min-button");
                if (minButton != null)
                    minButton.clicked += CountDecreaseMax;
                decreseButton = uiWindow.Q<Button>("decrease-button");
                if (decreseButton != null)
                    decreseButton.clicked += CountDecrease;

                increseButton = uiWindow.Q<Button>("increase-button");
                if (increseButton != null)
                    increseButton.clicked += CountIncrease;
                maxButton = uiWindow.Q<Button>("max-button");
                if (maxButton != null)
                    maxButton.clicked += CountIncreaseMax;
                inputCount = uiWindow.Q<UITextField>("crafting-count-textfield");

                panelToHide = uiWindow.Q<VisualElement>("element-to-hide");
                Debug.LogError("Hide hidden?=> "+panelToHide);
                panelToHide.HiddenVisualElement();
                Debug.LogError("Hide hidden");


            }
            else
            {
                requireList = uiWindow.Q<VisualElement>("require-items-grid");
                requireList.Clear();
                for (int i = 0; i < numberRequireSlots; i++)
                {
                    UIAtavismCraftingSlot script = new UIAtavismCraftingSlot();
                    // Instantiate the UXML template for the entry
                    var newListEntry = slotTemplate.Instantiate();
                    // Assign the controller script to the visual element
                    newListEntry.userData = script;
                    // Initialize the controller script
                    script.SetVisualElement(newListEntry);
                    script.slotNum = i;
                    script.allowOverwrite = true;
                    craftingSlots.Add(script);
                    requireList.Add(newListEntry);
                }
                resultList = uiWindow.Q<VisualElement>("result-grid");
                resultList.Clear();
                for (int i = 0; i < numberResultSlotsPerRow; i++)
                {
                    UIAtavismItemDisplay it = new UIAtavismItemDisplay();
                    resultSlots.Add(it);
                    resultList.Add(it);
                }
            }
            craftButton = uiWindow.Q<Button>("craft-button");
            if (craftButton != null)
                craftButton.clicked += DoCraft;
            return true;
        }

        protected override bool unregisterUI()
        {
            return true;
        }

        protected override void registerEvents()
        {
            AtavismEventSystem.RegisterEvent("CRAFTING_GRID_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CRAFTING_RECIPE_UPDATE", this);
            AtavismEventSystem.RegisterEvent("CRAFTING_START", this);
            AtavismEventSystem.RegisterEvent("CLOSE_CRAFTING_STATION", this);
            AtavismEventSystem.RegisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.RegisterEvent("SKILL_UPDATE", this);
            AtavismEventSystem.RegisterEvent("SKILL_ICON_UPDATE", this);
            if (ClientAPI.GetPlayerObject() != null)
            {
                if (ClientAPI.GetPlayerObject().GameObject != null)
                {
                    if (ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>() != null)
                    {
                        ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("recipes", HandleRecipe);
                    }
                    else
                    {
                        Debug.LogError("UIAtavismCraftingBookPanel: AtavismNode is null");
                    }
                }
                else
                {
                    Debug.LogError("UIAtavismCraftingBookPanel: GameObject is null");
                }
            }
            else
            {
                Debug.LogError("UIAtavismCraftingBookPanel: PlayerObject is null");
            }
            if (ClientAPI.GetPlayerObject() != null)
            {
                if (ClientAPI.GetPlayerObject().PropertyExists("recipes"))
                {
                    recipies.Clear();
                    LinkedList<object> recipeList = (LinkedList<object>)ClientAPI.GetPlayerObject().GetProperty("recipes");
                    foreach (string s in recipeList)
                    {
                        recipies.Add(int.Parse(s));
                    }
                }
            }

        }
        protected override void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("CRAFTING_GRID_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CRAFTING_RECIPE_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("CRAFTING_START", this);
            AtavismEventSystem.UnregisterEvent("CLOSE_CRAFTING_STATION", this);
            AtavismEventSystem.UnregisterEvent("INVENTORY_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("SKILL_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("SKILL_ICON_UPDATE", this);
            if (ClientAPI.GetPlayerObject() != null && ClientAPI.GetPlayerObject().GameObject != null && ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>() != null)
            {
                ClientAPI.GetPlayerObject().GameObject.GetComponent<AtavismNode>().RemoveObjectPropertyChangeHandler("recipes", HandleRecipe);
            }
        }

        
        protected override void OnEnable()
        {
            base.OnEnable();

        }


        private void HandleRecipe(object sender, PropertyChangeEventArgs args)
        {
            if (ClientAPI.GetPlayerObject() != null)
            {
                if (ClientAPI.GetPlayerObject().PropertyExists("recipes"))
                {
                    recipies.Clear();
                    LinkedList<object> recipeList = (LinkedList<object>)ClientAPI.GetPlayerObject().GetProperty("recipes");
                    foreach (string s in recipeList)
                    {
                        recipies.Add(int.Parse(s));
                    }
                }
            }
            updateRecipeList();
        }

        // public void Toggle()
        // {
        //
        //     if (showing)
        //         Hide();
        //     else
        //         Show();
        //
        // }

        public override void Show()
        {
            base.Show();
         //   AtavismSettings.Instance.OpenWindow(this);
         //  ;

             UpdateCraftingGrid();
            if (!newCraft)
                AtavismCursor.Instance.SetUIActivatableClickedOverride(PlaceCraftingItem);
            AtavismUIUtility.BringToFront(gameObject);
            if (newCraft)
            {
                updateRecipeList();
            }
        }

        public override void Hide()
        {
            base.Hide();
          //  AtavismSettings.Instance.CloseWindow(this);
         
            // Set all referenced items back to non referenced
            for (int i = 0; i < craftingSlots.Count; i++)
            {
                if (craftingSlots[i] != null)
                    craftingSlots[i].ResetSlot();
            }
            recipeSelected = null;
            HideObjects();
            Crafting.Instance.ClearGrid();
            Crafting.Instance.StationType = "";
            Crafting.Instance.Station = null;

            if (AtavismCursor.Instance != null)
                AtavismCursor.Instance.ClearUIActivatableClickedOverride(PlaceCraftingItem);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CRAFTING_GRID_UPDATE")
            {
                // Update 
                // UpdateCraftingGrid();
            }
            else if (eData.eventType == "CRAFTING_START")
            {
                if (!showing)
                {
                    Show();
                }
            }
            else if (eData.eventType == "CLOSE_CRAFTING_STATION")
            {
                Hide();
            }
            else if (eData.eventType == "CRAFTING_RECIPE_UPDATE" || eData.eventType == "INVENTORY_UPDATE" || eData.eventType == "SKILL_UPDATE"|| eData.eventType == "SKILL_ICON_UPDATE")
            {
                if (showing && newCraft)
                {
                    updateRecipeList();
                    if (recipeSelected != null)
                        selectRecipe(recipeSelected);
                }
            }

        }

        public void CountDecrease()
        {
            if (recipeSelected == null)
                return;
            string count = inputCount.text;
            if (count == "" || count == " ")
                count = "1";
            int _count = int.Parse(count);
            _count -= 1;
            if (_count < 1)
                _count = 1;
            inputCount.value = _count.ToString();
        }

        public void CountDecreaseMax()
        {
            inputCount.value = "1";
        }

        public void CountIncrease()
        {
            if (recipeSelected == null)
                return;
            string count = inputCount.text;
            if (count == "" || count == " ")
                count = "1";
            int _count = int.Parse(count);
            _count += 1;
            int number = 100000;
            int _countCraft = 0;
            for (int i = 0; i < recipeSelected.itemsReq.Count; i++)
            {
                int num = Inventory.Instance.GetCountOfItem(recipeSelected.itemsReq[i]);
                _countCraft = num / recipeSelected.itemsReqCounts[i];
                if (number > _countCraft)
                    number = _countCraft;
            }
            if (_count > number)
                _count = number;
            inputCount.value = _count.ToString();
        }

        public void CountIncreaseMax()
        {
            if (recipeSelected == null)
                return;
            int number = 100000;
            int _countCraft = 0;
            for (int i = 0; i < recipeSelected.itemsReq.Count; i++)
            {
                int num = Inventory.Instance.GetCountOfItem(recipeSelected.itemsReq[i]);
                _countCraft = num / recipeSelected.itemsReqCounts[i];
                if (number > _countCraft)
                    number = _countCraft;
            }
            inputCount.value = number.ToString();
        }

        public void ShowAvailable(ChangeEvent<bool> evt)
        {
            updateRecipeList();
        }

        public void ChangeSkill()
        {
            selectSkill = skillList.Index;
            updateRecipeList();
        }

        void HideObjects()
        {
            for (int i = 0; i < requiredItemSlots.Count; i++)
            {
                if (requiredItemSlots[i] != null)
                    requiredItemSlots[i].HideVisualElement();
                requiredItemSlots[i].UpdateCraftingBookSlotData(null);
            }
            for (int i = 0; i < craftingSlots1.Count; i++)
            {
                if (craftingSlots1[i] != null)
                    craftingSlots1[i].HideVisualElement();
                craftingSlots1[i].UpdateCraftingBookSlotData(null);
            }
            for (int i = 0; i < craftingSlots2.Count; i++)
            {
                if (craftingSlots2[i] != null)
                    craftingSlots2[i].HideVisualElement();
                craftingSlots2[i].UpdateCraftingBookSlotData(null);
            }
            for (int i = 0; i < craftingSlots3.Count; i++)
            {
                if (craftingSlots3[i] != null)
                    craftingSlots3[i].HideVisualElement();
                craftingSlots3[i].UpdateCraftingBookSlotData(null);
            }
            for (int i = 0; i < craftingSlots4.Count; i++)
            {
                if (craftingSlots4[i] != null)
                    craftingSlots4[i].HideVisualElement();
                craftingSlots4[i].UpdateCraftingBookSlotData(null);
            }

            if (panelToHide != null)
                panelToHide.HiddenVisualElement();
            Debug.LogError("Hide hidden");
        }


        void updateRecipeList()
        {
            if (ClientAPI.GetPlayerObject().PropertyExists("recipes"))
            {
                recipies.Clear();
                LinkedList<object> recipeList = (LinkedList<object>)ClientAPI.GetPlayerObject().GetProperty("recipes");
                foreach (string s in recipeList)
                {
                    recipies.Add(int.Parse(s));
//                    Debug.Log("Know Recipe "+s);
                }
            }

            if (stationName != null)
            {
#if AT_I2LOC_PRESET
                stationName.text = I2.Loc.LocalizationManager.GetTranslation(Crafting.Instance.StationType);
#else
                stationName.text = Crafting.Instance.StationType;
#endif
            }

            if (skillName != null)
            {
#if AT_I2LOC_PRESET
                skillName.text = I2.Loc.LocalizationManager.GetTranslation(Crafting.Instance.StationType);
#else
                skillName.text = Crafting.Instance.StationType;
#endif
            }

            List<int> skills = Crafting.Instance.GetShowAllSkills ?Crafting.Instance.GetShowAllKnownSkills? Skills.Instance.GetAllKnownCraftSkillsID() : Skills.Instance.GetAllCraftSkillsID() : Inventory.Instance.GetCraftingRecipeMatch(recipies, Crafting.Instance.StationType);
            //            Debug.LogError("Crafting skils list "+skills+" "+skills.Count);
            if (skillList == null)
                return;

//             if (skillList.options != null && skillList.options.Count > 0)
//                 skillList.options.Clear();
//             if(skills.Count>0)
            List<string> skillNames = new List<string>();
            foreach (int sid in skills)
            {
                Skill skill = Skills.Instance.GetSkillByID(sid);
                if (skill != null)
                {
#if AT_I2LOC_PRESET
               skillNames.Add(I2.Loc.LocalizationManager.GetTranslation(skill.skillname));
#else
                    skillNames.Add(skill.skillname);
#endif
                }
            }
            skillList.Options(skillNames);
//
//
//
//             if (skills.Count > selectSkill)
//                 skillList.Index = selectSkill;
//             else
//                 skillList.Index = 0;
//             if (skillList.options.Count > skillList.Index)
//                 skillList.captionText.text = skillList.options[skillList.Index].text;
//             else
//                 skillList.captionText.text = "";
//             if (skillList.options.Count == 0)
//             {
//                 foreach (UGUICraftRecipeSlot uguias in recipeSlots)
//                 {
//                     if (uguias != null)
//                         uguias.gameObject.SetActive(false);
//                 }
//                 if (skillExpSlider != null)
//                 {
//                     skillExpSlider.SetEnabled(false);
//                 }
//                 if (skillExp != null)
//                 {
//                     skillExp.text = "";
//                 }
//                 return;
//             }


            Skill _skill = Skills.Instance.GetSkillByID(skills[skillList.Index]);
            if (Skills.Instance.PlayerSkills.ContainsKey(skills[skillList.Index]))
            {
                Skill _skillPly = Skills.Instance.PlayerSkills[skills[skillList.Index]];
                if (skillExpSlider != null)
                {
                    if (!skillExpSlider.enabledInHierarchy)
                        skillExpSlider.SetEnabled(true);
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
                    skillExpSlider.highValue = 1;
                    skillExpSlider.value = 0;
                    skillExpSlider.SetEnabled(false);
                }
                if (skillExp != null)
                {
                    skillExp.text = "Not learned";
                }
            }
            //TODO
            // foreach (UIAtavismCraftRecipeSlot uguias in recipeSlots)
            // {
            //     if (uguias != null)
            //         uguias.gameObject.SetActive(false);
            // }
            int i = 1;
            //   Debug.LogError(recipies.Count);
            // foreach (AtavismCraftingRecipe recipe in Inventory.Instance.GetCraftingRecipeMatch(recipies, _skill.id))
            // {
            //     //   Debug.LogError("i:" + i + " recipe:" + recipe + "  recipe ID:" + recipe.recipeID);
            //     if (i > recipeSlots.Count)
            //     {
            //         GameObject go = (GameObject)Instantiate(craftPrefab, recipeGrid);
            //         recipeSlots.Add(go.GetComponent<UIAtavismCraftRecipeSlot>());
            //     }
            //     if (!recipeSlots[i - 1].gameObject.activeSelf)
            //         recipeSlots[i - 1].gameObject.SetActive(true);
            //     recipeSlots[i - 1].SetDetale(recipe, selectRecipe, availableToggle, onlyBackpackToggle, recipeSelected);
            //     i++;
            // }
            if (recipeSelected != null)
            {
                int number = 100000;
                int _countCraft = 0;
                for (int iii = 0; iii < recipeSelected.itemsReq.Count; iii++)
                {
                    int num = Inventory.Instance.GetCountOfItem(recipeSelected.itemsReq[iii]);
                    _countCraft = num / recipeSelected.itemsReqCounts[iii];
                    if (number > _countCraft)
                        number = _countCraft;
                }

                if (number > 0)
                {
                    if (Crafting.Instance.StationType != "" && recipeSelected != null && (recipeSelected.stationReq.Equals("Any") || recipeSelected.stationReq.Equals(Crafting.Instance.StationType)))
                    {
                        craftButton.SetEnabled(true);
                    }
                    else if (recipeSelected != null && recipeSelected.stationReq.Equals("none"))
                    {
                        craftButton.SetEnabled(true);
                    }
                    else
                    {
                        craftButton.SetEnabled(false);
                    }
                }
                else
                {
                    craftButton.SetEnabled(false);
                }
            }
            else
            {
                craftButton.SetEnabled(false);

            }

        }






        private void selectRecipe(AtavismCraftingRecipe recipe)
        {
            recipeSelected = recipe;
         //   Debug.LogError(" recipe:" + recipe + "  recipe ID:" + recipe.recipeID);
         //   Debug.LogError(" itemsReq:" + recipe.itemsReq.Count);

            // foreach (GameObject go in hideObjects)
            // {
            //     if (go != null)
            //         if (!go.activeSelf)
            //             go.SetActive(true);
            // }
         Debug.LogError("Show hidden");
            if (panelToHide != null)
                panelToHide.VisibleVisualElement();



            for (int i = 0; i < requiredItemSlots.Count; i++)
            {
                if (i < recipe.itemsReq.Count && Inventory.Instance.GetItemByTemplateID(recipe.itemsReq[i]) != null)
                {
                    CraftingComponent cc = new CraftingComponent();
                    cc.item = Inventory.Instance.GetItemByTemplateID(recipe.itemsReq[i]);
                    cc.count = recipe.itemsReqCounts[i];
                    if (requiredItemSlots[i] != null)
                        requiredItemSlots[i].ShowVisualElement();
                    requiredItemSlots[i].UpdateCraftingBookSlotData(cc);
                }
                else
                {
                    if (requiredItemSlots[i] != null)
                        requiredItemSlots[i].HideVisualElement();
                    requiredItemSlots[i].UpdateCraftingBookSlotData(null);
                }
            }


            for (int i = 0; i < craftingSlots1.Count; i++)
            {
                if (i < recipe.createsItems.Count && Inventory.Instance.GetItemByTemplateID(recipe.createsItems[i]) != null)
                {
                    CraftingComponent cc = new CraftingComponent();
                    cc.item = Inventory.Instance.GetItemByTemplateID(recipe.createsItems[i]);
                    cc.count = recipe.createsItemsCounts[i];
                    if (craftingSlots1[i] != null)
                        craftingSlots1[i].ShowVisualElement();
                    craftingSlots1[i].UpdateCraftingBookSlotData(cc);
                }
                else
                {
                    if (craftingSlots1[i] != null)
                        craftingSlots1[i].HideVisualElement();
                    craftingSlots1[i].UpdateCraftingBookSlotData(null);
                }
            }
            for (int i = 0; i < craftingSlots2.Count; i++)
            {
                if (i < recipe.createsItems2.Count && Inventory.Instance.GetItemByTemplateID(recipe.createsItems2[i]) != null)
                {
                    CraftingComponent cc = new CraftingComponent();
                    cc.item = Inventory.Instance.GetItemByTemplateID(recipe.createsItems2[i]);
                    cc.count = recipe.createsItemsCounts2[i];
                    if (craftingSlots2[i] != null)
                        craftingSlots2[i].ShowVisualElement();
                    craftingSlots2[i].UpdateCraftingBookSlotData(cc);
                }
                else
                {
                    if (craftingSlots2[i] != null)
                        craftingSlots2[i].HideVisualElement();
                    craftingSlots2[i].UpdateCraftingBookSlotData(null);
                }
            }
            for (int i = 0; i < craftingSlots3.Count; i++)
            {
                if (i < recipe.createsItems3.Count && Inventory.Instance.GetItemByTemplateID(recipe.createsItems3[i]) != null)
                {
                    CraftingComponent cc = new CraftingComponent();
                    cc.item = Inventory.Instance.GetItemByTemplateID(recipe.createsItems3[i]);
                    cc.count = recipe.createsItemsCounts3[i];
                    if (craftingSlots3[i] != null)
                        craftingSlots3[i].ShowVisualElement();
                    craftingSlots3[i].UpdateCraftingBookSlotData(cc);
                }
                else
                {
                    if (craftingSlots3[i] != null)
                        craftingSlots3[i].HideVisualElement();
                    craftingSlots3[i].UpdateCraftingBookSlotData(null);
                }
            }
            for (int i = 0; i < craftingSlots4.Count; i++)
            {
                if (i < recipe.createsItems4.Count && Inventory.Instance.GetItemByTemplateID(recipe.createsItems4[i]) != null)
                {
                    CraftingComponent cc = new CraftingComponent();
                    cc.item = Inventory.Instance.GetItemByTemplateID(recipe.createsItems4[i]);
                    cc.count = recipe.createsItemsCounts4[i];
                    if (craftingSlots4[i] != null)
                        craftingSlots4[i].ShowVisualElement();
                    craftingSlots4[i].UpdateCraftingBookSlotData(cc);
                }
                else
                {
                    if (craftingSlots4[i] != null)
                        craftingSlots4[i].HideVisualElement();
                    craftingSlots4[i].UpdateCraftingBookSlotData(null);
                }
            }
            int number = 100000;
            int _countCraft = 0;
            for (int iii = 0; iii < recipeSelected.itemsReq.Count; iii++)
            {
                int num = Inventory.Instance.GetCountOfItem(recipeSelected.itemsReq[iii]);
                _countCraft = num / recipeSelected.itemsReqCounts[iii];
                if (number > _countCraft)
                    number = _countCraft;
            }
            inputCount.value = number.ToString();
            if (number > 0)
            {
                if (Crafting.Instance.StationType != "")
                    craftButton.SetEnabled(true);
                else
                    craftButton.SetEnabled(false);
            }
            else
                craftButton.SetEnabled(false);
            updateRecipeList();
        }

        void UpdateCraftingGrid()
        {
            if (newCraft)
                return;
            for (int i = 0; i < craftingSlots.Count; i++)
            {
                if (i < Crafting.Instance.GridItems.Count && Crafting.Instance.GridItems[i].item != null)
                {
                    craftingSlots[i].UpdateCraftingSlotData(Crafting.Instance.GridItems[i]);
                }
                else
                {
                    craftingSlots[i].UpdateCraftingSlotData(null);
                }
            }
        
            for (int i = 0; i < resultSlots.Count; i++)
            {
                if (i < Crafting.Instance.ResultItems.Count)
                {
                    resultSlots[i].ShowVisualElement();
                    resultSlots[i].SetItemData(Crafting.Instance.ResultItems[i]);
                }
                else
                {
                    resultSlots[i].HideVisualElement();
                }
            }
        
            if (Crafting.Instance.ResultItems.Count > 0)
            {
                craftButton.SetEnabled(true);
            }
            else
            {
        
                craftButton.SetEnabled(false);
            }
        }

        public void DoCraft()
        {
            if (newCraft)
            {

                string count = inputCount.text;
                if (count.Length == 0)
                    count = "1";
                if (recipeSelected == null)
                    return;
                Crafting.Instance.CraftItemBook(recipeSelected.recipeID, int.Parse(count));
            }
            else
            {
                Crafting.Instance.CraftItem();
            }
        }

        public void PlaceCraftingItem(UIAtavismActivatable activatable)
        {
            if (activatable.Link != null)
                return;
            for (int i = 0; i < craftingSlots.Count; i++)
            {
                if (i < Crafting.Instance.GridItems.Count && Crafting.Instance.GridItems[i].item == null)
                {

                    craftingSlots[i].SetActivatable(activatable);
                    return;
                }
            }
        }
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            // Focus the window
            AtavismUIUtility.BringToFront(this.gameObject);
        }
    }
}