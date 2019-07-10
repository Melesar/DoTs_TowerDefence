using Unity.Entities;

namespace DoTs
{
    public struct Health : IComponentData
    {
        public float value;
        public float maxValue;
    }
}