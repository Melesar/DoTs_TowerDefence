using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DoTs
{
    public class FindTargetSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var targetsQuery = EntityManager.CreateEntityQuery(typeof(Target), typeof(Translation));
            var target = targetsQuery.GetSingleton<Translation>().Value;

            var commandBuffer = PostUpdateCommands.ToConcurrent();
            var enemies = Entities
                .WithAll(typeof(Enemy))
                .WithNone(typeof(TargetOwnership))
                .ToEntityQuery()
                .ToEntityArray(Allocator.TempJob);

            var assignJob = new AssignTargetJob
            {
                enemies = enemies,
                targetPosition = target,
                commandBuffer = commandBuffer
            };

            assignJob.Schedule(enemies.Length, 32).Complete();
        }

        private struct AssignTargetJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion]public NativeArray<Entity> enemies;
            public float3 targetPosition;
            public EntityCommandBuffer.Concurrent commandBuffer;

            public void Execute(int index)
            {
                var enemy = enemies[index];
                commandBuffer.AddComponent(index, enemy, new TargetOwnership {targetPosition = targetPosition});
            }
        }
    }
}