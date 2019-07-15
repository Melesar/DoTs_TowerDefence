using Unity.Entities;
using Unity.Mathematics;

namespace DoTs.Physics
{
    public struct RaycastResult
    {
        public Entity entity;
        public float distance;
        public float3 position;

        public bool IsHit()
        {
            return distance >= 0f;
        }
    }
}