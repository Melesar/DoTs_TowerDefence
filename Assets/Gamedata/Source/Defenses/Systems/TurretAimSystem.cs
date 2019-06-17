using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    [UpdateBefore(typeof(TurretRotationSystem))]
    public class TurretAimSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref TurretAim aim, ref TurretRotation rotationData, ref Translation translation) =>
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
                
                findTargetJob.Schedule(this).Complete();

                var hasTarget = EntityManager.HasComponent<TargetOwnership>(entity);
                if (hasTarget && hasFoundEnemies[0])
                {
                    PostUpdateCommands.SetComponent(entity, new TargetOwnership {targetPosition = foundEnemies[0]});
                }
                else if (hasTarget)
                {
                    PostUpdateCommands.RemoveComponent<TargetOwnership>(entity);
                }
                else if (hasFoundEnemies[0])
                {
                    PostUpdateCommands.AddComponent(entity, new TargetOwnership {targetPosition = foundEnemies[0]});
                }
                
                foundEnemies.Dispose();
                hasFoundEnemies.Dispose();

                var rotateTurretsJob = new RotateTowardsTargetJob();
                rotateTurretsJob.Schedule(this).Complete();
            });

        }
        
        [RequireComponentTag(typeof(Enemy))]
        [BurstCompile]
        private struct FindEnemiesWithinRangeJob : IJobForEachWithEntity<Translation>
        {
            public float3 turretPosition;
            public float aimRange;
            public NativeArray<float3> foundEnemies;
            public NativeArray<bool> indicator;

            private bool _hasFoundTarget;
            
            public void Execute(Entity entity, int entityIndex, [ReadOnly] ref Translation position)
            {
                if (_hasFoundTarget)
                {
                    return;
                }
                
                var sqrDistance = math.distancesq(position.Value, turretPosition);
                if (sqrDistance <= aimRange * aimRange)
                {
                    foundEnemies[0] = position.Value;
                    indicator[0] = true;
                    _hasFoundTarget = true;
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
    }
}