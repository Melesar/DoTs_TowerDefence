using System;
using Unity.Collections;
using Unity.Entities;

namespace DoTs.ESCBridge
{
    public interface IEntityQuery : IDisposable
    {
        NativeArray<T> GetComponentData<T>() where T : struct, IComponentData;
    }
}