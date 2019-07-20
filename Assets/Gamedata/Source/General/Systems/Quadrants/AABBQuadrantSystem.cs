using DoTs.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DoTs.Quadrants
{
    public struct AABBData
    {
        public Entity entity;
        public AABB aabb;
        public float3 position;
        public float scale;
        public LayerMask layerMask;
    }
    
    [UpdateInGroup(typeof(QuadrantSystemGroup))]
    public class AABBQuadrantSystem : QuadrantSystem<AABBData>
    {
        private struct AABBQuadrantJob : IJobForEachWithEntity<AABB, Translation, Scale, LayerMask>
        {
            public NativeMultiHashMap<int, AABBData>.Concurrent map;
            
            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref AABB aabb, 
                [ReadOnly] ref Translation t, 
                [ReadOnly] ref Scale s, 
                [ReadOnly] ref LayerMask layerMask)
            {
                var hash = GetQuadrantHash(t.Value);
                var data = new AABBData
                {
                    aabb = aabb,
                    layerMask = layerMask,
                    position = t.Value,
                    scale = s.Value,
                    entity = e
                };

                map.Add(hash, data);
            }
        }
        
        protected override JobHandle SetupJobs(JobHandle inputDeps)
        {
            var job = new AABBQuadrantJob
            {
                map = GetMapForJob()
            };

            return job.Schedule(_query, inputDeps);
        }

        protected override EntityQuery CreateQuery()
        {
            return GetEntityQuery(
                ComponentType.ReadOnly<AABB>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Scale>(),
                ComponentType.ReadOnly<LayerMask>()
            );
        }
    }
}