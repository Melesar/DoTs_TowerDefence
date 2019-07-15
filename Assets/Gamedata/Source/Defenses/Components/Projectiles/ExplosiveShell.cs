using Unity.Entities;

namespace DoTs
{
    public struct ExplosiveShell : IComponentData
    {
        public float explosionRadius;
        public float explosionDamage;
    }
}