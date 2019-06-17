using UnityEngine;
using Unity.Mathematics;

namespace DoTs.Sandbox
{
    public class Test : MonoBehaviour
    {
        public float targetAngle;
        public float idleTime;
        public bool isRotating;

        public float turnSpeed = 45f;

        public Transform target;

        private void Update()
        {
            CalculateRotation();
            
            var currentRotation = transform.rotation;
            var targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

            var rotationDelta = turnSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationDelta);
        }

        private void CalculateRotation()
        {
            var to = target.position - transform.position;
            var from = math.up();
            var targetQuaternion = Quaternion.FromToRotation(from, to);

            targetAngle = targetQuaternion.eulerAngles.z;
        }
    }
}