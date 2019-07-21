using Unity.Entities;
using UnityEngine;

namespace DoTs
{
    [UpdateInGroup(typeof(EnemiesSystemGroup))]
    public class EnemyAttackSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<EnemyAttack, TargetOwnership>()
                .ForEach((Entity enemyEntity, ref EnemyAttack attack, ref TargetOwnership target) =>
                {
                    if (attack.currentCooldown > 0)
                    {
                        attack.currentCooldown -= Time.deltaTime;
                    }

                    if (target.targetEntity == Entity.Null)
                    {
                        RemoveTarget(enemyEntity);
                        return;
                    }

                    if (!EntityManager.Exists(target.targetEntity))
                    {
                        RemoveTarget(enemyEntity);
                        return;
                    }

                    if (!EntityManager.HasComponent<Health>(target.targetEntity))
                    {
                        RemoveTarget(enemyEntity);
                        return;
                    }

                    var targetHealth = EntityManager.GetComponentData<Health>(target.targetEntity);
                    if (targetHealth.value <= 0)
                    {
                        RemoveTarget(enemyEntity);
                    }
                    else if (attack.currentCooldown <= 0)
                    {
                        targetHealth.value -= attack.damage;
                        attack.currentCooldown = attack.cooldown;
                        PostUpdateCommands.SetComponent(target.targetEntity, targetHealth);
                    }
                });
        }

        private void RemoveTarget(Entity entity)
        {
            PostUpdateCommands.RemoveComponent<TargetOwnership>(entity);
        }
    }
}