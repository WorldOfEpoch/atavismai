using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismVipWindow : UIAtavismWindowBase
    {

        [SerializeField] VisualTreeAsset vipListTemplate;
        private Label VipName;
        private Label VipTime;

        private Label VipStatus;
        private UIProgressBar VipProgress;
        private Label TitleVipA;
        private Label TitleVipB;
        private Button nextButton;
        private Button prevButton;
        public Color currentLevelTitleColor = Color.green;
        public Color defaultTitleColor = Color.white;
        private UIAtavismVipSlot prefab;
        private VisualElement grid;
        List<UIAtavismVipSlot> list = new List<UIAtavismVipSlot>();
        int levDisplay = 1;
        // Start is called before the first frame update

        protected override void registerEvents()
        {
            base.registerEvents();
            AtavismEventSystem.RegisterEvent("VIP_SHOW", this);
            AtavismEventSystem.RegisterEvent("VIPS_UPDATE", this);
            AtavismEventSystem.RegisterEvent("VIP_UPDATE", this);
            AtavismEventSystem.RegisterEvent("LOADING_SCENE_END", this);
        }

        protected override void unregisterEvents()
        {
            base.unregisterEvents();
            AtavismEventSystem.UnregisterEvent("VIP_SHOW", this);
            AtavismEventSystem.UnregisterEvent("VIPS_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("VIP_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("LOADING_SCENE_END", this);
        }

        protected override bool registerUI()
        {
            if (!base.registerUI())
                return false;
            VipName = uiWindow.Q<Label>("vip-name");
            VipTime = uiWindow.Q<Label>("vip-time");
            
            VipStatus = uiWindow.Q<Label>("vip-status");
            VipProgress = uiWindow.Q<UIProgressBar>("progress");
            TitleVipA = uiWindow.Q<Label>("vip-title-a");
            TitleVipB = uiWindow.Q<Label>("vip-title-b");
            grid = uiWindow.Q<VisualElement>("vip-grid");
            grid.Clear();

            nextButton = uiWindow.Q<Button>("next-button");
            nextButton.clicked += Next;
            prevButton = uiWindow.Q<Button>("prev-button");
            prevButton.clicked += Prev;
            return true;
        }

        void Start()
        {
           // AtavismVip.Instance.GetAllVips();
           // UpdateDisplay();
            // Hide();
            // if (VipTime!=null)
            // {
            //     defaultTimeColor = VipTime.color;
            // }
        }
        // Update is called once per frame
       protected override void Update()
        {
            base.Update();
            if(VipTime!=null)
            if (AtavismVip.Instance.GetTime != 0)
            {

                float timeLeft = AtavismVip.Instance.GetTimeElapsed - Time.time;
                int days = 0;
                int hours = 0;
                int minutes = 0;
                if (timeLeft > 86400)
                {
                    days = (int)timeLeft / 86400;
                    timeLeft -= days * 86400;
                }
                if (timeLeft > 3600)
                {
                    hours = (int)timeLeft / 3600;
                    timeLeft -= hours * 3600;
                }
                if (timeLeft > 60)
                {
                    minutes = (int)timeLeft / 60;
                    timeLeft = minutes * 60;
                }
                if (days > 0)
                {
#if AT_I2LOC_PRESET
                       VipTime.text =I2.Loc.LocalizationManager.GetTranslation("Expires in :") + days + " "+I2.Loc.LocalizationManager.GetTranslation("days");
#else
                        VipTime.text ="Expires in :" + days + " days";
#endif
                    VipTime.RemoveFromClassList("vip-expired");
                        // VipTime.color = defaultTimeColor;
                    }
                else if (hours > 0)
                {
#if AT_I2LOC_PRESET
                      VipTime.text =I2.Loc.LocalizationManager.GetTranslation("Expires in :") + hours + " "+I2.Loc.LocalizationManager.GetTranslation("hours");
#else
                        VipTime.text = "Expires in :" + hours + " hour";
#endif
                    VipTime.RemoveFromClassList("vip-expired");
                    }
                else if (minutes > 0 && timeLeft > 0)
                {
#if AT_I2LOC_PRESET
                       VipTime.text =I2.Loc.LocalizationManager.GetTranslation("Expires in :") + minutes + " "+I2.Loc.LocalizationManager.GetTranslation("minutes");
#else
                        VipTime.text = "Expires in :" + minutes + " minutes";
#endif
                    VipTime.RemoveFromClassList("vip-expired");
                    }
                else
                {
#if AT_I2LOC_PRESET
                       VipTime.text =I2.Loc.LocalizationManager.GetTranslation("Expired");
#else
                        VipTime.text = "Expired";
#endif
                        VipTime.AddToClassList("vip-expired");

                }
            }
            else
            {
                VipTime.text = "";
            }
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "VIP_SHOW")
            {
                Show();
                UpdateDisplay();
            } if (eData.eventType == "VIPS_UPDATE")
            {
                UpdateDisplay();
            }else if (eData.eventType == "VIP_UPDATE")
            {
                UpdateDisplay();
            } else if (eData.eventType == "LOADING_SCENE_END")
            {
                AtavismVip.Instance.GetAllVips();
                UpdateDisplay();
            }

        }

        void UpdateDisplay()
        {
           // Debug.LogError("VIP Window UpdateDisplay");
            if (VipName!=null)
            {
#if AT_I2LOC_PRESET
                VipName.text = I2.Loc.LocalizationManager.GetTranslation(AtavismVip.Instance.GetName);
#else
                VipName.text = AtavismVip.Instance.GetName;
#endif
            }
            if (VipProgress!=null)
            {
                if (AtavismVip.Instance.GetMaxPoints != 0)
                {
                    VipProgress.lowValue = 0;
                    VipProgress.highValue = AtavismVip.Instance.GetMaxPoints;
                    VipProgress.value = AtavismVip.Instance.GetPoints;
                    VipProgress.visible = true;
                    if (VipStatus!=null)
                    {
                        VipStatus.text = AtavismVip.Instance.GetPoints+" / "+ AtavismVip.Instance.GetMaxPoints;
                        // if (!VipStatus.visible)
                            VipStatus.visible = true;
                    }
                }
                else
                {
                    VipProgress.visible = false;
                    if (VipStatus!=null && VipStatus.visible)
                        VipStatus.visible = false;
                }
            }


             // Debug.LogError("VipFull: Start");
            if (levDisplay >= AtavismVip.Instance.GetVips.Count)
                levDisplay = 1;
            int c = 1;
            foreach (string s in AtavismVip.Instance.GetBonuseNames)
            {
               // Debug.LogError("VipFull: c="+c);
                if (c >= list.Count)
                {
                //    Debug.LogError("VipFull: Instantiate c=" + c);
                // Instantiate a controller for the data
                UIAtavismVipSlot newListEntryLogic = new UIAtavismVipSlot();
                // Instantiate the UXML template for the entry
                var newListEntry = vipListTemplate.Instantiate();
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);
                list.Add(newListEntryLogic);
                // Return the root of the instantiated visual tree
                grid.Add(newListEntry);
                }
                string vipA = "";
                string vipB = "";

                if (AtavismVip.Instance.GetVips.ContainsKey(levDisplay))
                {
                    if (TitleVipA!=null)
                    {
#if AT_I2LOC_PRESET
              TitleVipA.text= I2.Loc.LocalizationManager.GetTranslation( AtavismVip.Instance.GetVips[levDisplay].name);
#else
                        TitleVipA.text = AtavismVip.Instance.GetVips[levDisplay].name;
#endif
                        if (AtavismVip.Instance.GetLevel.Equals(AtavismVip.Instance.GetVips[levDisplay].level))
                            TitleVipA.style.color = currentLevelTitleColor;
                        else
                            TitleVipA.style.color = defaultTitleColor;

                    }
                    if (AtavismVip.Instance.GetVips[levDisplay].bonuses.ContainsKey(s))
                    {
                        if (AtavismVip.Instance.GetVips[levDisplay].bonuses[s].value != 0)
                        {
                            vipA = AtavismVip.Instance.GetVips[levDisplay].bonuses[s].value.ToString();
                        }
                        if (AtavismVip.Instance.GetVips[levDisplay].bonuses[s].percentage != 0)
                        {
                            if (AtavismVip.Instance.GetVips[levDisplay].bonuses[s].value != 0)
                            {
                                if (AtavismVip.Instance.GetVips[levDisplay].bonuses[s].percentage > 0)
                                    vipA += "\n&\n+" + AtavismVip.Instance.GetVips[levDisplay].bonuses[s].percentage +  " %";
                                else
                                    vipA += "\n&\n" + AtavismVip.Instance.GetVips[levDisplay].bonuses[s].percentage + " %";
                            }
                            else
                            {
                                if (AtavismVip.Instance.GetVips[levDisplay].bonuses[s].percentage > 0)
                                    vipA = "+" + AtavismVip.Instance.GetVips[levDisplay].bonuses[s].percentage + " %";
                                else
                                    vipA = AtavismVip.Instance.GetVips[levDisplay].bonuses[s].percentage + " %";
                            }
                        }
                    }
                    else
                    {
                        vipA = "-";
                    }

                }
                else
                {
                //   Debug.LogError("VipFull: Instantiate c=" + c+ " levDisplay="+ levDisplay+" level not on list");
                }

                if (AtavismVip.Instance.GetVips.ContainsKey(levDisplay+1))
                {
                    if (TitleVipB!=null)
                    {
                        TitleVipB.text = AtavismVip.Instance.GetVips[levDisplay+1].name;
                        if (AtavismVip.Instance.GetLevel.Equals(AtavismVip.Instance.GetVips[levDisplay+1].level))
                            TitleVipB.style.color = currentLevelTitleColor;
                        else
                            TitleVipB.style.color = defaultTitleColor;
                    }
                    if (AtavismVip.Instance.GetVips[levDisplay+1].bonuses.ContainsKey(s))
                    {
                        if (AtavismVip.Instance.GetVips[levDisplay+1].bonuses[s].value != 0)
                        {
                            vipB = AtavismVip.Instance.GetVips[levDisplay+1].bonuses[s].value.ToString();
                        }
                        if (AtavismVip.Instance.GetVips[levDisplay+1].bonuses[s].percentage != 0)
                        {
                            if (AtavismVip.Instance.GetVips[levDisplay + 1].bonuses[s].value != 0)
                            {
                                if (AtavismVip.Instance.GetVips[levDisplay + 1].bonuses[s].percentage > 0)
                                    vipB += "\n&\n+" + AtavismVip.Instance.GetVips[levDisplay + 1].bonuses[s].percentage + " %";
                                else
                                    vipB += "\n&\n" + AtavismVip.Instance.GetVips[levDisplay + 1].bonuses[s].percentage + " %";
                            }
                            else
                            {
                                if (AtavismVip.Instance.GetVips[levDisplay + 1].bonuses[s].percentage > 0)
                                    vipB = "+" + AtavismVip.Instance.GetVips[levDisplay + 1].bonuses[s].percentage + " %";
                                else
                                    vipB = AtavismVip.Instance.GetVips[levDisplay + 1].bonuses[s].percentage + " %";
                            }
                        }
                    }
                    else
                    {
                        vipB = "-";
                    }
                }
                else
                {
                    //  Debug.LogError("VipFull: Instantiate c=" + c + " levDisplay=" + (levDisplay+1) + " level not on list");
                    //  Debug.LogError("VipFull: Instantiate c=" + c + " levDisplay=" + (levDisplay+1) + " level not on list");
                    //  Debug.LogError("VipFull: Instantiate c=" + c + " levDisplay=" + (levDisplay+1) + " level not on list");
                    //  Debug.LogError("VipFull: Instantiate c=" + c + " levDisplay=" + (levDisplay+1) + " level not on list");
                    // Debug.LogError("VipFull: Instantiate c=" + c + " levDisplay=" + (levDisplay+1) + " level not on list");
                    // Debug.LogError("VipFull: Instantiate c=" + c + " levDisplay=" + (levDisplay+1) + " level not on list");
                    // Debug.LogError("VipFull: Instantiate c=" + c + " levDisplay=" + (levDisplay+1) + " level not on list");
                    // Debug.LogError("VipFull: Instantiate c=" + c + " levDisplay=" + (levDisplay+1) + " level not on list");
                }
                string name = s;
#if AT_I2LOC_PRESET
             name= I2.Loc.LocalizationManager.GetTranslation(name);
#endif
                 // Debug.LogError("VipFull: "+s+" "+vipA+" "+vipB);
                list[c - 1].UpdateDetaile(name, vipA, vipB);
                list[c - 1].Show();
                
                c++;
            }
            // Debug.LogError("VipFull: End");

        }
        public void Next()
        {
            levDisplay++;
            if (levDisplay >= AtavismVip.Instance.GetVips.Count - 1)
                levDisplay = AtavismVip.Instance.GetVips.Count - 1;
            UpdateDisplay();
        }
        public void Prev()
        {
            levDisplay--;
            if (levDisplay < 1)
                levDisplay = 1;
            UpdateDisplay();
        }
        public override void Show()
        {
            base.Show();
            // AtavismSocial.Instance.SendGetFriends();
            AtavismVip.Instance.GetAllVips();
            UpdateDisplay();
        }

        public override void Hide()
        {
            base.Hide();
        }

      
    }
}