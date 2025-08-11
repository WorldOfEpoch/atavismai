using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismPetCommands : UIAtavismWindowBase
    {


        [SerializeField] public UIProgressBar healthBar;
       
        VisualElement rootElement;
        [SerializeField] public Button defensiveButton;
        [SerializeField] public Button aggressiveButton;
        [SerializeField] public Button passiveButton;
        [SerializeField] public Button attackButton;
        [SerializeField] public Button despawnButton;
        [SerializeField] public Button stayButton;
        [SerializeField] public Button followButton;
        VisualElement _petListContainer;
        public VisualTreeAsset _petListAsset;
        int health = 0;
        int health_max = 0;
        private OID activePetOid;
        private AtavismObjectNode node;
        private Dictionary<int, List<long>> pets = new Dictionary<int, List<long>>();

        private List<UIAtavismPetProfileEntry> _petProfiles = new List<UIAtavismPetProfileEntry>();
        public string petInfo = "{{level}} ({{count}}/{{count_max}})"; 

        private void Awake()
        {

            rootElement = uiDocument.visualTreeAsset.CloneTree();
            GetComponent<UIDocument>().rootVisualElement.Add(rootElement);
            _petListContainer = rootElement.Q("pet-list-container");
            // Link the UI elements
            healthBar = rootElement.Q<UIProgressBar>("healthBar");
            defensiveButton = rootElement.Q<Button>("defensive-button");
            aggressiveButton = rootElement.Q<Button>("aggressive-button");
            passiveButton = rootElement.Q<Button>("passive-button");
            attackButton = rootElement.Q<Button>("attack-button");
            despawnButton = rootElement.Q<Button>("despawn-button");
            stayButton = rootElement.Q<Button>("stay-button");
            followButton = rootElement.Q<Button>("follow-button");


            // Add callbacks
            passiveButton.RegisterCallback<ClickEvent>(ev => PassiveCommand());
            defensiveButton.RegisterCallback<ClickEvent>(ev => DefensiveCommand());
            aggressiveButton.RegisterCallback<ClickEvent>(ev => AggressiveCommand());
            stayButton.RegisterCallback<ClickEvent>(ev => StayCommand());
            attackButton.RegisterCallback<ClickEvent>(ev => AttackCommand());
            despawnButton.RegisterCallback<ClickEvent>(ev => DespawnCommand());
            passiveButton.RegisterCallback<ClickEvent>(ev => SelectPet());
            followButton.RegisterCallback<ClickEvent>(ev => FollowCommand());
            Hide();


        }

        protected override void registerExtensionMessages()
        {
            base.registerExtensionMessages();
            NetworkAPI.RegisterExtensionMessageHandler("ao.pet_stats", PetStatHandler);
            NetworkAPI.RegisterExtensionMessageHandler("petListUpdate", PetListHandler);
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler("hasPet", ActiveHandler);
        }

        protected override void unregisterExtensionMessages()
        {
            if(ClientAPI.GetPlayerObject()!=null)
                ClientAPI.GetPlayerObject().RemovePropertyChangeHandler("hasPet", ActiveHandler);
            NetworkAPI.RemoveExtensionMessageHandler("ao.pet_stats", PetStatHandler);
            NetworkAPI.RemoveExtensionMessageHandler("petListUpdate", PetListHandler);
            base.unregisterExtensionMessages();
            
        }

        private void PetListHandler(Dictionary<string, object> props)
        {
            AtavismLogger.LogInfoMessage("Got petListHandler");
            // AtavismLogger.LogInfoMessage("Got petListHandler");
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
                // Debug.LogError("PET profiles "+num);
                for (int i = 0; i < num; i++)
                {
                    int profileId = (int)props["p" + i];
                    if (!pets.ContainsKey(profileId))
                        pets.Add(profileId, new List<long>());
                    int numPets = (int)props["p" + i + "num"];
                    // Debug.LogError("PET profileId "+profileId+" pets "+numPets);
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
          //  Debug.Log("PetListHandler " + pets.Count + " pets");
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
                _petProfiles.Clear();
                _petListContainer.Clear();
                foreach (var pet in pets.Keys)
                {
                    if (pets[pet].Count > 0)
                    {
                        long petOid = pets[pet][0];
                        AtavismObjectNode node = ClientAPI.GetObjectNode(petOid);

                        if (node == null)
                        {
                            // Debug.LogError("UpdatePetList node is null");
                            done = false;
                            break;
                        }

                        UIAtavismPetProfileEntry entry = new UIAtavismPetProfileEntry();
                        // Instantiate the UXML template for the entry
                        var newListEntry = _petListAsset.Instantiate();
                        // Assign the controller script to the visual element
                        newListEntry.userData = entry;
                        // Initialize the controller script
                        entry.SetVisualElement(newListEntry);
                        Texture2D icon = null;
                        if (node != null && node.GameObject != null)
                        {
                            AtavismMobAppearance ama = node.GameObject.GetComponent<AtavismMobAppearance>();
                            if (ama != null && ama.portraitIcon != null)
                                icon = ama.portraitIcon.texture;
                        }

                        //    Debug.LogError("Node "+node.Name+" at pet list "+node.Oid+" at pet list "+pet);
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
                        entry.UpdateData(icon, info, petOid);
                        _petListContainer.Add(newListEntry);
                        _petProfiles.Add(entry);
                    }
                    //  done = true;
                }

                yield return new WaitForSeconds (1f);

            }

        }

        private void PetStatHandler(Dictionary<string, object> props)
        {
            AtavismLogger.LogInfoMessage("Got petStatHandler");

            if (props.ContainsKey(AtavismCombat.Instance.HealthStat))
            {
                health = (int) props[AtavismCombat.Instance.HealthStat];
                AtavismLogger.LogInfoMessage("Got petStatHandler health=" + health);
            }

            if (props.ContainsKey(AtavismCombat.Instance.HealthMaxStat))
            {
                health_max = (int) props[AtavismCombat.Instance.HealthMaxStat];
                AtavismLogger.LogInfoMessage("Got petStatHandler health_max=" + health_max);
            }

            if (health > health_max)
            {
                health_max = health;
                AtavismLogger.LogInfoMessage("Got petStatHandler set  health_max = health");
            }

            //  Debug.LogError("Pet 1 health=" + health + " health_max=" + health_max);
            try
            {
                if (activePetOid != null)
                {
                    object hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(),
                        AtavismCombat.Instance.HealthMaxStat);
                    if (hm != null)
                        health_max = (int) hm;
                    hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthStat);
                    if (hm != null)
                        health = (int) hm;
                }
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Pet petStatHandler Exception " + e.Message + "\n\n" + e.StackTrace);
            }

            //  Debug.LogError("Pet 2 health=" + health + " health_max=" + health_max);

            if (healthBar != null && health_max > 0)
            {
                healthBar.highValue = health_max;
                healthBar.value = health ;
            }
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
        public override void Show()
        {
            base.Show(); // Calls the Show() of UIAtavismWindowBase

            showing = true;

            if (healthBar != null)
                healthBar.value = 1;

            if (passiveButton != null)
                passiveButton.RemoveFromClassList("pet-button-selected");
            if (defensiveButton != null)
                defensiveButton.AddToClassList("pet-button-selected");
            if (aggressiveButton != null)
                aggressiveButton.RemoveFromClassList("pet-button-selected");
            if (stayButton != null)
                stayButton.RemoveFromClassList("pet-button-selected");
            if (followButton != null)
                followButton.AddToClassList("pet-button-selected");
            if (attackButton != null)
                attackButton.RemoveFromClassList("pet-button-selected");
            if (despawnButton != null)
                despawnButton.RemoveFromClassList("pet-button-selected"); // Assuming default color for now

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
            }
            catch (Exception e)
            {
                AtavismLogger.LogError("Pet show Exception " + e.Message + "\n\n" + e.StackTrace);
            }
        }



        public override void Hide()
        {
            base.Hide(); // Calls the Hide() of UIAtavismWindowBase

            showing = false;

            if (activePetOid != null)
            {
                if (ClientAPI.GetObjectNode(activePetOid.ToLong()) != null)
                {
                    ClientAPI.GetObjectNode(activePetOid.ToLong())
                        .RemovePropertyChangeHandler(AtavismCombat.Instance.HealthMaxStat, PetHealthHandler);
                    ClientAPI.GetObjectNode(activePetOid.ToLong())
                        .RemovePropertyChangeHandler(AtavismCombat.Instance.HealthStat, PetHealthHandler);
                }
            }

            if (healthBar != null)
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
                if (activePet && activePetOid != null && ClientAPI.GetObjectNode(activePetOid.ToLong()) != null)
                {
                    ClientAPI.GetObjectNode(activePetOid.ToLong()).RegisterPropertyChangeHandler("health-max", PetHealthHandler);
                    ClientAPI.GetObjectNode(activePetOid.ToLong()).RegisterPropertyChangeHandler("health", PetHealthHandler);
                }
                Debug.Log("ActiveHandler " + pets.Count + " pets activePet="+activePet);

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
                AtavismLogger.LogError("Pet activeHandler Exception " + e.Message + "\n\n" + e.StackTrace);
            }
        }

        public void PetHealthHandler(object sender, PropertyChangeEventArgs args)
        {
            if (args.PropertyName.Equals(AtavismCombat.Instance.HealthStat))
            {
                object hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthStat);
                if (hm != null)
                    health = (int) hm;
                AtavismLogger.LogInfoMessage("Got petStatHandler health=" + health);
            }

            if (args.PropertyName.Equals(AtavismCombat.Instance.HealthMaxStat))
            {
                object hm = ClientAPI.GetObjectProperty(activePetOid.ToLong(), AtavismCombat.Instance.HealthMaxStat);
                if (hm != null)
                    health_max = (int) hm;

                AtavismLogger.LogInfoMessage("Got petStatHandler health_max=" + health_max);
            }

            if (health > health_max)
            {
                health_max = health;
                AtavismLogger.LogInfoMessage("Got petStatHandler set  health_max = health");
            }

            // Debug.LogError("Pet health=" + health + " health_max=" + health_max);
            if (healthBar != null && health_max > 0)
            {
                healthBar.highValue = health_max;
                healthBar.value = health ;
            }

        }


        public void PassiveCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand passive");
            if (passiveButton != null)
                passiveButton.AddToClassList("pet-button-selected");
            if (defensiveButton != null)
                defensiveButton.RemoveFromClassList("pet-button-selected");
            if (aggressiveButton != null)
                aggressiveButton.RemoveFromClassList("pet-button-selected");
        }

        public void DefensiveCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand defensive");
            if (passiveButton != null)
                passiveButton.RemoveFromClassList("pet-button-selected");
            if (defensiveButton != null)
                defensiveButton.AddToClassList("pet-button-selected");
            if (aggressiveButton != null)
                aggressiveButton.RemoveFromClassList("pet-button-selected");
        }

        public void AggressiveCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand aggressive");
            if (passiveButton != null)
                passiveButton.RemoveFromClassList("pet-button-selected");
            if (defensiveButton != null)
                defensiveButton.RemoveFromClassList("pet-button-selected");
            if (aggressiveButton != null)
                aggressiveButton.AddToClassList("pet-button-selected");
        }

        public void StayCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand stay");
            if (stayButton != null)
                stayButton.AddToClassList("pet-button-selected");
            if (followButton != null)
                followButton.RemoveFromClassList("pet-button-selected");
        }

        public void FollowCommand()
        {
            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/petCommand follow");
            if (stayButton != null)
                stayButton.RemoveFromClassList("pet-button-selected");
            if (followButton != null)
                followButton.AddToClassList("pet-button-selected");
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