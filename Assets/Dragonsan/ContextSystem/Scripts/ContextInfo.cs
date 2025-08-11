using Atavism;
using System.Collections.Generic;
using Atavism.UI;
using UnityEngine;

namespace Atavism
{
    public class ContextInfo : MonoBehaviour
    {
        // public Transform targetPoint; //Used as a reference point to grab coords for the canvas
        public Vector3 targetPosition = Vector3.up;
        [HideInInspector] public Vector3 centerPointCoords;

        [Space(10)] // 10 pixels of spacing here.

        public Sprite contextSpriteIcon; //Icon to display

        public string contextNameString = "";
        public string actionName = "";

        [Space(10)] public float contextObjectXOffset = 0f;
        public float contextObjectYOffset = 0f;

        [Space(10)] public float contextInteractXOffset = 0f;
        public float contextInteractYOffset = 0f;

        [Space(10)] public float interactTime = 0.25f;

        //[Space(10)]

        [HideInInspector] public AtavismNode parentNode;
        [HideInInspector] public GameObject parentObject;
        [HideInInspector] public UIAtavismContextPrefab parentUiObject;

        [HideInInspector]
        public bool isMob = false;

        [HideInInspector]
        public bool isResource = false;

        [HideInInspector]
        public bool isInteractiveObject = false;
        [HideInInspector]
        public bool isClaimObject = false;

        [HideInInspector] public bool isLoot = false;
        [HideInInspector] public bool contextUpdateNeeded = true;

        [HideInInspector]
        public bool hideContext = false;

        [HideInInspector]
        public bool hideInteract = false;

        [HideInInspector] public ClaimObject claimObject;

        [HideInInspector] public InteractiveObject interactiveObject;

        [HideInInspector] public ResourceNode resourceNode; 
        [HideInInspector] public GroundItemDisplay lootObject;
        private void OnDrawGizmosSelected()
        {

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(getPointPosition(), 0.1f);


        }

        private void Start()
        {
            contextUpdateNeeded = true;

            //   if (targetPoint == null)
            {
                if (GetComponent<Renderer>() != null)
                {
                    centerPointCoords = GetComponent<Renderer>().bounds.center;
                }
                else
                {
                    // targetPoint = transform;
                }
            }

            if (parentNode == null)
            {
                parentNode = GetComponent<AtavismNode>();
            }

            foreach (Collider child in GetComponentsInChildren<Collider>())
            {
                if (child.gameObject != gameObject)
                {
                    if (child.gameObject.GetComponent<ContextInfoChild>() == null)
                    {
                        ContextInfoChild cic = child.gameObject.AddComponent<ContextInfoChild>();
                        cic.parent = this;
                    }
                }
            }
            
            isResource = false;

            resourceNode = GetComponent<ResourceNode>();
            if (resourceNode != null)
            {
                isResource = true;
                actionName = "Harvest";
            }

            interactiveObject = GetComponent<InteractiveObject>();
            if (interactiveObject != null)
            {
                isInteractiveObject = true;
                actionName = "Interact";
                parentNode.AddLocalProperty("interactiveObject", true);
                
            }
            claimObject = GetComponent<ClaimObject>();
            if (claimObject != null)
            {
                isClaimObject = true;
                actionName = "Interact";
                parentNode.AddLocalProperty("interactiveObject", true);
                
            }
            lootObject =  GetComponent<GroundItemDisplay>();
            if (lootObject != null)
            {
                isLoot = true;
                actionName = "Loot";
            }
        }

        protected List<string> nodeProperties = new List<string>()
        {
            "harvestType",
            "mobType",
            "displayName",
            "lootable",
            "questconcludable",
            "questavailable",
            "questinprogress",
            "playerShop",
            "itemstosell",
            "dialogue_available",
            "bankteller",
            "Usable",
            "itemavailable",
            "skinnableLevel",
            "active",
            "interactiveObject",
            //"deadstate" //Not currently used
        };

        protected void ObjectNodeReady()
        {
            parentNode = GetComponent<AtavismNode>();

            if (parentNode != null)
            {
                foreach (string property in nodeProperties)
                {
                    parentNode.RegisterObjectPropertyChangeHandler(property, PropertyUpdateHandler);
                    if (!string.IsNullOrEmpty(property) && parentNode != null)
                    {
                        PropertyUpdateHandler(property, parentNode.GetProperty(property));
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (parentNode != null)
            {
                foreach (string property in nodeProperties)
                {
                    parentNode.RemoveObjectPropertyChangeHandler(property, PropertyUpdateHandler);
                }
            }
        }

        private void PropertyUpdateHandler(object sender, PropertyChangeEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.PropertyName) && parentNode != null)
            {
                PropertyUpdateHandler(args.PropertyName, parentNode.GetProperty(args.PropertyName));
            }
        }

        private void PropertyUpdateHandler(string propName, object propValue)
        {
            if (parentNode != null)
            {
                UpdateContext();
                contextUpdateNeeded = true;
            }
        }

        public void UpdateContext()
        {
            hideContext = false;
            IsMob = false;
            // Debug.LogError("ContextInfo.UpdateContext "+name);
            if (parentNode != null)
            {
                foreach (string property in nodeProperties)
                {
                    if (!string.IsNullOrEmpty(property))
                    {
                        object value = parentNode.GetProperty(property);
                        if (value != null)
                        {
                            switch (property)
                            {
                                case "active":
                                    if (!(bool)value)
                                    {
                                        hideInteract = true;
                                        hideContext = true;
                                   //     Debug.LogError("ContextInfo.UpdateContext active false return "+name);
                                        return;
                                    }
                                    else
                                    {
                                        hideInteract = false;
                                        hideContext = false;
                                    }

                                    break;
                                /*case "deadstate":

                                    break;*/
                                case "displayName":
                                    contextNameString = (string)value;
                                    break;
                                case "harvestType":
                                    actionName = "Harvest";
                                    isResource = true;
                                    break;
                                case "mobType":
                                    hideContext = true;
                                    hideInteract = true;
                                    IsMob = true;
                                    break;
                                case "skinnableLevel":
                                    actionName = "Skin";
                                    hideInteract = false;
                                    hideContext = false;
                                    break;
                                case "lootable":
                                    actionName = "Loot";
                                    hideInteract = false;
                                    hideContext = false;
                                    break;
                                case "questconcludable":
                                    actionName = "Talk";
                                    hideInteract = true;
                                    break;
                                case "questavailable":
                                    actionName = "Talk";
                                    hideInteract = false;
                                    break;
                                case "questinprogress":
                                    actionName = "Talk";
                                    hideInteract = false;
                                    break;
                                case "playerShop":
                                    actionName = "Shop";
                                    hideInteract = false;
                                    break;
                                case "itemstosell":
                                    actionName = "Shop";
                                    hideInteract = false;
                                    break;
                                case "dialogue_available":
                                    actionName = "Talk";
                                    hideInteract = false;
                                    break;
                                case "bankteller":
                                    actionName = "Access Bank";
                                    hideInteract = false;
                                    break;
                                case "Usable":
                                    actionName = "Interact";
                                    hideInteract = false;
                                    break;
                                case "itemavailable":
                                    actionName = "Loot";
                                    hideInteract = false;
                                    hideContext = false;
                                    break;
                                case "interactiveObject":
                                    actionName = "Interact";
                                    hideInteract = false;
                                    hideContext = false;
                                    break; 
                                case "claimObject":
                                    actionName = "Interact";
                                    hideInteract = false;
                                    hideContext = false;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                if (isClaimObject)
                {
                 //   Debug.LogError("ContextInfo.UpdateContext "+name+" claim");

                    if (!claimObject.InteractiveCheck())
                    {
                      // Debug.LogError("ContextInfo.UpdateContext "+name+" claim interact");

                        actionName = "Interact";
                        hideInteract = true;
                        hideContext = true;
                        return;
                    }
                    else
                    {
                       // Debug.LogError("ContextInfo.UpdateContext "+name+" claim not interact");

                        hideInteract = false;
                        hideContext = false;
                    }
                }

                if (isLoot)
                {
                    actionName = "Loot";
                    hideInteract = false;
                    hideContext = true;
                }


                if (string.IsNullOrEmpty(actionName))
                {
                    hideInteract = true;
                    hideContext = true;
                }
            }
            else
            {
              //  Debug.LogError("ContextInfo.UpdateContext "+name+" no parent node");
            }
        }

        public bool IsMob
        {
            get { return isMob; }

            set { isMob = value; }
        }

        public Vector3 getPointPosition()
        {
            return transform.position + targetPosition;
        }
    }
}