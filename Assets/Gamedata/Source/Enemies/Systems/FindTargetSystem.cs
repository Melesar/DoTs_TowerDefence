using DoTs.Path;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    [UpdateInGroup(typeof(EnemiesSystemGroup))]
    public class FindTargetSystem : JobComponentSystem
    {
        private Entity _pathEntity;
        private UltimateTarget _ultimateTarget;
        
        private EntityCommandBufferSystem _commandBufferSystem;
        private BufferFromEntity<EnemyPathPoint> _lookup;
        
        private struct UltimateTarget
        {
            public Entity entity;
            public float3 position;
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Movement))]
        private struct FollowPathJob : IJobForEach<PathIndex, Translation, TargetOwnership>
        {
            [ReadOnly]
            public DynamicBuffer<EnemyPathPoint> pathPoints;
            public UltimateTarget ultimateTarget;
            
            private const float MAX_DISTANCE_TO_TARGET = 0.1f;
            private const float MAX_DISTANCE_TO_TARGET_SQR = MAX_DISTANCE_TO_TARGET * MAX_DISTANCE_TO_TARGET;

            public void Execute(
                ref PathIndex pathIndex,
                [ReadOnly] ref Translation t,
                ref TargetOwnership target)
            {
                if (pathIndex.value == pathPoints.Length - 1)
                {
                    target.targetEntity = ultimateTarget.entity;
                    target.targetPosition = ultimateTarget.position;
                }
                else if (pathIndex.value < pathPoints.Length - 1)
                {
                    var currentTargetPoint = pathPoints[pathIndex.value].position;
                    if (math.distancesq(currentTargetPoint, t.Value) <= MAX_DISTANCE_TO_TARGET_SQR)
                    {
                        pathIndex.value += 1;
                        target.targetPosition = pathPoints[pathIndex.value].position;
                    }
                }
            }
        }        
        
        [ExcludeComponent(typeof(TargetOwnership))]
        private struct FindClosestPointJob : IJobForEachWithEntity<Translation, PathIndex>
        {
            [ReadOnly]
            public DynamicBuffer<EnemyPathPoint> pathPoints;
            public EntityCommandBuffer.Concurrent commands;
            public UltimateTarget ultimateTarget;
            
            public void Execute(
                Entity e,
                int entityIndex,
                [ReadOnly] ref Translation t,
                ref PathIndex pathIndex)
            {
                var position = pathIndex.value < pathPoints.Length
                    ? pathPoints[pathIndex.value].position
                    : ultimateTarget.position;
                
                commands.AddComponent(entityIndex, e, new TargetOwnership
                {
                    targetPosition = position
                });
            }
        }
        
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            _lookup = GetBufferFromEntity<EnemyPathPoint>(true);
            var buffer = _lookup[_pathEntity];
            
            var followPathJob = new FollowPathJob
            {
                pathPoints = buffer,
                ultimateTarget = _ultimateTarget
            };

            var findClosestPointJob = new FindClosestPointJob
            {
                pathPoints = buffer,
                ultimateTarget = _ultimateTarget,
                commands = _commandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            };

            var closestPointHandle = findClosestPointJob.Schedule(this, inputDeps);
            var followHandle = followPathJob.Schedule(this, closestPointHandle);

            _commandBufferSystem.AddJobHandleForProducer(followHandle);

            return followHandle;
        }

        protected override void OnStartRunning()
        {
            _pathEntity = GetSingletonEntity<Path.Path>();
            if (_pathEntity == Entity.Null)
            {
                Debug.LogError("Path entity was not found");
            }
            

            var targetQuery = GetEntityQuery(typeof(Target), typeof(Translation));
            _ultimateTarget.entity = targetQuery.GetSingletonEntity();
            _ultimateTarget.position = targetQuery.GetSingleton<Translation>().Value;
        }

        protected override void OnCreate()
        {
            _commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
    }
}