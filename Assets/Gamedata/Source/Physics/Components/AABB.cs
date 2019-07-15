using Unity.Entities;
using Unity.Mathematics;

namespace DoTs.Physics
{
    public struct AABB : IComponentData
    {
        public float3 extents;
    }
}