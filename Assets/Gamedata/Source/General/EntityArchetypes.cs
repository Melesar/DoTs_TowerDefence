using DoTs.Graphics;
using DoTs.Physics;
using Unity.Entities;
using Unity.Transforms;

namespace DoTs
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EntityArchetypes : ComponentSystem
    {
        public static EntityArchetype Enemy { get; private set; }
        public static EntityArchetype MainTurret { get; private set; }
        public static EntityArchetype Wall { get; private set; }
        public static EntityArchetype Shell { get; private set; }
        public static EntityArchetype ShellExplosion { get; private set; }
        
        
        protected override void OnCreateManager()
        {
            Enemy = EntityManager.CreateArchetype(
                typeof (Enemy),
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(Sprite),
                typeof(SpriteAnimationData),
                typeof(Health),
                typeof(AABB),
                typeof(AIAgent),
                typeof(RaycastAgent),
                typeof(RaycastResult),
                typeof(LayerMask),
                typeof(EnemyAttackRange)
            );
            
            MainTurret = EntityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(Sprite),
                typeof(TurretRotation),
                typeof(TurretAim),
                typeof(TurretShooting),
                typeof(Target),
                typeof(AABB),
                typeof(PhysicsStatic),
                typeof(LayerMask)
            );
            
            Wall = EntityManager.CreateArchetype(
                typeof(Translation),
                typeof(Scale),
                typeof(Sprite),
                typeof(Health),
                typeof(AABB),
                typeof(PhysicsStatic),
                typeof(LayerMask)
            );
            
            Shell = EntityManager.CreateArchetype(
                typeof(Translation),
                typeof(ExplosiveShell)
            );
            
            ShellExplosion = EntityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(Sprite),
                typeof(SpriteAnimationData),
                typeof(Lifetime)
            );

            Enabled = false;
        }

        protected override void OnUpdate()
        {
        }
    }
}