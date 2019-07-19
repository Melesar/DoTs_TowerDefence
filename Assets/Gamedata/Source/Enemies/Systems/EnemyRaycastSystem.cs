using DoTs.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DoTs
{
    public class EnemyRaycastSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct RaycastJob : IJobForEach<RaycastAgent, Translation, TargetOwnership, EnemyAttack>
        {
            public void Execute(
                [WriteOnly] ref RaycastAgent agent,
                [ReadOnly] ref Translation t,
                [ReadOnly] ref TargetOwnership target,
                [ReadOnly] ref EnemyAttack attack)
            {
                agent.direction = math.normalizesafe(target.targetPosition - t.Value);
                agent.layerMask = LayerMask.Create(Layer.Building);
                agent.maxDistance = attack.range;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new RaycastJob().Schedule(this, inputDeps);
        }
    }
}