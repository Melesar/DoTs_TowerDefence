using DoTs.Physics;
using Unity.Entities;

namespace DoTs
{
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public class EnemiesSystemGroup : ComponentSystemGroup
    {
        
    }
}