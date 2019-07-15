using System;
using DoTs.Graphics;
using DoTs.Physics;
using DoTs.Resources;
using DoTs.Utilites;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;
using SortingLayer = DoTs.Graphics.SortingLayer;

namespace DoTs
{
    public class EnemySpawner : ESCBehaviour
    {
        [SerializeField]
        private Transform _spawnOrigin;
        [SerializeField]
        private float _spawnRange = 4f;
        [SerializeField]
        private float _spawnInterval = 1.5f;
        [SerializeField]
        private int _spawnBatchSize = 2;
        
        private EntityArchetype _enemyArchetype;
        private float _spawnTime;
        private AnimationSequence _animationSequence;

        private void Update()
        {
            if (_spawnTime < _spawnInterval)
            {
                _spawnTime += Time.deltaTime;
                return;
            }

            _spawnTime -= _spawnInterval;
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            using (var enemies = new NativeArray<Entity>(_spawnBatchSize, Allocator.Temp))
            {
                _entityManager.CreateEntity(_enemyArchetype, enemies);
                foreach (var enemy in enemies)
                {
                    var position = _spawnOrigin.position + (Random.insideUnitCircle * _spawnRange).AsVector3();
                    position.z = 0;
                    _entityManager.SetComponentData(enemy, new Translation {Value = position});
                    _entityManager.SetComponentData(enemy, new Rotation{Value = quaternion.identity});
                    _entityManager.SetComponentData(enemy, new Scale {Value = 1f});
                    _entityManager.SetComponentData(enemy, new Graphics.Sprite
                    {
                        sortingLayer = SortingLayer.Units,
                        sortingOrder = 0
                    });
                    _entityManager.SetComponentData(enemy, new SpriteAnimationData
                    {
                        entityType = AnimationEntityType.Enemy,
                        currentFrame = Random.Range(0, _animationSequence.TotalFrames),
                        currentFrameTime = 0,
                        frameTime = _animationSequence.FrameTime,
                        maxFrame = _animationSequence.TotalFrames
                    });
                    _entityManager.SetComponentData(enemy, new Health
                    {
                        value = 8f,
                        maxValue = 8f,
                    });
                    _entityManager.SetComponentData(enemy, new AABB{extents = 0.15f});
                }
            }
        }

        private void Start()
        {
            var animationDataProvider = ResourceLocator<AnimationDataProvider>.GetResourceProvider();
            _animationSequence = animationDataProvider.GetAnimationSequence(AnimationEntityType.Enemy);
        }

        protected override void Awake()
        {
            base.Awake();
            
            _enemyArchetype = _entityManager.CreateArchetype(
                typeof (Enemy),
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(Graphics.Sprite),
                typeof(SpriteAnimationData),
                typeof(Health),
                typeof(AABB),
                typeof(AIAgent)
            );
        }
    }
}