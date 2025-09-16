using UnityEngine;

namespace AwesomeTechnologies.Demo
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Demo/HarvestingDemo_HarvestableObject")]
    [RequireComponent(typeof(Rigidbody))]
    public class HarvestingDemo_HarvestableObject : MonoBehaviour
    {
        Rigidbody rb;

        int currentHealth;
        int minHealth = 0;
        [SerializeField] int maxHealth = 100;

        void OnEnable()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;  // set kinematic to turn off automated physics

            currentHealth = maxHealth;
        }

        public void DamageObject(int _damage, Vector3 _direction, float _strength)
        {
            currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(_damage), minHealth, maxHealth);  // clamp the result to stay within expected results -- use Abs() to not "heal" the object should a negative value be passed
            if (currentHealth <= minHealth) // when below min health harvest/destroy the object
                HarvestObject(_direction, _strength);
        }

        void HarvestObject(Vector3 _direction, float _strength)
        {
            /// swap the original model for a new one indicating the object has been harvested
            /// => for better gameplay / visuals that don't look weird and buggy
            /// ex: swap the original tree model with two separated trunks without the leaves 
            ///

            rb.isKinematic = false; // re-enable automated physics
            rb.AddForce(_direction * _strength, ForceMode.Impulse); // add an impulse in the desired direction and strength to make the object move/fall correctly
        }
    }
}