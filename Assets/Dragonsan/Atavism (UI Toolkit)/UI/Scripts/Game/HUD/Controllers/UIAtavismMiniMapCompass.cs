using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
[RequireComponent(typeof(UIDocument))]
    public class UIAtavismMiniMapCompass : MonoBehaviour
    {

        UIDocument uiDocument;
        private Transform Target;
        public VisualElement CompassRoot;

        public VisualElement North;
        public VisualElement South;
        public VisualElement East;
        public VisualElement West;
        [HideInInspector] public int Grade;

        private int Rotation;
        private UIAtavismMiniMap MiniMap;

        private void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            MiniMap = GetComponent<UIAtavismMiniMap>();
            if (Target == null)
            {
                Target = MiniMap.Target;
            }
        }

        private void OnEnable()
        {
            if(uiDocument==null)
                uiDocument = GetComponent<UIDocument>();
           CompassRoot = uiDocument.rootVisualElement.Q<VisualElement>("Compass");
           North = CompassRoot.Q<Label>("N");
           South = CompassRoot.Q<Label>("S");
           East = CompassRoot.Q<Label>("E");
           West = CompassRoot.Q<Label>("W");

        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            if (Target == null)
            {
                Target = MiniMap.Target;
            }
            //return always positive
            if (Target != null)
            {
                Rotation = (int)Mathf.Abs(Target.eulerAngles.y);
            }
            else
            {
                Target = MiniMap.Target;
                Rotation = (int)Mathf.Abs(m_Transform.eulerAngles.y);
            }
            Rotation = Rotation % 360;//return to 0 


            Grade = Rotation;
            //opposite angle
            if (Grade > 180)
            {
                Grade = Grade - 360;
            }
            float cm = CompassRoot.resolvedStyle.width * 0.5f;
            if (MiniMap.useCompassRotation)
            {
                Vector3 north = Vector3.forward * 1000;
                Vector3 tar = Camera.main.transform.forward;
                tar.y = 0;

                float n = angle3602(north, tar, Target.right);
                //TODO
                 // Vector3 rot = CompassRoot.eulerAngles;
                 // rot.z = -n;
                 // CompassRoot.eulerAngles = rot;
            }
            else
            {
                
                North.style.left = (cm - (Grade * 2) - cm);
                South.style.left = (cm - Rotation * 2 + 360) - cm;
                East.style.left = (cm - Grade * 2 + 180) - cm;
                West.style.left = (cm - Rotation * 2 + 540) - cm;
            }

        }

        float angle3602(Vector3 from, Vector3 to, Vector3 right)
        {
            float angle = Vector3.Angle(from, to);
            Vector3 cross = Vector3.Cross(from, to);
            if (cross.y < 0)
            {
                angle = -angle;
            }
            return angle;
        }

        public float Angle360(Vector2 p1, Vector2 p2, Vector2 o = default(Vector2))
        {
            Vector2 v1, v2;
            if (o == default(Vector2))
            {
                v1 = p1.normalized;
                v2 = p2.normalized;
            }
            else
            {
                v1 = (p1 - o).normalized;
                v2 = (p2 - o).normalized;
            }
            float angle = Vector2.Angle(v1, v2);
            return Mathf.Sign(Vector3.Cross(v1, v2).z) < 0 ? (360 - angle) % 360 : angle;
        }

        private Transform t;
        private Transform m_Transform
        {
            get
            {
                if (t == null)
                {
                    t = this.GetComponent<Transform>();
                }
                return t;
            }
        }
    }
}