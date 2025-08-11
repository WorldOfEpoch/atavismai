using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class InteractiveObjectsManager : MonoBehaviour
    {

        static InteractiveObjectsManager instance;

        Dictionary<int, InteractiveObject> interactiveObjects = new Dictionary<int, InteractiveObject>();
        Dictionary<int, bool> interactiveObjectsQueueSatus = new Dictionary<int, bool>();
        Dictionary<int, string> interactiveObjectsQueueState = new Dictionary<int, string>();


        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("interactive_object_state", HandleInteractiveObjectStateMessage);
            NetworkAPI.RegisterExtensionMessageHandler("interactive_object_spawn", HandleInteractiveObjectSpawnMessage);
            NetworkAPI.RegisterExtensionMessageHandler("start_interactive_task", HandleStarInteractionTask);
            NetworkAPI.RegisterExtensionMessageHandler("interactive_task_interrupted", HandleInterruptTask);
            AtavismClient.Instance.NetworkHelper.RegisterPrefabMessageHandler("InteractiveObjects", InteractiveObjectProfilePrefabHandler);

        }

        private void InteractiveObjectProfilePrefabHandler(Dictionary<string, object> props)
        {
             if (AtavismLogger.isLogDebug())
            {
                Debug.LogWarning("InteractiveObjectProfilePrefabHandler " + Time.time);
                string keys = " [ ";
                foreach (var it in props.Keys)
                {
                    keys += " ; " + it + " => " + props[it];
                }

                Debug.LogWarning("InteractiveObjectProfilePrefabHandler: keys:" + keys);
            }

            try
            {
                int num = (int)props["num"];
                bool sendAll = (bool)props["all"];
                for (int i = 0; i < num; i++)
                {
                 //   Debug.LogError("AbilitiesPrefabHandler " + i); 
                    InteractiveObjectProfileData cpd = new InteractiveObjectProfileData();
                    cpd.id = (int)props["i" + i + "id"];
                    cpd.name = (string)props["i" + i + "name"];
                    cpd.interactionType = (string)props["i" + i + "it"];
                    cpd.minLevel = (int)props["i" + i + "nl"];
                    cpd.maxLevel = (int)props["i" + i + "ml"];

                    cpd.useLimit = (int)props["i" + i + "ul"];
                    cpd.questRequirement = (int)props["i" + i + "qr"];
                    cpd.itemRequirement = (int)props["i" + i + "ir"];
                    cpd.itemRequirementGet = (bool)props["i" + i + "irg"];
                    cpd.itemCountRequirement = (int)props["i" + i + "icr"];
                    cpd.currencyRequirement = (int)props["i" + i + "cr"];
                    cpd.currencyRequirementGet = (bool)props["i" + i + "crg"];
                    cpd.currencyCountRequirement = (int)props["i" + i + "ccr"];
                    
                    cpd.iconPath = (string)props["i" + i + "icon"];
                    string icon2 = (string)props["i" + i + "icon2"];
                    Texture2D tex = new Texture2D(2, 2);
                    bool wyn = tex.LoadImage(System.Convert.FromBase64String(icon2));
                    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                    AtavismPrefabManager.Instance.SaveIcon(sprite, icon2, cpd.iconPath);
                    cpd.date = (long)props["i" + i + "date"];
                 //    int punum = (int)props["i" + i + "punum"];
                 // //   Debug.LogWarning("ability punum=" + punum);
                 //    for (int j = 0; j < punum; j++)
                 //    {
                 //        long thresholdStartTime = (long)props["i" + i + "pu" + j + "t"];
                 //        if (AtavismLogger.isLogDebug())   Debug.LogWarning("ability id=" + cpd.id + " thresholdStartTime=" + thresholdStartTime);
                 //
                 //        cpd.powerup.AddLast(thresholdStartTime);
                 //        string effects = (string)props["i" + i + "pu" + j + "ef"];
                 //        string abilities = (string)props["i" + i + "pu" + j + "ab"];
                 //        
                 //    }
                    
                 //   Debug.LogError("Abilities "+cpd.name+" castTime:"+cpd.castTime+" cooldown:"+cpd.cooldownLength+" | "+ props["i" + i + "castTime"]+" "+ props["i" + i + "cooldL"]);
                    AtavismPrefabManager.Instance.SaveInteractiveObjectProfile(cpd);
                }
                if (props.ContainsKey("toRemove"))
                {
                    string keys = (string)props["toRemove"];
                    if (keys.Length > 0)
                    {
                        string[] _keys = keys.Split(';');
                        foreach (string k in _keys)
                        {
                            if (k.Length > 0)
                            {
                                AtavismPrefabManager.Instance.DeleteInteractiveObjectProfile(int.Parse(k));
                            }
                        }
                    }
                }

                if (sendAll)
                {
                    interactiveObjectsLoaded = true;
                    AtavismEventSystem.DispatchEvent("INTERACTIVE_OBJECT_PROFILE_UPDATE",  new string[]{});
                    AtavismPrefabManager.Instance.reloaded++;

                    if(AtavismLogger.logLevel <= LogLevel.Debug) 
                    Debug.Log("All data received. Running Queued interactive object profile update message.");
                }
                else
                {
                    AtavismPrefabManager.Instance.LoadInteractiveObjectProfilesData();
                    AtavismLogger.LogWarning("Not all interactive object profile data was sent by Prefab server");
                }
            }
            catch (System.Exception e)
            {
                AtavismLogger.LogError("Exception loading interactive object profile prefab data " + e);
            }
        }

        bool interactiveObjectsLoaded { get; set; }

        public void RegisterInteractiveObject(InteractiveObject iObj)
        {
            interactiveObjects[iObj.id] = iObj;
            if (interactiveObjectsQueueSatus.ContainsKey(iObj.id))
            {
                interactiveObjects[iObj.id].Active = interactiveObjectsQueueSatus[iObj.id];
                interactiveObjects[iObj.id].ResetHighlight();
                if (interactiveObjects[iObj.id].isLODChild)
                {
                    interactiveObjects[iObj.id].transform.parent.gameObject.SetActive(interactiveObjectsQueueSatus[iObj.id]);
                }
                else
                {
                    interactiveObjects[iObj.id].gameObject.SetActive(interactiveObjectsQueueSatus[iObj.id]);
                }

                if (interactiveObjectsQueueSatus[iObj.id])
                {
                    interactiveObjects[iObj.id].StateUpdated(interactiveObjectsQueueState[iObj.id]);
                }
                interactiveObjectsQueueSatus.Remove(iObj.id);
                interactiveObjectsQueueState.Remove(iObj.id);
            }
        }

        public void RemoveInteractiveObject(int id)
        {
            interactiveObjects.Remove(id);
        }

        public void HandleStarInteractionTask(Dictionary<string, object> props)
        {
           // ClientAPI.Write("Starting Interactive task with length: " + (float)props["length"]);
            float length = (float)props["length"];
            int  id = (int)props["intObjId"];
            string[] csArgs = new string[2];
            csArgs[0] = length.ToString();
            csArgs[1] = OID.fromLong(ClientAPI.GetPlayerOid()).ToString();
            AtavismEventSystem.DispatchEvent("CASTING_STARTED", csArgs);

          /*  Dictionary<string, object> props2 = new Dictionary<string, object>();
            props2.Add("gameObject", interactiveObjects[id].gameObject);
            CoordinatedEffectSystem.ExecuteCoordinatedEffect(interactiveObjects[id].activateCoordEffects[0].name, props2);*/
            //interactiveObjects[id].activateCoordEffects


        }
        public void HandleInterruptTask(Dictionary<string, object> props)
        {
            string[] args = new string[2];
            args[0] = "";
            args[1] = OID.fromLong(ClientAPI.GetPlayerOid()).ToString();
            AtavismEventSystem.DispatchEvent("CASTING_CANCELLED", args);

            ClientAPI.GetPlayerObject().MobController.PlayAnimation("", 0,"",1);
        }
        
        void HandleInteractiveObjectSpawnMessage(Dictionary<string, object> props)
        {
            int nodeID = (int)props["nodeID"];
            bool active = (bool)props["active"];
            string state = (string)props["state"];
            object goName = props["go"];
            
            AtavismLogger.LogDebugMessage("HandleInteractiveObjectSpawnMessage: nodeID="+nodeID+" active="+active+" state="+state+" goName="+goName);
            
          
        }

        void HandleInteractiveObjectStateMessage(Dictionary<string, object> props)
        {
            int nodeID = (int)props["nodeID"];
            bool active = (bool)props["active"];
            string state = (string)props["state"];
            if (interactiveObjects.ContainsKey(nodeID))
            {
                interactiveObjects[nodeID].Active = active;
                interactiveObjects[nodeID].ResetHighlight();

                if (interactiveObjects[nodeID].isLODChild)
                {
                    interactiveObjects[nodeID].transform.parent.gameObject.SetActive(active);
                }
                else
                {
                    interactiveObjects[nodeID].gameObject.SetActive(active);
                }

                if (active)
                {
                    interactiveObjects[nodeID].StateUpdated(state);
                }
            }
            else
            {
                if (interactiveObjectsQueueSatus.ContainsKey(nodeID))
                    interactiveObjectsQueueSatus[nodeID] = active;
                else
                    interactiveObjectsQueueSatus.Add(nodeID, active);
                if (interactiveObjectsQueueState.ContainsKey(nodeID))
                    interactiveObjectsQueueState[nodeID] = state;
                else
                    interactiveObjectsQueueState.Add(nodeID, state);
            }
        }

        public InteractiveObject getInteractiveObject(int id)
        {
            if (interactiveObjects.ContainsKey(id))
            {
                return interactiveObjects[id];
            }

            return null;
        }
        
        public static InteractiveObjectsManager Instance
        {
            get
            {
                return instance;
            }
        }
    }
}