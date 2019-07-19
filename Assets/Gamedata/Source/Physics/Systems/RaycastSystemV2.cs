using DoTs.Quadrants;
using DoTs.Utilites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace DoTs.Physics
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class RaycastSystemV2 : JobComponentSystem
    {
        private AABBQuadrantSystem _quadrantSystem;
        
        [BurstCompile]
        private struct RaycastJob : IJobForEach<Translation, RaycastAgent, RaycastResult>
        {
            public QuadrantSystem.QuadrantSystemAccess<AABBData> quadrants;
            
            public void Execute(
                [ReadOnly] ref Translation t,
                [ReadOnly] ref RaycastAgent agent,
                [WriteOnly] ref RaycastResult result)
            {
                var nearestAABB = new AABBData();
                var minDistance = float.MaxValue;
                var isFound = false;
                var aabbs = quadrants.GetActorsAlongTheRay(t.Value, agent.direction, agent.maxDistance);
                for (int i = 0; i < aabbs.Length; i++)
                {
                    var aabbData = aabbs[i];
                    var size = aabbData.scale * 2f * aabbData.aabb.extents;
                    var bounds = new Bounds(t.Value, size);
                    var ray = new Ray(t.Value, agent.direction);

                    isFound |= bounds.IntersectRay(ray, out var distance) &&
                               distance <= agent.maxDistance &&
                               aabbData.layerMask.IsMatch(agent.layerMask);
                    
                    if (isFound && distance < minDistance)
                    {
                        minDistance = distance;
                        nearestAABB = aabbData;
                    }
                }

                if (isFound)
                {
                    result.distance = minDistance;
                    result.entity = nearestAABB.entity;
                    result.position = nearestAABB.position;
                }
                else
                {
                    result.distance = float.NegativeInfinity;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new RaycastJob
            {
                quadrants = _quadrantSystem.GetQuadrantAccess()
            };

            return job.Schedule(this, inputDeps);
        }

        protected override void OnStartRunning()
        {
            _quadrantSystem = World.GetExistingSystem<AABBQuadrantSystem>();
        }
    }
}