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
        private EntityQuery _targetsQuery;

        private struct AssignTargetJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion]public NativeArray<Entity> enemies;
            public float3 targetPosition;
            public Entity targetEntity;
            public EntityCommandBuffer.Concurrent commandBuffer;

            public void Execute(int index)
            {
                var enemy = enemies[index];
                commandBuffer.AddComponent(index, enemy, new TargetOwnership
                {
                    targetPosition = targetPosition,
                    targetEntity = targetEntity
                });
            }
        }

        protected override void OnUpdate()
        {
            var target = _targetsQuery.GetSingletonEntity();
            var targetPosition = _targetsQuery.GetSingleton<Translation>().Value;

            var commandBuffer = PostUpdateCommands.ToConcurrent();
            var enemies = Entities
                .WithAll(typeof(Enemy))
                .WithNone(typeof(TargetOwnership))
                .ToEntityQuery()
                .ToEntityArray(Allocator.TempJob);

            var assignJob = new AssignTargetJob
            {
                enemies = enemies,
                targetPosition = targetPosition,
                targetEntity = target,
                commandBuffer = commandBuffer
            };

            assignJob.Schedule(enemies.Length, 32).Complete();
        }

        protected override void OnCreate()
        {
            _targetsQuery = EntityManager.CreateEntityQuery(typeof(Target), typeof(Translation));
        }
    }
}