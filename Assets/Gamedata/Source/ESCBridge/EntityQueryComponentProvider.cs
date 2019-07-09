using Unity.Collections;
using Unity.Entities;

namespace DoTs.ESCBridge
{
    public class EntityQueryComponentProvider : IEntityQuery
    {
        private readonly EntityQuery _query;

        public NativeArray<T> GetComponentData<T>() where T : struct, IComponentData
        {
            return _query.ToComponentDataArray<T>(Allocator.Temp);
        }

        public EntityQueryComponentProvider(EntityQuery query)
        {
            _query = query;
        }

        public void Dispose()
        {
            _query.Dispose();
        }
    }
}