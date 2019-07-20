using DoTs.Physics;
using DoTs.Resources;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    public class EnemyAISystem : ComponentSystem
    {
        private readonly Movement _enemyMovement = new Movement {speed = 1f};
        private readonly EnemyAttack _enemyAttack = new EnemyAttack
        {
            cooldown = 1f,
            damage = 3f,
        };
        
        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<AIAgent, Translation, RaycastResult, EnemyAttackRange>().ForEach(
                (Entity e, ref Translation t, ref RaycastResult raycastResult, ref EnemyAttackRange attackRange) =>
                {
                    if (raycastResult.IsHit() && raycastResult.distance <= attackRange.value)
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
    }
}