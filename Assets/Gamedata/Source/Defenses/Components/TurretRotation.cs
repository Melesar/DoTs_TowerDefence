using Unity.Entities;

namespace DoTs
{
    public struct TurretRotation : IComponentData
    {
        public float targetAngle;
        public bool isTurning;
        public float turnSpeed;

        public float idleTime;
        public float currentIdleTime;
        public float maxPossibleIdleTime;
    }
}