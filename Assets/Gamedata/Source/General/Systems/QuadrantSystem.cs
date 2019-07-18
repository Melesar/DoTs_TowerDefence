using DoTs.Resources;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    public struct EnemyData
    {
        public Entity entity;
        public float3 position;
    }
    
    public partial class QuadrantSystem : JobComponentSystem, IResourceProvider
    {
        private NativeMultiHashMap<int, EnemyData> _enemyQuadrantsMap;
        
        private EntityQuery _enemiesQuery;

        private const float CELL_SIZE = 4f;
        private const int Y_MULTIPLIER = 100;

        public QuadrantSystemAccess GetQuadrantsAccess()
        {
            return new QuadrantSystemAccess(_enemyQuadrantsMap);
        }
        
        private struct SetEnemyQuadrantsDataJob : IJobForEachWithEntity<Translation, Health>
        {
            public NativeMultiHashMap<int, EnemyData>.Concurrent map;
            
            public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref Health health)
            {
                var hash = GetQuadrantHash(translation.Value);
                var value = new EnemyData {entity = entity, position = translation.Value};

                map.Add(hash, value);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            _enemyQuadrantsMap.Clear();
            
            var enemiesCount = _enemiesQuery.CalculateLength();
            if (_enemyQuadrantsMap.Capacity < enemiesCount)
            {
                _enemyQuadrantsMap.Capacity = enemiesCount;
            }

            var job = new SetEnemyQuadrantsDataJob {map = _enemyQuadrantsMap.ToConcurrent()};
            inputDeps = job.Schedule(_enemiesQuery, inputDeps);
            
            return inputDeps;
        }

        private static int GetQuadrantHash(float3 position)
        {
            return Mathf.FloorToInt(position.x / CELL_SIZE) + Y_MULTIPLIER * Mathf.FloorToInt(position.y / CELL_SIZE);
        }

        protected override void OnCreate()
        {
            ResourceLocator<QuadrantSystem>.SetResourceProvider(this);
            _enemyQuadrantsMap = new NativeMultiHashMap<int, EnemyData>(0, Allocator.Persistent);
            _enemiesQuery = GetEntityQuery(EntityArchetypes.Enemy.GetComponentTypes());
        }

        protected override void OnDestroy()
        {
            ResourceLocator<QuadrantSystem>.SetResourceProvider(null);
            _enemyQuadrantsMap.Dispose();
        }
    }
}