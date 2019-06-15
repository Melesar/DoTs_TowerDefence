using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DoTs.Graphics
{
    public struct AnimationSequenceData : IDisposable
    {
        public NativeArray<float4> uvs;

        public void Dispose()
        {
            uvs.Dispose();
        }
    }
}