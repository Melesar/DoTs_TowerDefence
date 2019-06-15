using Unity.Entities;
using Unity.Mathematics;

namespace DoTs
{
    public struct TargetOwnership : IComponentData
    {
        public float3 targetPosition;
    }
}