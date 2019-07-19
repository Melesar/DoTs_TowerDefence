using Unity.Entities;
using Unity.Mathematics;

namespace DoTs.Physics
{
    public struct RaycastResult : IComponentData
    {
        public Entity entity;
        public float distance;
        public float3 position;
    }

    public static class RaycastResultExtensions
    {
        public static bool IsHit(this RaycastResult result)
        {
            return result.distance >= 0f;
        }
    }
}