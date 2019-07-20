using Unity.Entities;

namespace DoTs
{
    public struct EnemyAttack : IComponentData
    {
        public float damage;
        public float cooldown;
        public float currentCooldown;
    }
}