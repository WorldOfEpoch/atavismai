using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Atavism
{

    public enum CoordinatedEffectTarget
    {
        Caster,
        Target
    }

    public abstract class CoordinatedEffect : MonoBehaviour
    {

        public CoordinatedEffectTarget target;
        public float activationDelay = 0f;
        protected float activationTime = 0f;
        public float duration = 1.5f;
        public bool destroyWhenFinished = true;
        protected Dictionary<string, object> props;
        protected OID casterOid;
        protected OID targetOid;
        public long id;
        public abstract void Execute(Dictionary<string, object> props);
        public abstract void CancelCoordEffect();

        protected float castingMod = 1f;
        public bool useCastingModToActivationDelayMod = false;
        public void OnDestroy()
        {
            if(id != -1L)
                CoordinatedEffectSystem.RemoveCoordinatedEffect(id);
        }

        
        
    }

    public class CoordinatedEffectSystem : MonoBehaviour
    {

        static Dictionary<long, GameObject> effectList = new Dictionary<long, GameObject>();
        // Dictionary of effects
        static Dictionary<string, CoordinatedEffect> effectRegistry =
                new Dictionary<string, CoordinatedEffect>();

        public static void RegisterCoordinatedEffect(string effectName, CoordinatedEffect co)
        {
            effectRegistry[effectName] = co;
            AtavismLogger.LogInfoMessage("Registered coordinated effect: " + effectName);
        }

        public static void UnregisterCoordinatedEffect(string effectName)
        {
            if (!effectRegistry.ContainsKey(effectName))
                return;
            effectRegistry.Remove(effectName);
        }

        public static void RemoveCoordinatedEffect(long ceId)
        {
            Monitor.Enter(effectList);
            if (effectList.ContainsKey(ceId))
            {
                effectList.Remove(ceId);
            }
            Monitor.Exit(effectList);
        } 
        public static void ExecuteCoordinatedEffect(string effectName, Dictionary<string, object> props)
        {
            if (AtavismLogger.isLogDebug())
            {
                string keys = " [ ";
                foreach (var it in props.Keys)
                {
                    keys += " ; " + it + " => " + props[it];
                }

                AtavismLogger.LogDebugMessage("Executing effect with name: " + effectName+"\n"+keys);
            }
            
            
            if (AtavismClient.Instance.resourceManager == null)
            {

                long ceId = (long)props["ceId"];
                if (props.ContainsKey("cancel"))
                {
                   Monitor.Enter(effectList);
                   if (effectList.ContainsKey(ceId))
                   {
                       GameObject go = effectList[ceId];
                       if (go != null)
                       {
                           go.SendMessage("CancelCoordEffect");
                           //    go.SendMessage("CancelCoordEffect", SendMessageOptions.DontRequireReceiver);
                           Destroy(go);
                       }

                       effectList.Remove(ceId);
                   }
                   Monitor.Exit(effectList);
                }
                else
                {
                    // Load prefab resource
                    if (effectName.Contains(".prefab"))
                    {
                        int resourcePathPos = effectName.IndexOf("Resources/");
                        effectName = effectName.Substring(resourcePathPos + 10);
                        effectName = effectName.Remove(effectName.Length - 7);
                    }
                    else
                    {
                        effectName = "Content/CoordinatedEffects/" + effectName;
                    }

                    AtavismLogger.LogDebugMessage("Executing effect with filename: " + effectName);
                    GameObject coordPrefab = (GameObject)Resources.Load(effectName);
                    if (coordPrefab == null)
                        return;
                    AtavismLogger.LogDebugMessage("Got coord prefab: " + coordPrefab.name);
                    GameObject coordObject = (GameObject)UnityEngine.Object.Instantiate(coordPrefab, Vector3.zero, Quaternion.identity);
                    AtavismLogger.LogDebugMessage("About to execute matching coord effect " + effectName + " " + coordObject);
                    CoordinatedEffect[] ces =   coordObject.GetComponents<CoordinatedEffect>();
                    foreach (var ce in ces)
                    {
                        ce.id = ceId;
                    }
                    coordObject.SendMessage("Execute", props);
                    Monitor.Enter(effectList);
                    if(ceId != -1L)
                    effectList.Add(ceId, coordObject);
                    Monitor.Exit(effectList);
                }
            }
            else
            {
                AtavismLogger.LogDebugMessage("Loading coordinated effect from external resource manager");
                long ceId = (long)props["ceId"];
                if (props.ContainsKey("cancel"))
                {
                    Monitor.Enter(effectList);
                    if (effectList.ContainsKey(ceId))
                    {
                        GameObject go = effectList[ceId];
                        if(go!=null)
                            go.SendMessage("CancelCoordEffect", SendMessageOptions.DontRequireReceiver);
                        effectList.Remove(ceId);
                    }
                    Monitor.Exit(effectList);
                }
                else
                {
                    string path = "";
                    string fileName = effectName;
                    int splitPos = effectName.LastIndexOf('/');
                    //Debug.Log("Split pos: " + splitPos);
                    if (splitPos != -1)
                    {
                        path = effectName.Substring(0, splitPos + 1);
                        fileName = effectName.Substring(splitPos + 1);
                    }

                    object asset = AtavismClient.Instance.resourceManager.LoadAsset(props, path, fileName);
                    if (asset != null)
                    {
                        GameObject coordObject = (GameObject)UnityEngine.Object.Instantiate((GameObject)asset, Vector3.zero, Quaternion.identity);
                        AtavismLogger.LogDebugMessage("About to execute matching coord effect");
                        CoordinatedEffect[] ces =   coordObject.GetComponents<CoordinatedEffect>();
                        foreach (var ce in ces)
                        {
                            ce.id = ceId;
                        }
                        coordObject.SendMessage("Execute", props);
                        Monitor.Enter(effectList);
                        if(ceId != -1L)
                            effectList.Add(ceId, coordObject);
                        Monitor.Exit(effectList);
                    }
                }
            }

            AtavismLogger.LogDebugMessage("ExecuteCoordinatedEffect end " + effectName);


           
        }
    }
}