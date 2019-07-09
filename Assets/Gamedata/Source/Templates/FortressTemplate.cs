using DoTs.Utilites;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DoTs.Templates
{
    public class FortressTemplate : EntityTemplate
    {
        [SerializeField]
        private float _scale;
        [SerializeField]
        private GameObject[] _walls;
        [SerializeField]
        private GameObject _turret;
        
        public override Entity CreateEntity(EntityManager entityManager, Vector3 position)
        {
            foreach (var wall in _walls)
            {
                CreateWall(entityManager, position, wall);
            }

            CreateTurret(entityManager, position);

            return entityManager.CreateEntity();
        }

        private void CreateTurret(EntityManager entityManager, Vector3 position)
        {
            var turretEntity = entityManager.CreateEntity(GetTurretArchetype(entityManager));
            entityManager.SetName(turretEntity, "Fortress turret");

            var worldPosition = position + _turret.transform.position;
            var scale = _turret.transform.localScale.x;
            entityManager.SetComponentData(turretEntity, new Translation {Value = worldPosition});
            entityManager.SetComponentData(turretEntity, new Scale {Value = scale});

            var sprite = _turret.GetComponent<SpriteRenderer>().sprite;
            entityManager.SetComponentData(turretEntity, new Graphics.Sprite
            {
                matrix = float4x4.TRS(worldPosition, quaternion.identity, scale),
                uv = sprite.GetUvRect()
            });

            const float maxPossibleIdleTime = 3f;
            entityManager.SetComponentData(turretEntity, new TurretRotation
            {
                maxPossibleIdleTime = maxPossibleIdleTime,
                idleTime = Random.Range(0, maxPossibleIdleTime),
                targetAngle = Random.Range(0f, 360f),
                turnSpeed = 45
            });
            
            entityManager.SetComponentData(turretEntity, new TurretAim
            {
                aimRange = 10f
            });
            
            entityManager.SetComponentData(turretEntity, new TurretShooting
            {
                currentCooldownTime = 0f,
                totalCooldownTime = 3f
            });
        }

        private void CreateWall(EntityManager entityManager, Vector3 position, GameObject wall)
        {
            var wallEntity = entityManager.CreateEntity(GetWallArchetype(entityManager));
            entityManager.SetName(wallEntity, "Fortress wall");

            var worldPosition = position + wall.transform.position;
            var scale = wall.transform.localScale.x;
            entityManager.SetComponentData(wallEntity, new Translation {Value = worldPosition});
            entityManager.SetComponentData(wallEntity, new Scale {Value = scale});

            var sprite = wall.GetComponent<SpriteRenderer>().sprite;
            entityManager.SetComponentData(wallEntity, new Graphics.Sprite
            {
                matrix = float4x4.TRS(worldPosition, quaternion.identity, scale),
                uv = sprite.GetUvRect()
            });
            entityManager.SetComponentData(wallEntity, new Scale {Value = _scale});
            entityManager.SetComponentData(wallEntity, new Health{value = 35f});
        }

        private static EntityArchetype GetTurretArchetype(EntityManager entityManager)
        {
            return entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(Graphics.Sprite),
                typeof(TurretRotation),
                typeof(TurretAim),
                typeof(TurretShooting),
                typeof(Target));
        }

        private static EntityArchetype GetWallArchetype(EntityManager entityManager)
        {
            return entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Scale),
                typeof(Graphics.Sprite),
                typeof(Health)
            );
        }
    }
}