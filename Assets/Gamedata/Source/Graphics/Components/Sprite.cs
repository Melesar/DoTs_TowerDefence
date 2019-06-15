using Unity.Entities;
using Unity.Mathematics;

namespace DoTs.Graphics
{
    public struct Sprite : IComponentData
    {
        public float4x4 matrix;
        public float4 uv;
    }
}