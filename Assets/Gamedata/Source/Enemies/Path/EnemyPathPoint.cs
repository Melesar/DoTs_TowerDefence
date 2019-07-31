using Unity.Entities;
using Unity.Mathematics;

namespace DoTs.Path
{
    public struct EnemyPathPoint : IBufferElementData
    {
        public float3 position;
    }
}