using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    [UpdateInGroup(typeof(TurretsSystemGroup))]
    [UpdateAfter(typeof(TurretShootingSystem))]
    public class ShellsExplosionSystem : JobComponentSystem
    {
        private struct ExplosionData
        {
            public float3 position;
            public float range;
            public float damage;
        }
        
        [BurstCompile]
        private struct ExplosionDataCollectionJob : IJobParallelFor
        {
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<Translation> positions;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<ExplosiveShell> shells;
            
            public NativeArray<ExplosionData> explosions;
            
            public void Execute(int index)
            {
                var explosionData = new ExplosionData
                {
                    position = positions[index].Value,
                    range = shells[index].explosionRadius,
                    damage = shells[index].explosionDamage
                };

                explosions[index] = explosionData;
            }
        }
        
        [BurstCompile]
        [RequireComponentTag(typeof(Enemy))]
        private struct ExplosionJob : IJobForEachWithEntity<Translation, Health>
        {
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<ExplosionData> explosions;
            
            public void Execute(
                Entity entity,
                int index,
                [ReadOnly] ref Translation enemyPosition,
                ref Health enemyHealth)
            {
                for (var i = 0; i < explosions.Length; i++)
                {
                    var explosionData = explosions[i];
                    if (math.distance(enemyPosition.Value, explosionData.position) < explosionData.range)
                    {
                        enemyHealth.value -= explosionData.damage;
                    }
                }
            }
        }

        private struct ShellDisposeJob : IJobForEachWithEntity<ExplosiveShell>
        {
            public EntityCommandBuffer.Concurrent commandBuffer;
            
            public void Execute(Entity entity, int index, ref ExplosiveShell shell)
            {
                commandBuffer.DestroyEntity(index, entity);
            }
        }

        private EntityQuery _explosionQuery;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var queryLength = _explosionQuery.CalculateLength();
            var explosions = new NativeArray<ExplosionData>(queryLength, Allocator.TempJob);

            var collectDataJob = new ExplosionDataCollectionJob
            {
                positions = _explosionQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                shells = _explosionQuery.ToComponentDataArray<ExplosiveShell>(Allocator.TempJob),
                explosions = explosions
            };

            var explosionJob = new ExplosionJob
            {
                explosions = explosions,
            };

            var commandBuffer = _commandBufferSystem.CreateCommandBuffer();
            var shellDisposalJob = new ShellDisposeJob
            {
                commandBuffer = commandBuffer.ToConcurrent()
            };

            inputDeps = collectDataJob.Schedule(queryLength, 10, inputDeps);
            inputDeps = explosionJob.Schedule(this, inputDeps);
            inputDeps = shellDisposalJob.Schedule(this, inputDeps);
            
            _commandBufferSystem.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }

        protected override void OnCreate()
        {
            _explosionQuery = GetEntityQuery(typeof(Translation), typeof(ExplosiveShell));
            _commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
    }    
}
