using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Atavism;
using UnityEngine.SceneManagement;

namespace Atavism
{
    public class ContextManager : MonoBehaviour
    {
        public GameObject contextPrefab;

        public Transform centerPointSensor;
        public LayerMask targetLayers;

        public float maxRadiusSensor;
        public float maxDistanceNearby;
        public float secondsToCheck = 1; //How often should the in range check occur

        public float timeToInteract = 0.25f;
        [HideInInspector] public float lastInteractTime = 0f;
        public float interactCooldownTime = 2f;

        private bool interacted = false;
        private float lastInteractActivatedTime = 0f;

        Dictionary<int, ContextPrefab> contextObjects = new Dictionary<int, ContextPrefab>();

        private ContextPrefab closestContext = null;
        [HideInInspector] public AtavismNode closestContextNode; //Used for caching the node   

        public GameObject contextCanvas;

        [HideInInspector] public AtavismNode hoverObject = null;


        // Start is called before the first frame update
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                StartCoroutine(checkSensorsAndRange(secondsToCheck));
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (contextCanvas == null)
            {
                contextCanvas = GameObject.FindGameObjectWithTag("ContextUI");
            }
        }

        private bool keybindDown = false;

        // Update is called once per frame
        void Update()
        {
            if (centerPointSensor == null && ClientAPI.GetPlayerObject() != null)
            {
                centerPointSensor = ClientAPI.GetPlayerObject().GetControllingTransform();
            }

            if (closestContextNode != null && closestContext != null && closestContext.contextInfo != null)
            {
                if (!ClientAPI.UIHasFocus() && (Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().action.key) || Input.GetKeyDown(AtavismSettings.Instance.GetKeySettings().action.altKey)))
                {
                    keybindDown = true;
                }

                if (!ClientAPI.UIHasFocus() && (Input.GetKeyUp(AtavismSettings.Instance.GetKeySettings().action.key) || Input.GetKeyUp(AtavismSettings.Instance.GetKeySettings().action.altKey)))
                {
                    keybindDown = false;
                }

                if (!interacted && keybindDown && !AtavismCursor.Instance.IsMouseOverUI())
                {
                    lastInteractTime += Time.deltaTime;

                    if (lastInteractTime >= closestContext.contextInfo.interactTime)
                    {
                        Interact(closestContextNode);
                        lastInteractTime = 0f;
                    }
                }

                if (closestContext != null && closestContext.contextInfo.parentObject != null)
                {
                    if (closestContext.interactFiller != null)
                    {
                        closestContext.interactFiller.fillAmount = Mathf.Lerp(0, 1, lastInteractTime / closestContext.contextInfo.interactTime);
                    }
                }
            }

            if (!keybindDown)
            {
                if (lastInteractTime > 0)
                {
                    lastInteractTime -= Time.deltaTime;
                }
                else
                {
                    lastInteractTime = 0f;
                }

                if (interacted && (Time.time - lastInteractActivatedTime) > interactCooldownTime)
                {
                    interacted = false;
                    lastInteractActivatedTime = 0f;
                    lastInteractTime = 0f;
                }
            }
        }

        void UpdateContext(ContextPrefab contextPrefab)
        {
          //  Debug.LogError("UpdateContext");
            if (contextPrefab != null && contextPrefab.contextInfo != null)
            {
                bool sameAsClosestContextNode = false;
                if (closestContextNode != null && closestContextNode == contextPrefab.contextInfo.parentNode)
                {
                    sameAsClosestContextNode = true;
                }
                // else
                //     Debug.LogError("UpdateContext else 2");


                if (sameAsClosestContextNode || contextPrefab.contextInfo.contextUpdateNeeded) //If it is the closest context node always update else only do it once.
                {
                    contextPrefab.contextInfo.UpdateContext();
                    contextPrefab.SetContextName(contextPrefab.contextInfo.contextNameString);
                    contextPrefab.SetInteractRow1Text(contextPrefab.contextInfo.actionName);
                    contextPrefab.contextInfo.contextUpdateNeeded = false; //Context updated set to false
                    contextPrefab.buttonText.text =
                        AtavismSettings.Instance.GetKeySettings().action.key != KeyCode.None ? AtavismSettings.Instance.GetKeySettings().action.key.ToString().ToUpper() :
                        AtavismSettings.Instance.GetKeySettings().action.altKey != KeyCode.None ? AtavismSettings.Instance.GetKeySettings().action.key.ToString().ToUpper() : ""; //Set button text to match the keybind
                }
                // else
                //     Debug.LogError("UpdateContext else 3");
            }
            // else
            //     Debug.LogError("UpdateContext else 1");
        }

        public void Interact(AtavismNode contextNode)
        {
            // Debug.LogError("Interact");
            lastInteractActivatedTime = Time.time;
            interacted = true;

            if (contextNode != null)
            {
                if (contextNode.CheckBooleanProperty("lootable"))
                {
                    Debug.LogError("Interact");
                    if (closestContext != null && closestContext.contextInfo != null && closestContext.contextInfo.isLoot)
                    {
                        Debug.LogError("Interact Loot");
                        GroundItemDisplay gid = closestContext.contextInfo.lootObject;
                        if (gid != null)
                        {
                            gid.Loot();
                        }
                    }
                    else
                    {
                        NetworkAPI.SendTargetedCommand(contextNode.Oid, "/getLootList");
                    }
                }
                else if (contextNode.CheckBooleanProperty("questconcludable"))
                {
                    NpcInteraction.Instance.GetInteractionOptionsForNpc(contextNode.Oid);
                }
                else if (contextNode.CheckBooleanProperty("questavailable"))
                {
                    NpcInteraction.Instance.GetInteractionOptionsForNpc(contextNode.Oid);
                }
                else if (contextNode.CheckBooleanProperty("questinprogress"))
                {
                    NpcInteraction.Instance.GetInteractionOptionsForNpc(contextNode.Oid);
                }
                else if (contextNode.PropertyExists("playerShop") && (bool)contextNode.GetProperty("playerShop"))
                {
                    Dictionary<string, object> props = new Dictionary<string, object>();
                    props.Add("playerOid", contextNode.Oid);
                    props.Add("shop", contextNode.GetProperty("plyShopId"));
                    NetworkAPI.SendExtensionMessage(0, false, "inventory.openPlayerShop", props);
                }
                else if (contextNode.CheckBooleanProperty("itemstosell"))
                {
                    NpcInteraction.Instance.GetInteractionOptionsForNpc(contextNode.Oid);
                }
                else if (contextNode.PropertyExists("dialogue_available"))
                {
                    int dialogueID = (int)contextNode.GetProperty("dialogue_available");
                    if (dialogueID > 0)
                    {
                        NpcInteraction.Instance.GetInteractionOptionsForNpc(contextNode.Oid);
                    }
                }
                else if (contextNode.CheckBooleanProperty("bankteller"))
                {
                    NpcInteraction.Instance.GetInteractionOptionsForNpc(contextNode.Oid);
                }
                else if (contextNode.CheckBooleanProperty("Usable"))
                {
                    long playerOid = ClientAPI.GetPlayerObject().Oid;
                    Dictionary<string, object> props = new Dictionary<string, object>();
                    props.Add("object", contextNode.Oid);
                    NetworkAPI.SendExtensionMessage(playerOid, false, "ao.OBJECT_ACTIVATED", props);
                }
                else if (contextNode.CheckBooleanProperty("itemavailable"))
                {
                    NetworkAPI.SendTargetedCommand(contextNode.Oid, "/openMob");
                }
                else if (contextNode.PropertyExists("skinnableLevel"))
                {
                    int id = (int)contextNode.Oid * -1;
                    Crafting.Instance.HarvestResource(id);
                }
                else if (closestContext != null && closestContext.contextInfo != null && closestContext.contextInfo.isResource)
                {
                    // Debug.LogError("Interact Resource");
                    ResourceNode resourceNode = closestContext.contextInfo.GetComponent<ResourceNode>();
                    if (resourceNode != null)
                    {
                        resourceNode.HarvestResource();
                    }
                }
                else if (closestContext != null && closestContext.contextInfo != null && closestContext.contextInfo.isInteractiveObject)
                {
                    // Debug.LogError("Interact Interactive Object");
                    InteractiveObject interactiveObject = closestContext.contextInfo.GetComponent<InteractiveObject>();
                    if (interactiveObject != null)
                    {
                        interactiveObject.StartInteraction();
                    }
                } else if (closestContext != null && closestContext.contextInfo != null && closestContext.contextInfo.isClaimObject)
                {
                    // Debug.LogError("Interact Claim");
                    ClaimObject claimObject = closestContext.contextInfo.GetComponent<ClaimObject>();
                    if (claimObject != null)
                    {
                        claimObject.Interact();
                    }
                }

                //NetworkAPI.SendTargetedCommand(Inventory.Instance.LootTarget.ToLong(), "/lootAll");
            }
        }

        IEnumerator checkSensorsAndRange(float waitTime)
        {
            while (true)
            {
                if (centerPointSensor != null && Camera.main != null)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(centerPointSensor.position, maxRadiusSensor, targetLayers);

                    Dictionary<int, ContextPrefab> contexts = new Dictionary<int, ContextPrefab>(contextObjects);

                    float closestContextRange = Mathf.Infinity;
                    bool focusedOverwrite = false; //Used to priortize gameobject that is nearby and focused by the cursor.                

                    GameObject mouseOver = null;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, maxRadiusSensor, targetLayers))
                    {
                        mouseOver = hit.transform.gameObject;
                    }

                    // Get objectnode from object
                    if (mouseOver != null)
                    {
                        hoverObject = mouseOver.GetComponent<AtavismNode>();
                    }
                    else
                    {
                        hoverObject = null;
                    }

                    ContextPrefab tempPrefab = null;

                    foreach (Collider hitCollider in hitColliders)
                    {
                        if (hitCollider != null && hitCollider.gameObject != null)
                        {
                            ContextPrefab context = null;
                            ContextInfo contextInfo = null;
                            int instanceID = hitCollider.gameObject.GetInstanceID();
                            if (contextObjects.TryGetValue(instanceID, out context))
                            {
                                contexts.Remove(instanceID); //Remove from list because it is found
                                contextInfo = context.contextInfo;
                            }
                            else
                            {
                                contextInfo = hitCollider.gameObject.GetComponent<ContextInfo>();
                                if (contextInfo == null)
                                {
                                    ContextInfoChild cic = hitCollider.gameObject.GetComponent<ContextInfoChild>();
                                    if (cic != null)
                                    {
                                        contextInfo = cic.parent;
                                    }
                                }
                                if (contextInfo != null)
                                {
                                    if (contextInfo.parentObject == null && contextPrefab != null && contextCanvas != null)
                                    {
                                        contextInfo.parentObject = Instantiate(contextPrefab, contextCanvas.transform);
                                        context = contextInfo.parentObject.GetComponent<ContextPrefab>();

                                        if (context != null)
                                        {
                                            context.contextInfo = contextInfo;
                                            contextObjects.Add(instanceID, context);
                                        }
                                    }
                                }
                                else
                                {
                                 //   Debug.Log("contextInfo is null for " + hitCollider.gameObject.name);
                                }
                            }



                            if (context != null && contextInfo != null)
                            {
                                context.playerInRange = true;
                                context.isFocused = false; //Reset Focus

                                if (context.gameObject != null && !focusedOverwrite)
                                {
                                    float distance = getDistance(centerPointSensor.position, contextInfo.getPointPosition());
                                    if (distance <= maxDistanceNearby)
                                    {
                                        if (contextInfo.parentNode == null)
                                        {
                                            contextInfo.parentNode = contextInfo.GetComponent<AtavismNode>();
                                        }

                                        if (contextInfo.parentNode != null && hoverObject != null && hoverObject == contextInfo.parentNode)
                                        {
                                            focusedOverwrite = true;
                                        }

                                        if (focusedOverwrite || distance < closestContextRange)
                                        {
                                            closestContext = context;
                                            tempPrefab = context;
                                            closestContextRange = distance;
                                            closestContextNode = contextInfo.parentNode;
                                        }
                                    }
                                }

                                UpdateContext(context);
                            }
                        }
                    }

                    if (tempPrefab == null)
                    {
                        if (closestContext != null)
                        {
                            if (closestContext.interactFiller != null)
                            {
                                closestContext.interactFiller.fillAmount = 0f;
                            }

                            lastInteractTime = 0f;
                        }

                        closestContext = null;
                    }

                    if (closestContext != null)
                    {
                        closestContext.isFocused = true;
                    }

                    //Check left over and remove from list because they are no longer found nearby
                    foreach (KeyValuePair<int, ContextPrefab> keyValuePair in contexts)
                    {
                        ContextPrefab context = keyValuePair.Value;
                        if (context != null)
                        {
                            context.playerInRange = false;
                            Destroy(context.contextInfo.parentObject);
                            context.contextInfo.parentObject = null;
                            if (context.contextInfo.parentNode == closestContextNode)
                            {
                                closestContextNode = null;
                            }
                        }

                        contextObjects.Remove(keyValuePair.Key);
                    }
                }

                yield return new WaitForSeconds(waitTime);
            }
        }

        private float getDistance(Vector3 pos, Vector3 targetPos)
        {
            return Vector3.Distance(pos, targetPos);
        }

        static ContextManager instance;

        public static ContextManager Instance
        {
            get { return instance; }
        }
    }
}