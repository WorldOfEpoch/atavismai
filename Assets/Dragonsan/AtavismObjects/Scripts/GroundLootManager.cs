using System;
using System.Collections;
using System.Collections.Generic;
using Atavism.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Atavism
{
    public class GroundItem
    {
        public long id;
        public Vector3 loc;
        public int templateId;
        public int stack;
    }

    public class GroundLootManager : MonoBehaviour
    {
        static GroundLootManager instance;
        protected Dictionary<long, GroundItem> items = new Dictionary<long, GroundItem>();
        protected Dictionary<long, GroundItemDisplay> itemsSpawned = new Dictionary<long, GroundItemDisplay>();
        protected Dictionary<int, Dictionary<int, Dictionary<int, long>>> itemsLocation = new Dictionary<int, Dictionary<int, Dictionary<int, long>>>();

        [SerializeField] private GroundItemDisplayUGUI itemUiPrefab;
        [SerializeField] private VisualTreeAsset uiElementItemPrefab;
        [SerializeField] private float distanceToDespawn = 10f;
        [SerializeField] private float distanceMaxSpawn = 10f;
        [SerializeField] private float secondsToCheckSpawn = 0.5f;
        [SerializeField] private float gridSize = 0.5f;
        [SerializeField] float spawnOverTheTerrain = 0.2f;
        [SerializeField] LayerMask groundLayer = (1 << 0) | (1 << 30) | (1 << 26) | (1 << 20);
         RaycastHit groundHit;
         [SerializeField] float groundMaxRayDistance = 5f;

        [SerializeField] private float secondsToCheckLabels = 0.2f;
        [SerializeField] private bool useCoroutineToCheckLabels = true;
        [HideInInspector] GameObject itemCanvas;
        [HideInInspector] public VisualElement uiItemCanvas;
        [HideInInspector] Transform centerPointSensor;

        // Start is called before the first frame update
        void Start()
        {

            if (instance != null)
            {
                Destroy(this);
                return;
            }

            instance = this;
            StartCoroutine(checkRange(secondsToCheckSpawn));
            NetworkAPI.RegisterExtensionMessageHandler("LootGroundUpdate", HandleLootGroundUpdate);
            SceneManager.sceneLoaded += OnSceneLoaded;
            if(useCoroutineToCheckLabels)
            StartCoroutine(checkLabels(secondsToCheckLabels));
        }

        private void OnDestroy()
        {
            NetworkAPI.RemoveExtensionMessageHandler("LootGroundUpdate", HandleLootGroundUpdate);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private IEnumerator checkRange(float f)
        {
            WaitForSeconds delay = new WaitForSeconds(f);
            while (true)
            {
                Vector3 position = Vector3.zero;
                if (ClientAPI.GetPlayerObject() != null && ClientAPI.GetPlayerObject().GameObject != null)
                    position = ClientAPI.GetPlayerObject().GameObject.transform.position;

                bool spawned = false;
                // Debug.LogError("Items count "+items.Count);
                foreach (var item in items.Values)
                {
                    float distance = Vector3.Distance(item.loc, position);
                    //    Debug.LogError("distance "+distance+" distanceToDespawn="+distanceToDespawn+" distanceMaxSpawn="+distanceMaxSpawn);
                    if (distance > distanceToDespawn)
                    {
                        if (itemsSpawned.ContainsKey(item.id))
                        {
                            GroundItemDisplay go = itemsSpawned[item.id];
                            itemsSpawned.Remove(item.id);
                            if (go != null)
                            {
                                itemsLocation[go.gridLocY][go.gridLocX].Remove(go.gridLocZ);
                                if (itemsLocation[go.gridLocY][go.gridLocX].Count == 0)
                                    itemsLocation[go.gridLocY].Remove(go.gridLocX);
                                if (itemsLocation[go.gridLocY].Count == 0)
                                    itemsLocation.Remove(go.gridLocY);
                                if (go.uiElement != null)
                                    Destroy(go.uiElement.gameObject);
                                Destroy(go.gameObject);
                            }
                        }
                    }
                    else if (distance < distanceMaxSpawn)
                    {
                        
                        if (!itemsSpawned.ContainsKey(item.id) && !spawned)
                        {
                            ItemPrefabData ipd = AtavismPrefabManager.Instance.GetItemTemplateByID(item.templateId);
                            if (ipd.groundPrefab != null && ipd.groundPrefab.Length > 0)
                            {
                                string prefabName = ipd.groundPrefab;
                                if (prefabName.Contains(".prefab"))
                                {
                                    int resourcePathPos = prefabName.IndexOf("Resources/");
                                    prefabName = prefabName.Substring(resourcePathPos + 10);
                                    prefabName = prefabName.Remove(prefabName.Length - 7);
                                }

                                GameObject prefab = (GameObject)Resources.Load(prefabName);
                                GameObject go = null;
                                int x = (int)(item.loc.x / gridSize);
                                int y = (int)(item.loc.y / gridSize);
                                int z = (int)(item.loc.z / gridSize);

                                bool found = false;
                                if (itemsLocation.ContainsKey(y))
                                {
                                    if (itemsLocation[y].ContainsKey(x))
                                    {
                                        if (itemsLocation[y][x].ContainsKey(z))
                                        {
                                            for (var i = 1; i < 10; i++)
                                            {
                                                if (found)
                                                    break;
                                                for (var ix = x - i; ix < x + i; ix++)
                                                {
                                                    if (found)
                                                        break;
                                                    for (var iz = z - i; iz < z + i; iz++)
                                                    {
                                                        if (itemsLocation[y].ContainsKey(ix) && itemsLocation[y][ix].ContainsKey(iz))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            found = true;
                                                            if (!itemsLocation[y].ContainsKey(ix))
                                                                itemsLocation[y].Add(ix, new Dictionary<int, long>());
                                                            itemsLocation[y][ix].Add(iz, item.id);
                                                            x = ix;
                                                            z = iz;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            itemsLocation[y][x].Add(z, item.id);
                                        }

                                    }
                                    else
                                    {
                                        itemsLocation[y].Add(x, new Dictionary<int, long>());
                                        itemsLocation[y][x].Add(z, item.id);
                                    }
                                }
                                else
                                {
                                    itemsLocation.Add(y, new Dictionary<int, Dictionary<int, long>>());
                                    itemsLocation[y].Add(x, new Dictionary<int, long>());
                                    itemsLocation[y][x].Add(z, item.id);
                                }

                                Vector3 spawnLoc = new(x * gridSize, y * gridSize, z * gridSize);
                                Ray ray1 = new Ray(spawnLoc + new Vector3(0, 2f, 0), Vector3.down);
                                // raycast for check the ground distance
                                if (Physics.Raycast(ray1, out groundHit, groundMaxRayDistance, groundLayer))
                                {
                                    spawnLoc = groundHit.point;
                                }

                                spawnLoc += Vector3.up * spawnOverTheTerrain;
                                if (prefab != null)
                                {
                                    go = (GameObject)Instantiate(prefab, spawnLoc, Quaternion.identity);
                                }
                                else
                                {
                                    Debug.LogError("item prefab is null model: " + prefabName);
                                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    go.transform.position = spawnLoc;
                                    go.AddComponent<GroundItemDisplay>();
                                    var ci =go.AddComponent<ContextInfo>();
                                    ci.actionName = "Loot";
                                }

                                if (go != null)
                                {
                                    GroundItemDisplay groundItemDisplay = go.GetComponent<GroundItemDisplay>();
                                    itemsSpawned.Add(item.id, groundItemDisplay);

                                    if (itemCanvas != null)
                                    {
                                        groundItemDisplay.uiElement = Instantiate(itemUiPrefab, itemCanvas.transform);
                                        groundItemDisplay.uiElement.groundItemDisplay = groundItemDisplay;
                                    }

                                    if (uiItemCanvas != null)
                                    {
                                        VisualElement v = uiElementItemPrefab.Instantiate();
                                        groundItemDisplay.uiElementItem =
                                            new UIAtavismGroundItemDisplay(v, groundItemDisplay);
                                        uiItemCanvas.Add(groundItemDisplay.uiElementItem.m_Root);
                                    }

                                    groundItemDisplay.Setup(item.id, item.templateId, item.stack, "", item.loc, spawnLoc, x, y, z);

                                }
                            }

                            spawned = true;
                        }
                    }
                }

                //   Debug.LogError("checkRange itemsSpawned count "+itemsSpawned.Count);
                yield return delay;
            }

        }

        private void HandleLootGroundUpdate(Dictionary<string, object> props)
        {
            if (AtavismLogger.isLogDebug())
            {
                AtavismLogger.LogDebugMessage("HandleLootGroundUpdate");
                string keys = " [ ";
                foreach (string it in props.Keys)
                {
                    if (!it.Contains("icon2"))
                        keys += " ; " + it + " => " + props[it] + "\n";
                    if (keys.Length > 10000)
                    {
                        AtavismLogger.LogDebugMessage("HandleLootGroundUpdate: keys:" + keys);
                        keys = "";
                    }
                }

                AtavismLogger.LogDebugMessage("HandleLootGroundUpdate: keys:" + keys);
            }

            int num = (int)props["num"];
            List<long> keyList = new List<long>(this.items.Keys);

            for (int i = 0; i < num; i++)
            {
                OID id = (OID)props["i" + i + "id"];
                keyList.Remove(id.ToLong());
                if (!items.ContainsKey(id.ToLong()))
                {
                    int templateId = (int)props["i" + i + "tid"];
                    int stack = (int)props["i" + i + "s"];
                    Vector3 loc = (Vector3)props["i" + i + "loc"];
                    //  Debug.LogError("HandleLootGroundUpdate id " + id + " templateId = " + templateId + " stack=" + stack + " loc=" + loc);
                    GroundItem gi = new GroundItem();
                    gi.id = id.ToLong();
                    gi.templateId = templateId;
                    gi.stack = stack;
                    gi.loc = loc;
                    items.Add(gi.id, gi);
                }
                else
                {
                    //  Debug.LogError("HandleLootGroundUpdate ContainsKey id " + id);

                }
            }

            foreach (var key in keyList)
            {
                if (itemsSpawned.ContainsKey(key))
                {
                    GroundItemDisplay go = itemsSpawned[key];
                    itemsSpawned.Remove(key);
                    itemsLocation[go.gridLocY][go.gridLocX].Remove(go.gridLocZ);
                    if (itemsLocation[go.gridLocY][go.gridLocX].Count == 0)
                        itemsLocation[go.gridLocY].Remove(go.gridLocX);
                    if (itemsLocation[go.gridLocY].Count == 0)
                        itemsLocation.Remove(go.gridLocY);
                    GameObject.Destroy(go.gameObject);
                }

                items.Remove(key);
            }
            // Debug.LogError("HandleLootGroundUpdate items "+items.Count);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals("Login"))
            {
                items.Clear();
                itemsSpawned.Clear();
                itemsLocation.Clear();
            }

            if (itemCanvas == null)
            {
                itemCanvas = GameObject.FindGameObjectWithTag("ItemUI");
            }

        }

        // Update is called once per frame
        void Update()
        {
            if(camera==null)
                camera = Camera.main;
            if (centerPointSensor == null && ClientAPI.GetPlayerObject() != null)
            {
                centerPointSensor = ClientAPI.GetPlayerObject().GetControllingTransform();
            }

            if (!useCoroutineToCheckLabels)
            {
                CheckPositionsOfLabels();
            }
        }

        private Camera camera = null;
        IEnumerator checkLabels(float waitTime)
        {
            while (true)
            {
    
                try
                {
                    CheckPositionsOfLabels();
                }
                catch (Exception e)
                {
                    
                }
                //  Debug.LogError("Start !!!!!!!!!!!!!");
                yield return new WaitForSeconds(waitTime);
            }
        }

        void CheckPositionsOfLabels()
        {
            bool skip = false;
            int c = 0;
            int d = 0;
            if (camera != null)
            {
                Dictionary<long, GroundItemDisplay> _itemsSpawned = new Dictionary<long, GroundItemDisplay>(itemsSpawned);
                foreach (var gid1 in _itemsSpawned.Values)
                {
                    skip = false;
                    d = 0;
                    if (gid1 != null)
                    {
                        // UGUI
                        Vector3 screenPos = camera.WorldToScreenPoint(gid1.getPointPosition());
                        if (gid1.uiElement != null)
                        {
                            gid1.uiElement.transform.position = new Vector3(screenPos.x, screenPos.y);
                            //   Debug.LogError(" screenPos " + gid1.Id + " " + c + " " + d +" move  ---> " + gid1.uiElement.rect.position+ " "+ gid1.uiElement.rect.rect.position+" size "+gid1.uiElement.rect.rect.size);

                            if (gid1.uiElement != null)
                            {

                                float dy1 = 0f;
                                float dy2 = 0f;
                                float dx1 = 0f;
                                float dx2 = 0f;
                                int numChange = 0;
                                List<RectTransform> oldCheck = new List<RectTransform>();
                                foreach (var gid in _itemsSpawned.Values)
                                {

                                    if (!skip)
                                        if (gid1.Id != gid.Id)
                                        {
                                            if (CheckOverlap(gid1.uiElement.rect, gid.uiElement.rect))
                                            {
                                                //  Debug.LogError("OverLap "+gid1.Id+ " "+gid.Id+" "+c+" "+d);
                                                // float _dy1 = (gid1.uiElement.rect.localPosition.y - gid.uiElement.rect.localPosition.y);
                                                // float _dx1 = (gid1.uiElement.rect.localPosition.x - gid1.uiElement.rect.rect.width / 2 - gid.uiElement.rect.localPosition.x + gid.uiElement.rect.rect.width / 2);
                                                gid1.uiElement.transform.position = new Vector3(gid1.uiElement.transform.position.x, gid.uiElement.transform.position.y + (gid1.uiElement.rect.rect.height + 5) * gid1.uiElement.rect.lossyScale.y);

                                                //    Debug.LogError("Overlaped " + gid1.Id + " " + gid.Id + " " + c + " " + d +" move from" + screenPos + " ---> " + gid1.uiElement.rect.position+ " "+ gid1.uiElement.rect.rect.position+" size "+gid1.uiElement.rect.rect.size);
                                                //    yield return new WaitForEndOfFrame();
                                                int ildid = 0;
                                                bool run = true;

                                                while (run && ildid < 1000)
                                                {
                                                    run = false;
                                                    foreach (var v in oldCheck)
                                                    {
                                                        if (CheckOverlap(v, gid1.uiElement.rect))
                                                        {
                                                            // float aa_dy1 = (gid1.uiElement.rect.localPosition.y - gid.uiElement.rect.localPosition.y);
                                                            // float aa_dx1 = (gid1.uiElement.rect.localPosition.x - gid1.uiElement.rect.rect.width / 2 - gid.uiElement.rect.localPosition.x + gid.uiElement.rect.rect.width / 2);
                                                            // float a_dy1 = (gid1.uiElement.rect.localPosition.y + gid1.uiElement.rect.rect.height - v.localPosition.y);
                                                            // float a_dy2 = (v.localPosition.y + v.rect.height - gid1.uiElement.rect.localPosition.y);
                                                            // float a_dx1 = (v.localPosition.x + v.rect.width - gid1.uiElement.rect.localPosition.x);
                                                            // float a_dx2 = (gid1.uiElement.rect.localPosition.x + gid1.uiElement.rect.rect.width - v.localPosition.x);
                                                            //  Debug.LogError("OverLap " + gid1.Id + " " + gid.Id + " " + c + " " + d + " old OverLap check " + ildid + " old [" + aa_dy1 + " " + aa_dx1 + "]  [" + a_dy1 + " " + a_dy2 + " " + a_dx1 + " " + a_dx2 + "] v size " + v.rect.size +" " + v.position);
                                                            gid1.uiElement.transform.position = new Vector3(gid1.uiElement.transform.position.x, v.position.y + (gid1.uiElement.rect.rect.height + 5) * gid1.uiElement.rect.lossyScale.y);

                                                            run = true;
                                                        }

                                                        ildid++;
                                                    }
                                                }

                                                oldCheck.Add(gid.uiElement.rect);
                                                numChange++;
                                            }
                                            else
                                            {
                                                oldCheck.Add(gid.uiElement.rect);
                                            }
                                        }
                                        else
                                        {
                                            skip = true;
                                        }

                                    d++;
                                }
                            }
                        }
                        
                        //UI Toolkit
                        Vector3 screenPosition = camera.WorldToViewportPoint(gid1.getPointPosition());
                          if (gid1.uiElementItem != null)
                        {
                            // gid1.uiElementItem.m_Root.transform.position = new Vector3(screenPosition.x, screenPosition.y);
                          var left1 = screenPosition.x * gid1.uiElementItem.m_Root.parent.resolvedStyle.width - gid1.uiElementItem.m_Root.resolvedStyle.width/2;
                            var top1  = gid1.uiElementItem.m_Root.parent.resolvedStyle.height - (screenPosition.y * gid1.uiElementItem.m_Root.parent.resolvedStyle.height + gid1.uiElementItem.m_Root.resolvedStyle.height/2);
                            var width1 = gid1.uiElementItem.m_Root.resolvedStyle.width ;
                            var height1 = gid1.uiElementItem.m_Root.resolvedStyle.height ;
                            //   Debug.LogError(" screenPos " + gid1.Id + " " + c + " " + d +" move  ---> " + gid1.uiElement.rect.position+ " "+ gid1.uiElement.rect.rect.position+" size "+gid1.uiElement.rect.rect.size);
                            
                            
                            
                         

                                float dy1 = 0f;
                                float dy2 = 0f;
                                float dx1 = 0f;
                                float dx2 = 0f;
                                int numChange = 0;
                                List<VisualElement> oldCheck = new List<VisualElement>();
                                foreach (var gid in _itemsSpawned.Values)
                                {

                                    if (!skip)
                                        if (gid1.Id != gid.Id)
                                        {
                                            // if (CheckOverlap(gid1.uiElementItem.m_Root, gid.uiElementItem.m_Root))
                                            
                                             if (CheckOverlap(left1,top1,width1,height1, gid.uiElementItem.m_Root.resolvedStyle.left,gid.uiElementItem.m_Root.resolvedStyle.top,gid.uiElementItem.m_Root.resolvedStyle.width, gid.uiElementItem.m_Root.resolvedStyle.height))
                                            {
                                                //  Debug.LogError("OverLap "+gid1.Id+ " "+gid.Id+" "+c+" "+d);
                                                // float _dy1 = (gid1.uiElement.rect.localPosition.y - gid.uiElement.rect.localPosition.y);
                                                // float _dx1 = (gid1.uiElement.rect.localPosition.x - gid1.uiElement.rect.rect.width / 2 - gid.uiElement.rect.localPosition.x + gid.uiElement.rect.rect.width / 2);
                                              //  gid1.uiElement.transform.position = new Vector3(gid1.uiElement.transform.position.x, gid.uiElement.transform.position.y + (gid1.uiElement.rect.rect.height + 5) * gid1.uiElement.rect.lossyScale.y);
                                                // gid1.uiElementItem.m_Root.transform.position = new Vector3(gid1.uiElementItem.m_Root.transform.position.x, gid.uiElementItem.m_Root.transform.position.y + (gid1.uiElementItem.m_Root.layout.height + 5) * gid1.uiElementItem.m_Root.transform.scale.y);
                                                
                                                
                                                // gid1.uiElementItem.m_Root.style.left = screenPosition.x * gid1.uiElementItem.m_Root.parent.resolvedStyle.width;

                                                top1 = gid.uiElementItem.m_Root.resolvedStyle.top - (gid1.uiElementItem.m_Root.resolvedStyle.height + gid.uiElementItem.m_Root.resolvedStyle.height + 5); 
                                                
                                                
                                                
                                                //    Debug.LogError("Overlaped " + gid1.Id + " " + gid.Id + " " + c + " " + d +" move from" + screenPos + " ---> " + gid1.uiElement.rect.position+ " "+ gid1.uiElement.rect.rect.position+" size "+gid1.uiElement.rect.rect.size);
                                                //    yield return new WaitForEndOfFrame();
                                                int ildid = 0;
                                                bool run = true;

                                                while (run && ildid < 1000)
                                                {
                                                    run = false;
                                                    foreach (var v in oldCheck)
                                                    {
                                                        // if (CheckOverlap(v, gid1.uiElementItem.m_Root))
                                                       if (CheckOverlap(v.resolvedStyle.left,v.resolvedStyle.top,v.resolvedStyle.width, v.resolvedStyle.height, left1, top1, width1, height1))
                                                          {
                                                            // float aa_dy1 = (gid1.uiElement.rect.localPosition.y - gid.uiElement.rect.localPosition.y);
                                                            // float aa_dx1 = (gid1.uiElement.rect.localPosition.x - gid1.uiElement.rect.rect.width / 2 - gid.uiElement.rect.localPosition.x + gid.uiElement.rect.rect.width / 2);
                                                            // float a_dy1 = (gid1.uiElement.rect.localPosition.y + gid1.uiElement.rect.rect.height - v.localPosition.y);
                                                            // float a_dy2 = (v.localPosition.y + v.rect.height - gid1.uiElement.rect.localPosition.y);
                                                            // float a_dx1 = (v.localPosition.x + v.rect.width - gid1.uiElement.rect.localPosition.x);
                                                            // float a_dx2 = (gid1.uiElement.rect.localPosition.x + gid1.uiElement.rect.rect.width - v.localPosition.x);
                                                            //  Debug.LogError("OverLap " + gid1.Id + " " + gid.Id + " " + c + " " + d + " old OverLap check " + ildid + " old [" + aa_dy1 + " " + aa_dx1 + "]  [" + a_dy1 + " " + a_dy2 + " " + a_dx1 + " " + a_dx2 + "] v size " + v.rect.size +" " + v.position);
                                                            
                                                            // gid1.uiElement.transform.position = new Vector3(gid1.uiElement.transform.position.x, v.position.y + (gid1.uiElement.rect.rect.height + 5) * gid1.uiElement.rect.lossyScale.y);
                                                            
                                                            // gid1.uiElementItem.m_Root.transform.position = new Vector3(gid1.uiElementItem.m_Root.transform.position.x, v.transform.position.y + (gid1.uiElementItem.m_Root.layout.height + 5) * gid1.uiElementItem.m_Root.transform.scale.y);
                                                            top1 = v.resolvedStyle.top - (v.resolvedStyle.height + gid1.uiElementItem.m_Root.resolvedStyle.height + 5); 
                                                            
                                                            
                                                            
                                                            run = true;
                                                        }

                                                        ildid++;
                                                    }
                                                }

                                                oldCheck.Add(gid.uiElementItem.m_Root);
                                                numChange++;
                                            }
                                            else
                                            {
                                                oldCheck.Add(gid.uiElementItem.m_Root);
                                            }
                                        }
                                        else
                                        {
                                            skip = true;
                                        }

                                    d++;
                                }
                                
                                
                                gid1.uiElementItem.m_Root.style.left = left1;
                                gid1.uiElementItem.m_Root.style.top = top1;
                                
                        }
                        
                    }

                    c++;
                }
            }
        }

        bool CheckOverlap(RectTransform image1rect, RectTransform image2rect)
        {
            if (image1rect == null || image2rect == null)
                return false;
            if (image1rect.localPosition.x < image2rect.localPosition.x + image2rect.rect.width &&
                image1rect.localPosition.x + image1rect.rect.width > image2rect.localPosition.x &&
                image1rect.localPosition.y < image2rect.localPosition.y + image2rect.rect.height &&
                image1rect.localPosition.y + image1rect.rect.height > image2rect.localPosition.y)
            {
                return true;
            }

            return false;
        }

        bool CheckOverlap(VisualElement image1rect, VisualElement image2rect)
        {
            if (image1rect == null || image2rect == null)
                return false;
            
            // Debug.LogError("CheckOverlap check o1 "
            //                +image1rect.name+ " left="+image1rect.resolvedStyle.left+" top="+image1rect.resolvedStyle.top+" width"+image1rect.resolvedStyle.width+" height="+image1rect.resolvedStyle.height+
            // " | o2 "+image2rect.name+" left="+image2rect.resolvedStyle.left+" top="+image2rect.resolvedStyle.top+" width"+image2rect.resolvedStyle.width+" height="+image2rect.resolvedStyle.height
            // );
            
            return CheckOverlap(image1rect.resolvedStyle.left,image1rect.resolvedStyle.top,image1rect.resolvedStyle.width,image1rect.resolvedStyle.height, image2rect.resolvedStyle.left, image2rect.resolvedStyle.top,image2rect.resolvedStyle.width,image2rect.resolvedStyle.height);
            
         

            return false;
        }
        
        bool CheckOverlap(float left1, float top1, float width1, float height1,float left2, float top2, float width2, float height2)
        {
            
            // Debug.LogError("CheckOverlap check o1 left="+left1+" top="+top1+" width"+width1+" height="+height1+
            //                " | o2  left="+left2+" top="+top2+" width"+width2+" height="+height2
            // );
            
            if (left1 < left2 + width2 &&
                left1 + width1 > left2 &&
                top1 < top2 + height2 &&
                top1 + height1 > top2)
            {
                // Debug.LogError("CheckOverlap  overlap");
                return true;
            }

            return false;
        }
        
        
        

        public static GroundLootManager Instance
        {
            get { return instance; }
        }

    }
}