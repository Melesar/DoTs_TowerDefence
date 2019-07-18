using Unity.Entities;
using Unity.Transforms;

namespace DoTs
{
    [UpdateInGroup(typeof(TurretsSystemGroup))]
    [UpdateAfter(typeof(TurretAimSystemV2))]
    public class TurretUpdateTargetsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<TurretAim, TargetOwnership>().ForEach((Entity e, ref TargetOwnership target) =>
            {
                if (!EntityManager.Exists(target.targetEntity) || target.targetEntity == Entity.Null)
                {
                    PostUpdateCommands.RemoveComponent<TargetOwnership>(e);
                }
                else
                {
                    target.targetPosition = EntityManager.GetComponentData<Translation>(target.targetEntity).Value;
                }
            });
        }
    }
}