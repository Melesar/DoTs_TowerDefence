using DoTs.Resources;
using Unity.Entities;

namespace DoTs.ESCBridge
{
    public interface IEntityQueryProvider : IResourceProvider
    {
        IEntityQuery GetQuery(params ComponentType[] componentTypes);
    }
}