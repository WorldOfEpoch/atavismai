using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public static class UIAtavismMiniMapUtils
    {


        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewPoint"></param>
        /// <param name="maxAnchor"></param>
        /// <returns></returns>
      
        
        public static Vector3 CalculateMiniMapPosition(Vector3 viewPoint, VisualElement maxAnchor)
        {
            viewPoint = new Vector2((viewPoint.x * maxAnchor.resolvedStyle.width) - (maxAnchor.resolvedStyle.width * 0.5f),
                (viewPoint.y * maxAnchor.resolvedStyle.height) - (maxAnchor.resolvedStyle.height * 0.5f));
            viewPoint += new Vector3(maxAnchor.resolvedStyle.width * 0.5f,maxAnchor.resolvedStyle.height * 0.5f);
            return viewPoint;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // public static bl_MiniMap GetMiniMap(int id = 0)
        // {
        //     if (AtavismSettings.Instance.MiniMap != null)
        //         return AtavismSettings.Instance.MiniMap;
        //     bl_MiniMap[] allmm = GameObject.FindObjectsOfType<bl_MiniMap>();
        //     if (allmm.Length > id)
        //         return allmm[id];
        //     else
        //         return null;
        // }
        public static UIAtavismMiniMap GetMiniMap(int id = 0)
        {
            if (AtavismSettings.Instance.UiMiniMap != null)
                return AtavismSettings.Instance.UiMiniMap;
            UIAtavismMiniMap[] allmm = GameObject.FindObjectsOfType<UIAtavismMiniMap>();
            if (allmm.Length > id)
                return allmm[id];
            else
                return null;
        }

        public static bool IsInLayerMask(int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }
    }
}