using Unity.Entities;

namespace DoTs
{
    public struct TurretAim : IComponentData
    {
        public float aimRange;
        public bool isAimed;
    }
}