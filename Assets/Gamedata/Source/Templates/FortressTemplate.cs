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
            Vector3 worldPosition;
            Sprite sprite;
            float scale;
            foreach (var wall in _walls)
            {
                var wallEntity = entityManager.CreateEntity(typeof(Translation), typeof(Scale), typeof(Graphics.Sprite));
                
                worldPosition = position + wall.transform.position;
                scale = wall.transform.localScale.x;
                entityManager.SetComponentData(wallEntity, new Translation {Value = worldPosition});
                entityManager.SetComponentData(wallEntity, new Scale {Value = scale});

                sprite = wall.GetComponent<SpriteRenderer>().sprite;
                entityManager.SetComponentData(wallEntity, new Graphics.Sprite
                {
                    matrix = float4x4.TRS(worldPosition, quaternion.identity, scale),
                    uv = sprite.GetUvRect()
                });
                entityManager.SetComponentData(wallEntity, new Scale {Value = _scale});
            }

            var turretEntity = entityManager.CreateEntity(GetTurretArchetype(entityManager));
            entityManager.SetName(turretEntity, "Fortress turret");
            
            worldPosition = position + _turret.transform.position;
            scale = _turret.transform.localScale.x;
            entityManager.SetComponentData(turretEntity, new Translation {Value = worldPosition});
            entityManager.SetComponentData(turretEntity, new Scale {Value = scale});
            
            sprite = _turret.GetComponent<SpriteRenderer>().sprite;
            entityManager.SetComponentData(turretEntity, new Graphics.Sprite
            {
                matrix = float4x4.TRS(worldPosition, quaternion.identity, scale),
                uv = sprite.GetUvRect()
            });

            var maxPossibleIdleTime = 3f;
            entityManager.SetComponentData(turretEntity, new TurretRotation
            {
                maxPossibleIdleTime = maxPossibleIdleTime,
                idleTime = Random.Range(0, maxPossibleIdleTime),
                targetAngle = Random.Range(0f, 360f),
                turnSpeed = 45
            });

            return entityManager.CreateEntity();
        }

        private static EntityArchetype GetTurretArchetype(EntityManager entityManager)
        {
            return entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(Graphics.Sprite),
                typeof(TurretRotation),
                typeof(Target));
        }
    }
}