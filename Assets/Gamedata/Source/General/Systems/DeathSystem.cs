using Unity.Entities;

namespace DoTs
{
    public class DeathSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<Health>().ForEach((Entity entity, ref Health health) =>
            {
                if (health.value <= 0f)
                {
                    PostUpdateCommands.DestroyEntity(entity);
                }
            });
        }
    }
}