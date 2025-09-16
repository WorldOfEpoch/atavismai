using UnityEngine;

namespace AwesomeTechnologies.Demo
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Demo/FlyCamDemo")]
    public class FlyCamDemo : MonoBehaviour
    {
        public float cameraSensitivity = 2;

        public float moveSpeed = 50;
        public float slowMoveFactor = 0.125f;
        public float fastMoveFactor = 15;

        private float rotationX;
        private float rotationY;
        public float rotationSmoothingFactor = 25;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            rotationX = transform.eulerAngles.y;
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            // rotation
            if (Input.GetMouseButton(1))
            {
                rotationX += Input.GetAxisRaw("Mouse X") * cameraSensitivity;
                rotationY += Input.GetAxisRaw("Mouse Y") * cameraSensitivity;
                rotationY = Mathf.Clamp(rotationY, -90, 90);
            }

            Quaternion targetRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            targetRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothingFactor);

            // movement
            float speedFactor = 1f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                speedFactor = fastMoveFactor;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                speedFactor = slowMoveFactor;

            transform.position += (Input.GetAxisRaw("Vertical") * moveSpeed * speedFactor * Time.deltaTime) * transform.forward;
            transform.position += (Input.GetAxisRaw("Horizontal") * moveSpeed * speedFactor * Time.deltaTime) * transform.right;

            float upAxis = 0;
            if (Input.GetKey(KeyCode.Q)) upAxis = -0.5f;
            if (Input.GetKey(KeyCode.E)) upAxis = 0.5f;
            transform.position += (moveSpeed * speedFactor * Time.deltaTime * upAxis) * transform.up;
        }
    }
}