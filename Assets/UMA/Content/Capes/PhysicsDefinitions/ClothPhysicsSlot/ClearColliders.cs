using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.Dynamics;

public class ClearColliders : MonoBehaviour 
{
    public void Clear(UMAData umaData)
    {
        UMAPhysicsAvatar avatar = umaData.GetComponent<UMAPhysicsAvatar>();
        if (avatar != null)
            Destroy(avatar);
    }
}
