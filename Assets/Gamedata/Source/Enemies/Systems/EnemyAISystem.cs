using DoTs.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DoTs
{
    public struct RaycastResultState : ISystemStateComponentData
    {
        public float raycastDistance;
    }
    
    [UpdateInGroup(typeof(EnemiesSystemGroup))]
    public class EnemyAISystem : JobComponentSystem
    {
        private readonly Movement _enemyMovement = new Movement {speed = 3f};
        private readonly EnemyAttack _enemyAttack = new EnemyAttack
        {
            cooldown = 1f,
            damage = 3f,
        };

        private EntityQuery _updateQuery;
        private EntityQuery _updateQueryNoState;
        private EntityQuery _cleanupQuery;
        private EntityCommandBufferSystem _commandBufferSystem;

        private struct UpdateJob : IJobChunk
        {
            public EnemyAttack attackTemplate;
            public Movement movementTemplate;
            
            [ReadOnly] public ArchetypeChunkEntityType entityType;
            [ReadOnly] public ArchetypeChunkComponentType<RaycastResult> raycastResultType;
            [ReadOnly] public ArchetypeChunkComponentType<Movement> movementType;
            [ReadOnly] public ArchetypeChunkComponentType<EnemyAttack> attackType;
            
            public ArchetypeChunkComponentType<TargetOwnership> targetType;
            public ArchetypeChunkComponentType<RaycastResultState> raycastResultStateType;

            public EntityCommandBuffer.Concurrent commands;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                if (chunk.Has(raycastResultStateType))
                {
                    ProcessChunkWithState(chunk, chunkIndex);
                }
                else
                {
                    ProcessChunkWithoutState(chunk, chunkIndex);
                }
            }

            private void ProcessChunkWithState(ArchetypeChunk chunk, int chunkIndex)
            {
                var entities = chunk.GetNativeArray(entityType);
                var raycastResults = chunk.GetNativeArray(raycastResultType);
                var raycastResultStates = chunk.GetNativeArray(raycastResultStateType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity = entities[i];
                    var raycastResult = raycastResults[i];
                    var raycastResultState = raycastResultStates[i];
                    if (!raycastResult.IsHit() && chunk.Has(attackType) && !chunk.Has(movementType))
                    {
                        commands.RemoveComponent<EnemyAttack>(chunkIndex, entity);
                        commands.AddComponent(chunkIndex, entity, movementTemplate);
                        commands.RemoveComponent<RaycastResultState>(chunkIndex, entity);
                        if (chunk.Has(targetType))
                        {
                            commands.RemoveComponent<TargetOwnership>(chunkIndex, entity);
                        }
                    }
                    else if (!CompareDistance(raycastResult, raycastResultState) && chunk.Has(targetType))
                    {
                        commands.SetComponent(chunkIndex, entity, new TargetOwnership
                        {
                            targetEntity = raycastResult.entity,
                            targetPosition = raycastResult.position
                        });
                    }
                }
            }

            private void ProcessChunkWithoutState(ArchetypeChunk chunk, int chunkIndex)
            {
                var entities = chunk.GetNativeArray(entityType);
                var raycastResults = chunk.GetNativeArray(raycastResultType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity = entities[i];
                    var raycastResult = raycastResults[i];
                    if (raycastResult.IsHit() && chunk.Has(movementType) && !chunk.Has(attackType))
                    {
                        commands.RemoveComponent<Movement>(chunkIndex, entity);
                        commands.AddComponent(chunkIndex, entity, attackTemplate);
                        var targetOwnership = new TargetOwnership
                        {
                            targetEntity = raycastResult.entity,
                            targetPosition = raycastResult.position
                        };
                        if (chunk.Has(targetType))
                        {
                            commands.SetComponent(chunkIndex, entity, targetOwnership);
                        }
                        else
                        {
                            commands.AddComponent(chunkIndex, entity, targetOwnership); 
                        }
                        commands.AddComponent(chunkIndex, entity, new RaycastResultState
                        {
                            raycastDistance = raycastResult.distance
                        });
                    }
                    else if (!chunk.Has(movementType))
                    {
                        commands.AddComponent(chunkIndex, entity, movementTemplate);
                    }
                }
            }

            private static bool CompareDistance(RaycastResult raycastResult, RaycastResultState raycastResultState)
            {
                return math.abs(raycastResult.distance - raycastResultState.raycastDistance) < 0.01f;
            }
        }

        private struct CleanupJob : IJobForEachWithEntity<RaycastResultState>
        {
            public EntityCommandBuffer.Concurrent commands;

            public void Execute(Entity entity, int index, [ReadOnly] ref RaycastResultState agent)
            {
                commands.RemoveComponent<RaycastResultState>(index, entity);
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = _commandBufferSystem.CreateCommandBuffer().ToConcurrent();
            var updateJob = new UpdateJob
            {
                attackTemplate = _enemyAttack,
                movementTemplate = _enemyMovement,
                entityType = GetArchetypeChunkEntityType(),
                raycastResultStateType = GetArchetypeChunkComponentType<RaycastResultState>(false),
                raycastResultType = GetArchetypeChunkComponentType<RaycastResult>(true),
                movementType = GetArchetypeChunkComponentType<Movement>(true),
                attackType = GetArchetypeChunkComponentType<EnemyAttack>(true),
                targetType = GetArchetypeChunkComponentType<TargetOwnership>(false),
                commands = commandBuffer
            };

            var cleanupJob = new CleanupJob
            {
                commands = commandBuffer
            };

            inputDeps = updateJob.Schedule(_updateQuery, inputDeps);
            inputDeps = updateJob.Schedule(_updateQueryNoState, inputDeps);
            inputDeps = cleanupJob.Schedule(_cleanupQuery, inputDeps);
            
            _commandBufferSystem.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }

        protected override void OnCreate()
        {
            _updateQuery = GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new[]
                    {
                        ComponentType.ReadOnly<AIAgent>(), ComponentType.ReadOnly<RaycastResult>()
                    },
                    None = new[] {ComponentType.ReadWrite<RaycastResultState>()}
                }
            );
            
            _updateQueryNoState = GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new[]
                    {
                        ComponentType.ReadOnly<AIAgent>(), ComponentType.ReadOnly<RaycastResult>(), ComponentType.ReadWrite<RaycastResultState>(), 
                    },
                }
            );

            _cleanupQuery = GetEntityQuery(ComponentType.ReadOnly<RaycastResultState>(),
                ComponentType.Exclude<AIAgent>());

            _commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
    }
}