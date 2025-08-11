using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Atavism
{

    public class UGUIPetCommand : MonoBehaviour
    {

        //  CanvasGroup cg;
        bool showing = false;

        void Start()
        {
            NetworkAPI.RegisterExtensionMessageHandler("ao.pet_stats", PetStatHandler);
            NetworkAPI.RegisterExtensionMessageHandler("petListUpdate", PetListHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler("hasPet", ActiveHandler);
            Hide();
        }

        private void OnDestroy()
        {
            if(ClientAPI.GetPlayerObject()!=null)
                ClientAPI.GetPlayerObject().RemovePropertyChangeHandler("hasPet", ActiveHandler);
            NetworkAPI.RemoveExtensionMessageHandler("ao.pet_stats", PetStatHandler);
            NetworkAPI.RemoveExtensionMessageHandler("petListUpdate", PetListHandler);
        }

        public Slider healthBar;
        [SerializeField] Text passiveButtonText;
        [SerializeField] TextMeshProUGUI passiveButtonTextTMP;
        [SerializeField] Text defensiveButtonText;
        [SerializeField] TextMeshProUGUI defensiveButtonTextTMP;
        [SerializeField] Text aggressiveButtonText;
        [SerializeField] TextMeshProUGUI aggressiveButtonTextTMP;
        [SerializeField] Text stayButtonText;
        [SerializeField] TextMeshProUGUI stayButtonTextTMP;
        [SerializeField] Text followButtonText;
        [SerializeField] TextMeshProUGUI followButtonTextTMP;
        [SerializeField] Text attackButtonText;
        [SerializeField] TextMeshProUGUI attackButtonTextTMP;
        [SerializeField] Color selectedTextColor = Color.green;
        [SerializeField] Color defaultTextColor = Color.white;

        public Transform _petListContainer;
        public GameObject _petListContainerPrefab;
        
        int health = 0;
        int health_max = 0;
        private OID activePetOid;
        private AtavismObjectNode node;
        private Dictionary<int, List<long>> pets = new Dictionary<int, List<long>>();
        private List<UGUIPetProfileEntry> _petProfiles = new List<UGUIPetProfileEntry>();
        public string petInfo = "{{level}} ({{count}}/{{count_max}})"; 
        
        private void PetListHandler(Dictionary<string, object> props)
        {
            AtavismLogger.LogInfoMessage("Got petListHandler");
            if (AtavismLogger.isLogDebug())
            {
                string keys = " [ ";
                foreach (var it in props.Keys)
                {
                    keys += " ; " + it + " => " + props[it];
                }

                Debug.LogWarning("PetListHandler: keys:" + keys);
            }

            pets.Clear();
            int petCount = 0;
            try
            {
                int num = (int)props["num"];
                for (int i = 0; i < num; i++)
                {
                    int profileId = (int)props["p" + i];
                    if (!pets.ContainsKey(profileId))
                        pets.Add(profileId, new List<long>());
                    int numPets = (int)props["p" + i + "num"];
                   // Debug.Log("PetListHandler profileId=" + profileId + " numPets="+numPets);
                    if (numPets > 0)
                    {
                        for (int j = 0; j < numPets; j++)
                        {
                            pets[profileId].Add((long)props["p" + i + "m" + j]);
                            petCount++;
                        }
                    }
                    else
                    {
                        pets.Remove(profileId);
                    }

                    AtavismLogger.LogDebugMessage("Got pet list");
                }
                string pp = "";
                foreach (var profile in pets.Keys)
                {
                    if (pets[profile].Count > 0)
                    {
                        if(pp.Length > 0)
                            pp += "|";
                        pp += profile;
                    }
                }
                AtavismEventSystem.DispatchEvent("PET_LIST_UPDATE", new String[]{pp});
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Pet petStatHandler Exception " + e.Message + "\n\n" + e.StackTrace);
            }
           // Debug.Log("PetListHandler " + pets.Count + " pets");
            if(petCount > 0 && !showing)
                Show();
            else if(showing && petCount == 0)
                Hide();
            StopAllCoroutines();
            if(petCount > 0)
                StartCoroutine(UpdatePetList());

        }

        private IEnumerator UpdatePetList()
        {
            bool done = false;
            while (!done)
            {
                AtavismLogger.LogDebugMessage("UpdatePetList");
                int p = 0;
                foreach (var pet in pets.Keys)
                {
                    if (pets[pet].Count > 0)
                    {
                        long petOid = pets[pet][0];
                        AtavismObjectNode node = ClientAPI.GetObjectNode(petOid);
                        if (node == null)
                        {
                            Debug.LogError("UpdatePetList node is null");
                            done = false;
                            break;
                        }

                        if (_petProfiles.Count <= p)
                        {
                            GameObject go = Instantiate(_petListContainerPrefab, _petListContainer.transform);
                            UGUIPetProfileEntry entry = go.GetComponent<UGUIPetProfileEntry>();
                            _petProfiles.Add(entry);
                        }

                        if (!_petProfiles[p].gameObject.activeSelf)
                            _petProfiles[p].gameObject.SetActive(true);
                        AtavismMobAppearance ama = node.GameObject.GetComponent<AtavismMobAppearance>();

                        string info = petInfo;
                        if (node.GetProperty("petLevel") != null)
                        {
                            int level = (int)node.GetProperty("petLevel");
                            info = info.Replace("{{level}}", level.ToString());

                        }
                        if (node.GetProperty("petCountStat") != null)
                        {
                            int petCountStat = (int)node.GetProperty("petCountStat");
                            string stat = AtavismPrefabManager.Instance.GetStat(petCountStat);
                            int max_count = (int)ClientAPI.GetPlayerObject().GetProperty(stat);
                            info = info.Replace("{{count_max}}", max_count.ToString());

                        }
                        else
                        {
                            Debug.LogError("Not Found param petCountStat");
                        }
                        info = info.Replace("{{count}}", pets[pet].Count.ToString());
                        if (_petProfiles[p].icon != null && ama != null && ama.portraitIcon != null)
                                _petProfiles[p].icon.sprite = ama.portraitIcon;
                        if (_petProfiles[p].info != null)
                            _petProfiles[p].info.text = info;
                        if (_petProfiles[p].button != null)
                        {
                            _petProfiles[p].button.onClick.RemoveAllListeners();
                            _petProfiles[p].button.onClick
                                .AddListener(() => UGUIPetCharacterPanel.Instance.UpdateCharacterData(petOid));
                        }

                        p++;
                    }
                    // done = true;
                }

                for (int i = p; i < _petProfiles.Count; i++)
                {
                    if (_petProfiles[i].gameObject.activeSelf)
                        _petProfiles[i].gameObject.SetActive(false);

                }
              
                yield return new WaitForSeconds (1f);
            }
        }

        private void PetStatHandler(Dictionary<string, object> props)
        {
            AtavismLogger.LogInfoMessage("Got petStatHeandler");

            if (props.ContainsKey(AtavismCombat.Instance.HealthStat))
            {
                health = (int) props[AtavismCombat.Instance.HealthStat];
                AtavismLogger.LogInfoMessage("Got petStatHeandler health=" + health);
            }

            if (props.ContainsKey(AtavismCombat.Instance.HealthMaxStat))
            {
                health_max = (int) props[AtavismCombat.Instance.HealthMaxStat];
                AtavismLogger.LogInfoMessage("Got petStatHeandler health_max=" + health_max);
            }

            if (health > health_max)
            {
                health_max = health;
                AtavismLogger.LogInfoMessage("Got petStatHeandler set  health_max = health");
            }

          //  Debug.LogError("Pet 1 health=" + health + " health_max=" + health_max);
            try
            {
                if (activePetOid != null)
                {
                    object hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthMaxStat);
                    if (hm != null)
                        health_max = (int) hm;
                    hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthStat);
                    if (hm != null)
                        health = (int) hm;
                }
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Pet petStatHeandler Exception " + e.Message+"\n\n"+e.StackTrace);
            }

          //  Debug.LogError("Pet 2 health=" + health + " health_max=" + health_max);

            if (healthBar != null && health_max > 0)
                healthBar.value = (float) health / (float) health_max;
            AtavismLogger.LogInfoMessage("Got petStatHeandler End");
        }

        /// <summary>
        /// Function switching show / hide
        /// </summary>
        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }

        /// <summary>
        /// Function showing window of pet actions
        /// </summary>
        public void Show()
        {
            showing = true;
            GetComponent<CanvasGroup>().alpha = 1f;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            GetComponent<CanvasGroup>().interactable = true;
            AtavismUIUtility.BringToFront(this.gameObject);
            if (healthBar)
                healthBar.value = 1;

            if (passiveButtonText)
                passiveButtonText.color = defaultTextColor;
            if (passiveButtonTextTMP)
                passiveButtonTextTMP.color = defaultTextColor;
            if (defensiveButtonText)
                defensiveButtonText.color = selectedTextColor;
            if (defensiveButtonTextTMP)
                defensiveButtonTextTMP.color = selectedTextColor;
            if (aggressiveButtonText)
                aggressiveButtonText.color = defaultTextColor;
            if (aggressiveButtonTextTMP)
                aggressiveButtonTextTMP.color = defaultTextColor;
            if (stayButtonText)
                stayButtonText.color = defaultTextColor;
            if (stayButtonTextTMP)
                stayButtonTextTMP.color = defaultTextColor;
            if (followButtonText)
                followButtonText.color = selectedTextColor;
            if (followButtonTextTMP)
                followButtonTextTMP.color = selectedTextColor;
            if (attackButtonText)
                attackButtonText.color = defaultTextColor;
            if (attackButtonTextTMP)
                attackButtonTextTMP.color = defaultTextColor;

            try
            {
                if (activePetOid != null)
                {
                    object hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthMaxStat);
                    if (hm != null)
                        health_max = (int)hm;
                    hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthStat);
                    if (hm != null)
                        health = (int)hm;
                }
                //  Debug.LogError("Pet health=" + health + " health_max=" + health_max);
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Pet show Exception " + e.Message+"\n\n"+e.StackTrace);
            }
        }

        public void Hide()
        {
            showing = false;
            GetComponent<CanvasGroup>().alpha = 0f;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            if (activePetOid != null)
                if (ClientAPI.GetObjectNode(activePetOid.ToLong()) != null)
                {
                    ClientAPI.GetObjectNode(activePetOid.ToLong()).RemovePropertyChangeHandler(AtavismCombat.Instance.HealthMaxStat, PetHealthHandler);
                    ClientAPI.GetObjectNode(activePetOid.ToLong()).RemovePropertyChangeHandler(AtavismCombat.Instance.HealthStat, PetHealthHandler);
                }

            if (healthBar)
                healthBar.value = 1;
            activePetOid = null;
            node = null;
        }

        public void ActiveHandler(object sender, PropertyChangeEventArgs args)
        {
            try
            {
                AtavismLogger.LogInfoMessage("Pet activeHandler");
                bool activePet = (bool) ClientAPI.GetPlayerObject().GetProperty("hasPet");
                activePetOid = (OID) ClientAPI.GetPlayerObject().GetProperty("aP");
                AtavismLogger.LogInfoMessage("Pet activeHandler " + DateTime.Now + " activePet=" + activePet);
                //ClientAPI.GetObjectNode(activePetOid.ToLong()).RegisterPropertyChangeHandler("health-max", petHealthHandler);
                //ClientAPI.GetObjectNode(activePetOid.ToLong()).RegisterPropertyChangeHandler("health", petHealthHandler);
     //           Debug.Log("ActiveHandler " + pets.Count + " pets activePet="+activePet);
                if (!activePet)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Pet activeHandler Exception " + e.Message+"\n\n"+e.StackTrace);
            }
        }

        public void PetHealthHandler(object sender, PropertyChangeEventArgs args)
        {
            if (args.PropertyName.Equals(AtavismCombat.Instance.HealthStat))
            {
                object hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthStat);
                if (hm != null)
                    health = (int) hm;
                AtavismLogger.LogInfoMessage("Got petStatHeandler health=" + health);
            }

            if (args.PropertyName.Equals(AtavismCombat.Instance.HealthMaxStat))
            {
                object hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthMaxStat);
                if (hm != null)
                    health_max = (int) hm;

                AtavismLogger.LogInfoMessage("Got petStatHeandler health_max=" + health_max);
            }

            if (health > health_max)
            {
                health_max = health;
                AtavismLogger.LogInfoMessage("Got petStatHeandler set  health_max = health");
            }

           // Debug.LogError("Pet health=" + health + " health_max=" + health_max);
            if (healthBar != null && health_max > 0)
                healthBar.value = (float) health / (float) health_max;
        }



        public void PassiveCommand()
        {

            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand passive");
            if (passiveButtonText)
                passiveButtonText.color = selectedTextColor;
            if (passiveButtonTextTMP)
                passiveButtonTextTMP.color = selectedTextColor;
            if (defensiveButtonText)
                defensiveButtonText.color = defaultTextColor;
            if (defensiveButtonTextTMP)
                defensiveButtonTextTMP.color = defaultTextColor;
            if (aggressiveButtonText)
                aggressiveButtonText.color = defaultTextColor;
            if (aggressiveButtonTextTMP)
                aggressiveButtonTextTMP.color = defaultTextColor;

        }

        public void DefensiveCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand defensive");
            if (passiveButtonText)
                passiveButtonText.color = defaultTextColor;
            if (passiveButtonTextTMP)
                passiveButtonTextTMP.color = defaultTextColor;
            if (defensiveButtonText)
                defensiveButtonText.color = selectedTextColor;
            if (defensiveButtonTextTMP)
                defensiveButtonTextTMP.color = selectedTextColor;
            if (aggressiveButtonText)
                aggressiveButtonText.color = defaultTextColor;
            if (aggressiveButtonTextTMP)
                aggressiveButtonTextTMP.color = defaultTextColor;

        }

        public void AggressiveCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand aggressive");
            if (passiveButtonText)
                passiveButtonText.color = defaultTextColor;
            if (passiveButtonTextTMP)
                passiveButtonTextTMP.color = defaultTextColor;
            if (defensiveButtonText)
                defensiveButtonText.color = defaultTextColor;
            if (defensiveButtonTextTMP)
                defensiveButtonTextTMP.color = defaultTextColor;
            if (aggressiveButtonText)
                aggressiveButtonText.color = selectedTextColor;
            if (aggressiveButtonTextTMP)
                aggressiveButtonTextTMP.color = selectedTextColor;

        }

        public void StayCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand stay");

            if (stayButtonText)
                stayButtonText.color = selectedTextColor;
            if (stayButtonTextTMP)
                stayButtonTextTMP.color = selectedTextColor;
            if (followButtonText)
                followButtonText.color = defaultTextColor;
            if (followButtonTextTMP)
                followButtonTextTMP.color = defaultTextColor;

        }

        public void FollowCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand follow");

            if (stayButtonText)
                stayButtonText.color = defaultTextColor;
            if (stayButtonTextTMP)
                stayButtonTextTMP.color = defaultTextColor;
            if (followButtonText)
                followButtonText.color = selectedTextColor;
            if (followButtonTextTMP)
                followButtonTextTMP.color = selectedTextColor;

        }

        public void AttackCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand attack");

        }

        public void DespawnCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand despawn");
        }

        public void SelectPet()
        {
            if (activePetOid != null)
                ClientAPI.SetTarget(activePetOid.ToLong());
        }

    }
}