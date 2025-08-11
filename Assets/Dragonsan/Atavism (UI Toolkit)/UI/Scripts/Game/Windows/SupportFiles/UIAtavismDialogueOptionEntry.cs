using Atavism;
using Atavism.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIAtavismDialogueOptionEntry 
{
    private Label m_text;
    private VisualElement m_Icon;
    private VisualElement m_payItemIcon;
    private VisualElement m_payCurrency;
    private Button m_container;
    private NpcInteractionEntry interaction;
    private UIAtavismDialoguePanel parentScript; 
    public void SetVisualElement(VisualElement visualElement,UIAtavismDialoguePanel parent )
    {
       // m_container = visualElement;
        m_text = visualElement.Q<Label>("dialogue-option-text");
        m_Icon = visualElement.Q<VisualElement>("dialogue-option-image");
        m_payItemIcon = visualElement.Q<VisualElement>("dialogue-option-item-image");
        m_payCurrency = visualElement.Q<VisualElement>("dialogue-option-currency-image");
        m_container = visualElement.Q<Button>("dialogue-option-container");
        m_container.clicked += Clicked;
        parentScript = parent;
    }

    public void SetData(NpcInteractionEntry entry)
    {
        interaction = entry;
#if AT_I2LOC_PRESET
        if (interaction.interactionTitle.IndexOf("(Repeatable)") > 0)
        {
            if (m_text != null) m_text.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + interaction.interactionTitle.Substring(0, interaction.interactionTitle.IndexOf("(Repeatable)") - 1)) + " (" + I2.Loc.LocalizationManager.GetTranslation("Repeatable") + ")";
        }
        else if(interaction.interactionTitle.IndexOf("(Complete)") > 0)
        {
            if (m_text != null) m_text.text = I2.Loc.LocalizationManager.GetTranslation("Quests/" + interaction.interactionTitle.Substring(0, interaction.interactionTitle.IndexOf("(Complete)") - 1)) + " (" + I2.Loc.LocalizationManager.GetTranslation("Complete") + ")";
        }
        else
        {
            if (m_text != null) m_text.text = string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Quests/" + interaction.interactionTitle)) ? interaction.interactionTitle : I2.Loc.LocalizationManager.GetTranslation("Quests/" + interaction.interactionTitle);
        }
#else
            if (m_text != null)
                m_text.text = interaction.interactionTitle;
#endif
        Texture2D dialogIcon = null;
        Texture2D itemIcon = null;
        Texture2D currencyIcon = null;
         if (interaction.interactionType == "offered_quest" || interaction.interactionType == "Quest")
            {
                if (parentScript.newQuestSprite != null)
                    dialogIcon = parentScript.newQuestSprite.texture;
            }
            else if (interaction.interactionType == "progress_quest" || interaction.interactionType == "QuestPregerss")
            {
                if (parentScript.progressQuestSprite != null)
                    dialogIcon = parentScript.progressQuestSprite.texture;
            }
            else if (interaction.interactionType == "dialogue" || interaction.interactionType == "Ability")
            {
                if (parentScript.dialogueSprite != null)
                    dialogIcon = parentScript.dialogueSprite.texture;
            }
            else if (interaction.interactionType == "merchant" || interaction.interactionType == "Merchant")
            {
                if (parentScript.merchantSprite != null)
                    dialogIcon = parentScript.merchantSprite.texture;
            }
            else if (interaction.interactionType == "Bank")
            {
                if (parentScript.bankSprite != null)
                    dialogIcon = parentScript.bankSprite.texture;
            }
            else if (interaction.interactionType == "Repair")
            {
                if (parentScript.repairSprite != null)
                    dialogIcon = parentScript.repairSprite.texture;
            }
            else if (interaction.interactionType == "Ability")
            {
                if (parentScript.abilitySprite != null)
                    dialogIcon = parentScript.abilitySprite.texture;
            }
            else if (interaction.interactionType == "Auction")
            {
                if (parentScript.auctionSprite != null)
                    dialogIcon = parentScript.auctionSprite.texture;
            }
            else if (interaction.interactionType == "Mail")
            {
                if (parentScript.mailSprite != null)
                    dialogIcon = parentScript.mailSprite.texture;
            }
            else if (interaction.interactionType == "GearModification")
            {
                if (parentScript.gearModificationSprite != null)
                    dialogIcon = parentScript.gearModificationSprite.texture;
            }
            else if (interaction.interactionType == "GuildWarehouse")
            {
                if (parentScript.guildWarehouseSprite != null)
                    dialogIcon = parentScript.guildWarehouseSprite.texture;
            }
        
            if (m_payCurrency != null)
                if (interaction.currency > 0 && interaction.currencyAmmount > 0)
                {
                    m_payCurrency.visible=true;
                    string curr = Inventory.Instance.GetCostString(interaction.currency, interaction.currencyAmmount);
                    currencyIcon = AtavismPrefabManager.Instance.GetCurrencyIconByID(interaction.currency).texture;
                    m_payCurrency.tooltip = curr;

                    m_payCurrency.RegisterCallback<MouseEnterEvent>(
                        e =>
                        {
                            UIAtavismMiniTooltip.Instance.SetDescription(curr);
                            UIAtavismMiniTooltip.Instance.Show(m_payCurrency);
                        });
                    m_payCurrency.RegisterCallback<MouseLeaveEvent>(
                        e =>
                        {
                            UIAtavismMiniTooltip.Instance.Hide();
                        });
                    // UGUIMiniTooltipEvent mte = currencyIcon.transform.GetComponent<UGUIMiniTooltipEvent>();
                    // if (mte != null)
                    //     mte.dectName = curr;
                } else
                {
                    m_payCurrency.visible=false;
                }
            
            if (m_payItemIcon != null)
                if (interaction.itemId > 0 )
                {
                    m_payItemIcon.visible=true;
                    // UGUIItemDisplay uid = itemIcon.transform.GetComponent<UGUIItemDisplay>();
                    // if (uid != null)
                    // {
                    AtavismInventoryItem aii = AtavismPrefabManager.Instance.LoadItem(interaction.itemId);
                    if (aii != null)
                    {
                        itemIcon = aii.Icon.texture;
                        m_payItemIcon.tooltip = aii.BaseName;
                        m_payItemIcon.RegisterCallback<MouseEnterEvent>(
                            e =>
                            {
                                aii.ShowUITooltip(m_payItemIcon);
                            });
                        m_payItemIcon.RegisterCallback<MouseLeaveEvent>(
                            e =>
                            {
                                aii.HideUITooltip();
                            });
                    }
                    //         uid.SetItemData(aii, null);
                    // }
                }
                else
                {
                    m_payItemIcon.visible=false;
                }
                
        
        if (m_Icon != null)
            m_Icon.SetBackgroundImage(dialogIcon);
        if (m_payItemIcon != null)
            m_payItemIcon.SetBackgroundImage(itemIcon);
        if (m_payCurrency != null)
            m_payCurrency.SetBackgroundImage(currencyIcon);
       
      //  m_container.clickable = null;
    }

    public void Clicked()
    {
        if (interaction != null )
            interaction.StartInteraction();
    }
    
    public void Show()
    {
        m_container.ShowVisualElement();
    }

    public void Hide()
    {
        m_container.HideVisualElement();
    }
}
