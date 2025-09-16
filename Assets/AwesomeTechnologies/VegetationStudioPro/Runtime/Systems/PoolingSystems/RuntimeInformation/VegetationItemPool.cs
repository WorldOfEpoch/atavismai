using AwesomeTechnologies.Utility;
using UnityEngine;

public class VegetationItemPool // base class for collider system / run-time prefab system
{
    public virtual GameObject GetObject(ItemSelectorInstanceInfo _info)
    {
        return null;
    }

    public virtual void ReturnObject(GameObject _returnObject)
    {

    }
}