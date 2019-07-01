using Unity.Entities;

namespace DoTs
{
    public struct TurretShooting : IComponentData
    {
        public float totalCooldownTime;
        public float currentCooldownTime;
    }
}