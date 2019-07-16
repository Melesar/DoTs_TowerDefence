using Unity.Entities;

namespace DoTs
{
    public struct LayerMask : IComponentData
    {
        public int value;

        public static LayerMask Create(Layer layer)
        {
            return new LayerMask {value = (int) layer};
        }
    }
}