using DoTs.Resources;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs.Quadrants
{
    
    
    public abstract partial class QuadrantSystem : JobComponentSystem, IResourceProvider
    {
        protected EntityQuery _query;

        private const float CELL_SIZE = 4f;
        private const int Y_MULTIPLIER = 100;
        
        protected abstract EntityQuery CreateQuery();
        
        protected static int GetQuadrantHash(float3 position)
        {
            return Mathf.FloorToInt(position.x / CELL_SIZE) + Y_MULTIPLIER * Mathf.FloorToInt(position.y / CELL_SIZE);
        }

        protected override void OnCreate()
        {
            _query = CreateQuery();
        }
    }

    public abstract class QuadrantSystem<T> : QuadrantSystem where T : struct
    {
        protected NativeMultiHashMap<int, T> _actorsMap;
        
        public QuadrantSystemAccess<T> GetQuadrantAccess()
        {
            return new QuadrantSystemAccess<T>(_actorsMap);
        }

        protected NativeMultiHashMap<int, T>.Concurrent GetMapForJob()
        {
            return _actorsMap.ToConcurrent();
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            _actorsMap.Clear();
            
            var actorsCount = _query.CalculateLength();
            if (_actorsMap.Capacity < actorsCount)
            {
                _actorsMap.Capacity = actorsCount;
            }

            return SetupJobs(inputDeps);
        }

        protected abstract JobHandle SetupJobs(JobHandle inputDeps);

        protected override void OnCreate()
        {
            base.OnCreate();
            _actorsMap = new NativeMultiHashMap<int, T>(0, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _actorsMap.Dispose();
        }
    }
}