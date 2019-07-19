using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DoTs.Quadrants
{
    public struct EnemyData
    {
        public Entity entity;
        public float3 position;
    }
    
    [UpdateInGroup(typeof(QuadrantSystemGroup))]
    public class EnemiesQuadrantSystem : QuadrantSystem<EnemyData>
    {
        private struct SetEnemyQuadrantsDataJob : IJobForEachWithEntity<Translation>
        {
            public NativeMultiHashMap<int, EnemyData>.Concurrent map;
            
            public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
            {
                var hash = GetQuadrantHash(translation.Value);
                var value = new EnemyData {entity = entity, position = translation.Value};

                map.Add(hash, value);
            }
        }
        
        protected override EntityQuery CreateQuery()
        {
            return GetEntityQuery(EntityArchetypes.Enemy.GetComponentTypes());
        }

        protected override JobHandle SetupJobs(JobHandle inputDeps)
        {
            var job = new SetEnemyQuadrantsDataJob
            {
                map = _actorsMap.ToConcurrent()
            };

            return job.Schedule(_query, inputDeps);
        }
    }
}