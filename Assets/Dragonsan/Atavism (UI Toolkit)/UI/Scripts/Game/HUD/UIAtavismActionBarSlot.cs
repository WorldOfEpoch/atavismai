using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismActionBarSlot : UIAtavismDraggableSlot
    {

        Button button;
        private Label keyText;
        AtavismAction action;
        bool mouseEntered = false;
        public KeyCode activateKey;

        //	float cooldownExpiration = -1;
        public int barNum = 0;

        private VisualElement m_Root;

        // Use this for initialization
        void Start()
        {

        }

        public void SetVisualElement(VisualElement visualElement)
        {
            m_Root = visualElement.Q<VisualElement>("slot");
            keyText = visualElement.Q<Label>("slot-hotkey");

            if (keyText != null)
            {
                string key = activateKey.ToString();
                if (key.Contains("Alpha"))
                    key = key.Substring(5);
                keyText.text = key;
            }

            slotBehaviour = DraggableBehaviour.Reference;
        }

        Transform cam = null;

        // Update is called once per frame
        public void Update()
        {
            if (activateKey == null)
                return;
            if (action != null && action.actionObject is AtavismInventoryItem)
            {
                if (Input.GetKeyUp(activateKey) && !ClientAPI.UIHasFocus() && Actions.Instance.MainActionBar == barNum)
                {
                    Activate();
                }
            }

            if (Camera.main == null)
                return;
            if (cam == null)
                cam = Camera.main.transform;
            if (!ClientAPI.UIHasFocus() && Actions.Instance.MainActionBar == barNum)
            {
                if (action != null && action.actionObject is AtavismAbility)
                {

                    if (Input.GetKeyDown(activateKey))
                    {
                        AtavismAbility aa = (AtavismAbility)action.actionObject;
                        Cooldown _cooldown = aa.GetLongestActiveCooldown();
                        if (_cooldown != null)
                        {
                            if (_cooldown.expiration > Time.time)
                                return;
                        }

                        AbilityPrefabData apd = AtavismPrefabManager.Instance.GetAbilityPrefab(aa.id);
                        if (apd.powerup.Count > 1)
                        {
                            int coId = -1;
                            int cId = -1;
                            if (WorldBuilder.Instance.SelectedClaimObject != null)
                            {
                                coId = WorldBuilder.Instance.SelectedClaimObject.ID;
                                cId = WorldBuilder.Instance.SelectedClaimObject.ClaimID;
                            }


                            SDETargeting sde = cam.transform.GetComponent<SDETargeting>();
                            ClickToMoveInputController ctmic = cam.GetComponent<ClickToMoveInputController>();
                            if (ctmic == null)
                            {
                                if (sde != null && sde.softTargetMode)
                                {

                                    float skipZone =
                                        (cam.position - ClientAPI.GetPlayerObject().GameObject.transform.position)
                                        .magnitude;
                                    Vector3 v = cam.position + cam.forward * skipZone + cam.forward *
                                        ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius"))
                                            ? apd.aoeRadius
                                            : apd.maxRange);
                                    RaycastHit hit;
                                    if (Physics.Raycast(new Ray(cam.position + cam.forward * skipZone, cam.forward),
                                            out hit,
                                            ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius"))
                                                ? apd.aoeRadius
                                                : apd.maxRange), ClientAPI.Instance.playerLayer))
                                    {
                                        v = hit.point;
                                    }

                                    // Debug.LogError("ActivateWait vector " + v + " caster=" + ClientAPI.GetPlayerOid() + " target=" + ClientAPI.GetTargetOid());
                                    NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(),
                                        "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " +
                                        v.z);
                                    if (apd.powerup != null && apd.powerup.Count > 1)
                                    {
                                        Abilities.Instance.abilityPowerUp = aa.id;
                                    }
                                }
                                else
                                {
                                    NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(),
                                        "/ability " + aa.id + " " + cId + " " + coId);
                                    if (apd.powerup != null && apd.powerup.Count > 1)
                                    {
                                        Abilities.Instance.abilityPowerUp = aa.id;
                                    }
                                }
                            }
                            else
                            {


                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hit;
                                if (Physics.Raycast(ray, out hit, 100, ClientAPI.Instance.playerLayer))
                                {
                                    ClientAPI.GetPlayerObject().GameObject.transform.LookAt(hit.point);
                                    Vector3 v = hit.point;
                                    NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(),
                                        "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " +
                                        v.z);
                                    if (apd.powerup != null && apd.powerup.Count > 1)
                                    {
                                        Abilities.Instance.abilityPowerUp = aa.id;
                                    }
                                }
                                else if (Physics.Raycast(ray, out hit, 100, ctmic.groundLayers))
                                {
                                    ClientAPI.GetPlayerObject().GameObject.transform.LookAt(hit.point);
                                    Vector3 v = hit.point + Vector3.up * 0.8f;
                                    NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(),
                                        "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " +
                                        v.z);
                                    if (apd.powerup != null && apd.powerup.Count > 1)
                                    {
                                        Abilities.Instance.abilityPowerUp = aa.id;
                                    }
                                }
                            }
                        }
                        else
                        {
                            action.Activate();
                        }
                    }

                    if (Input.GetKeyUp(activateKey))
                    {
                        if (Abilities.Instance.abilityPowerUp > 0)
                        {
                            AtavismAbility aa = (AtavismAbility)action.actionObject;
                            AbilityPrefabData apd = AtavismPrefabManager.Instance.GetAbilityPrefab(aa.id);

                            ClickToMoveInputController ctmic = cam.GetComponent<ClickToMoveInputController>();
                            if (apd.powerup.Count > 1)
                            {
                                int coId = -1;
                                int cId = -1;
                                if (WorldBuilder.Instance.SelectedClaimObject != null)
                                {
                                    coId = WorldBuilder.Instance.SelectedClaimObject.ID;
                                    cId = WorldBuilder.Instance.SelectedClaimObject.ClaimID;
                                }

                                if (ctmic == null)
                                {
                                    SDETargeting sde = cam.transform.GetComponent<SDETargeting>();
                                    if (sde != null && sde.softTargetMode)
                                    {
                                        float skipZone =
                                            (cam.position - ClientAPI.GetPlayerObject().GameObject.transform.position)
                                            .magnitude;
                                        Vector3 v = cam.position + cam.forward * skipZone + cam.forward *
                                            ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius"))
                                                ? apd.aoeRadius
                                                : apd.maxRange);
                                        //Debug.LogError("ActivateWait vector " + v + " caster=" + ClientAPI.GetPlayerOid() + " target=" + ClientAPI.GetTargetOid());
                                        RaycastHit hit;
                                        if (Physics.Raycast(new Ray(cam.position + cam.forward * skipZone, cam.forward),
                                                out hit,
                                                ((apd.targetType == TargetType.AoE &&
                                                  apd.aoeType.Equals("PlayerRadius"))
                                                    ? apd.aoeRadius
                                                    : apd.maxRange), ClientAPI.Instance.playerLayer))
                                        {
                                            v = hit.point;
                                        }

                                        NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(),
                                            "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " +
                                            v.z);
                                    }
                                    else
                                    {
                                        NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(),
                                            "/ability " + aa.id + " " + cId + " " + coId);
                                    }
                                }
                                else
                                {
                                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                    RaycastHit hit;
                                    if (Physics.Raycast(ray, out hit, 100, ClientAPI.Instance.playerLayer))
                                    {
                                        ClientAPI.GetPlayerObject().GameObject.transform.LookAt(hit.point);
                                        Vector3 v = hit.point;
                                        NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(),
                                            "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " +
                                            v.z);
                                        if (apd.powerup != null && apd.powerup.Count > 1)
                                        {
                                            Abilities.Instance.abilityPowerUp = aa.id;
                                        }
                                    }
                                    else if (Physics.Raycast(ray, out hit, 100, ctmic.groundLayers))
                                    {
                                        ClientAPI.GetPlayerObject().GameObject.transform.LookAt(hit.point);
                                        Vector3 v = hit.point + Vector3.up * 0.8f;
                                        NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(),
                                            "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " +
                                            v.z);
                                        if (apd.powerup != null && apd.powerup.Count > 1)
                                        {
                                            Abilities.Instance.abilityPowerUp = aa.id;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateActionData(AtavismAction action, int barNum)
        {
            // Debug.LogError("UpdateActionData "+slotNum+" | "+(action!=null?action.actionType+" "+action.actionObject:"")+"  | "+barNum);
            this.action = action;
            this.barNum = barNum;
            if (action == null || action.actionObject == null)
            {
                //  Debug.LogError("UpdateActionData remove");
                if (uiActivatable != null)
                {
                    //   Debug.LogError("UpdateActionData remove uiActivatable");
                    uiActivatable.m_Root.RemoveFromHierarchy();
                    uiActivatable = null;
                    var it = m_Root.Q<TemplateContainer>();
                    if (it != null) m_Root.Remove(it);

                }
            }
            else
            {
                if (uiActivatable != null)
                {
                    uiActivatable.m_Root.RemoveFromHierarchy();
                    uiActivatable = null;
                }


                if (uiActivatable == null)
                {
                    var it = m_Root.Q<TemplateContainer>();
                    if (it != null)
                        m_Root.Remove(it);
                    uiActivatable = new UIAtavismActivatable(m_Root);
#if AT_MOBILE
                    uiActivatable.m_Root.AddToClassList("mobile-activatableContainer-action-bar");
#else
                    uiActivatable.m_Root.AddToClassList("activatableContainer-action-bar");
#endif                    
                    m_Root.Add(uiActivatable.m_Root);
                }
                else
                {
                }


                uiActivatable.SetActivatable(action.actionObject, ActivatableType.Action, this);
            }
            //  Debug.LogError("UpdateActionData End");
        }

        public override void OnMouseEnter(MouseEnterEvent eventData)
        {
#if !AT_MOBILE
            MouseEntered = true;
#endif
        }

        public override void OnMouseLeave(MouseLeaveEvent eventData)
        {
#if !AT_MOBILE
            MouseEntered = false;
#endif
        }

        public override void OnDrop(DropEvent eventData)
        {
            //   Debug.LogError("UIAtavismActionBarSlot OnDrop");
            UIAtavismActivatable droppedActivatable = DragDropManager.CurrentlyDraggedObject;
            if (droppedActivatable == null)
                return;
            //  Debug.LogError("UIAtavismActionBarSlot OnDrop |");

            if (droppedActivatable.ActivatableObject is AtavismAbility)
            {
                AtavismAbility ability = (AtavismAbility)droppedActivatable.ActivatableObject;
                if (ability.passive)
                    return;
            }
            //  Debug.LogError("UIAtavismActionBarSlot OnDrop ||");

            // Reject any temporaries or bag slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary ||
                droppedActivatable.Link != null
                || droppedActivatable.ActivatableType == ActivatableType.Bag)
            {
                return;
            }
            //   Debug.LogError("UIAtavismActionBarSlot OnDrop |||");

            if (uiActivatable != null && uiActivatable != droppedActivatable)
            {
                // Delete existing child
                // m_Root.Remove(uiActivatable.m_Root);
            }
            else if (uiActivatable == droppedActivatable)
            {
                droppedOnSelf = true;
            }

            if (droppedActivatable.Source == this)
            {
                droppedActivatable.PreventDiscard();
                return;
            }
            // Debug.LogError("UIAtavismActionBarSlot OnDrop |V");

            // If the source was a reference slot, clear it
            bool fromOtherSlot = false;
            int sourceBar = 0;
            int sourceSlot = 0;
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference)
            {
                fromOtherSlot = true;
                sourceSlot = droppedActivatable.Source.slotNum;
                UIAtavismActionBarSlot sourceBarSlot = (UIAtavismActionBarSlot)droppedActivatable.Source;
                sourceBar = sourceBarSlot.barNum;
                if (uiActivatable != null && uiActivatable != droppedActivatable)
                {

                    // sourceBarSlot.m_Root.Add(uiActivatable.m_Root);
                    //sourceBarSlot.uiActivatable = uiActivatable;

                    //  Actions.Instance.SetAction(sourceBarSlot.barNum, sourceBarSlot.slotNum, uiActivatable.ActivatableObject, fromOtherSlot, barNum, slotNum);
                    uiActivatable.m_Root.RemoveFromHierarchy();
                    uiActivatable = null;
                }

                droppedActivatable.Source.UIActivatable = null;

                m_Root.Add(droppedActivatable.m_Root);
                //  uiActivatable = droppedActivatable;

            }

            droppedActivatable.SetDropTarget(this);
            Actions.Instance.SetAction(barNum, slotNum, droppedActivatable.ActivatableObject, fromOtherSlot, sourceBar,
                sourceSlot);
            //  Debug.LogError("UIAtavismActionBarSlot OnDrop End");

        }

        public override void ClearChildSlot()
        {
            //  Debug.LogError("ActionSlot Clear");
            uiActivatable = null;
            action = null;
            Actions.Instance.SetAction(barNum, slotNum, null, false, 0, 0);
        }

        public override void Discarded()
        {
            // Debug.LogError("ActionSlot Discarded");
            if (droppedOnSelf)
            {
                droppedOnSelf = false;
                return;
            }

            if(uiActivatable!=null)
                m_Root.Remove(uiActivatable.m_Root);
            ClearChildSlot();
        }

        public override void Activate()
        {
            if (action != null)
                if (action.actionObject is AtavismInventoryItem)
                {
                    AtavismInventoryItem item = (AtavismInventoryItem)action.actionObject;
                    if (item.ItemId == null)
                    {
                        AtavismInventoryItem matchingItem = Inventory.Instance.GetInventoryItemOrEquip(item.templateId);
                        if (matchingItem == null)
                            return;
                        action.actionObject = matchingItem;
                    }
                }

            if (action != null)
                action.Activate();
        }

        void HideTooltip()
        {
            UIAtavismTooltip.Instance.Hide();
            // if (cor != null)
            // StopCoroutine(cor);
        }

        public bool MouseEntered
        {
            get { return mouseEntered; }
            set
            {
                mouseEntered = value;
                if (mouseEntered && action != null && action.actionObject != null)
                {
                    if (uiActivatable != null)
                        uiActivatable.ShowTooltip(m_Root);
                    // cor = StartCoroutine(CheckOver());
                }
                else
                {
                    HideTooltip();
                }
            }
        }


    }
}