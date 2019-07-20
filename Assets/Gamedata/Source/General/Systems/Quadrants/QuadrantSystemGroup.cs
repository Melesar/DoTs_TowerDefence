using DoTs.Physics;
using Unity.Entities;

namespace DoTs.Quadrants
{
    
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public class QuadrantSystemGroup : ComponentSystemGroup
    {
        
    }
}