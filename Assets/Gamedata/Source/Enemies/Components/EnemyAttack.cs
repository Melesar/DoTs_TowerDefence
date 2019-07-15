using Unity.Entities;

namespace DoTs
{
    public struct EnemyAttack : IComponentData
    {
        public float range;
        public float damage;
        public float cooldown;
    }
}