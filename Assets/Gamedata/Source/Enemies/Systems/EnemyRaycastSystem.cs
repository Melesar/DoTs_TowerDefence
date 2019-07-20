using DoTs.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    public class EnemyRaycastSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct RaycastJob : IJobForEach<RaycastAgent, Translation, TargetOwnership, EnemyAttackRange>
        {
            public void Execute(
                [WriteOnly] ref RaycastAgent agent,
                [ReadOnly] ref Translation t,
                [ReadOnly] ref TargetOwnership target,
                [ReadOnly] ref EnemyAttackRange attackRange)
            {
                agent.direction = math.normalizesafe(target.targetPosition - t.Value);
                agent.layerMask = LayerMask.Create(Layer.Building);
                agent.maxDistance = attackRange.value;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new RaycastJob().Schedule(this, inputDeps);
        }
    }
    
    public class EnemyRaycastDebugSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref Translation t, ref RaycastAgent agent) =>
            {
                var origin = t.Value;
                var endPoint = origin + agent.direction * agent.maxDistance;
                
                Debug.DrawLine(origin, endPoint, Color.blue);
            });
        }
    }
}