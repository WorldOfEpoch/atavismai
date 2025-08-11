using Atavism;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using UnityEngine;
using UnityEngine.UIElements;

public class UIAtavismBuildObject : MonoBehaviour
{
    private VisualElement m_Root;
    private Button button;
    private Label nameLabel;
    private Label categoryLabel;
    private Label requirementsLabel;
    private AtavismBuildObjectTemplate template;

    public void SetVisualElement(VisualElement visualElement)
    {
        m_Root = visualElement;
        // Query UI elements
        button = m_Root.Q<Button>("icon" );
        nameLabel = m_Root.Q<Label>("name");
        categoryLabel = m_Root.Q<Label>("category");
        requirementsLabel = m_Root.Q<Label>("requirements");

        // Add callbacks
        button.clicked += BuildObjectClicked;
    }

    public void UpdateBuildObjectInfo(AtavismBuildObjectTemplate template)
    {
        this.template = template;

        // Update the button icon
        button.style.backgroundImage = new StyleBackground(template.Icon);

        // Update the name label
        if (nameLabel != null)
            nameLabel.text = template.buildObjectName;

        // Update the category label (if you have one in your UI Toolkit layout)
        if (categoryLabel != null)
        {
            categoryLabel.text = WorldBuilder.Instance.GetBuildingCategory(template.category);
        }

        // Calculate build time requirements
        var buildTimeReq = template.buildTimeReq;
        int days = (int)(buildTimeReq / (3600 * 24));
        int hours = (int)((buildTimeReq % (3600 * 24)) / 3600);
        int minutes = (int)((buildTimeReq % 3600) / 60);
        int seconds = (int)(buildTimeReq % 60);

        string outTime = FormatTime(days, hours, minutes, seconds);

        // Build the requirements string
        string requirements = $"Build Time: {outTime}\nRequires: ";

        foreach (var req in template.itemsReq)
        {
            AtavismInventoryItem item = Inventory.Instance.GetItemByTemplateID(req);
            if (item != null)
            {
                requirements += $"{item.name} x{template.itemsReqCount[template.itemsReq.IndexOf(req)]};  ";
            }
        }

        if (template.skill > 0)
        {
            Skill skill = Skills.Instance.GetSkillByID(template.skill);
            if (skill != null)
            {
                string skillReq = skill.CurrentLevel >= template.skillLevelReq ?
                                  $"Skill {skill.skillname} level {template.skillLevelReq}" :
                                  $"<color=#ff0000ff>Skill {skill.skillname} level {template.skillLevelReq}</color>";
                requirements += skillReq;
            }
            else
            {
                Debug.LogError($"Building Object Skill {template.skill} can't be found");
            }
        }

        // Update the requirements label
        if (requirementsLabel != null)
            requirementsLabel.text = requirements;
    }

    private string FormatTime(int days, int hours, int minutes, int seconds)
    {
        string result = "";
        if (days > 0)
            result += days > 1 ? $"{days} days " : $"{days} day ";
        if (hours > 0)
            result += $"{hours} h ";
        if (minutes > 0)
            result += $"{minutes} m ";
        if (seconds > 0)
            result += $"{seconds} s ";

        return result;
    }

    public void BuildObjectClicked()
    {
        Debug.LogError("BuildObjectClicked "+template);
        WorldBuilder.Instance.BuildingState = WorldBuildingState.SelectItem;

        if (BuildManager.Instance.Pieces.Count != 0)
        {
            if (BuilderBehaviour.Instance != null)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
                bool found = false;
                foreach (var piece in BuildManager.Instance.Pieces)
                {
                    if (piece != null && piece.BuildObjDefId == template.id)
                    {
                        AtavismLogger.LogDebugMessage($"BuildObjectClicked piece {piece.Id} {template.id} piece.BuildObjDefId={piece.BuildObjDefId} {piece.name}");
                        BuilderBehaviour.Instance.SelectPrefab(piece);
                        found = true;
                        break; // Stop iterating as we found the relevant piece
                    }
                }

                AtavismLogger.LogDebugMessage($"BuildObjectClicked {template.id} found={found}");
                if (!found)
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
                }
            }
            else
            {
                WorldBuilder.Instance.StartPlaceBuildObject(template.id);
            }
        }
    }

  
}
