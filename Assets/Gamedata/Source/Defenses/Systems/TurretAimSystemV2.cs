using DoTs.Quadrants;
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
    public class TurretAimSystemV2 : JobComponentSystem
    {
        private EnemiesQuadrantSystem _quadrantsSystem;
        private EndSimulationEntityCommandBufferSystem _commandsSystem;

        [ExcludeComponent(typeof(TargetOwnership))]
        private struct FindTargetJob : IJobForEachWithEntity<Translation, TurretAim>
        {
            public EntityCommandBuffer.Concurrent commands;
            public QuadrantSystem.QuadrantSystemAccess<EnemyData> quadrantsAccess;

            public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref TurretAim aim)
            {
                using (var enemiesInRadius = quadrantsAccess.GetActorsWithinRadius(translation.Value, aim.aimRange, Allocator.Temp))
                {
                    if (enemiesInRadius.Length == 0)
                    {
                        return;
                    }
                
                    QuickSort(translation.Value, enemiesInRadius, 0, enemiesInRadius.Length - 1);

                    var nearestEnemyIndex = 0;
                    while (nearestEnemyIndex < enemiesInRadius.Length && 
                           !CheckDistance(enemiesInRadius[nearestEnemyIndex], translation.Value, aim.aimRange))
                    {
                        nearestEnemyIndex++;
                    }

                    if (nearestEnemyIndex >= enemiesInRadius.Length)
                    {
                        return;
                    }

                    var nearestEnemy = enemiesInRadius[nearestEnemyIndex];
                    commands.AddComponent(index, entity, new TargetOwnership
                    {
                        targetEntity = nearestEnemy.entity,
                        targetPosition = nearestEnemy.position
                    });
                }
            }
            
            private static void QuickSort(float3 position, NativeList<EnemyData> arr, int startIndex, int endIndex) 
            {
                if (startIndex >= endIndex)
                {
                    return;
                }

                var pivot = Partition(position, arr, startIndex, endIndex);
                if (pivot > 1)
                {
                    QuickSort(position, arr, startIndex, pivot - 1);
                }

                if (pivot + 1 < endIndex)
                {
                    QuickSort(position, arr, pivot + 1, endIndex);
                }
            }

            private static int Partition(float3 position, NativeList<EnemyData> arr, int startIndex, int endIndex)
            {
                var pivot = GetDistance(arr[startIndex], position);
                while (true)
                {
                    while (GetDistance(arr[startIndex], position) < pivot)
                    {
                        startIndex++;
                    }

                    while (GetDistance(arr[endIndex], position) > pivot)
                    {
                        endIndex--;
                    }

                    if (startIndex < endIndex)
                    {
                        if (Mathf.Approximately(GetDistance(arr[startIndex], position), GetDistance(arr[endIndex], position)))
                        {
                            return endIndex;
                        }

                        var temp = arr[startIndex];
                        arr[startIndex] = arr[endIndex];
                        arr[endIndex] = temp;
                    }
                    else
                    {
                        return endIndex;
                    }
                }
            }

            private static float GetDistance(EnemyData enemyData, float3 position)
            {
                return math.distancesq(enemyData.position, position);
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
        
        private struct CheckDistanceToTargetJob : IJobForEachWithEntity<TargetOwnership, TurretAim, Translation>
        {
            public EntityCommandBuffer.Concurrent commands;
            
            public void Execute(Entity entity, int index, ref TargetOwnership target, [ReadOnly] ref TurretAim aim, [ReadOnly] ref Translation t)
            {
                if (!CheckDistance(new EnemyData {position = target.targetPosition}, t.Value, aim.aimRange))
                {
                    commands.RemoveComponent<TargetOwnership>(index, entity);
                }
            }
        }
        
        [BurstCompile]
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

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var findTargetJob = new FindTargetJob
            {
                commands = _commandsSystem.CreateCommandBuffer().ToConcurrent(),
                quadrantsAccess = _quadrantsSystem.GetQuadrantAccess()
            };

            var checkDistanceJob = new CheckDistanceToTargetJob {commands = _commandsSystem.CreateCommandBuffer().ToConcurrent()};
            var rotateJob = new RotateTowardsTargetJob();
            var updateAimJob = new UpdateAimStatusJob();
            
            inputDeps = findTargetJob.Schedule(this, inputDeps);
            inputDeps = checkDistanceJob.Schedule(this, inputDeps);
            inputDeps = rotateJob.Schedule(this, inputDeps);
            inputDeps = updateAimJob.Schedule(this, inputDeps);

            _commandsSystem.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }

        protected override void OnStartRunning()
        {
            _quadrantsSystem = World.GetExistingSystem<EnemiesQuadrantSystem>();
            _commandsSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        private static bool CheckDistance(EnemyData enemyData, float3 position, float radius)
        {
            var enemyPosition = enemyData.position;
            var distanceSqr = math.distancesq(enemyPosition, position);
            return distanceSqr <= radius * radius;
        }
    }
}