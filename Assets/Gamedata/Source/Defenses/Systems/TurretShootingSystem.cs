using DoTs.Graphics;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using SortingLayer = DoTs.Graphics.SortingLayer;
using Sprite = UnityEngine.Sprite;

namespace DoTs
{
    [UpdateInGroup(typeof(TurretsSystemGroup))]
    [UpdateAfter(typeof(TurretRotationSystem))]
    public class TurretShootingSystem : JobComponentSystem
    {
        private ShellData _shellTemplate;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;
        private EntityQuery _query;

        [BurstCompile]
        private struct ReloadJob : IJobForEachWithEntity<TurretShooting, TurretAim, TargetOwnership>
        {
            public NativeArray<float3> shellPositions;
            public float delta;
            
            public void Execute(Entity entity, int index,
                ref TurretShooting shooting,
                [ReadOnly] ref TurretAim aim,
                [ReadOnly] ref TargetOwnership target)
            {
                if (!aim.isAimed)
                {
                    return;
                }

                if (shooting.currentCooldownTime > 0)
                {
                    shooting.currentCooldownTime -= delta;
                    return;
                }
                
                //Shoot
                shellPositions[index] = target.targetPosition;

                shooting.currentCooldownTime = shooting.totalCooldownTime;
            }
        }

        private struct ShootJob : IJob
        {
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<float3> shellPositions;
            public EntityCommandBuffer commandBuffer;
            public ShellData shellTemplate;
            
            public void Execute()
            {
                foreach (var shellPosition in shellPositions)
                {
                    SpawnExplosion(shellPosition);
                }
            }

            private void SpawnExplosion(float3 position)
            {
                var shell = commandBuffer.CreateEntity(EntityArchetypes.Shell);
                commandBuffer.SetComponent(shell, new Translation{Value = position});
                commandBuffer.SetComponent(shell, shellTemplate.shellData);
                

                var explosion = commandBuffer.CreateEntity(EntityArchetypes.ShellExplosion);
                commandBuffer.SetComponent(explosion, new Translation{Value = position});
                commandBuffer.SetComponent(explosion, new Rotation{Value = quaternion.identity});
                commandBuffer.SetComponent(explosion, new Scale {Value = 4f});
                commandBuffer.SetComponent(explosion, new Lifetime {value = 1.2f});
                commandBuffer.SetComponent(explosion, shellTemplate.animationData);
                commandBuffer.SetComponent(explosion, shellTemplate.spriteData);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var count = _query.CalculateLength();
            var shellsQueue = new NativeArray<float3>(count, Allocator.TempJob);
            var reloadJob = new ReloadJob
            {
                delta = Time.deltaTime,
                shellPositions = shellsQueue
            };

            var shootJob = new ShootJob
            {
                shellPositions = shellsQueue,
                commandBuffer = _commandBufferSystem.CreateCommandBuffer(),
                shellTemplate = _shellTemplate
            };

            var handle = reloadJob.Schedule(_query);
            handle = shootJob.Schedule(handle);

            _commandBufferSystem.AddJobHandleForProducer(handle);

            return handle;
        }
        
        private struct ShellData
        {
            public ExplosiveShell shellData;
            public SpriteAnimationData animationData;
            public Graphics.Sprite spriteData;
        }

        protected override void OnCreate()
        {
            _commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            _query = GetEntityQuery(typeof(TurretAim), typeof(TurretShooting), typeof(TargetOwnership));

            SetShellData();
        }

        private void SetShellData()
        {
            _shellTemplate.shellData = new ExplosiveShell
            {
                explosionDamage = 3f,
                explosionRadius = 2.5f,
            };

            _shellTemplate.animationData = new SpriteAnimationData
            {
                currentFrameTime = 0f,
                frameTime = 0.2f,
                currentFrame = 0,
                maxFrame = 6,
                entityType = AnimationEntityType.TurretExplosion
            };

            _shellTemplate.spriteData = new Graphics.Sprite
            {
                sortingLayer = SortingLayer.Units,
                sortingOrder = 10
            };
        }
    }
}