using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIAtavismMobCreatorListEntry //: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Label Name;
        int id = -1;
        UIAtavismMobCreator mobCreator;
        bool mobtemplate = false;
        bool startQuest = false;
        bool endQuest = false;
        bool merchant = false;
        bool patrolPath = false;
        bool dialog = false;
        int pos = -1;
        public void SetVisualElement(VisualElement visualElement)
        {
            // uiRoot = visualElement;
            visualElement.RegisterCallback<MouseDownEvent>(EntryClicked);
            Name = visualElement.Q<Label>("name");
        }
        
        public void SetEntryDetails(string name, int id, bool mob, bool startQuest, bool endQuest, bool merchant, bool dialog, bool patrolPath, int pos, UIAtavismMobCreator mobCreator)
        {
            this.Name.text = id + ". " + name;
            this.id = id;
            this.mobCreator = mobCreator;
            this.mobtemplate = mob;
            this.startQuest = startQuest;
            this.endQuest = endQuest;
            this.pos = pos;
            this.merchant = merchant;
            this.dialog = dialog;
            this.patrolPath = patrolPath;
            //     UpdateDisplay();

        }
        public void EntryClicked(MouseDownEvent evt)
        {
            if (mobtemplate)
                mobCreator.SelectTemplate(id);
            if (startQuest)
                mobCreator.StartQuestClicked(id);
            if (endQuest)
                mobCreator.EndQuestClicked(id);
            if (merchant)
                mobCreator.MerchandTableClicked(id);
            if (dialog)
                mobCreator.DialoguesClicked(id);
            if (patrolPath)
                mobCreator.PatrolPathClicked(id);
        }

    }
}