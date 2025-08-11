using UnityEngine;
using System.Collections;

namespace Atavism
{
    public class GroundItemDisplayChild : MonoBehaviour
    {
        public GroundItemDisplay parent;
        void OnMouseDown()
        {
         
            
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
                parent.Loot();
        }
    }
}