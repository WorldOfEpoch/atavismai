using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public delegate void RecipeResponse(AtavismCraftingRecipe auction);

        public class UIAtavismCraftRecipeSlot //: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            public VisualElement icon;
            public VisualElement selected;
            public Label name;
            public Label level;
            public Label status;
            public Label count;
            AtavismInventoryItem item;
            RecipeResponse response;
            AtavismCraftingRecipe recipe;
            bool mouseEntered = false;
            private VisualElement m_Root;
            public void SetVisualElement(VisualElement visualElement)
            {
                // Debug.LogError("SetVisualElement ");
                m_Root = visualElement;
                m_Root.RegisterCallback<MouseLeaveEvent>(OnPointerExit);
                m_Root.RegisterCallback<MouseEnterEvent>(OnPointerEnter);
                
                icon = visualElement.Q<VisualElement>("icon");
                selected = visualElement.Q<VisualElement>("selected");
               if(selected!=null) selected.pickingMode = PickingMode.Ignore;
                name = visualElement.Q<Label>("name");
                level = visualElement.Q<Label>("level");
                status = visualElement.Q<Label>("status");
                count = visualElement.Q<Label>("count");
                // Debug.LogError("SetVisualElement End");
            }
            
            public void SetDetale(AtavismCraftingRecipe recipe, RecipeResponse response, Toggle avaiable,  Toggle backpack, AtavismCraftingRecipe selectedRecipe)
            {

                // Debug.LogError("UIAtavismCraftRecipeSlot::SetDetale called "+recipe+" response="+response+" avaiable="+avaiable+" backpack="+backpack+" selectedRecipe="+selectedRecipe);
                    
                int number = 10000;
                this.response = response;
                this.recipe = recipe;
                if (icon != null && recipe.Icon != null)
                    icon.style.backgroundImage = recipe.Icon.texture;
                if (selected != null)
                    if (selectedRecipe != null)
                    {
                        if (selectedRecipe.recipeID == recipe.recipeID)
                            selected.AddToClassList("List-Selected");
                        else
                            selected.RemoveFromClassList("List-Selected");
                    }
                    else
                    {
                        selected.RemoveFromClassList("List-Selected");

                    }
                for (int i = 0; i < recipe.itemsReq.Count; i++)
                {
                    int num = Inventory.Instance.GetCountOfItem(recipe.itemsReq[i]);
                    int _count = num / recipe.itemsReqCounts[i];
                    if (number > _count)
                        number = _count;
                }

          
                if (count != null)
                    count.text = number.ToString();
                if (level != null)
                    level.text = recipe.skillLevelReq.ToString();
#if AT_I2LOC_PRESET
                if (name != null) name.text = I2.Loc.LocalizationManager.GetTranslation(recipe.recipeName)+" ("+number+")" ;
#else
                if (name != null)
                    name.text = recipe.recipeName + " (" + number + ")";
#endif
                if (status != null)
                {
                    status.RemoveFromClassList("resource-error");
                    status.RemoveFromClassList("resource-low");
                    status.RemoveFromClassList("resource-skill-low");
                }
                string msg = "";
            //    Debug.LogError(recipe.recipeName+" >"+recipe.stationReq+"<  >"+Crafting.Instance.StationType+"< "+Crafting.Instance.StationType.Length+" "+recipe.stationReq.Equals(Crafting.Instance.StationType)+" "+(Crafting.Instance.StationType.Length > 0 && !recipe.stationReq.Equals("Any"))+" "+(Crafting.Instance.StationType.Length ==0 && (recipe.stationReq.Equals("Any")||!recipe.stationReq.Equals("none")))+" "+recipe.stationReq.Equals("none"));
                if (Skills.Instance.GetPlayerSkillLevel(recipe.skillID) < recipe.skillLevelReq)
                {
#if AT_I2LOC_PRESET
                    msg = I2.Loc.LocalizationManager.GetTranslation("Low Skill") ;
#else
                    msg = "Low Skill";
#endif
                    if (status != null)
                    {
                        status.AddToClassList("resource-skill-low");
                    }
                }
                else if (number < 1)
                {
#if AT_I2LOC_PRESET
                    msg = I2.Loc.LocalizationManager.GetTranslation("Low Resource");
#else
                    msg = "Low Resources";
#endif
                    if (status != null)
                    {
                        status.AddToClassList("resource-low");
                    }
                }
                else if(!recipe.stationReq.Equals(Crafting.Instance.StationType) && ((Crafting.Instance.StationType.Length > 0 && !recipe.stationReq.Equals("Any"))||(Crafting.Instance.StationType.Length ==0 && (recipe.stationReq.Equals("Any")||!recipe.stationReq.Equals("none"))))&& !recipe.stationReq.Equals("none"))
                {
#if AT_I2LOC_PRESET
                    msg = I2.Loc.LocalizationManager.GetTranslation("Wrong Station");
#else
                    msg = "Wrong Station";
#endif
                    if (status != null)
                    {
                        status.AddToClassList("resource-error");
                    }
                }

                if (status != null)
                    status.text = msg;

            }
            public void Click()
            {
                if (response != null)
                    response(recipe);

            }
            public void OnPointerEnter(MouseEnterEvent evt)
            {
#if !AT_MOBILE   
                MouseEntered = true;
#endif                
            }

            public void OnPointerExit(MouseLeaveEvent evt)
            {
#if !AT_MOBILE   
                MouseEntered = false;
#endif                
            }
            void HideTooltip()
            {
                UIAtavismTooltip.Instance.Hide();

            }

            public bool MouseEntered
            {
                get
                {
                    return mouseEntered;
                }
                set
                {
                    mouseEntered = value;
                    if (mouseEntered && recipe != null)
                    {
                        recipe.ShowTooltip(m_Root);
                        //   cor = StartCoroutine(CheckOver());
                    }
                    else
                    {
                        HideTooltip();
                    }
                }
            }
        }
    }
