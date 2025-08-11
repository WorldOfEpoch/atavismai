using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;

namespace Atavism
{
    public class UGUICastingBar : MonoBehaviour
    {

        static UGUICastingBar instance;

        public Image icon;
        public GameObject iconGameObject;
        public Text castName;
        public TextMeshProUGUI TMPCastName;
        public Text castTime;
        public TextMeshProUGUI TMPCastTime;
        public Image castFill;
        float startTime;
        float endTime = -1;
        private int abilityId = -1;

        private bool send = false;
        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                GameObject.DestroyImmediate(gameObject);
                return;
            }
            instance = this;

            Hide();
            AtavismEventSystem.RegisterEvent("CASTING_STARTED", this);
            AtavismEventSystem.RegisterEvent("CASTING_CANCELLED", this);
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("CASTING_STARTED", this);
            AtavismEventSystem.UnregisterEvent("CASTING_CANCELLED", this);
        }

        // Update is called once per frame
        void Update()
        {
            if (abilityId > 0 && endTime - 0.2 < Time.time && !send)
            {
                send = true;
                AbilityPrefabData apd = AtavismPrefabManager.Instance.GetAbilityPrefab(abilityId);
                Transform cam = Camera.main.transform;
                ClickToMoveInputController ctmic = cam.GetComponent<ClickToMoveInputController>();
                if (ctmic == null)
                {
                    SDETargeting sde = cam.GetComponent<SDETargeting>();
                    if (sde != null && sde.softTargetMode && sde.showCrosshair)
                    {
                        Vector3 v1 = cam.position + cam.forward * ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius")) ? apd.aoeRadius : apd.maxRange);
                        Vector3 v = v1;
                        RaycastHit hit;
                        float skipZone = (cam.position - ClientAPI.GetPlayerObject().GameObject.transform.position).magnitude;
                        if (Physics.Raycast(new Ray(cam.position + cam.forward * skipZone, cam.forward), out hit, ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius")) ? apd.aoeRadius : apd.maxRange), ClientAPI.Instance.playerLayer))
                        {
                            v = hit.point;
                        }

                        Dictionary<string, object> props = new Dictionary<string, object>();
                        props.Add("aid", abilityId);
                        props.Add("v", v);
                        //  Debug.LogError("UGUICastingBar change vector "+v);
                        NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "combat.ABILITY_VECTOR", props);
                    }
                }
            }
            
            
            if (endTime != -1 && endTime > Time.time)
            {
                float total = endTime - startTime;
                float currentTime = endTime - Time.time;
                if (GetComponent<Slider>() != null)
                    GetComponent<Slider>().value = 1 - ((float)currentTime / (float)total);
                if (castFill != null)
                    castFill.fillAmount = /*1 - */((float)currentTime / (float)total);
                if (castTime != null)
                    castTime.text = string.Format("{0:0.0}", currentTime) + "s";
                if (TMPCastTime != null)
                    TMPCastTime.text = string.Format("{0:0.0}", currentTime) + "s";
            }
            else
            {
                Hide();
            }
        }

        void Show()
        {
            GetComponent<CanvasGroup>().alpha = 1f;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            GetComponent<CanvasGroup>().ignoreParentGroups = true;
        }

        public void Hide()
        {
            GetComponent<CanvasGroup>().alpha = 0f;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            GetComponent<CanvasGroup>().ignoreParentGroups = false;
            abilityId = -1;
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "CASTING_STARTED")
            {
                if (eData.eventArgs.Length > 1 && OID.fromString(eData.eventArgs[1]).ToLong() != ClientAPI.GetPlayerOid())
                    return;
                Show();
                send = false;
                startTime = Time.time;
                endTime = Time.time + float.Parse(eData.eventArgs[0]);
                abilityId = -1;
                if(eData.eventArgs.Length>2)
                    abilityId = int.Parse(eData.eventArgs[2]);
                if (iconGameObject != null)
                {
                    if (abilityId > 0)
                    {
                        iconGameObject.SetActive(true);
                    }
                    else
                    {
                        iconGameObject.SetActive(false);
                    }
                }
                if (icon != null)
                {
                    if (abilityId > 0)
                    {
                        AtavismAbility aa = Abilities.Instance.GetAbility(abilityId);
                        icon.sprite = aa.Icon;
                        icon.enabled = true;
#if AT_I2LOC_PRESET
                       if (castTime != null)
                            castTime.text =  I2.Loc.LocalizationManager.GetTranslation(aa.name);
                       if (TMPCastName != null)
                            TMPCastName.text =  I2.Loc.LocalizationManager.GetTranslation(aa.name);
#else
                        if (castTime)
                            castTime.text = aa.name;
                        if (TMPCastName)
                            TMPCastName.text = aa.name;
#endif
                    }
                    else
                    {
                        icon.enabled = false;
                    }
                }

            }
            else if (eData.eventType == "CASTING_CANCELLED")
            {
                if (eData.eventArgs.Length > 1 && OID.fromString(eData.eventArgs[1]).ToLong() != ClientAPI.GetPlayerOid())
                    return;
                Hide();
                endTime = -1;
            }
        }

        public static UGUICastingBar Instance
        {
            get
            {
                return instance;
            }
        }
    }
}