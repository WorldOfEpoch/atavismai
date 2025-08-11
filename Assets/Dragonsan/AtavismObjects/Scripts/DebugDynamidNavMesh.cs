using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = System.Random;

namespace Atavism
{
   public class DebugDynamidNavMesh : MonoBehaviour
   {
       public GameObject box;
       public GameObject sphere;
       public GameObject capsule;
       public List<GameObject> objects = new List<GameObject>();
       public GameObject raycastPoints;
       public float raycastPointsDestroyDelay = 25f;
       public float raycastSize = 0.05f;
       public Color raycastHitColor = Color.magenta;
       public Color raycastNoHitColor = Color.green;
        // Start is called before the first frame update
        void Start()
        {
            NetworkAPI.RegisterExtensionMessageHandler("DNM_colliders", HandleNMMessage);
            NetworkAPI.RegisterExtensionMessageHandler("DNM_visibility", HandleNMVisibilityMessage);
            NetworkAPI.RegisterExtensionMessageHandler("ability_targets_aoe", HandleAbilityTargetsAOEMessage);
            NetworkAPI.RegisterExtensionMessageHandler("behavior_target", HandleMobTargetMessage);
        }

        private void OnDestroy()
        {
            NetworkAPI.RemoveExtensionMessageHandler("DNM_colliders", HandleNMMessage);
            NetworkAPI.RemoveExtensionMessageHandler("DNM_visibility", HandleNMVisibilityMessage);
            NetworkAPI.RemoveExtensionMessageHandler("ability_targets_aoe", HandleAbilityTargetsAOEMessage);
            NetworkAPI.RemoveExtensionMessageHandler("behavior_target", HandleMobTargetMessage);
        }
        
   private void HandleMobTargetMessage(Dictionary<string, object> props)
        {
            float locX = (float)props["locX"];
            float locY = (float)props["locY"];
            float locZ = (float)props["locZ"];
            Vector3 sP = new Vector3(locX, locY, locZ);
            
            float locX1 = (float)props["locX1"];
            float locY1 = (float)props["locY1"];
            float locZ1 = (float)props["locZ1"];
            Vector3 tP1 = new Vector3(locX1, locY1, locZ1);
            float locX2 = (float)props["locX2"];
            float locY2 = (float)props["locY2"];
            float locZ2 = (float)props["locZ2"];
            Vector3 tP2 = new Vector3(locX2, locY2, locZ2);

            // Debug.LogWarning("HandleMobTargetMessage sP=" + sP + " tP1=" + tP1 + " tP2=" + tP2 + " Distance=" + Vector3.Distance(sP, tP1));

            if (raycastPoints != null)
            {
                GameObject go = Instantiate(raycastPoints, sP, Quaternion.identity);
                Destroy(go, raycastPointsDestroyDelay);
                // GameObject go2 = Instantiate(raycastPoints, tP1, Quaternion.identity);
                // Destroy(go2, raycastPointsDestroyDelay);
                // GameObject go3 = Instantiate(raycastPoints, tP2, Quaternion.identity);
                // Destroy(go3, raycastPointsDestroyDelay);
             
            }

            DrawLineInGameView(sP + Vector3.up, tP1 + Vector3.up, Color.magenta, raycastPointsDestroyDelay);
            DrawLineInGameView(sP + Vector3.up, tP2 + Vector3.up, Color.green, raycastPointsDestroyDelay);



        }
        private void HandleAbilityTargetsAOEMessage(Dictionary<string, object> props)
        {
            Debug.Log("HandleAbilityTargetsAOEMessage");
            float locX = (float)props["locX"];
            float locY = (float)props["locY"];
            float locZ = (float)props["locZ"];
            Vector3 sP = new Vector3(locX, locY, locZ);
            float orientationx = (float)props["orientationx"];
            float orientationy = (float)props["orientationy"];
            float orientationz = (float)props["orientationz"];
            float orientationw = (float)props["orientationw"];
            string targetType = (string)props["targetType"];
            string targetSubType = (string)props["targetSubType"];
            // Object mR = props["minRange"];
            float minRange = (int)props["minRange"];
            float maxRange = (int)props["maxRange"];
            Quaternion q = new Quaternion(orientationx, orientationy, orientationz, orientationw);
            
            float locX1 = (float)props["locX1"];
            float locY1 = (float)props["locY1"];
            float locZ1 = (float)props["locZ1"];
            Vector3 tP1 = new Vector3(locX1, locY1, locZ1);
            float locX2 = (float)props["locX2"];
            float locY2 = (float)props["locY2"];
            float locZ2 = (float)props["locZ2"];
            Vector3 tP2 = new Vector3(locX2, locY2, locZ2);
            float locX3 = (float)props["locX3"];
            float locY3 = (float)props["locY3"];
            float locZ3 = (float)props["locZ3"];
            Vector3 tP3 = new Vector3(locX3, locY3, locZ3);
            // Debug.LogWarning("HandleAbilityTargetsAOEMessage targetType="+targetType+" targetSubType="+targetSubType+" minRange="+minRange+
            //                " maxRange="+maxRange+" sP=" + sP + " tP1=" + tP1 + " tP2=" + tP2+" tP3="+tP3+
            //                " Distance="+Vector3.Distance(sP,tP1));

            if (raycastPoints != null)
            {
                GameObject go = Instantiate(raycastPoints, sP, Quaternion.identity);
                Destroy(go, raycastPointsDestroyDelay);
                // GameObject go2 = Instantiate(raycastPoints, tP1, Quaternion.identity);
                // Destroy(go2, raycastPointsDestroyDelay);
                // GameObject go3 = Instantiate(raycastPoints, tP2, Quaternion.identity);
                // Destroy(go3, raycastPointsDestroyDelay);
                // GameObject go4 = Instantiate(raycastPoints, tP3, Quaternion.identity);
                // Destroy(go4, raycastPointsDestroyDelay);
            }
                DrawLineInGameView(sP+Vector3.up, tP1+Vector3.up, Color.cyan, raycastPointsDestroyDelay);
                DrawLineInGameView(sP+Vector3.up, tP2+Vector3.up, Color.yellow, raycastPointsDestroyDelay);
                DrawLineInGameView(sP+Vector3.up, tP3+Vector3.up, Color.red, raycastPointsDestroyDelay);

        }
        
        private void HandleNMVisibilityMessage(Dictionary<string, object> props)
        {
            
            float sPx = (float)props["sPx"];
            float sPy = (float)props["sPy"];
            float sPz = (float)props["sPz"];
            Vector3 sP = new Vector3(sPx, sPy, sPz);
            float ePx = (float)props["ePx"];
            float ePy = (float)props["ePy"];
            float ePz = (float)props["ePz"];
            Vector3 eP = new Vector3(ePx, ePy, ePz);
            bool hit = (bool)props["hit"];
           // Debug.LogError("HandleNMVisibilityMessage hit=" + hit + " sP=" + sP + " eP=" + eP);
            Vector3 hitP = eP;
            if (hit)
            {
                float x = (float)props["hitX"];
                float y = (float)props["hitY"];
                float z = (float)props["hitZ"];
                hitP = new Vector3(x, y, z);
              //  Debug.LogError("HandleNMVisibilityMessage hit=" + hit + " point=" + hitP);
            }

            if (raycastPoints != null)
            {
                GameObject go = Instantiate(raycastPoints, sP, Quaternion.identity);
                //  go.hideFlags = HideFlags.HideAndDontSave;
                Destroy(go, raycastPointsDestroyDelay);
                GameObject go2 = Instantiate(raycastPoints, eP, Quaternion.identity);
                //  go2.hideFlags = HideFlags.HideAndDontSave;
                Destroy(go2, raycastPointsDestroyDelay);
            }

            if (hit)
                DrawLineInGameView(sP, hitP, raycastHitColor, raycastPointsDestroyDelay);
            else
                DrawLineInGameView(sP, hitP, raycastNoHitColor, raycastPointsDestroyDelay);


        }

        private void HandleNMMessage(Dictionary<string, object> props)
        {
            bool clear = (bool)props["clear"];
           Debug.LogError("HandleNMMessage clear="+clear);
            if (clear && objects.Count > 0)
            {
                foreach (var obj in objects)
                {
                    Destroy(obj);
                }
                objects.Clear();
            }
            if (props.ContainsKey("num")){
                int num = (int)props["num"];
                for (int i = 0; i < num; i++)
                {
                    int col = (int)props["o" + i + "num"];
                    Vector3 pos = Vector3.zero;
                    Vector3 he1 = Vector3.zero;
                    Vector3 he2 = Vector3.zero;
                    Vector3 he3 = Vector3.zero;

                    for (int j = 0; j < col; j++)
                    {
                        if (props.ContainsKey("t" + i + "_" + j))
                        {
                            string t = (string)props["t" + i + "_" + j];
                            //   Debug.LogError("t=" + t);
                            switch (t)
                            {
                                case "Box":
                                    float x = (float)props["p" + i + "_" + j + "X"];
                                    float y = (float)props["p" + i + "_" + j + "Y"];
                                    float z = (float)props["p" + i + "_" + j + "Z"];
                                    pos = new Vector3(x, y, z);
                                    float hex1 = (float)props["p" + i + "_" + j + "X0"];
                                    float hey1 = (float)props["p" + i + "_" + j + "Y0"];
                                    float hez1 = (float)props["p" + i + "_" + j + "Z0"];
                                    he1 = new Vector3(hex1, hey1, hez1);
                                    float hex2 = (float)props["p" + i + "_" + j + "X1"];
                                    float hey2 = (float)props["p" + i + "_" + j + "Y1"];
                                    float hez2 = (float)props["p" + i + "_" + j + "Z1"];
                                    he2 = new Vector3(hex2, hey2, hez2);
                                    float hex3 = (float)props["p" + i + "_" + j + "X2"];
                                    float hey3 = (float)props["p" + i + "_" + j + "Y2"];
                                    float hez3 = (float)props["p" + i + "_" + j + "Z2"];
                                    he3 = new Vector3(hex3, hey3, hez3);
                                    Quaternion q = Quaternion.Euler(he1);
                                    if (box != null)
                                    {
                                        GameObject go = Instantiate(box, pos, q);
                                        go.transform.localScale = new Vector3(he2.magnitude * 2, he1.magnitude * 2, he3.magnitude * 2);
                                        objects.Add(go);
                                    }

                                    break;
                                case "Sphere":
                                    float sx = (float)props["p" + i + "_" + j + "X"];
                                    float sy = (float)props["p" + i + "_" + j + "Y"];
                                    float sz = (float)props["p" + i + "_" + j + "Z"];
                                    pos = new Vector3(sx, sy, sz);
                                    float sr = (float)props["r" + i + "_" + j];
                                    if (sphere != null)
                                    {
                                        GameObject go = Instantiate(sphere, pos, Quaternion.identity);
                                        go.transform.localScale = new Vector3(sr, sr, sr);
                                        objects.Add(go);
                                    }

                                    break;
                                case "Capsule":
                                    float cx1 = (float)props["p" + i + "_" + j + "X0"];
                                    float cy1 = (float)props["p" + i + "_" + j + "Y0"];
                                    float cz1 = (float)props["p" + i + "_" + j + "Z0"];
                                    he1 = new Vector3(cx1, cy1, cz1);
                                    float cx2 = (float)props["p" + i + "_" + j + "X1"];
                                    float cy2 = (float)props["p" + i + "_" + j + "Y1"];
                                    float cz2 = (float)props["p" + i + "_" + j + "Z1"];
                                    he2 = new Vector3(cx2, cy2, cz2);
                                    Vector3 v = he1 - he2;
                                    float h = v.magnitude;
                                    Vector3 v2 = v / 2;
                                    pos = he1 + v2;
                                    Quaternion cq = Quaternion.Euler(v);

                                    float cr = (float)props["r" + i + "_" + j];
                                    if (capsule != null)
                                    {
                                        GameObject go = Instantiate(capsule, pos, cq);
                                        var c = go.GetComponent<CapsuleCollider>();
                                        if (c != null)
                                        {
                                            c.radius = cr;
                                            c.height = h;
                                        }

                                        //go.transform.localScale = new Vector3(he2.magnitude * 2, he1.magnitude * 2, he3.magnitude * 2);
                                        objects.Add(go);
                                    }

                                    break;
                            }
                        }
                        else
                        {
                            Debug.LogError("missing " + "t" + i + "_" + j);
                        }
                    }
                }
            }
        }

        
        public void DrawLineInGameView(Vector3 start, Vector3 end, Color color, float delay)
        {
            GameObject lineObj = new GameObject("LineObj");
            Destroy(lineObj, delay);
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
            //Particles/Additive
            lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

            //Set color
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            //Set width
            lineRenderer.startWidth = raycastSize;
            lineRenderer.endWidth = raycastSize;

            //Set line count which is 2
            lineRenderer.positionCount = 2;

            //Set the postion of both two lines
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
        
        // Update is called once per frame
        void Update()
        {

        }
    }
}