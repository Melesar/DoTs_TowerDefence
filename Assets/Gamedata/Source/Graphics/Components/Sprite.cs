using System;
using Unity.Entities;
using Unity.Mathematics;

namespace DoTs.Graphics
{
    public struct Sprite : IComponentData, IComparable<Sprite>
    {
        public float4x4 matrix;
        public float4 uv;

        public SortingLayer sortingLayer;
        public int sortingOrder;

        public int CompareTo(Sprite other)
        {
            var sortingLayerComparison = ((int)sortingLayer).CompareTo((int) other.sortingLayer);
            if (sortingLayerComparison != 0)
            {
                return sortingLayerComparison;
            }

            return sortingOrder.CompareTo(other.sortingOrder);
        }
    }
}