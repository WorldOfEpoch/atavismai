using UnityEngine;

namespace AwesomeTechnologies.Utility.Extentions
{
    public static class LayerMaskExtension
    {
        public static bool Contains(this LayerMask _mask, int _layer)   // simple check if a layer is in a layermask
        {
            return _mask == (_mask | (1 << _layer));
        }
    }
}

