using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class ColliderPool : VegetationItemPool
{
    private int colliderCounter;
    private readonly List<GameObject> colliderPoolList = new();
    private readonly VegetationSystemPro vegetationSystemPro;
    private readonly VegetationItemInfoPro vegetationItemInfoPro;
    private readonly VegetationItemModelInfo vegetationItemModelInfo;

    private readonly GameObject sourceColliderObject;
    private readonly Transform colliderParent;
    private readonly string colliderTag;
    private LayerMask colliderLayer;
    private bool showColliders;

    public ColliderPool(VegetationItemInfoPro _vegetationItemInfoPro, VegetationItemModelInfo _vegetationItemModelInfo, VegetationSystemPro _vegetationSystemPro, Transform _colliderParent, bool _showColliders)
    {
        vegetationSystemPro = _vegetationSystemPro;
        vegetationItemInfoPro = _vegetationItemInfoPro;
        vegetationItemModelInfo = _vegetationItemModelInfo;
        colliderParent = _colliderParent;
        colliderTag = _vegetationItemInfoPro.ColliderTag == "" ? "Untagged" : _vegetationItemInfoPro.ColliderTag;
        colliderLayer = _vegetationSystemPro.vegetationRenderSettings.GetLayer(_vegetationItemInfoPro.VegetationType);
        showColliders = _showColliders;

        if (vegetationItemInfoPro.ColliderType == ColliderType.FromPrefab && _vegetationItemInfoPro.VegetationPrefab)
        {
            GameObject tmpColliderObject = Object.Instantiate(_vegetationItemInfoPro.VegetationPrefab);
            tmpColliderObject.hideFlags = HideFlags.DontSave;
            tmpColliderObject.name = "Source_" + vegetationItemInfoPro.Name;
            tmpColliderObject.transform.SetParent(colliderParent);
            sourceColliderObject = CreateSourceCollider(tmpColliderObject);
            DestroyObject(tmpColliderObject);
        }
    }

    private void AddVegetationItemInstanceInfo(GameObject _colliderObject)
    {
        VegetationItemInstanceInfo vegetationItemInstanceInfo = _colliderObject.AddComponent<VegetationItemInstanceInfo>();
        vegetationItemInstanceInfo.VegetationType = vegetationItemInfoPro.VegetationType;
        vegetationItemInstanceInfo.VegetationItemID = vegetationItemInfoPro.VegetationItemID;

        RuntimeObjectInfo runtimeObjectInfo = _colliderObject.AddComponent<RuntimeObjectInfo>();
        runtimeObjectInfo.VegetationItemInfo = vegetationItemInfoPro;
    }

    private void UpdateVegetationItemInstanceInfo(GameObject _colliderObject, ItemSelectorInstanceInfo _info)
    {
        VegetationItemInstanceInfo vegetationItemInstanceInfo = _colliderObject.GetComponent<VegetationItemInstanceInfo>();

        if (vegetationItemInstanceInfo)
        {
            vegetationItemInstanceInfo.Position = _info.Position;
            vegetationItemInstanceInfo.VegetationItemInstanceID = ((int)math.round(vegetationItemInstanceInfo.Position.x * 100f)).ToString() + "_" +
                                                                  ((int)math.round(vegetationItemInstanceInfo.Position.y * 100f)).ToString() + "_" +
                                                                  ((int)math.round(vegetationItemInstanceInfo.Position.z * 100f)).ToString();
            vegetationItemInstanceInfo.Rotation = _info.Rotation;
            vegetationItemInstanceInfo.Scale = _info.Scale;
        }
    }


    private void AddNavMeshObstacle(GameObject _go)
    {
        NavMeshObstacle navMeshObstacle;
        switch (vegetationItemInfoPro.NavMeshObstacleType)
        {
            case NavMeshObstacleType.Box:
                navMeshObstacle = _go.AddComponent<NavMeshObstacle>();
                navMeshObstacle.shape = NavMeshObstacleShape.Box;
                navMeshObstacle.center = vegetationItemInfoPro.NavMeshObstacleCenter;
                navMeshObstacle.size = vegetationItemInfoPro.NavMeshObstacleSize;
                navMeshObstacle.carving = vegetationItemInfoPro.NavMeshObstacleCarve;
                break;
            case NavMeshObstacleType.Capsule:
                navMeshObstacle = _go.AddComponent<NavMeshObstacle>();
                navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
                navMeshObstacle.center = vegetationItemInfoPro.NavMeshObstacleCenter;
                navMeshObstacle.radius = vegetationItemInfoPro.NavMeshObstacleRadius;
                navMeshObstacle.height = vegetationItemInfoPro.NavMeshObstacleHeight;
                navMeshObstacle.carving = vegetationItemInfoPro.NavMeshObstacleCarve;
                break;
        }
    }

    public void SetColliderVisibility(bool _value)
    {
        showColliders = _value;
        for (int i = 0; i < colliderPoolList.Count; i++)
            if (_value)
                colliderPoolList[i].hideFlags = HideFlags.DontSave;
            else
                colliderPoolList[i].hideFlags = HideFlags.HideAndDontSave;
    }

    private HideFlags GetVisibilityHideFlags()
    {
        return showColliders ? HideFlags.DontSave : HideFlags.HideAndDontSave;
    }

    public override GameObject GetObject(ItemSelectorInstanceInfo _info)
    {
        if (colliderPoolList.Count <= 0)
            return CreateColliderObject(_info); // create a collider if none are in the pool => set pos, rot, scale, etc

        GameObject colliderObject = colliderPoolList[colliderPoolList.Count - 1]; // else get one from the pool => do usual stuff
        colliderPoolList.RemoveAtSwapBack(colliderPoolList.Count - 1);
        PositionColliderObject(colliderObject, _info);  // set pos, rot, scale, etc
        colliderObject.SetActive(true);
        return colliderObject;
    }

    public override void ReturnObject(GameObject _colliderObject)
    {
        if (_colliderObject)
        {
            _colliderObject.SetActive(false);
            colliderPoolList.Add(_colliderObject);
        }
    }

    private void PositionColliderObject(GameObject _colliderObject, ItemSelectorInstanceInfo _info)
    {
        _colliderObject.transform.position = _info.Position + vegetationSystemPro.floatingOriginOffset;
        _colliderObject.transform.localScale = _info.Scale;
        _colliderObject.transform.rotation = _info.Rotation;

        UpdateVegetationItemInstanceInfo(_colliderObject, _info);
    }

    public GameObject CreateColliderObject(ItemSelectorInstanceInfo _info)
    {
        GameObject newColliderGO;
        if (vegetationItemInfoPro.ColliderType == ColliderType.FromPrefab)
        {
            newColliderGO = Object.Instantiate(sourceColliderObject);
            newColliderGO.name = "CopyCollider_" + colliderCounter.ToString();
            newColliderGO.transform.SetParent(colliderParent);
            newColliderGO.SetActive(true);  // re-enable "sourceCollider" copies
        }
        else
            newColliderGO = CreatePrimitiveCollider();

        AddVegetationItemInstanceInfo(newColliderGO);
        AddNavMeshObstacle(newColliderGO);
        PositionColliderObject(newColliderGO, _info);   // last

        colliderCounter++;
        return newColliderGO;
    }

    private GameObject CreatePrimitiveCollider()
    {
        switch (vegetationItemInfoPro.ColliderType)
        {
            case ColliderType.Capsule:
                GameObject newCapsuleColliderObject = new("CapsuleCollider_" + colliderCounter);
                newCapsuleColliderObject.tag = colliderTag;
                newCapsuleColliderObject.layer = colliderLayer;
                newCapsuleColliderObject.transform.SetParent(colliderParent);
                newCapsuleColliderObject.hideFlags = GetVisibilityHideFlags();

                CapsuleCollider capsuleCollider = newCapsuleColliderObject.AddComponent<CapsuleCollider>();
                capsuleCollider.height = vegetationItemInfoPro.ColliderHeight;
                capsuleCollider.radius = vegetationItemInfoPro.ColliderRadius;
                capsuleCollider.isTrigger = vegetationItemInfoPro.ColliderTrigger;
                capsuleCollider.center = capsuleCollider.transform.InverseTransformPoint(vegetationItemInfoPro.ColliderOffset);
                return newCapsuleColliderObject;

            case ColliderType.Sphere:
                GameObject newSphereColliderObject = new("SphereCollider_" + colliderCounter);
                newSphereColliderObject.tag = colliderTag;
                newSphereColliderObject.layer = colliderLayer;
                newSphereColliderObject.transform.SetParent(colliderParent);
                newSphereColliderObject.hideFlags = GetVisibilityHideFlags();
                SphereCollider sphereCollider = newSphereColliderObject.AddComponent<SphereCollider>();
                sphereCollider.radius = vegetationItemInfoPro.ColliderRadius;
                sphereCollider.isTrigger = vegetationItemInfoPro.ColliderTrigger;
                sphereCollider.center = sphereCollider.transform.InverseTransformPoint(vegetationItemInfoPro.ColliderOffset);
                return newSphereColliderObject;

            case ColliderType.Box:
                GameObject newColliderObject = new("BoxCollider_" + colliderCounter);
                newColliderObject.tag = colliderTag;
                newColliderObject.layer = colliderLayer;
                newColliderObject.transform.SetParent(colliderParent);
                newColliderObject.hideFlags = GetVisibilityHideFlags();
                BoxCollider boxCollider = newColliderObject.AddComponent<BoxCollider>();
                boxCollider.size = new float3(vegetationItemInfoPro.ColliderSize.x, vegetationItemInfoPro.ColliderSize.y, vegetationItemInfoPro.ColliderSize.z);
                boxCollider.isTrigger = vegetationItemInfoPro.ColliderTrigger;
                boxCollider.center = boxCollider.transform.InverseTransformPoint(vegetationItemInfoPro.ColliderOffset);
                return newColliderObject;

            case ColliderType.CustomMesh:
                if (vegetationItemInfoPro.ColliderMesh.isReadable == false)
                    Debug.LogError("VSP internal error log: Colliders: Mesh is not Read/Write enabled " + vegetationItemInfoPro.ColliderMesh.name);
                GameObject newCustomMeshColliderObject = new("MeshCollider_" + colliderCounter);
                newCustomMeshColliderObject.tag = colliderTag;
                newCustomMeshColliderObject.layer = colliderLayer;
                newCustomMeshColliderObject.transform.SetParent(colliderParent);
                newCustomMeshColliderObject.hideFlags = GetVisibilityHideFlags();
                MeshCollider customMeshCollider = newCustomMeshColliderObject.AddComponent<MeshCollider>();
                customMeshCollider.sharedMesh = vegetationItemInfoPro.ColliderMesh;
                customMeshCollider.convex = vegetationItemInfoPro.ColliderConvex;
                if (customMeshCollider.convex) customMeshCollider.isTrigger = vegetationItemInfoPro.ColliderTrigger;
                return newCustomMeshColliderObject;

            case ColliderType.Mesh:
                if (vegetationItemModelInfo.vegetationMeshLod0.isReadable == false)
                    Debug.LogError("VSP internal error log: Colliders: Mesh is not Read/Write enabled " + vegetationItemModelInfo.vegetationMeshLod0.name);
                GameObject newMeshColliderObject = new("MeshCollider_" + colliderCounter);
                newMeshColliderObject.tag = colliderTag;
                newMeshColliderObject.layer = colliderLayer;
                newMeshColliderObject.transform.SetParent(colliderParent);
                newMeshColliderObject.hideFlags = GetVisibilityHideFlags();
                MeshCollider meshCollider = newMeshColliderObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = vegetationItemModelInfo.vegetationMeshLod0;
                meshCollider.convex = vegetationItemInfoPro.ColliderConvex;
                if (meshCollider.convex) meshCollider.isTrigger = vegetationItemInfoPro.ColliderTrigger;
                return newMeshColliderObject;
        }

        return new GameObject("Empty collider object");
    }

    private GameObject CreateSourceCollider(GameObject _sourceObject)   // used with "FromPrefab"
    {
        _sourceObject.transform.SetPositionAndRotation(float3.zero, quaternion.identity);
        _sourceObject.transform.localScale = Vector3.one;

        GameObject colliderObject = new(_sourceObject.name) { hideFlags = HideFlags.DontSave };
        colliderObject.transform.SetParent(colliderParent);
        colliderObject.transform.SetPositionAndRotation(float3.zero, quaternion.identity);
        colliderObject.transform.localScale = Vector3.one;
        colliderObject.tag = colliderTag;
        colliderObject.layer = colliderLayer;
        colliderObject.SetActive(false);

        MeshCollider[] meshColliders = _sourceObject.GetComponentsInChildren<MeshCollider>();
        SphereCollider[] sphereColliders = _sourceObject.GetComponentsInChildren<SphereCollider>();
        BoxCollider[] boxColliders = _sourceObject.GetComponentsInChildren<BoxCollider>();
        CapsuleCollider[] capsuleColliders = _sourceObject.GetComponentsInChildren<CapsuleCollider>();

        for (int i = 0; i < capsuleColliders.Length; i++)
        {
            GameObject capsuleObject = new("CapsuleCollider") { hideFlags = HideFlags.DontSave };
            capsuleObject.transform.SetParent(colliderObject.transform);
            capsuleObject.transform.SetPositionAndRotation(capsuleColliders[i].transform.position, capsuleColliders[i].transform.rotation);
            capsuleObject.transform.localScale = capsuleColliders[i].transform.localScale;
            capsuleObject.tag = _sourceObject.tag;
            capsuleObject.layer = _sourceObject.layer;

            CapsuleCollider newCollider = capsuleObject.AddComponent<CapsuleCollider>();
            newCollider.radius = capsuleColliders[i].radius;
            newCollider.height = capsuleColliders[i].height;
            newCollider.center = capsuleColliders[i].center;
            newCollider.direction = capsuleColliders[i].direction;
            newCollider.sharedMaterial = capsuleColliders[i].sharedMaterial;
            newCollider.isTrigger = capsuleColliders[i].isTrigger;
        }

        for (int i = 0; i < boxColliders.Length; i++)
        {
            GameObject boxobject = new("BoxCollider") { hideFlags = HideFlags.DontSave };
            boxobject.transform.SetParent(colliderObject.transform);
            boxobject.transform.SetPositionAndRotation(boxColliders[i].transform.position, boxColliders[i].transform.rotation);
            boxobject.transform.localScale = boxColliders[i].transform.localScale;
            boxobject.tag = _sourceObject.tag;
            boxobject.layer = _sourceObject.layer;

            BoxCollider newCollider = boxobject.AddComponent<BoxCollider>();
            newCollider.center = boxColliders[i].center;
            newCollider.size = boxColliders[i].size;
            newCollider.sharedMaterial = boxColliders[i].sharedMaterial;
            newCollider.isTrigger = boxColliders[i].isTrigger;
        }

        for (int i = 0; i < sphereColliders.Length; i++)
        {
            GameObject sphereObject = new("SphereCollider") { hideFlags = HideFlags.DontSave };
            sphereObject.transform.SetParent(colliderObject.transform);
            sphereObject.transform.SetPositionAndRotation(sphereColliders[i].transform.position, sphereColliders[i].transform.rotation);
            sphereObject.transform.localScale = sphereColliders[i].transform.localScale;
            sphereObject.tag = _sourceObject.tag;
            sphereObject.layer = _sourceObject.layer;

            SphereCollider newCollider = sphereObject.AddComponent<SphereCollider>();
            newCollider.center = sphereColliders[i].center;
            newCollider.radius = sphereColliders[i].radius;
            newCollider.sharedMaterial = sphereColliders[i].sharedMaterial;
            newCollider.isTrigger = sphereColliders[i].isTrigger;
        }

        for (int i = 0; i < meshColliders.Length; i++)
        {
            GameObject meshObject = new("MeshCollider") { hideFlags = HideFlags.DontSave };
            meshObject.transform.SetParent(colliderObject.transform);
            meshObject.transform.SetPositionAndRotation(meshColliders[i].transform.position, meshColliders[i].transform.rotation);
            meshObject.transform.localScale = meshColliders[i].transform.localScale;
            meshObject.tag = _sourceObject.tag;
            meshObject.layer = _sourceObject.layer;

            MeshCollider newCollider = meshObject.AddComponent<MeshCollider>();
            newCollider.cookingOptions = meshColliders[i].cookingOptions;
            newCollider.convex = meshColliders[i].convex;
            newCollider.sharedMesh = meshColliders[i].sharedMesh;
            newCollider.sharedMaterial = meshColliders[i].sharedMaterial;
            newCollider.isTrigger = meshColliders[i].isTrigger;
        }

        return colliderObject;
    }

    private static void DestroyObject(GameObject _go)
    {
        if (Application.isPlaying)
            Object.Destroy(_go);
        else
            Object.DestroyImmediate(_go);
    }

    public void Dispose()
    {
        for (int i = 0; i < colliderPoolList.Count; i++)
            DestroyObject(colliderPoolList[i]);
        colliderPoolList.Clear();

        if (vegetationItemInfoPro.ColliderType == ColliderType.FromPrefab)
            DestroyObject(sourceColliderObject);
    }
}