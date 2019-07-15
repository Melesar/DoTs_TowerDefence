using DoTs.Physics;
using DoTs.Resources;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DoTs
{
    public class EnemyAISystem : ComponentSystem
    {
        private EntityQuery _query;
        private IRaycastProvider _raycastProvider;

        private readonly Movement _enemyMovement = new Movement {speed = 0.3f};
        private readonly EnemyAttack _enemyAttack = new EnemyAttack
        {
            range = 1f,
            cooldown = 1f,
            damage = 3f,
        };
        
        protected override void OnUpdate()
        {
            var count = _query.CalculateLength();
            var positions = _query.ToComponentDataArray<Translation>(Allocator.TempJob);
            var targets = _query.ToComponentDataArray<TargetOwnership>(Allocator.TempJob);
            var entities = _query.ToEntityArray(Allocator.TempJob);
            
            for (int i = 0; i < count; i++)
            {
                var enemyPosition = positions[i].Value;
                var targetPosition = targets[i].targetPosition;
                var direction = targetPosition - enemyPosition;

                var enemyEntity = entities[i];
                var raycastResult = _raycastProvider.Raycast(enemyPosition, direction);
                if (raycastResult.IsHit() && raycastResult.distance <= _enemyAttack.range)
                {
                    RemoveComponent<Movement>(enemyEntity);
                    UpdateComponent(enemyEntity, _enemyAttack);
                }
                else
                {
                    RemoveComponent<EnemyAttack>(enemyEntity);
                    UpdateComponent(enemyEntity, _enemyMovement);
                }
            }
            
            positions.Dispose();
            targets.Dispose();
            entities.Dispose();
        }

        private void UpdateComponent<T>(Entity e, T component) where T : struct, IComponentData
        {
            if (EntityManager.HasComponent<T>(e))
            {
                PostUpdateCommands.SetComponent(e, component);
            }
            else
            {
                PostUpdateCommands.AddComponent(e, component);
            }
        }

        private void RemoveComponent<T>(Entity e) where T : struct, IComponentData
        {
            if (EntityManager.HasComponent<T>(e))
            {
                PostUpdateCommands.RemoveComponent<T>(e);
            }
        }

        protected override void OnCreate()
        {
            _query = Entities.WithAllReadOnly<AIAgent, TargetOwnership, Translation>().ToEntityQuery();
        }

        protected override void OnStartRunning()
        {
            _raycastProvider = ResourceLocator<IRaycastProvider>.GetResourceProvider();
        }
    }
}