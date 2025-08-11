using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    // [Serializable]
    // public class DamageType
    // {
    //     public string type ;
    //     public Color damageColor;
    //     public Color criticDamageColor;
    //     
    // }
    
    public class UIAtavismFloatingMobPanel 
    {

        public UIAtavismFloatingMobPanelController FloatingPanelController;
        public int ThisPanelID;

        public VisualElement m_root;
        
        public Label m_name;
        public Label m_tag; // Guild names will be shown here
        public Label m_level;
        //public Text combatText;  //TODO Create an object pool of "Text" so more than one combat event can be seen.
        public Label m_combat;
        // public TMP_FontAsset CombatTextFont, MessageFont, XPFont, CombatHealFont, CombatHealCritFont, BuffGainedFont, DebuffGainedFont, SelfDamageFont, SelfDamageCritFont, EnemyDamageCritFont;
        public VisualElement m_chatPanel;
        public Label m_chat;
        public bool usingWorldSpaceCanvas = false;
        public bool faceCamera = true;
        public float renderDistance = 50f;
        public float chatDisplayTime = 3f;
        
        // public Color friendlyNameColour = Color.green;
        // public Color neutralNameColour = Color.yellow;
        // public Color enemyNameColour = Color.red;
        
     //  public Color myDamageColour = Color.red;
       // public Color targetDamageColour = Color.white;
        // public Color myMessageColour = Color.yellow;
        // public Color targetMessageColour = Color.yellow;
        // public Color XPTextColor = Color.white;
        // public Color HealColor;
        // public Color CriticalHealColor;
        // public Color BuffGainedColor;
        // public Color DebuffGainedColor;
       // public Color MagicalDamageColor;
        // public Color DamageColor;
        // public Color SelfDamageColor;
        // public Color CriticalDamageColor;
      //  public Color MagicalCriticalDamageColor;
        // public List<DamageType> damageTypeColor = new  List<DamageType>();
        public float CriticalSizeUpRate;
        public float CriticalSizeDownRate;

        //public Image NpcIcon;
        //public TextMeshPro textMeshPro;
       // public TextMeshProUGUI textMeshProUgui;
        float addNameHeight = 0f;
        AtavismObjectNode mobNode;
        float stopDisplay;
        float stopChatDisplay;

        float combatDisplayTime = 1.5f;
         float NormalDamageDisplayTime = 2f;
         float CriticalDmgDisplayTime = 2f;

        private float initialScaleFactor = 1.5f;
        public float initialAlphaFactor = 3f;
        // public float combatTextSpeed = 60f;
        public float combatColorSpeed = 1f;
        public float combatTextScaleDownSpeed = 3f;

        //Set automatically
        Vector3 startPosition;
        Vector3 startScale;
        float currentScale = 1f;
        float currentAlpha = 1f;
        float currentOffset = 0;
        bool showName = true;

        public bool IsCritical, MaxCritSizedReached;
       public float ThisTextTime;


       public void Setup(VisualElement visualElement)
       {
           m_root = visualElement.Q<VisualElement>("container");
           m_name = m_root.Q<Label>("name");
           m_combat = m_root.Q<Label>("combat");
           m_tag = m_root.Q<Label>("tag");
           m_chatPanel = m_root.Q<VisualElement>("chatPanel");
           m_chat = m_root.Q<Label>("chat");
           m_level = m_root.Q<Label>("level");
           if (m_combat != null)
           {
               this.startPosition = m_combat.transform.position;
               this.startScale = m_combat.transform.scale;
               m_combat.text = "";
           }
           this.currentScale = initialScaleFactor;
           this.currentAlpha = initialAlphaFactor;
           if (m_chat != null)
           {
               m_chat.text = "";
           }

       }
       
     

        void OnDestroy()
        {
            if (mobNode != null)
            {
                mobNode.RemovePropertyChangeHandler("level", LevelHandler);
                mobNode.RemovePropertyChangeHandler("reaction", TargetTypeHandler);
                mobNode.RemovePropertyChangeHandler("adminLevel", AdminLevelHandler);
                mobNode.RemovePropertyChangeHandler("nameDisplay", NameDisplayHandler);
                mobNode.RemovePropertyChangeHandler("guildName", GuildNameDisplayHandler);
                mobNode.RemovePropertyChangeHandler("level", LevelHandler);
                mobNode.RemovePropertyChangeHandler("questavailable", QuestAvailableHandler);
                mobNode.RemovePropertyChangeHandler("questinprogress", QuestInProgressHandler);
                mobNode.RemovePropertyChangeHandler("questconcludable", QuestConcludableHandler);
                mobNode.RemovePropertyChangeHandler("dialogue_available", DialogueAvailableHandler);
                mobNode.RemovePropertyChangeHandler("itemstosell", ItemsToSellHandler);
                mobNode.RemovePropertyChangeHandler("mount", HandleMount);
            }
        }

        // Update is called once per frame
        public void RunUpdate(Camera cam)
        {
            //GUI.color = new Color (1.0f, 1.0f, 1.0f, 1.0f - (cameraDistance - fadeDistance) / (hideDistance - fadeDistance));
            if (mobNode == null || (mobNode!=null && mobNode.MobController == null))
            {
                return;
            }

            Vector3 worldPos = new Vector3(mobNode.Position.x, mobNode.Position.y + mobNode.MobController.nameHeight + addNameHeight, mobNode.Position.z);
            if (!usingWorldSpaceCanvas)
            {
                //GetComponent<CanvasGroup>().alpha = 1f;
                Vector3 screenPos = cam.WorldToViewportPoint(worldPos);
                Vector2 size = new Vector2(FloatingPanelController.uiDocument.rootVisualElement.resolvedStyle.width, FloatingPanelController.uiDocument.rootVisualElement.resolvedStyle.height) * 0.5f;
               var v3 = new Vector3((screenPos.x * FloatingPanelController.uiDocument.rootVisualElement.resolvedStyle.width), (screenPos.y * FloatingPanelController.uiDocument.rootVisualElement.resolvedStyle.height), 0f);
                //var v3 =UIAtavismMiniMapUtils.CalculateMiniMapPosition(screenPos, FloatingPanelController.uiDocument.rootVisualElement);
               
                // Debug.LogError("World Space Canvas "+screenPos.ToString());
                m_root.style.bottom = v3.y;
                m_root.style.left = v3.x;
                // m_root.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            }
            else
            {
                m_root.transform.position = worldPos;
                if (faceCamera)
                {
                    m_root.transform.rotation = cam.transform.rotation;
                }
                else
                {
                    Quaternion cameraRotation = cam.transform.rotation;
                    cameraRotation = Quaternion.Euler(0, cameraRotation.eulerAngles.y, 0);
                    m_root.transform.rotation = cameraRotation;
                }

            }

            UpdateCombatText();

            /*  if (Time.time > stopChatDisplay)
              {
                  HideChatBubble();
              }*/
        }

        void ResetCombatText()
        {
            currentOffset = 0;
            currentAlpha = initialAlphaFactor;
            if (m_combat != null)
                m_combat.text = "";

            currentScale = initialScaleFactor;
            if (m_combat != null)
            {
                m_combat.transform.position = startPosition;
                m_combat.transform.scale = startScale;

                m_combat.transform.position = startPosition + new Vector3(0, currentOffset, 0);

                // var color = combatText.color;
                // color.a = currentAlpha;
                // combatText.color = color;

                m_combat.transform.scale = startScale * currentScale;

                foreach (var type in FloatingPanelController.dmgType)
                {
                    m_combat.RemoveFromClassList("floating-panel-damage-"+type+"-self");
                    m_combat.RemoveFromClassList("floating-panel-damage-"+type);
                    m_combat.RemoveFromClassList("floating-panel-damage-"+type+"-critic-self");
                    m_combat.RemoveFromClassList("floating-panel-damage-"+type+"-critic");

                    m_combat.RemoveFromClassList("floating-panel-heal-"+type+"-self");
                    m_combat.RemoveFromClassList("floating-panel-heal-"+type);
                    m_combat.RemoveFromClassList("floating-panel-heal-"+type+"-critic-self");
                    m_combat.RemoveFromClassList("floating-panel-heal-"+type+"-critic");


                }
                m_combat.RemoveFromClassList("floating-panel-exp");
                m_combat.RemoveFromClassList("floating-panel-buff-gain-target");
                m_combat.RemoveFromClassList("floating-panel-buff-gain");
                m_combat.RemoveFromClassList("floating-panel-debuff-gain-target");
                m_combat.RemoveFromClassList("floating-panel-debuff-gain");
                foreach (var eventType in FloatingPanelController.eventTypes)
                {
                    m_combat.RemoveFromClassList("floating-panel-event" + eventType + "-self");
                    m_combat.RemoveFromClassList("floating-panel-event" + eventType + "-target");
                }
            }
            startPosition.x = 0;
            IsCritical = false;
            MaxCritSizedReached = false;
            ThisTextTime = 0;
        }

        void UpdateCombatText()
        {
            if (stopDisplay == 0)
                return;
            if (Time.time > stopDisplay)
            {
              //  Debug.LogError("UpdateCombatText stop TIME");

                FloatingPanelController.ALLcombatTextData[ThisPanelID].CurrentNode = null;
                FloatingPanelController.ALLcombatTextData[ThisPanelID].CurrentCliamObject=null;
                FloatingPanelController.ALLcombatTextData[ThisPanelID].RendererReference = null;
                FloatingPanelController.ALLcombatTextData[ThisPanelID].LastTimeUsed = 0;
                 FloatingPanelController.ALLcombatTextData[ThisPanelID].FloatingPanelGO.HideVisualElement();


                stopDisplay = 0;

                if (m_combat != null)
                    m_combat.text = "";



                if (m_chat != null)
                {
                    m_chat.text = "";
                }

                if (m_chatPanel != null)
                    m_chatPanel.HideVisualElement();

                return;
            }

            //Update the current position.
            if (!IsCritical)
            {
                currentOffset += Time.deltaTime * FloatingPanelController.combatTextSpeed;
                if (m_combat != null)
                {
                
                        m_combat.style.bottom = startPosition.y + currentOffset; 
//                    Debug.LogError("POSITION " +m_root.transform.position+" "+startPosition +" "+( new Vector3(0, currentOffset, 0)));
                    //m_combat.transform.position = startPosition + new Vector3(0, currentOffset, 0);
                }


                //Update the current alpha channel.
                currentAlpha = Mathf.Max(0, currentAlpha - Time.deltaTime * combatColorSpeed);
                if (m_combat != null)
                    m_combat.style.opacity = currentAlpha;
                // if (combatText != null)
                // {
                //     var color = combatText.color;
                //     color.a = currentAlpha;
                //     combatText.color = color;
                // }

                //Update the current scale.
                currentScale = Mathf.Max(1f, currentScale - Time.deltaTime * combatTextScaleDownSpeed);
                if (m_combat != null)
                    m_combat.transform.scale = startScale * currentScale;

            }

            else
            {

                //Update the current alpha channel.
                currentAlpha = Mathf.Max(0, currentAlpha - Time.deltaTime * combatColorSpeed);
                // if (combatText != null)
                // {
                //     var color = combatText.color;
                //     color.a = currentAlpha;
                //     combatText.color = color;
                // }
              
                if (m_combat != null)
                    m_combat.style.opacity = currentAlpha;
                //Update the current scale.


                if (m_combat.transform.scale.x <= 1.5f)
                {
                    if (!MaxCritSizedReached)
                    {
                        float newscale = m_combat.transform.scale.x + CriticalSizeUpRate;

                        m_combat.transform.scale = new Vector3(newscale, newscale, newscale);
                    }
                }
                else
                {
                    MaxCritSizedReached = true;
                    if (m_combat.transform.scale.x > 1)
                    {
                        float newscale = m_combat.transform.scale.x - CriticalSizeDownRate;

                        m_combat.transform.scale = new Vector3(newscale, newscale, newscale);
                    }
                }



            }
        }

        public void SetMobDetails(AtavismObjectNode mobNode, bool showName)
        {
            this.mobNode = mobNode;
            if(mobNode!=null)
                m_root.name = "mob-"+mobNode.Name+"-"+this.mobNode.Oid;
            this.showName = showName;
            if (showName && mobNode != null)
            {
                mobNode.RegisterPropertyChangeHandler("level", LevelHandler);
                mobNode.RegisterPropertyChangeHandler("reaction", TargetTypeHandler);
                mobNode.RegisterPropertyChangeHandler("adminLevel", AdminLevelHandler);
                mobNode.RegisterPropertyChangeHandler("nameDisplay", NameDisplayHandler);
                mobNode.RegisterPropertyChangeHandler("guildName", GuildNameDisplayHandler);
                mobNode.RegisterPropertyChangeHandler("questavailable", QuestAvailableHandler);
                mobNode.RegisterPropertyChangeHandler("questinprogress", QuestInProgressHandler);
                mobNode.RegisterPropertyChangeHandler("questconcludable", QuestConcludableHandler);
                mobNode.RegisterPropertyChangeHandler("dialogue_available", DialogueAvailableHandler);
                mobNode.RegisterPropertyChangeHandler("itemstosell", ItemsToSellHandler);
                mobNode.RegisterPropertyChangeHandler("mount", HandleMount);
            }
            UpdateNameDisplay(showName);

        }

        private void HandleMount(object sender, PropertyChangeEventArgs args)
        {
            if (ClientAPI.GetObjectNode(mobNode.Oid).Parent != null)
            {
                CharacterController col = ClientAPI.GetObjectNode(mobNode.Oid).Parent.GetComponent<CharacterController>();
                if (col != null)
                    addNameHeight = col.height * ClientAPI.GetObjectNode(mobNode.Oid).Parent.transform.localScale.y;
            }
            else
            {
                addNameHeight = 0f;
            }
        }

        void UpdateNameDisplay(bool showName)
        {
            if (mobNode != null)
                if (mobNode.PropertyExists("nameDisplay") && !mobNode.CheckBooleanProperty("nameDisplay"))
                {
                    showName = false;
                }
            if (mobNode != null)
                if (ClientAPI.GetObjectNode(mobNode.Oid) != null && ClientAPI.GetObjectNode(mobNode.Oid).Parent != null)
                {
                    CharacterController col = ClientAPI.GetObjectNode(mobNode.Oid).Parent.GetComponent<CharacterController>();
                    if (col != null)
                        addNameHeight = col.height * ClientAPI.GetObjectNode(mobNode.Oid).Parent.transform.localScale.y;
                }
                else
                {
                    addNameHeight = 0f;
                }
            if (mobNode!=null && showName)
            {
                if (m_level != null && mobNode.PropertyExists("level"))
                {
                    int mobLevel = (int)mobNode.GetProperty("level");
                    if (ClientAPI.GetPlayerObject().PropertyExists("level"))
                    {
                        int playerLevel = (int)ClientAPI.GetPlayerObject().GetProperty("level");
                        if (mobLevel - playerLevel > 5)
                        {
                            m_level.AddToClassList("floating-panel-target-level-high");
                        }
                        else if (playerLevel - mobLevel > 5)
                        {
                            m_level.AddToClassList("floating-panel-target-level-low");
                        }
                    }

                    if (m_name != null && showName)
                    {
                        m_name.text = mobNode.Name;
                        m_name.ShowVisualElement();
                    }
#if AT_I2LOC_PRESET
                m_level.text = "[" + I2.Loc.LocalizationManager.GetTranslation("Level") + " " + mobLevel + "]";
#else
                    m_level.text = "[" + "Level" + " " + mobLevel + "]";
#endif
                }
                else
                {
                    if (m_name != null && showName)
                    {
                        m_name.text = mobNode.Name;
                        m_name.ShowVisualElement();
                    }

                    if (m_level != null)
                        m_level.text = "";
                }
                // Show tag if the player is in a Guild
                if (m_tag != null)
                {
                    if (mobNode.PropertyExists("guildName"))
                    {
                        string guildName = (string)mobNode.GetProperty("guildName");
                        if (guildName != null && guildName != "")
                        {
                            m_tag.text = "<" + guildName + ">";
                        }
                        else
                        {
                            m_tag.text = "";
                        }
                    }
                    else
                    {
                        m_tag.text = "";
                    }
                }

                // Set name colour based on target type
                if (m_name != null)
                    m_name.AddToClassList("floating-panel-name-neutral");
                if (m_tag != null)
                {
                    m_tag.AddToClassList("floating-panel-name-neutral");
                }
                if (mobNode.PropertyExists("reaction"))
                {
                    int targetType = (int)mobNode.GetProperty("reaction");
                    if (targetType < 0)
                    {
                        if (m_name != null)
                            m_name.AddToClassList("floating-panel-name-enemy");
                        if (m_tag != null)
                        {
                            m_tag.AddToClassList("floating-panel-name-enemy");
                        }
                    }
                    else if (targetType > 0)
                    {
                        if (m_name != null)
                            m_name.AddToClassList("floating-panel-name-friendly");
                        if (m_tag != null)
                        {
                            m_tag.AddToClassList("floating-panel-name-friendly");
                        }
                    }
                }
            }
            else
            {
                if (m_name != null)
                    m_name.text = "";
                if (m_level != null)
                {
                    m_level.text = "";
                }
                if (m_tag != null)
                {
                    m_tag.text = "";
                }
            }

            // Show admin Icon?
            /*if (mobNode != null && mobNode.PropertyExists("adminLevel")) {
                int adminLevel = (int)mobNode.GetProperty("adminLevel");
                adminIcon.gameObject.SetActive(adminLevel == 5);
            } else {
                adminIcon.gameObject.SetActive(false);
            }*/
        }

        public void ShowCombatText(string message, string eventType, string damageType)
        {
          //  Debug.LogError("ShowCombatText message="+message+" eventType="+eventType+" damageType="+damageType);
            if (eventType == "CombatBuffGained" || eventType == "CombatDebuffGained" || eventType == "CombatDebuffLost" || eventType == "CombatBuffLost" || eventType == "CombatAbilityLearned")
            {
                return;
            }

            ResetCombatText();
            if (m_combat != null)
            {
                m_combat.text = message;
            }

            float screenHeight = FloatingPanelController.uiDocument.rootVisualElement.resolvedStyle.height;

            ThisTextTime = Time.time;

            //stopDisplay = Time.time + combatDisplayTime;
             //    Debug.LogError(" ShowCombatText " + message + " | " + eventType);
            // Change colour based on eventType

            
           // Debug.LogError("Float  "+m_root.transform.position+" "+FloatingPanelController.uiDocument.rootVisualElement.resolvedStyle.height);
            
            
            stopDisplay = Time.time + NormalDamageDisplayTime;




            if (eventType == "CombatDamage")
            {
             //   Debug.LogError("ShowCombatText CombatDamage");
                // print("in combat physical damage");

                if (mobNode is AtavismPlayer)
                {
                //    Debug.LogError("ShowCombatText CombatDamage AtavismPlayer");
                    if (mobNode.Oid == ClientAPI.GetPlayerOid())
                    {
                     //   Debug.LogError("ShowCombatText CombatDamage Player Self");

                     if (m_combat != null)
                     {
                         m_combat.AddToClassList("floating-panel-damage-"+damageType+"-self");

                         m_combat.text = "- " + message;
                         startPosition.x = 225;
                     }
                    }
                    else
                    {
                      //  Debug.LogError("ShowCombatText CombatDamage player not Self");

                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-damage-"+damageType);
                       

                            float RdmX = UnityEngine.Random.Range(-70, 70);
                            float RdmY = UnityEngine.Random.Range(-70, 70);

                            startPosition.x = RdmX;
                            startPosition.y = screenHeight + RdmY;
                        }
                    }
                }
                else
                {
                //    Debug.LogError("ShowCombatText CombatDamage no player "+CombatTextFont);

                    if (m_combat != null){
                        m_combat.AddToClassList("floating-panel-damage-"+damageType);

                        float RdmX = UnityEngine.Random.Range(-70, 70);
                        float RdmY = UnityEngine.Random.Range(-70, 70);

                        startPosition.x = RdmX;
                        startPosition.y = screenHeight+ RdmY;
                    }
                }
            }
            else if (eventType == "CombatDamageCritical")
            {
                IsCritical = true;
                stopDisplay = Time.time + CriticalDmgDisplayTime;

                if (mobNode is AtavismPlayer)
                {
                    if (mobNode.Oid == ClientAPI.GetPlayerOid())
                    {
                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-damage-"+damageType+"-critic-self");
                       
                            if(message.Equals("0"))
                                m_combat.text = message;
                            else
                                m_combat.text = "- " + message;
                            startPosition.x = 225;
                        }
                    }
                    else
                    {
                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-damage-"+damageType+"-critic");

                            float RdmX = UnityEngine.Random.Range(-70, 70);
                            float RdmY = UnityEngine.Random.Range(-70, 70);

                            startPosition.x = RdmX;
                            startPosition.y = RdmY;
                        }
                    }
                }
                else
                {
                    if (m_combat != null){
                        m_combat.AddToClassList("floating-panel-damage-"+damageType+"-critic");

                        float RdmX = UnityEngine.Random.Range(-70, 70);
                        float RdmY = UnityEngine.Random.Range(-70, 70);

                        startPosition.x = RdmX;
                        startPosition.y = RdmY;
                    }
                }
            }
        
            else if (eventType == "CombatExpGained")
            {
                if (m_combat != null){
                    m_combat.text = "Exp: " + message;
                    m_combat.AddToClassList("floating-panel-exp");
                    startPosition.x = 0;
                }
            }
            if (eventType == "CombatHeal")
            {
                if (mobNode is AtavismPlayer)
                {
                    if (mobNode.Oid == ClientAPI.GetPlayerOid())
                    {
                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-heal-"+damageType+"-self");
                            m_combat.text = "+ " + message;
                            startPosition.x = -225;
                        }
                    }
                    else
                    {
                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-heal-"+damageType);

                            float RdmX = UnityEngine.Random.Range(-70, 70);
                            float RdmY = UnityEngine.Random.Range(-70, 70);

                            startPosition.x = RdmX;
                            startPosition.y = RdmY;
                        }
                    }
                }
                else
                {
                    if (m_combat != null){
                        m_combat.AddToClassList("floating-panel-heal-"+damageType);;
                        
                        float RdmX = UnityEngine.Random.Range(-70, 70);
                        float RdmY = UnityEngine.Random.Range(-70, 70);

                        startPosition.x = RdmX;
                        startPosition.y = RdmY;
                    }
                }

            }
            else  if (eventType == "CombatHealCritical")
            {
                IsCritical = true;
                stopDisplay = Time.time + CriticalDmgDisplayTime;
                if (mobNode is AtavismPlayer)
                {
                    if (mobNode.Oid == ClientAPI.GetPlayerOid())
                    {
                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-heal-"+damageType+"-critic-self");
                            m_combat.text = "+ " + message;
                            startPosition.x = -225;
                        }
                    }
                    else
                    {
                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-heal-"+damageType+"-critic");

                            float RdmX = UnityEngine.Random.Range(-70, 70);
                            float RdmY = UnityEngine.Random.Range(-70, 70);

                            startPosition.x = RdmX;
                            startPosition.y = RdmY;
                        }
                    }
                }
                else
                {
                    if (m_combat != null){
                        m_combat.AddToClassList("floating-panel-heal-"+damageType+"-critic");

                        float RdmX = UnityEngine.Random.Range(-70, 70);
                        float RdmY = UnityEngine.Random.Range(-70, 70);

                        startPosition.x = RdmX;
                        startPosition.y = RdmY;
                    }
                }

            }
            else if (eventType == "CombatBuffGained")
            {
                if (mobNode is AtavismPlayer)
                {
                    if (m_combat != null){
                        m_combat.AddToClassList( "floating-panel-buff-gain");
                        startPosition.x = 0;
                    }
                }
                else
                {
                    if (m_combat != null){
                        m_combat.AddToClassList( "floating-panel-buff-gain-target");
                        startPosition.x = 0;
                    }
                }
            }
            else if (eventType == "CombatDebuffGained")
            {
                if (mobNode is AtavismPlayer)
                {
                    if (m_combat != null){
                        m_combat.AddToClassList( "floating-panel-debuff-gain");
                        startPosition.x = 0;
                    }
                }
                else
                {
                    if (m_combat != null){
                        m_combat.AddToClassList( "floating-panel-debuff-gain-target");
                        startPosition.x = 0;
                    }
                }
            }
            else
            {



                if (eventType != "CombatDebuffGained" && eventType != "CombatBuffGained" && eventType != "CombatHeal" && eventType != "CombatHealCritical" && eventType != "CombatExpGained" &&
                    eventType != "CombatDamage" && eventType != "CombatDamageCritical" )
                {
                  //  Debug.LogError(eventType);
                    //print("non displayed event type is : " + eventType);

                    if (mobNode is AtavismPlayer)
                    {
                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-event"+eventType+"-self");
                            startPosition.x = 0;
                        }
                    }
                    else
                    {
                        if (m_combat != null){
                            m_combat.AddToClassList("floating-panel-event"+eventType+"-target");
                            startPosition.x = 0;
                        }
                    }
                }
            }
        }

        public void LevelHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }

        public void TargetTypeHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }

        public void AdminLevelHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }

        public void NameDisplayHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }

        public void GuildNameDisplayHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }
        public void QuestAvailableHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }
        public void QuestInProgressHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }
        public void QuestConcludableHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }
        public void DialogueAvailableHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }
        public void ItemsToSellHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(showName);
        }

        public void ShowChatBubble(string text)
        {
            if (m_chatPanel != null)
                m_chatPanel.ShowVisualElement();
            if (m_chat != null)
            {
                int numLines = text.Length / 60;
                for (int i = 0; i < numLines; i++)
                {
                    int spacePos = text.IndexOf(" ", (i + 1) * 60);
                    if (spacePos > 0)
                        text = text.Insert(spacePos, "\n");
                }
                m_chat.text = text;
                stopDisplay = Time.time + chatDisplayTime;
            }
         
        }

        // DamageType getDamageTypeColor(string damageType)
        // {
        //     DamageType d = new DamageType() {criticDamageColor = CriticalDamageColor, type = "", damageColor = DamageColor};
        //     foreach (var dt in damageTypeColor)
        //     {
        //         if (dt.type.Equals(damageType))
        //         {
        //             d = dt;
        //         }
        //     }
        //
        //     return d;
        // }
        public void HideChatBubble()
        {
          //  Debug.LogError("HideChatBubble bUBBLE ");
            FloatingPanelController.ALLcombatTextData[ThisPanelID].CurrentNode = null;
            FloatingPanelController.ALLcombatTextData[ThisPanelID].RendererReference = null;
            FloatingPanelController.ALLcombatTextData[ThisPanelID].LastTimeUsed = 0;
            FloatingPanelController.ALLcombatTextData[ThisPanelID].FloatingPanelGO.HideVisualElement();


            if (m_chat != null)
            {
                m_chat.text = "";
            }
           
          
        }
    }
}