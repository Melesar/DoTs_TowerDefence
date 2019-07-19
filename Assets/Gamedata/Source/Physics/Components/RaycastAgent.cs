using Unity.Entities;
using Unity.Mathematics;

namespace DoTs.Physics
{
    public struct RaycastAgent : IComponentData
    {
        public float3 direction;
        public float maxDistance;
        public LayerMask layerMask;
    }
}