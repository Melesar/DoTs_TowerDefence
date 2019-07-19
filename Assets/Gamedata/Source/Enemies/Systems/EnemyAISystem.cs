using DoTs.Physics;
using DoTs.Resources;
using Unity.Entities;
using Unity.Transforms;

namespace DoTs
{
    public class EnemyAISystem : ComponentSystem
    {
        private IRaycastProvider _raycastProvider;

        private readonly Movement _enemyMovement = new Movement {speed = 0.3f};
        private readonly EnemyAttack _enemyAttack = new EnemyAttack
        {
            range = 0.5f,
            cooldown = 1f,
            damage = 3f,
        };
        
        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<AIAgent, Translation, TargetOwnership>().ForEach(
                (Entity e, ref Translation t, ref TargetOwnership target) =>
                {
                    var enemyPosition = t.Value;
                    var targetPosition = target.targetPosition;
                    var direction = targetPosition - enemyPosition;

//                    var raycastResult =
//                        _raycastProvider.Raycast(enemyPosition, direction, LayerMask.Create(Layer.Building));
                    var raycastResult = new RaycastResult
                    {
                        distance = -100f
                    };
                    if (raycastResult.IsHit() && raycastResult.distance <= _enemyAttack.range)
                    {
                        RemoveComponent<Movement>(e);
                        UpdateComponent(e, new TargetOwnership
                        {
                            targetEntity = raycastResult.entity,
                            targetPosition = raycastResult.position
                        });
                        UpdateComponent(e, _enemyAttack);
                    }
                    else
                    {
                        RemoveComponent<EnemyAttack>(e);
                        UpdateComponent(e, _enemyMovement);
                    }
                });
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

        protected override void OnStartRunning()
        {
            _raycastProvider = ResourceLocator<IRaycastProvider>.GetResourceProvider();
        }
    }
}