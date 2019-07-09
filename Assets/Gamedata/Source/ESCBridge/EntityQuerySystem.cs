using DoTs.Resources;
using Unity.Entities;

namespace DoTs.ESCBridge
{
    public class EntityQuerySystem : ComponentSystem, IEntityQueryProvider
    {
        public IEntityQuery GetQuery(params ComponentType[] componentTypes)
        {
            var query = EntityManager.CreateEntityQuery(componentTypes);
            return new EntityQueryComponentProvider(query);
        }

        protected override void OnCreate()
        {
            ResourceLocator<IEntityQueryProvider>.SetResourceProvider(this);
        }

        protected override void OnUpdate()
        {
        }
    }
}