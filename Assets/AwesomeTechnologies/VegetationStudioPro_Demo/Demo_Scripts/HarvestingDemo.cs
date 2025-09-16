#if VEGETATION_STUDIO_PRO && VSP_PACKAGES
using System.Collections;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Demo
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Demo/HarvestingDemo")]
    public class HarvestingDemo : MonoBehaviour
    {
        [SerializeField] VegetationType harvestableType = VegetationType.Tree;

        HarvestingDemo_Weapon weapon;
        [SerializeField] GameObject weaponGO;
        [SerializeField] bool changeCursor;

        [SerializeField] GameObject spawnEffect;
        [SerializeField] float effectDestroyDelay = 1.5f;

        [SerializeField] bool replaceWithGameObject;
        [SerializeField] bool interactOnFirstHit;

        void OnEnable()
        {
            weapon = weaponGO.GetComponent<HarvestingDemo_Weapon>();    // store the reference to the weapon used for harvesting

            if (weaponGO == null)
                return; // safety filter

            weapon.forceDirection = Camera.main ? Camera.main.transform.forward : Vector3.zero; // let the harvestable object move/fall in the direction the camera is looking
        }

        void Update()
        {
            if (weaponGO == null)
                return; // safety filter

            SearchForHarvestableObject();   // search/ray cast for valid colliders
        }

        void SearchForHarvestableObject()
        {
            if (!Camera.main) return;   // return when no "MainCamera" is active in the scene

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);    // setup the ray cast from the mouse cursor into the game's world space
            RaycastHit raycastHit;  // stores information about what the ray cast hit

            if (Physics.Raycast(ray, out raycastHit))   // if hit something
            {
                GameObject hitObject = raycastHit.collider.gameObject;  // store the gameObject of the hit collider
                if (hitObject == null) return;

                if ((hitObject.GetComponent<VegetationItemInstanceInfo>()   // Get the "VegetationItemInstanceInfo" directly
                    || hitObject.GetComponentInParent<VegetationItemInstanceInfo>() // or when using the "FromPrefab" collider type get it from the parent
                    || hitObject.GetComponent<HarvestingDemo_HarvestableObject>())  // else search for any harvestable object
                    == false)
                {
                    Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto); // (re)set the cursor
                    return; // return if nothing valid/harvestable got hit
                }

                if (changeCursor)   // when enabled change the cursor
                    Cursor.SetCursor(weapon.weaponCursorIcon, new Vector2(0, 0), CursorMode.Auto);

                if (Input.GetMouseButtonDown(0))    // on left click
                    InteractWithHarvestableObject(hitObject);   // run interaction logic
            }
        }

        void InteractWithHarvestableObject(GameObject _hitObject)
        {
            /// get the correct script reference (again)
            /// then run the desired logic
            /// => if it is a VSP vegetation instance then mask it out and spawn a gameObject version
            /// => if it is a gameObject damage/harvest it
            /// 

            if (_hitObject.TryGetComponent(out VegetationItemInstanceInfo _info))   // for direct VSP colliders
                MaskVSPInstance(_info); // mask out -- replace
            else if (_hitObject.transform.parent && _hitObject.transform.parent.TryGetComponent(out VegetationItemInstanceInfo _infoParent))    // for "FromPrefab" colliders
                MaskVSPInstance(_infoParent);   // mask out -- replace
            else if (_hitObject.TryGetComponent(out HarvestingDemo_HarvestableObject _object))  // for gameObject colliders
                DamageHarvestableObject(_object, weapon.damage);    // damage/harvest
        }

        void MaskVSPInstance(VegetationItemInstanceInfo _vegetationItemInstanceInfo)
        {
            // => Either mask out the vegetation instance directly via its function
            //vegetationItemInstanceInfo.MaskVegetationItem();

            // => Or mask out the vegetation instance manually and define other custom logic
            if (_vegetationItemInstanceInfo.VegetationType != harvestableType) return;  // return and don't mask out the vegetation instance if it is not the desired harvestableType

            GameObject vegetationItemMaskObject = new GameObject { name = "VegetationItemMask - " + _vegetationItemInstanceInfo.gameObject.name };  // create a new gameObject and give it a matching name
            vegetationItemMaskObject.transform.position = _vegetationItemInstanceInfo.Position; // set the position of the gameObject to the same as of the vegetation instance
            vegetationItemMaskObject.AddComponent<VegetationItemMask>().SetVegetationItemInstanceInfo(_vegetationItemInstanceInfo); // add a "VegetationItemMask" to the gameObject and pass the needed information

            SpawnHarvestEffect(_vegetationItemInstanceInfo.Position);   // spawn a particle effect or something else to let the player know they hit the vegetation instance

            if (replaceWithGameObject)  // replace the vegetation instance with a normal gameObject to trigger physics, animations and do other scripting with it
                SpawnCopyGameObject(_vegetationItemInstanceInfo, interactOnFirstHit);
        }

        void SpawnHarvestEffect(Vector3 _position)
        {
            if (!spawnEffect) return;   // return when no spawnEffect got assigned
            GameObject effect = Instantiate(spawnEffect, _position, Quaternion.identity);   // instantiate the effect
            StartCoroutine(DestroyEffect(effect, effectDestroyDelay));  // start a coroutine to destroy the effect after a set time
        }

        IEnumerator DestroyEffect(GameObject _go, float _time)
        {
            yield return new WaitForSeconds(_time); // wait function
            Destroy(_go);   // destroy function
        }

        void SpawnCopyGameObject(VegetationItemInstanceInfo _instanceInfo, bool _interactOnFirstHit = false)
        {
            // get the info of the vegetation item which contains all the rules and prefab
            VegetationItemInfoPro runtimeInfo = _instanceInfo.GetComponent<RuntimeObjectInfo>().VegetationItemInfo;

            // intantiate the copy of the vegetation instance and give it 1:1 the same position, rotation and scale
            GameObject copyObject = Instantiate(runtimeInfo.VegetationPrefab);
            copyObject.transform.position = _instanceInfo.Position;
            copyObject.transform.rotation = _instanceInfo.Rotation;
            copyObject.transform.localScale = _instanceInfo.Scale;

            if (_interactOnFirstHit == false) return;   // should the object immediately also be interacted with or not
            // do other custom code like physics, animations, damage, etc
            //
            HarvestingDemo_HarvestableObject harvestableObject = copyObject.GetComponent<HarvestingDemo_HarvestableObject>();
            DamageHarvestableObject(harvestableObject, weapon.damage);  // damage the object
        }

        void DamageHarvestableObject(HarvestingDemo_HarvestableObject _harvestableObject, int _damage)
        {
            _harvestableObject?.DamageObject(_damage, weapon.forceDirection, weapon.forceStrength);  // damage the object -- let it fall in the desired direction(based on the weapon) -- add strength of the current weapon
        }
    }
}
#endif