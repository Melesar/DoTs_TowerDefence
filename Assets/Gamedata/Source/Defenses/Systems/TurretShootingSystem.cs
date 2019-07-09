using DoTs.Graphics;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    [UpdateAfter(typeof(TurretRotationSystem))]
    public class TurretShootingSystem : ComponentSystem
    {
        private EntityArchetype _shellArchetype;
        private ShellData _shellTemplate;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        [BurstCompile]
        private struct ReloadJob : IJobForEachWithEntity<TurretShooting, TurretAim, TargetOwnership>
        {
            public NativeHashMap<int, float3>.Concurrent shellPositions;
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
                shellPositions.TryAdd(index, target.targetPosition);

                shooting.currentCooldownTime = shooting.totalCooldownTime;
            }
        }

        private struct ShootJob : IJob
        {
            [ReadOnly]
            public NativeHashMap<int, float3> shellPositions;
            public EntityCommandBuffer commandBuffer;
            public EntityArchetype shellArchetype;
            public ShellData shellTemplate;
            
            public void Execute()
            {
                var positions = shellPositions.GetValueArray(Allocator.Temp);
                foreach (var shellPosition in positions)
                {
                    SpawnExplosion(shellPosition);
                }
            }

            private void SpawnExplosion(float3 position)
            {
                var entity = commandBuffer.CreateEntity(shellArchetype);
                commandBuffer.SetComponent(entity, new Translation{Value = position});
                commandBuffer.SetComponent(entity, new Rotation{Value = quaternion.identity});
                commandBuffer.SetComponent(entity, new Scale {Value = 4f});
                commandBuffer.SetComponent(entity, shellTemplate.shellData);
                commandBuffer.SetComponent(entity, shellTemplate.animationData);
            }
        }

        protected override void OnUpdate()
        {
            var shellsQueue = new NativeHashMap<int, float3>(10, Allocator.TempJob);
            var reloadJob = new ReloadJob
            {
                delta = Time.deltaTime,
                shellPositions = shellsQueue.ToConcurrent()
            };

            var shootJob = new ShootJob
            {
                shellPositions = shellsQueue,
                commandBuffer = _commandBufferSystem.CreateCommandBuffer(),
                shellArchetype = _shellArchetype,
                shellTemplate = _shellTemplate
            };

            var handle = reloadJob.Schedule(this);
            handle = shootJob.Schedule(handle);

            handle.Complete();
            
            shellsQueue.Dispose();
        }
        
        private struct ShellData
        {
            public ExplosiveShell shellData;
            public SpriteAnimationData animationData;
        }

        protected override void OnCreate()
        {
            _commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

            SetShellData();

            _shellArchetype = EntityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(ExplosiveShell),
                typeof(DoTs.Graphics.Sprite),
                typeof(SpriteAnimationData)
            );
        }

        private void SetShellData()
        {
            _shellTemplate.shellData = new ExplosiveShell
            {
                explosionDamage = 50f,
                explosionRadius = 10f,
                lifetime = 1.2f
            };

            _shellTemplate.animationData = new SpriteAnimationData
            {
                currentFrameTime = 0f,
                frameTime = 0.2f,
                currentFrame = 0,
                maxFrame = 6,
                entityType = AnimationEntityType.TurretExplosion
            };
            
            
        }
    }
}