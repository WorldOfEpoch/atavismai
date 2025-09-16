using UnityEngine;

namespace AwesomeTechnologies.Demo
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Demo/HarvestingDemo_Weapon")]
    public class HarvestingDemo_Weapon : MonoBehaviour
    {
        internal enum EWeaponType
        {
            Fist,
            Axe
        }

        [SerializeField] internal EWeaponType eWeaponType = EWeaponType.Fist;

        [SerializeField] internal Texture2D weaponCursorIcon;

        [SerializeField] internal int damage = 1;
        [SerializeField] internal float forceStrength = 1;  // should be changed to a realistic value
        internal Vector3 forceDirection;    // should be changed to a realistic value later in a script based on weapon ex: the direction a projectile is flying, direction an axe is swung
    }
}