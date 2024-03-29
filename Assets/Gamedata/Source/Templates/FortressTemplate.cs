using DoTs.Physics;
using DoTs.Utilites;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;
using SortingLayer = DoTs.Graphics.SortingLayer;

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
            var turretEntity = entityManager.CreateEntity(EntityArchetypes.MainTurret);
            entityManager.SetName(turretEntity, "Fortress turret");

            var worldPosition = position + _turret.transform.position;
            var scale = _turret.transform.localScale.x;
            entityManager.SetComponentData(turretEntity, new Translation {Value = worldPosition});
            entityManager.SetComponentData(turretEntity, new Scale {Value = scale});

            var sprite = _turret.GetComponent<SpriteRenderer>().sprite;
            entityManager.SetComponentData(turretEntity, new Graphics.Sprite
            {
                matrix = float4x4.TRS(worldPosition, quaternion.identity, scale),
                uv = sprite.GetUvRect(),
                sortingLayer = SortingLayer.Buildings,
                sortingOrder = 10
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
            
            entityManager.SetComponentData(turretEntity, new AABB {extents = 0.45f});
            entityManager.SetComponentData(turretEntity, LayerMask.Create(Layer.Building));
            entityManager.SetComponentData(turretEntity, new Health {value = float.MaxValue, maxValue = float.MaxValue});
        }

        private void CreateWall(EntityManager entityManager, Vector3 position, GameObject wall)
        {
            var wallEntity = entityManager.CreateEntity(EntityArchetypes.Wall);
            entityManager.SetName(wallEntity, "Fortress wall");

            var worldPosition = position + wall.transform.position;
            var scale = wall.transform.localScale.x;
            entityManager.SetComponentData(wallEntity, new Translation {Value = worldPosition});
            entityManager.SetComponentData(wallEntity, new Scale {Value = scale});

            var sprite = wall.GetComponent<SpriteRenderer>().sprite;
            entityManager.SetComponentData(wallEntity, new Graphics.Sprite
            {
                matrix = float4x4.TRS(worldPosition, quaternion.identity, scale),
                uv = sprite.GetUvRect(),
                sortingLayer = SortingLayer.Buildings,
                sortingOrder = 1
            });
            entityManager.SetComponentData(wallEntity, new Health
            {
                value = 35f,
                maxValue = 35f,
            });
            entityManager.SetComponentData(wallEntity, new AABB {extents = 0.4f});
            entityManager.SetComponentData(wallEntity, LayerMask.Create(Layer.Building));
        }
    }
}