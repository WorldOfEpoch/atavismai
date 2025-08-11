using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Atavism.UI;
using UnityEngine.UIElements;

namespace Atavism
{

    public class AtavismAbility : Activatable
    {

        public int id = 0;
        //public string command = "";
        //public string style = "";
        public string rank = "";
        public int cost = 0;
        public float costPercentage = 0;
        public string costProperty = "mana";
        public int pulseCost = 0;
        public float pulseCostPercentage = 0;
        public string pulseCostProperty = "mana";
        public int distance = 0;
        public float castTime = 0;
        public bool globalcd = false;
        public bool weaponcd = false;
        public string cooldownType = "";
        public float cooldownLength = 0;
        public string weaponReq = "";
        public int reagentReq = -1;
        public bool passive = false;
        public int reqLevel = 1;
        public bool castingInRun = false;
        //public string stancereq = "";
        public TargetType targetType;
        public TargetSubType targetSubType;
        public string aoeType = "";
        public int maxRange = 4;                // How far away the target can be (in metres)
        public int minRange = 0;                // How close the target can be (in metres)
        public int aoeRadius = 1;
        public float speed = 0;
        public bool toggle = false;
        //Coroutine _activateWait = null;
        // Use this for initialization
        void Start()
        {

        }

        public AtavismAbility Clone()
        {
            AtavismAbility clone = new AtavismAbility();
            clone.id = id;
            clone.name = name;
         //   clone.icon = icon;
            //clone.style = style;
            clone.costProperty = costProperty;
            clone.pulseCostProperty = pulseCostProperty;
            clone.rank = rank;
            clone.cost = cost;
            clone.costPercentage = costPercentage;
            clone.pulseCost = pulseCost;
            clone.pulseCostPercentage = pulseCostPercentage;
            clone.distance = distance;
            clone.castTime = castTime;
            clone.globalcd = globalcd;
            clone.weaponcd = weaponcd;
            clone.cooldownType = cooldownType;
            clone.cooldownLength = cooldownLength;
            clone.weaponReq = weaponReq;
            clone.targetType = targetType;
            clone.targetSubType = targetSubType;
            clone.tooltip = tooltip;
            clone.passive = passive;
            clone.reqLevel = reqLevel;
            clone.castingInRun = castingInRun;
            clone.aoeType = aoeType;
            clone.aoeRadius = aoeRadius;
            clone.minRange = minRange;
            clone.maxRange = maxRange;
            clone.toggle = toggle;
            clone.speed = speed;
            return clone;
        }

        public override bool Activate()
        {
           // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" Ability Activate "+id);

           
            Cooldown _cooldown = GetLongestActiveCooldown();
            if (_cooldown != null)
            {
                if (_cooldown.expiration > Time.time)
                    return false;
            }
            // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" || Ability Activate "+id);


            // Debug.LogError("Ability Activate target="+ClientAPI.GetTargetObject()+" targetType="+targetType+" targetSubType="+targetSubType);

             if (ClientAPI.GetTargetObject() == null && targetType == TargetType.Single_Target && targetSubType != TargetSubType.Self)
                {
                    List<AtavismMobNode> list = AtavismClient.Instance.WorldManager.GetMobNodes();
                    foreach (var node in list)
                    {
                        if (node != null && ClientAPI.GetTargetObject() == null)
                        {
                            if (node.GetProperty("attackable")!=null && (bool)node.GetProperty("attackable") )
                            {
                                Vector3 camVector3 = Camera.main.transform.forward;
                                Vector3 targetVector3 = (node.Position - ClientAPI.GetPlayerObject().Position);
                                float dist = (node.Position - ClientAPI.GetPlayerObject().Position).magnitude;
                                
                                // Calculate dot product of vectors
                                double dotProduct = 0.0;
                                
                                dotProduct += camVector3.x * targetVector3.x;
                                dotProduct += camVector3.y * targetVector3.y;
                                dotProduct += camVector3.z * targetVector3.z;
                                
                                
                                // Calculate magnitude of vectors
                                double v1Magnitude = Math.Sqrt(camVector3.x*camVector3.x + camVector3.y*camVector3.y + camVector3.z*camVector3.z);
                                double v2Magnitude = Math.Sqrt(targetVector3.x*targetVector3.x + targetVector3.y*targetVector3.y + targetVector3.z*targetVector3.z);
                                
                                // Calculate the angle between two vectors using the dot product and magnitude
                                double angle = Math.Acos(dotProduct / (v1Magnitude * v2Magnitude)) * 180 / Math.PI; 
                                // Debug.LogError(node.Name+" angle="+angle+" dist="+dist);
                                if (AtavismCombat.Instance.angleToAutoSelectSingleTarget > Math.Abs(angle))
                                {
                                    ClientAPI.SetTarget(node.Oid);
                                    
                                }
                            }
                            else
                            {
                                // Debug.LogError(node.Name+":"+node.Oid+" not attackable");
                            }
                        }else
                        {
                            // Debug.LogError("node null or target not null");
                        }
                    }
                }
            // DateTime now2 = DateTime.Now;
            // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" Ability AOE aoeType=" + aoeType + " targetType=" + targetType +" id="+id);
            if (aoeType.Equals("LocationRadius") && (targetType == TargetType.AoE || targetType == TargetType.Location))
            {
                // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+"Ability AOE aoeRadius="+ aoeRadius+ " minRange="+ minRange+ " maxRange="+ maxRange);
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("aoeRadius", aoeRadius.ToString());
                Abilities.Instance.StartAoeAbility(id, minRange, maxRange, param);
            }
            else if(ClientAPI.GetTargetObject() != null && targetSubType != TargetSubType.Self)
            {
                // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " isTarget and not self");
                if (castTime > 0 && !castingInRun)
                {
                    Abilities.Instance.StartAbility(id);
                }
                else
                {
                    int coId = -1;
                    int cId = -1;
                    if (WorldBuilder.Instance.SelectedClaimObject != null)
                    {
                        coId = WorldBuilder.Instance.SelectedClaimObject.ID;
                        cId = WorldBuilder.Instance.SelectedClaimObject.ClaimID;
                    }

                    AbilityPrefabData apd = AtavismPrefabManager.Instance.GetAbilityPrefab(id);
                    Transform cam = Camera.main.transform;
                    SDETargeting sde = cam.GetComponent<SDETargeting>();
                    ClickToMoveInputController ctmic = cam.GetComponent<ClickToMoveInputController>();
                    if (ctmic == null)
                    {
                        if (sde != null && sde.softTargetMode && sde.showCrosshair )
                        {
                            float skipZone = (cam.position - ClientAPI.GetPlayerObject().GameObject.transform.position).magnitude;
                            Vector3 v = cam.position + cam.forward * skipZone + cam.forward * ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius"))? apd.aoeRadius:apd.maxRange);
                            // Debug.LogError("ActivateWaitSelf vector " + v);
                            RaycastHit hit;
                            if (Physics.Raycast(new Ray(cam.position + cam.forward * skipZone, cam.forward), out hit, ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius")) ? apd.aoeRadius : apd.maxRange), ClientAPI.Instance.playerLayer))
                            {
                                v = hit.point;
                                //  Debug.LogError(" hit "+hit.point);
                            }
                            else
                            {
                                // Debug.LogError("no hit");    
                            }

                            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                            if (apd.powerup != null && apd.powerup.Count > 1)
                            {
                                Abilities.Instance.abilityPowerUp = id;
                            }

                        }
                        else
                        {
                            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + id + " " + cId + " " + coId);
                            if (apd.powerup != null && apd.powerup.Count > 1)
                            {
                                Abilities.Instance.abilityPowerUp = id;
                            }
                        }
                    }
                    else 
                    {
                      //  Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  isTarget and not self | else");
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100, ClientAPI.Instance.playerLayer))
                        {
                            Vector3 v =  hit.point;
                            AtavismNode an = hit.collider.GetComponent<AtavismNode>();
                            if (an != null)
                            {
                                if (an.CheckBooleanProperty("deadstate"))
                                    return false;
                                if (an.PropertyExists("reaction"))
                                {
                                    int targetType = (int)an.GetProperty("reaction");
                                    if (an.PropertyExists("aggressive"))
                                    {
                                        if ((bool)an.GetProperty("aggressive"))
                                        {
                                            targetType = -1;
                                        }
                                    }

                                    if (targetType > 0)
                                    {
                                        return false;
                                    }
                                }

                            }
                          //  Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" Abilities use ability "+id);
                            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                            if (apd.powerup != null && apd.powerup.Count > 1)
                            {
                             //   Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)+" Abilities set abilityPowerUp "+id);
                                Abilities.Instance.abilityPowerUp = id;
                            }
                        }
                        else
                        {
                            NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + id + " " + cId + " " + coId);
                            if (apd.powerup != null && apd.powerup.Count > 1)
                            {
                                Abilities.Instance.abilityPowerUp = id;
                            }
                           //  Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  isTarget and not self | else | no raycast");
                        }
                    }
                }
            } else 
            {
                // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " else");
                if (castTime > 0 && !castingInRun)
                {
                    Abilities.Instance.StartAbility(id);
                }
                else
                {
                    int coId = -1;
                    int cId = -1;
                    if (WorldBuilder.Instance.SelectedClaimObject != null)
                    {
                        coId = WorldBuilder.Instance.SelectedClaimObject.ID;
                        cId = WorldBuilder.Instance.SelectedClaimObject.ClaimID;
                    }
                    
                    AbilityPrefabData apd = AtavismPrefabManager.Instance.GetAbilityPrefab(id);
                    Transform cam = Camera.main.transform;
                    SDETargeting sde = cam.GetComponent<SDETargeting>();
                    ClickToMoveInputController ctmic = cam.GetComponent<ClickToMoveInputController>();
                    if (ctmic == null)
                    {
                        if (sde != null && sde.softTargetMode && sde.showCrosshair)
                        {
                            float skipZone = (cam.position - ClientAPI.GetPlayerObject().GameObject.transform.position).magnitude;
                            Vector3 v = cam.position + cam.forward * skipZone + cam.forward * ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius"))? apd.aoeRadius:apd.maxRange);
                            // Debug.LogError("ActivateWaitSelf vector " + v);
                            RaycastHit hit;
                            if (Physics.Raycast(new Ray(cam.position + cam.forward * skipZone, cam.forward), out hit, ((apd.targetType == TargetType.AoE && apd.aoeType.Equals("PlayerRadius")) ? apd.aoeRadius : apd.maxRange), ClientAPI.Instance.playerLayer))
                            {
                                v = hit.point;
                            }

                            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                            if (apd.powerup != null && apd.powerup.Count > 1)
                            {
                                Abilities.Instance.abilityPowerUp = id;
                            }
                        }
                        else
                        {
                            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + id + " " + cId + " " + coId);
                            if (apd.powerup != null && apd.powerup.Count > 1)
                            {
                                Abilities.Instance.abilityPowerUp = id;
                            }
                        }
                    }
                    else
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100, ClientAPI.Instance.playerLayer))
                        {
                            Vector3 v =  hit.point;
                            AtavismNode an = hit.collider.GetComponent<AtavismNode>();
                            if (an != null)
                            {
                                if (an.CheckBooleanProperty("deadstate"))
                                    return false;
                                if (an.PropertyExists("reaction"))
                                {
                                    int targetType = (int)an.GetProperty("reaction");
                                    if (an.PropertyExists("aggressive"))
                                    {
                                        if ((bool)an.GetProperty("aggressive"))
                                        {
                                            targetType = -1;
                                        }
                                    }

                                    if (targetType > 0)
                                    {
                                        return false;
                                    }
                                }

                            }
                            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                            if (apd.powerup != null && apd.powerup.Count > 1)
                            {
                                Abilities.Instance.abilityPowerUp = id;
                            }
                        }
                        else
                        {
                           // Debug.LogError(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " else | no raycast");
                           if (Abilities.Instance.abilityPowerUp > 0)
                           {
                               if (Physics.Raycast(ray, out hit, 100, ~ctmic.ignoreLayers))
                               {
                                   Vector3 v =  hit.point;
                                   AtavismNode an = hit.collider.GetComponent<AtavismNode>();
                                   NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + id + " " + cId + " " + coId + " " + v.x + " " + v.y + " " + v.z);
                               } 
                           }
                           
                        }
                    }
                }
            }
            return true;
        }

        /**
         * 
         */
        public Sprite Icon
        {
            get
            {
                Sprite icon = AtavismPrefabManager.Instance.GetAbilityIconByID(id);
                if (icon == null)
                {
                    return AtavismSettings.Instance.defaultItemIcon;
                }
                return icon;
            }
        }
        
        /// <summary>
        /// Coroutine to stop player for start cast
        /// </summary>
        /// <returns></returns>
        IEnumerator ActivateWait()
        {
            ClientAPI.GetPlayerObject().GameObject.SendMessage("NoMove", 0.05f);

            yield return new WaitForSeconds(0.01f);
            //Debug.LogError("Ability " + id + " " + ClientAPI.GetTargetObject() + " " + targetType + " " + targetSubType);
            if (ClientAPI.GetTargetObject() != null && targetSubType != TargetSubType.Self)
                NetworkAPI.SendTargetedCommand(ClientAPI.GetTargetOid(), "/ability " + id);
            else
                NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/ability " + id);

        }

        /// <summary>
        /// Draws the tooltip via the old GUI system
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public override void DrawTooltip(float x, float y)
        {
            int width = 150;
            int height = 50;
            Rect tooltipRect = new Rect(x, y - height, width, height);
            GUI.Box(tooltipRect, "");
            GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 5, 140, 20), name);
            GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 25, 140, 20), cost + " " + costProperty);
        }
        /// <summary>
        /// Function return longest effect cooldown 
        /// </summary>
        /// <returns></returns>
        public override Cooldown GetLongestActiveCooldown()
        {
            if (cooldownType == "" && Abilities.Instance != null)
            {
                return Abilities.Instance.GetCooldown(id.ToString(), true);
            }
            else
            {
                return Abilities.Instance.GetCooldown(cooldownType, true);
            }
        }

        /// <summary>
        /// Shows the tooltip using the new UGUI system.
        /// </summary>
        /// <param name="target">Target.</param>
        public void ShowTooltip(GameObject target)
        {

#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UGUITooltip.Instance.SetTitle(name);
#endif
//            UGUITooltip.Instance.SetQuality(1);
            UGUITooltip.Instance.SetQualityColor(AtavismSettings.Instance.abilityQualityColor);

            if (Icon != null)
            {
                UGUITooltip.Instance.SetIcon(Icon);
            }


            if (passive)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UGUITooltip.Instance.SetType("Passive");
#endif
                UGUITooltip.Instance.SetWeight("");

                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAttribute("Required Weapon: ", weaponReq, true);
#endif
                }


              /*  if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color="+ colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/

            }
            else
            {
                if ( targetType == TargetType.Single_Target && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UGUITooltip.Instance.SetType("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.AoE && targetSubType == TargetSubType.Enemy  )
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("AoE Enemy"));
#else
                    UGUITooltip.Instance.SetType("Target: AoE Enemy");
#endif
                }
                else if (targetType == TargetType.AoE && targetSubType == TargetSubType.Friendly )
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " +I2.Loc.LocalizationManager.GetTranslation("AoE Friendly"));
#else
                    UGUITooltip.Instance.SetType("Target: AoE Friendly");
#endif
                }
                else if ( targetType == TargetType.Single_Target && targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " +I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UGUITooltip.Instance.SetType("Target: Friendly");
#endif
                }
                else if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Self)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UGUITooltip.Instance.SetType("Target: Self");
#endif
                }
                else
                {
                    UGUITooltip.Instance.SetType("");
                }
                UGUITooltip.Instance.SetWeight("");
                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UGUITooltip.Instance.AddAttribute("CastTime ", "Instant", true);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+castTime + " seconds");
#else
                    UGUITooltip.Instance.AddAttribute("CastTime: ", castTime + " seconds", true);
#endif
                }


                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UGUITooltip.Instance.AddAttribute("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }
                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAttribute("Required Weapon: ", weaponReq, true);
#endif
                }
              /*  if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
                UGUITooltip.Instance.SetTypeColour(UGUITooltip.Instance.abilityCastTimeColour);
                if (cost > 0 || costPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", (cost>0?cost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(costPercentage>0?(cost > 0 ? I2.Loc.LocalizationManager.GetTranslation("and")+" ":"")+ costPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAttribute("Cost: ", (cost>0?cost.ToString() + " "+"points"+" ":"")+(costPercentage>0?(cost > 0 ? "and" +" ":"")+ costPercentage.ToString()+"% ":"") + costProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }
                if (pulseCost > 0 || pulseCostPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Pulse Cost") + ": ", (pulseCost>0?pulseCost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(pulseCostPercentage>0?I2.Loc.LocalizationManager.GetTranslation("and")+" "+ pulseCostPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(pulseCostProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAttribute("Pulse Cost: ", (pulseCost > 0 ? pulseCost.ToString() + " " + "points" + " " : "") +
                        (pulseCostPercentage > 0 ? (pulseCost > 0 ? "and" + " " : "") + pulseCostPercentage.ToString() + "% " : "") + pulseCostProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }
                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAttribute("Range: ", distance.ToString() + "m", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAttribute("Range: ", "Melee ", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UGUITooltip.Instance.SetDescription(tooltip);
#endif
            UGUITooltip.Instance.Show(target);
        }

        /// <summary>
        /// Show additional tooltip using the new UGUI system.
        /// </summary>
        public void ShowAdditionalTooltip()
        {
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetAdditionalTitle(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UGUITooltip.Instance.SetAdditionalTitle(name);
#endif
            UGUITooltip.Instance.SetAdditionalQuality(1);
            if (Icon != null)
            {
                UGUITooltip.Instance.SetAdditionalIcon(Icon);
            }

            if (passive)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UGUITooltip.Instance.SetAdditionalType("Passive");
#endif
                UGUITooltip.Instance.SetAdditionalWeight("");
                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Required Weapon: ", weaponReq, true);
#endif
                }
               /* if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
            }
            else
            {
                if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UGUITooltip.Instance.SetAdditionalType("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UGUITooltip.Instance.SetAdditionalType("Target: Friendly");
#endif
                }
                else if (targetType == TargetType.AoE && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("AoE Enemy"));
#else
                    UGUITooltip.Instance.SetAdditionalType("Target: AoE Enemy");
#endif
                }
                else if (targetType == TargetType.AoE && targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("AoE Friendly"));
#else
                    UGUITooltip.Instance.SetAdditionalType("Target: AoE Friendly");
#endif
                }
                else if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Self)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UGUITooltip.Instance.SetAdditionalType("Target: Self");
#endif
                }
                else
                {
                    UGUITooltip.Instance.SetAdditionalType("");
                }

                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UGUITooltip.Instance.SetAdditionalWeight("CastTime : Instant");
#endif
                }
                else if (castTime > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": " + castTime + " seconds");
#else
                    UGUITooltip.Instance.SetAdditionalWeight("CastTime: " + castTime + " seconds");
#endif
                }

                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }

                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Required Weapon: ", weaponReq, true);
#endif
                }

                if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }

                if (cost > 0 || costPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", (cost>0?cost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(costPercentage>0?(cost > 0 ? I2.Loc.LocalizationManager.GetTranslation("and")+" ":"")+ costPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Cost: ", (cost > 0 ? cost.ToString() + " " + "points" + " " : "") + (costPercentage > 0 ? (cost > 0 ? "and" + " " : "") + costPercentage.ToString() + "% " : "") + costProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }
                if (pulseCost > 0 || pulseCostPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Pulse Cost") + ": ", (pulseCost>0?pulseCost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(pulseCostPercentage>0?I2.Loc.LocalizationManager.GetTranslation("and")+" "+ pulseCostPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(pulseCostProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Pulse Cost: ", (pulseCost > 0 ? pulseCost.ToString() + " " + "points" + " " : "") +
                        (pulseCostPercentage > 0 ? (pulseCost > 0 ? "and" + " " : "") + pulseCostPercentage.ToString() + "% " : "") + pulseCostProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }

                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Range: ", distance.ToString() + "m", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute("Range: ", "Melee ", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.SetAdditionalDescription(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UGUITooltip.Instance.SetAdditionalDescription(tooltip);
#endif
            UGUITooltip.Instance.ShowAdditionalTooltip();
        }

        /// <summary>
        /// Show secound additional tooltip using the new UGUI system.
        /// </summary>
        public void ShowAdditionalTooltip2()
        {
#if AT_I2LOC_PRESET
        UGUITooltip.Instance.SetAdditionalTitle2(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UGUITooltip.Instance.SetAdditionalTitle2(name);
#endif
            UGUITooltip.Instance.SetAdditionalQuality2(1);
            if (Icon != null)
            {
                UGUITooltip.Instance.SetAdditionalIcon2(Icon);
            }

            if (passive)
            {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UGUITooltip.Instance.SetAdditionalType2("Passive");
#endif
                UGUITooltip.Instance.SetAdditionalWeight2("");
                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Required Weapon: ", weaponReq, true);
#endif
                }
            /*    if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
            }
            else
            {
                if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UGUITooltip.Instance.SetAdditionalType2("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.Single_Target &&  targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UGUITooltip.Instance.SetAdditionalType2("Target: Friendly");
#endif
                }
                else  if (targetType == TargetType.AoE && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("AoE Enemy"));
#else
                    UGUITooltip.Instance.SetAdditionalType2("Target: AoE Enemy");
#endif
                }
                else if (targetType == TargetType.AoE &&  targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("AoE Friendly"));
#else
                    UGUITooltip.Instance.SetAdditionalType2("Target: AoE Friendly");
#endif
                }
                else if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Self)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UGUITooltip.Instance.SetAdditionalType2("Target: Self");
#endif
                }
                else
                {
                    UGUITooltip.Instance.SetAdditionalType2("");
                }

                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight2(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UGUITooltip.Instance.SetAdditionalWeight2("CastTime : Instant");
#endif
                }
                else if (castTime > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.SetAdditionalWeight2(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": " + castTime + " seconds");
#else
                    UGUITooltip.Instance.SetAdditionalWeight2("CastTime: " + castTime + " seconds");
#endif
                }

                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }

                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Required Weapon: ", weaponReq, true);
#endif
                }

                /*    if (reqLevel > 0)
                    {
                        if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                        {
                            string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.itemStatLowerColour.r), ToByte(UGUITooltip.Instance.itemStatLowerColour.g), ToByte(UGUITooltip.Instance.itemStatLowerColour.b));
    #if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
    #else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
    #endif
                        }
                        else
                        {
                            string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UGUITooltip.Instance.defaultTextColour.r), ToByte(UGUITooltip.Instance.defaultTextColour.g), ToByte(UGUITooltip.Instance.defaultTextColour.b));

    #if AT_I2LOC_PRESET
                    UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
    #else
                            UGUITooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
    #endif
                        }
                    }*/

                if (cost > 0 || costPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", (cost>0?cost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(costPercentage>0?(cost > 0 ? I2.Loc.LocalizationManager.GetTranslation("and")+" ":"")+ costPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Cost: ", (cost > 0 ? cost.ToString() + " " + "points" + " " : "") + (costPercentage > 0 ? (cost > 0 ? "and" + " " : "") + costPercentage.ToString() + "% " : "") + costProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }
                if (pulseCost > 0 || pulseCostPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Pulse Cost") + ": ", (pulseCost>0?pulseCost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(pulseCostPercentage>0?I2.Loc.LocalizationManager.GetTranslation("and")+" "+ pulseCostPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(pulseCostProperty) , true, UGUITooltip.Instance.abilityCostColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Pulse Cost: ", (pulseCost > 0 ? pulseCost.ToString() + " " + "points" + " " : "") +
                        (pulseCostPercentage > 0 ? (pulseCost > 0 ? "and" + " " : "") + pulseCostPercentage.ToString() + "% " : "") + pulseCostProperty, true, UGUITooltip.Instance.abilityCostColour);
#endif
                }

                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Range: ", distance.ToString() + "m", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UGUITooltip.Instance.abilityRangeColour);
#else
                    UGUITooltip.Instance.AddAdditionalAttribute2("Range: ", "Melee ", true, UGUITooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UGUITooltip.Instance.SetAdditionalDescription2(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UGUITooltip.Instance.SetAdditionalDescription2(tooltip);
#endif
            UGUITooltip.Instance.ShowAdditionalTooltip2();
        }
                
        /// <summary>
        /// Shows the tooltip using the new UI Elements system.
        /// </summary>
        /// <param name="target">Target.</param>
        public void ShowUITooltip(VisualElement target)
        {

#if AT_I2LOC_PRESET
        UIAtavismTooltip.Instance.SetTitle(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UIAtavismTooltip.Instance.SetTitle(name);
#endif
//            UIAtavismTooltip.Instance.SetQuality(1);
            UIAtavismTooltip.Instance.SetQualityColor(AtavismSettings.Instance.abilityQualityColor);

            if (Icon != null)
            {
                UIAtavismTooltip.Instance.SetIcon(Icon);
            }


            if (passive)
            {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UIAtavismTooltip.Instance.SetType("Passive");
#endif
                UIAtavismTooltip.Instance.SetWeight("");

                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Required Weapon: ", weaponReq, true);
#endif
                }


              /*  if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.r), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.g), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.b));

#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.defaultTextColour.r), ToByte(UIAtavismTooltip.Instance.defaultTextColour.g), ToByte(UIAtavismTooltip.Instance.defaultTextColour.b));
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color="+ colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/

            }
            else
            {
                if ( targetType == TargetType.Single_Target && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UIAtavismTooltip.Instance.SetType("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.AoE && targetSubType == TargetSubType.Enemy  )
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("AoE Enemy"));
#else
                    UIAtavismTooltip.Instance.SetType("Target: AoE Enemy");
#endif
                }
                else if (targetType == TargetType.AoE && targetSubType == TargetSubType.Friendly )
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " +I2.Loc.LocalizationManager.GetTranslation("AoE Friendly"));
#else
                    UIAtavismTooltip.Instance.SetType("Target: AoE Friendly");
#endif
                }
                else if ( targetType == TargetType.Single_Target && targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " +I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UIAtavismTooltip.Instance.SetType("Target: Friendly");
#endif
                }
                else if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Self)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UIAtavismTooltip.Instance.SetType("Target: Self");
#endif
                }
                else
                {
                    UIAtavismTooltip.Instance.SetType("");
                }
                UIAtavismTooltip.Instance.SetWeight("");
                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UIAtavismTooltip.Instance.AddAttribute("CastTime ", "Instant", true);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+castTime + " seconds");
#else
                    UIAtavismTooltip.Instance.AddAttribute("CastTime: ", castTime + " seconds", true);
#endif
                }


                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }
                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Required Weapon: ", weaponReq, true);
#endif
                }
              /*  if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.r), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.g), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.b));

#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.defaultTextColour.r), ToByte(UIAtavismTooltip.Instance.defaultTextColour.g), ToByte(UIAtavismTooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
                UIAtavismTooltip.Instance.SetTypeColour(UIAtavismTooltip.Instance.abilityCastTimeColour);
                if (cost > 0 || costPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", (cost>0?cost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(costPercentage>0?(cost > 0 ? I2.Loc.LocalizationManager.GetTranslation("and")+" ":"")+ costPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UIAtavismTooltip.Instance.abilityCostColour);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Cost: ", (cost>0?cost.ToString() + " "+"points"+" ":"")+(costPercentage>0?(cost > 0 ? "and" +" ":"")+ costPercentage.ToString()+"% ":"") + costProperty, true, UIAtavismTooltip.Instance.abilityCostColour);
#endif
                }
                if (pulseCost > 0 || pulseCostPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Pulse Cost") + ": ", (pulseCost>0?pulseCost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(pulseCostPercentage>0?I2.Loc.LocalizationManager.GetTranslation("and")+" "+ pulseCostPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(pulseCostProperty) , true, UIAtavismTooltip.Instance.abilityCostColour);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Pulse Cost: ", (pulseCost > 0 ? pulseCost.ToString() + " " + "points" + " " : "") +
                        (pulseCostPercentage > 0 ? (pulseCost > 0 ? "and" + " " : "") + pulseCostPercentage.ToString() + "% " : "") + pulseCostProperty, true, UIAtavismTooltip.Instance.abilityCostColour);
#endif
                }
                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UIAtavismTooltip.Instance.abilityRangeColour);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Range: ", distance.ToString() + "m", true, UIAtavismTooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UIAtavismTooltip.Instance.abilityRangeColour);
#else
                    UIAtavismTooltip.Instance.AddAttribute("Range: ", "Melee ", true, UIAtavismTooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.SetDescription(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UIAtavismTooltip.Instance.SetDescription(tooltip);
#endif
            UIAtavismTooltip.Instance.Show(target);
        }

        /// <summary>
        /// Show additional tooltip using the new UI Elements system.
        /// </summary>
        public void ShowUIAdditionalTooltip()
        {
#if AT_I2LOC_PRESET
        UIAtavismTooltip.Instance.SetAdditionalTitle(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UIAtavismTooltip.Instance.SetAdditionalTitle(name);
#endif
            UIAtavismTooltip.Instance.SetAdditionalQuality(1);
            if (Icon != null)
            {
                UIAtavismTooltip.Instance.SetAdditionalIcon(Icon);
            }

            if (passive)
            {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UIAtavismTooltip.Instance.SetAdditionalType("Passive");
#endif
                UIAtavismTooltip.Instance.SetAdditionalWeight("");
                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute("Required Weapon: ", weaponReq, true);
#endif
                }
               /* if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.r), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.g), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.defaultTextColour.r), ToByte(UIAtavismTooltip.Instance.defaultTextColour.g), ToByte(UIAtavismTooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
            }
            else
            {
                if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType("Target: Friendly");
#endif
                }
                else if (targetType == TargetType.AoE && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("AoE Enemy"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType("Target: AoE Enemy");
#endif
                }
                else if (targetType == TargetType.AoE && targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("AoE Friendly"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType("Target: AoE Friendly");
#endif
                }
                else if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Self)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType("Target: Self");
#endif
                }
                else
                {
                    UIAtavismTooltip.Instance.SetAdditionalType("");
                }

                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalWeight("CastTime : Instant");
#endif
                }
                else if (castTime > 0)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalWeight(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": " + castTime + " seconds");
#else
                    UIAtavismTooltip.Instance.SetAdditionalWeight("CastTime: " + castTime + " seconds");
#endif
                }

                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }

                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute("Required Weapon: ", weaponReq, true);
#endif
                }

                if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.r), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.g), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.defaultTextColour.r), ToByte(UIAtavismTooltip.Instance.defaultTextColour.g), ToByte(UIAtavismTooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAdditionalAttribute("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }

                if (cost > 0 || costPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", (cost>0?cost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(costPercentage>0?(cost > 0 ? I2.Loc.LocalizationManager.GetTranslation("and")+" ":"")+ costPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UIAtavismTooltip.Instance.abilityCostColour);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute("Cost: ", (cost > 0 ? cost.ToString() + " " + "points" + " " : "") + (costPercentage > 0 ? (cost > 0 ? "and" + " " : "") + costPercentage.ToString() + "% " : "") + costProperty, true, UIAtavismTooltip.Instance.abilityCostColour);
#endif
                }
                if (pulseCost > 0 || pulseCostPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Pulse Cost") + ": ", (pulseCost>0?pulseCost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(pulseCostPercentage>0?I2.Loc.LocalizationManager.GetTranslation("and")+" "+ pulseCostPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(pulseCostProperty) , true, UIAtavismTooltip.Instance.abilityCostColour);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute("Pulse Cost: ", (pulseCost > 0 ? pulseCost.ToString() + " " + "points" + " " : "") +
                        (pulseCostPercentage > 0 ? (pulseCost > 0 ? "and" + " " : "") + pulseCostPercentage.ToString() + "% " : "") + pulseCostProperty, true, UIAtavismTooltip.Instance.abilityCostColour);
#endif
                }

                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UIAtavismTooltip.Instance.abilityRangeColour);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute("Range: ", distance.ToString() + "m", true, UIAtavismTooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UIAtavismTooltip.Instance.abilityRangeColour);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute("Range: ", "Melee ", true, UIAtavismTooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.SetAdditionalDescription(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UIAtavismTooltip.Instance.SetAdditionalDescription(tooltip);
#endif
            UIAtavismTooltip.Instance.ShowAdditionalTooltip();
        }

        /// <summary>
        /// Show secound additional tooltip  using the new UI Elements system.
        /// </summary>
        public void ShowUIAdditionalTooltip2()
        {
#if AT_I2LOC_PRESET
        UIAtavismTooltip.Instance.SetAdditionalTitle2(I2.Loc.LocalizationManager.GetTranslation("Ability/" + name));
#else
            UIAtavismTooltip.Instance.SetAdditionalTitle2(name);
#endif
            UIAtavismTooltip.Instance.SetAdditionalQuality2(1);
            if (Icon != null)
            {
                UIAtavismTooltip.Instance.SetAdditionalIcon2(Icon);
            }

            if (passive)
            {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Passive"));
#else
                UIAtavismTooltip.Instance.SetAdditionalType2("Passive");
#endif
                UIAtavismTooltip.Instance.SetAdditionalWeight2("");
                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2("Required Weapon: ", weaponReq, true);
#endif
                }
            /*    if (reqLevel > 0)
                {
                    if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.r), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.g), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.b));
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                    else
                    {
                        string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.defaultTextColour.r), ToByte(UIAtavismTooltip.Instance.defaultTextColour.g), ToByte(UIAtavismTooltip.Instance.defaultTextColour.b));

#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#else
                        UIAtavismTooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
#endif
                    }
                }*/
            }
            else
            {
                if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Enemy"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType2("Target: Enemy");
#endif
                }
                else if (targetType == TargetType.Single_Target &&  targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Friendly"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType2("Target: Friendly");
#endif
                }
                else  if (targetType == TargetType.AoE && targetSubType == TargetSubType.Enemy)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": "+ I2.Loc.LocalizationManager.GetTranslation("AoE Enemy"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType2("Target: AoE Enemy");
#endif
                }
                else if (targetType == TargetType.AoE &&  targetSubType == TargetSubType.Friendly)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("AoE Friendly"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType2("Target: AoE Friendly");
#endif
                }
                else if (targetType == TargetType.Single_Target && targetSubType == TargetSubType.Self)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalType2(I2.Loc.LocalizationManager.GetTranslation("Target") + ": " + I2.Loc.LocalizationManager.GetTranslation("Self"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalType2("Target: Self");
#endif
                }
                else
                {
                    UIAtavismTooltip.Instance.SetAdditionalType2("");
                }

                if (castTime == 0)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalWeight2(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": "+ I2.Loc.LocalizationManager.GetTranslation("Instant"));
#else
                    UIAtavismTooltip.Instance.SetAdditionalWeight2("CastTime : Instant");
#endif
                }
                else if (castTime > 0)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.SetAdditionalWeight2(I2.Loc.LocalizationManager.GetTranslation("CastTime") + ": " + castTime + " seconds");
#else
                    UIAtavismTooltip.Instance.SetAdditionalWeight2("CastTime: " + castTime + " seconds");
#endif
                }

                if (cooldownLength > 0)
                {
#if AT_I2LOC_PRESET
            UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Cooldown") + ": ", cooldownLength.ToString() + "s", true);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2("Cooldown: ", cooldownLength.ToString() + "s", true);
#endif
                }

                if (weaponReq != "~ none ~" && weaponReq != "")
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredWeapon") + ": ", I2.Loc.LocalizationManager.GetTranslation(weaponReq), true);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2("Required Weapon: ", weaponReq, true);
#endif
                }

                /*    if (reqLevel > 0)
                    {
                        if ((int)ClientAPI.GetPlayerObject().GetProperty("level") < reqLevel)
                        {
                            string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.r), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.g), ToByte(UIAtavismTooltip.Instance.itemStatLowerColour.b));
    #if AT_I2LOC_PRESET
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
    #else
                            UIAtavismTooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
    #endif
                        }
                        else
                        {
                            string colourText = string.Format("#{0:X2}{1:X2}{2:X2}ff", ToByte(UIAtavismTooltip.Instance.defaultTextColour.r), ToByte(UIAtavismTooltip.Instance.defaultTextColour.g), ToByte(UIAtavismTooltip.Instance.defaultTextColour.b));

    #if AT_I2LOC_PRESET
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("RequiredLevel") + " ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
    #else
                            UIAtavismTooltip.Instance.AddAdditionalAttribute2("Required Level ", "<color=" + colourText + ">" + reqLevel + "</color>", true);
    #endif
                        }
                    }*/

                if (cost > 0 || costPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Cost") + ": ", (cost>0?cost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(costPercentage>0?(cost > 0 ? I2.Loc.LocalizationManager.GetTranslation("and")+" ":"")+ costPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(costProperty) , true, UIAtavismTooltip.Instance.abilityCostColour);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2("Cost: ", (cost > 0 ? cost.ToString() + " " + "points" + " " : "") + (costPercentage > 0 ? (cost > 0 ? "and" + " " : "") + costPercentage.ToString() + "% " : "") + costProperty, true, UIAtavismTooltip.Instance.abilityCostColour);
#endif
                }
                if (pulseCost > 0 || pulseCostPercentage > 0)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Pulse Cost") + ": ", (pulseCost>0?pulseCost.ToString() + " " +I2.Loc.LocalizationManager.GetTranslation("points")+" ":"")
                    +(pulseCostPercentage>0?I2.Loc.LocalizationManager.GetTranslation("and")+" "+ pulseCostPercentage.ToString()+"% ":"")+ I2.Loc.LocalizationManager.GetTranslation(pulseCostProperty) , true, UIAtavismTooltip.Instance.abilityCostColour);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2("Pulse Cost: ", (pulseCost > 0 ? pulseCost.ToString() + " " + "points" + " " : "") +
                        (pulseCostPercentage > 0 ? (pulseCost > 0 ? "and" + " " : "") + pulseCostPercentage.ToString() + "% " : "") + pulseCostProperty, true, UIAtavismTooltip.Instance.abilityCostColour);
#endif
                }

                if (distance > 4)
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ", distance.ToString() + "m" , true, UIAtavismTooltip.Instance.abilityRangeColour);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2("Range: ", distance.ToString() + "m", true, UIAtavismTooltip.Instance.abilityRangeColour);
#endif
                }
                else
                {
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.AddAdditionalAttribute2(I2.Loc.LocalizationManager.GetTranslation("Range") + ": ",  I2.Loc.LocalizationManager.GetTranslation("Melee") , true, UIAtavismTooltip.Instance.abilityRangeColour);
#else
                    UIAtavismTooltip.Instance.AddAdditionalAttribute2("Range: ", "Melee ", true, UIAtavismTooltip.Instance.abilityRangeColour);
#endif
                }
            }
#if AT_I2LOC_PRESET
                UIAtavismTooltip.Instance.SetAdditionalDescription2(I2.Loc.LocalizationManager.GetTranslation("Ability/" +tooltip));
#else
            UIAtavismTooltip.Instance.SetAdditionalDescription2(tooltip);
#endif
            UIAtavismTooltip.Instance.ShowAdditionalTooltip2();
        }
        
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}