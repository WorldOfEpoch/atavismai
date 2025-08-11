using System.Collections.Generic;
using Atavism.UI;
using UnityEngine;

namespace Atavism
{


    public class GroundItemDisplay : MonoBehaviour
    {
        public GameObject dropEffectObject;
        public GameObject lootEffectObject;
        public string lootCoordEffect;

        [AtavismSeparator("Moving Object Setting")]
        public GameObject moveObject;
        public AnimationCurve moveCurve;
        public Vector3 moveOffset;
        public AnimationCurve rotationXCurve;
        public AnimationCurve rotationYCurve;
        public AnimationCurve rotationZCurve;
        public GameObject finalObject;
        [AtavismSeparator("Additional Marker Setting")]
        public GameObject marker;
        private long lootId;
        private int templateId = -1;
        private int stack = -1;
        private string Name = "";
        [HideInInspector]public int gridLocX;
        [HideInInspector]public int gridLocY;
        [HideInInspector]public int gridLocZ;
        public Vector3 startLoc;
        public Vector3 destLoc;
        protected float _animationTimePosition;
        protected bool dropAudioPlayed = false;
        
        [AtavismSeparator("Ui Element Setting")]
        public float height = 0.6f;
        [HideInInspector]public GroundItemDisplayUGUI uiElement;
        [HideInInspector]public UIAtavismGroundItemDisplay uiElementItem;
        Vector3 startRotation = Vector3.zero;

        public bool test = false;
        // Start is called before the first frame update
        void Start()
        {
            AtavismNode node = gameObject.AddComponent<AtavismNode>();
            node.AddLocalProperty("lootable", true);
        }


        public void Setup(long lootId, int templateId, int stack, string textName, Vector3 startLoc, Vector3 destLoc, int gridLocX, int gridLocY, int gridLocZ)
        {
            this.lootId = lootId;
            this.templateId = templateId;
            this.stack = stack;
            this.Name = textName;
            this.gridLocX = gridLocX;
            this.gridLocY = gridLocY;
            this.gridLocZ = gridLocZ;
            this.destLoc = destLoc;
            this.startLoc = startLoc;
            
            if (marker)
            {
                if(marker.activeSelf)
                    marker.SetActive(false);
            }
            ItemPrefabData ipd = AtavismPrefabManager.Instance.GetItemTemplateByID(templateId);
            if (ipd != null)
            {
                if (ipd.audioProfile > 0)
                {
                    ItemAudioProfileData iapd = AtavismPrefabManager.Instance.GetItemAudioProfileByID(ipd.audioProfile);
                    if (iapd != null)
                    {
                        AtavismInventoryAudioManager.Instance.PlayAudio(iapd.fall, gameObject);
                    }
                }
            }

            if (finalObject)
            {
                if (finalObject.activeSelf)
                    finalObject.SetActive(false);
            }
            if (moveObject != null)
            {
                startRotation = moveObject.transform.rotation.eulerAngles;
                moveObject.transform.position = startLoc;
                
            }
            if (uiElement)
            {
                if (uiElement.text)
                {
                    string title = stack > 1 ? stack + " " : "";
#if AT_I2LOC_PRESET
                    title = I2.Loc.LocalizationManager.GetTranslation("Items/"+ipd.name);
#else
                    title =  ipd.name;
#endif                    
                    uiElement.text.text = title;
                    uiElement.text.color = AtavismSettings.Instance.ItemQualityColor(ipd.quality);
                }
                uiElement.gameObject.SetActive(false);
            }
            if (uiElementItem!=null)
            {
                if (uiElementItem.text != null)
                {
                    string title = stack > 1 ? stack + " " : "";
#if AT_I2LOC_PRESET
                    title = I2.Loc.LocalizationManager.GetTranslation("Items/"+ipd.name);
#else
                    title =  ipd.name;
#endif                    
                    uiElementItem.text.text = title;
                    uiElementItem.m_Root.name = title;
                    uiElementItem.text.style.color = AtavismSettings.Instance.ItemQualityColor(ipd.quality);
                }
                uiElementItem.Hide();
            }
        }

        
        // Update is called once per frame
        void Update()
        {
            if (test)
            {
                test = false;
                moveObject.gameObject.SetActive(true);
                moveObject.transform.position = startLoc;
                moveObject.transform.rotation = Quaternion.Euler(startRotation);
                dropAudioPlayed = false;
                _animationTimePosition = 0;
                if(dropEffectObject && dropEffectObject.activeSelf)
                    dropEffectObject.SetActive(false);
                if(marker &&marker.activeSelf)
                        marker.SetActive(false);
                if (finalObject)
                {
                    if (finalObject.activeSelf)
                        finalObject.SetActive(false);
                }
            }
                
            if (moveObject != null)
            {
                if (destLoc != moveObject.transform.position)
                {
                    _animationTimePosition += Time.deltaTime;
                    Vector3 positionOffset = moveCurve.Evaluate(_animationTimePosition) * moveOffset;
                    moveObject.transform.position = Vector3.Lerp(startLoc, destLoc, _animationTimePosition)+positionOffset;
                    float rotationX = rotationXCurve.Evaluate(_animationTimePosition) * 360;
                    float rotationY = rotationYCurve.Evaluate(_animationTimePosition) * 360;
                    float rotationZ = rotationZCurve.Evaluate(_animationTimePosition) * 360;
                    
                    moveObject.transform.rotation = Quaternion.Euler(startRotation.x+rotationX,startRotation.y+rotationY,startRotation.z+rotationZ);
                    if (Vector3.Distance(destLoc, moveObject.transform.position) < 0.019)
                    {
                        moveObject.transform.position = destLoc;
                    }
                }
                else
                {
                    
                    if (!dropAudioPlayed )
                    {
                        dropAudioPlayed = true;
                        ItemPrefabData ipd = AtavismPrefabManager.Instance.GetItemTemplateByID(templateId);
                        if (ipd != null)
                        {
                            if (ipd.audioProfile > 0)
                            {
                                ItemAudioProfileData iapd = AtavismPrefabManager.Instance.GetItemAudioProfileByID(ipd.audioProfile);
                                if (iapd != null)
                                {
                                    AtavismInventoryAudioManager.Instance.PlayAudio(iapd.drop, gameObject);
                                }
                            }
                        }
                       
                    }
                    
                    if (finalObject!=null && moveObject)
                    {
                        if (moveObject.gameObject.activeSelf)
                            moveObject.gameObject.SetActive(false);
                    }
                    
                    if (finalObject)
                    {
                        if (!finalObject.activeSelf)
                            finalObject.SetActive(true);
                    }
                    
                    if (marker)
                    {
                        if(!marker.activeSelf)
                            marker.SetActive(true);
                    }

                    if (dropEffectObject)
                    {
                        if(!dropEffectObject.activeSelf)
                            dropEffectObject.SetActive(true);
                    }
                    if (uiElement)
                    {
                        if (!uiElement.gameObject.activeSelf)
                        {
                            uiElement.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (uiElement)
            {
                Destroy(uiElement.gameObject);
            }
            if (uiElementItem!=null)
                uiElementItem.Delete();
        }

        public void Loot()
        {
            if (lootId != null)
            {
                if (lootEffectObject)
                {
                    GameObject go = Instantiate(lootEffectObject, transform.position, transform.rotation);
                    if (!go.activeSelf)
                        go.SetActive(true);
                    Destroy(go,10);
                }

                if (lootCoordEffect.Length > 0)
                { 
                    Dictionary<string, object> props = new Dictionary<string, object>();
                    props.Add("gameObject", gameObject);
                    props.Add("sourceOID", OID.fromLong(ClientAPI.GetPlayerOid()));
                    props.Add("targetOID", OID.fromLong(ClientAPI.GetPlayerOid()));
                    props.Add("ceId", -1L);
                    CoordinatedEffectSystem.ExecuteCoordinatedEffect(lootCoordEffect, props);
                }
            }

            Dictionary<string, object> props1 = new Dictionary<string, object>();
            int i = 0;
            props1.Add("o" + i, lootId);
            props1.Add("num", 1);
            NetworkAPI.SendExtensionMessage(0, false, "inventory.LOOT_GROUND", props1);
            ItemPrefabData ipd = AtavismPrefabManager.Instance.GetItemTemplateByID(templateId);
            if (ipd != null)
            {
                if (ipd.audioProfile > 0)
                {
                    ItemAudioProfileData iapd = AtavismPrefabManager.Instance.GetItemAudioProfileByID(ipd.audioProfile);
                    if (iapd != null)
                    {
                        AtavismInventoryAudioManager.Instance.PlayAudio(iapd.pick_up, gameObject);
                    }
                }
            }
        }
    
        
        void OnMouseDown()
          {
           //   Debug.LogError("OnMouseDown");
              if (!AtavismSettings.Instance.isWindowOpened() && !AtavismSettings.Instance.isMenuBarOpened)
              {
                  Transform cam = Camera.main.transform;
                  SDETargeting sde = cam.transform.GetComponent<SDETargeting>();

                  if (sde != null && sde.softTargetMode)
                  {
                      return;
                  }
              }

              if (!AtavismCursor.Instance.IsMouseOverUI())
              {
                  
                  Loot();
              }
          }

          void OnMouseOver()
          {
            
          }

          void OnMouseExit()
          {
            
          }
          static string ToRGBHex(Color c)
          {
              return string.Format("#{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
          }

          private static byte ToByte(float f)
          {
              f = Mathf.Clamp01(f);
              return (byte)(f * 255);
          }

          public int TemplateId
          {
              get { return templateId; }
          }
          public long Id
          {
              get { return lootId; }
          }
          
          public Vector3 getPointPosition()
          {
              return transform.position + Vector3.up * height;
          }
    }
}