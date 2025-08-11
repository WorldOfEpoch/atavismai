using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism
{
    public class UTILS
    {
        public static Sprite GetPortrait(AtavismObjectNode objNode)
        {
            if (objNode == null)
                return null;
            if (objNode.GameObject == null)
                return null;

            AtavismNode node = objNode.GameObject.GetComponent<AtavismNode>();

            Sprite portraitSprite = null;
            if (PortraitManager.Instance != null)
                portraitSprite =  PortraitManager.Instance.LoadPortrait(node);
            if (portraitSprite == null)
            {
                if (node != null)
                    portraitSprite = node.PropertyExists("portrait") ? PortraitManager.Instance.LoadPortrait((string)node.GetProperty("portrait")) :
                        node.PropertyExists("custom:portrait") ?
                            PortraitManager.Instance.LoadPortrait((string)node.GetProperty("custom:portrait")) : null;
            }
            
            return portraitSprite;
        }

        public static Color ChangeColorAlpha(Color color, float alpha)
        {
            Color result = new Color(color.r, color.g, color.b, color.a);
            result.a = alpha;
            return result;
        }
    }
}