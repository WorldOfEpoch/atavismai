using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace Atavism
{

    public class UGUIActionBarSlot : UGUIDraggableSlot
    {

        Button button;
        AtavismAction action;
        bool mouseEntered = false;
        public KeyCode activateKey;
        //	float cooldownExpiration = -1;
        int barNum = 0;

        // Use this for initialization
        void Start()
        {
            slotBehaviour = DraggableBehaviour.Reference;
        }

        Transform cam = null;
        // Update is called once per frame
        void Update()
        {
            if (action != null && action.actionObject is AtavismInventoryItem)
            {
                if (Input.GetKeyUp(activateKey) && !ClientAPI.UIHasFocus() && Actions.Instance.MainActionBar == barNum)
                {
                    Activate();
                }
            }

            if (Camera.main == null)
                return;
            if(cam==null )
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

                                    float skipZone = (cam.position - ClientAPI.GetPlayerObject().GameObject.transform.position).magnitude;
                                    Vector3 v = cam.position + cam.forward * skipZone + cam.forward * ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius"))? apd.aoeRadius:apd.maxRange);
                                    RaycastHit hit;
                                    if (Physics.Raycast(new Ray(cam.position + cam.forward * skipZone, cam.forward), out hit, ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius")) ? apd.aoeRadius : apd.maxRange), ClientAPI.Instance.playerLayer))
                                    {
                                        v = hit.point;
                                    }

                                    // Debug.LogError("ActivateWait vector " + v + " caster=" + ClientAPI.GetPlayerOid() + " target=" + ClientAPI.GetTargetOid());
                                    NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                                    if (apd.powerup != null && apd.powerup.Count > 1)
                                    {
                                        Abilities.Instance.abilityPowerUp = aa.id;
                                    }
                                }
                                else
                                {
                                    NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + aa.id + " " + cId + " " + coId);
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
                                    Vector3 v =  hit.point;
                                    NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                                    if (apd.powerup != null && apd.powerup.Count > 1)
                                    {
                                        Abilities.Instance.abilityPowerUp = aa.id;
                                    }
                                }else if (Physics.Raycast(ray, out hit, 100, ctmic.groundLayers))
                                {
                                    ClientAPI.GetPlayerObject().GameObject.transform.LookAt(hit.point);
                                    Vector3 v =  hit.point+Vector3.up*0.8f;
                                    NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
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
                                        float skipZone = (cam.position - ClientAPI.GetPlayerObject().GameObject.transform.position).magnitude;
                                        Vector3 v = cam.position + cam.forward * skipZone + cam.forward * ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius"))? apd.aoeRadius:apd.maxRange);
                                        //Debug.LogError("ActivateWait vector " + v + " caster=" + ClientAPI.GetPlayerOid() + " target=" + ClientAPI.GetTargetOid());
                                        RaycastHit hit;
                                        if (Physics.Raycast(new Ray(cam.position + cam.forward * skipZone, cam.forward), out hit, ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius")) ? apd.aoeRadius : apd.maxRange), ClientAPI.Instance.playerLayer))
                                        {
                                            v = hit.point;
                                        }

                                        NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                                    }
                                    else
                                    {
                                        NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + aa.id + " " + cId + " " + coId);
                                    }
                                }
                                else
                                {
                                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                    RaycastHit hit;
                                    if (Physics.Raycast(ray, out hit, 100, ClientAPI.Instance.playerLayer))
                                    {
                                        ClientAPI.GetPlayerObject().GameObject.transform.LookAt(hit.point);
                                        Vector3 v =  hit.point;
                                        NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                                        if (apd.powerup != null && apd.powerup.Count > 1)
                                        {
                                            Abilities.Instance.abilityPowerUp = aa.id;
                                        }
                                    }else if (Physics.Raycast(ray, out hit, 100, ctmic.groundLayers))
                                    {
                                        ClientAPI.GetPlayerObject().GameObject.transform.LookAt(hit.point);
                                        Vector3 v =  hit.point+Vector3.up*0.8f;
                                        NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + aa.id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
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
            this.action = action;
            this.barNum = barNum;
            if (action == null || action.actionObject == null)
            {
                if (uguiActivatable != null)
                {
                    DestroyImmediate(uguiActivatable.gameObject);
                }
            }
            else
            {
                if (uguiActivatable == null)
                {
                    if (action.actionType == ActionType.Ability)
                    {
                        if (AtavismSettings.Instance.actionBarPrefab != null)
                            uguiActivatable = (UGUIAtavismActivatable)Instantiate(AtavismSettings.Instance.actionBarPrefab, transform, false);
                        else
                            uguiActivatable = (UGUIAtavismActivatable)Instantiate(Abilities.Instance.uguiAtavismAbilityPrefab, transform, false);
                    }
                    else
                    {
                        if (AtavismSettings.Instance.actionBarPrefab != null)
                            uguiActivatable = (UGUIAtavismActivatable)Instantiate(AtavismSettings.Instance.actionBarPrefab, transform, false);
                        else
                            uguiActivatable = (UGUIAtavismActivatable)Instantiate(Inventory.Instance.uguiAtavismItemPrefab);
                    }
                    //	uguiActivatable.transform.SetParent(transform, false);
                    uguiActivatable.transform.localScale = Vector3.one;
                    uguiActivatable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                }
                else
                {
                    //TODO: something to update the count text?
                }
                uguiActivatable.SetActivatable(action.actionObject, ActivatableType.Action, this);
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
#if !AT_MOBILE
            MouseEntered = true;
#endif            
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
#if !AT_MOBILE
            MouseEntered = false;
#endif            
        }

        public override void OnDrop(PointerEventData eventData)
        {
            UGUIAtavismActivatable droppedActivatable = eventData.pointerDrag.GetComponent<UGUIAtavismActivatable>();
            if (droppedActivatable == null)
                return;

            if (droppedActivatable.ActivatableObject is AtavismAbility)
            {
                AtavismAbility ability = (AtavismAbility)droppedActivatable.ActivatableObject;
                if (ability.passive)
                    return;
            }

            // Reject any temporaries or bag slots
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Temporary || droppedActivatable.Link != null
                || droppedActivatable.ActivatableType == ActivatableType.Bag)
            {
                return;
            }

            if (uguiActivatable != null && uguiActivatable != droppedActivatable)
            {
                // Delete existing child
                DestroyImmediate(uguiActivatable.gameObject);
            }
            else if (uguiActivatable == droppedActivatable)
            {
                droppedOnSelf = true;
            }
            if (droppedActivatable.Source == this)
            {
                droppedActivatable.PreventDiscard();
                return;
            }

            // If the source was a reference slot, clear it
            bool fromOtherSlot = false;
            int sourceBar = 0;
            int sourceSlot = 0;
            if (droppedActivatable.Source.SlotBehaviour == DraggableBehaviour.Reference)
            {
                fromOtherSlot = true;
                sourceSlot = droppedActivatable.Source.slotNum;
                droppedActivatable.Source.UguiActivatable = null;
                UGUIActionBarSlot sourceBarSlot = (UGUIActionBarSlot)droppedActivatable.Source;
                sourceBar = sourceBarSlot.barNum;
                //droppedActivatable.Source.action = null;
                //droppedActivatable.Source.ClearChildSlot();
                if (uguiActivatable != null && uguiActivatable != droppedActivatable)
                {
                    sourceBarSlot.uguiActivatable = uguiActivatable;
                    sourceBarSlot.uguiActivatable.transform.SetParent(sourceBarSlot.transform, false);
                    sourceBarSlot.uguiActivatable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    Actions.Instance.SetAction(sourceBarSlot.barNum, sourceBarSlot.slotNum, sourceBarSlot.uguiActivatable.ActivatableObject, fromOtherSlot, barNum, slotNum);
                }

                uguiActivatable = droppedActivatable;

                uguiActivatable.transform.SetParent(transform, false);
                uguiActivatable.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            }

            droppedActivatable.SetDropTarget(this);
            Actions.Instance.SetAction(barNum, slotNum, droppedActivatable.ActivatableObject, fromOtherSlot, sourceBar, sourceSlot);
        }

        public override void ClearChildSlot()
        {
           // Debug.LogError("ActionSlot Clear");
            uguiActivatable = null;
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
            DestroyImmediate(uguiActivatable.gameObject);
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
            UGUITooltip.Instance.Hide();
            if (cor != null)
                StopCoroutine(cor);
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
                if (mouseEntered && action != null && action.actionObject != null)
                {
                    if(uguiActivatable)
                        uguiActivatable.ShowTooltip(gameObject);
                    cor = StartCoroutine(CheckOver());
                }
                else
                {
                    HideTooltip();
                }
            }
        }
    }
}