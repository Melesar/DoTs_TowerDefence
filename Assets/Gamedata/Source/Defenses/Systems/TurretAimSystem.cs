using System.Collections.Generic;
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
    [UpdateBefore(typeof(TurretRotationSystem))]
    public class TurretAimSystem : ComponentSystem
    {
        private NativeList<JobHandle> _handles;
        
        protected override void OnUpdate()
        {
            _handles.Clear();
            var index = 0;
            Entities.WithAllReadOnly<TurretAim, Translation>()
                .ForEach((Entity entity, ref TurretAim aim, ref Translation translation) =>
            {
                var foundEnemies = new NativeArray<float3>(1, Allocator.TempJob);
                var hasFoundEnemies = new NativeArray<bool>(1, Allocator.TempJob);
                var findTargetJob = new FindEnemiesWithinRangeJob
                {
                    turretPosition = translation.Value,
                    aimRange = aim.aimRange,
                    foundEnemies = foundEnemies,
                    indicator = hasFoundEnemies
                };

                var hasTarget = EntityManager.HasComponent<TargetOwnership>(entity);
                var updateTargetJob = new UpdateTargetJob
                {
                    index = index,
                    entity = entity,
                    hasTarget = hasTarget,
                    commandBuffer = PostUpdateCommands.ToConcurrent(),
                    enemyPositions = foundEnemies,
                    hasFoundEnemies = hasFoundEnemies,
                };

                var handle = findTargetJob.Schedule(this);
                handle = updateTargetJob.Schedule(handle);

                _handles.Add(handle);
            });
            
            JobHandle.CompleteAll(_handles.AsArray());
            
            var rotateTurretsJob = new RotateTowardsTargetJob();
            var outerHandle = rotateTurretsJob.Schedule(this);
            outerHandle = new UpdateAimStatusJob().Schedule(this, outerHandle);
            
            outerHandle.Complete();
        }

        [RequireComponentTag(typeof(Enemy))]
        [BurstCompile]
        private struct FindEnemiesWithinRangeJob : IJobForEachWithEntity<Translation>
        {
            public float3 turretPosition;
            public float aimRange;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> foundEnemies;
            [NativeDisableParallelForRestriction]
            public NativeArray<bool> indicator;
            
            public void Execute(Entity entity, int entityIndex, [ReadOnly] ref Translation position)
            {
                if (indicator[0])
                {
                    return;
                }
                
                var sqrDistance = math.distancesq(position.Value, turretPosition);
                if (sqrDistance <= aimRange * aimRange)
                {
                    foundEnemies[0] = position.Value;
                    indicator[0] = true;
                }
            }
        }

        private struct UpdateTargetJob : IJob
        {
            public int index;
            public Entity entity;
            public bool hasTarget;
            public EntityCommandBuffer.Concurrent commandBuffer;
            
            [ReadOnly, DeallocateOnJobCompletion] 
            public NativeArray<float3> enemyPositions;
            [ReadOnly, DeallocateOnJobCompletion] 
            public NativeArray<bool> hasFoundEnemies;


            public void Execute()
            {
                if (hasTarget && hasFoundEnemies[0])
                {
                    commandBuffer.SetComponent(index, entity, new TargetOwnership {targetPosition = enemyPositions[0]});
                }
                else if (hasTarget)
                {
                    commandBuffer.RemoveComponent<TargetOwnership>(index, entity);
                }
                else if (hasFoundEnemies[0])
                {
                    commandBuffer.AddComponent(index, entity, new TargetOwnership {targetPosition = enemyPositions[0]});
                }
            }
        }

        [BurstCompile]
        private struct RotateTowardsTargetJob : IJobForEach<TargetOwnership, TurretRotation, Rotation, Translation>
        {
            public void Execute([ReadOnly] ref TargetOwnership target,
                [WriteOnly] ref TurretRotation rotationData,
                [ReadOnly] ref Rotation rotation,
                [ReadOnly] ref Translation translation)
            {
                var to = target.targetPosition - translation.Value;
                var from = math.up();
                var targetQuaternion = Quaternion.FromToRotation(from, to);

                rotationData.isTurning = true;
                rotationData.targetAngle = targetQuaternion.eulerAngles.z;
            }
        }
        
        
        [RequireComponentTag(typeof(TargetOwnership))]
        private struct UpdateAimStatusJob : IJobForEachWithEntity<TurretRotation, TurretAim, Rotation>
        {
            public void Execute(Entity entity, int index,
                [ReadOnly] ref TurretRotation turretRotation,
                [WriteOnly] ref TurretAim aim,
                [ReadOnly] ref Rotation rotation)
            {
                var targetRotation = Quaternion.Euler(0f, 0f, turretRotation.targetAngle);
                var angle = Quaternion.Angle(targetRotation, rotation.Value);

                var isAimed = angle <= 5f;
                aim.isAimed = isAimed;
            }
        }

        protected override void OnCreate()
        {
            _handles = new NativeList<JobHandle>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _handles.Dispose();
        }
    }
}