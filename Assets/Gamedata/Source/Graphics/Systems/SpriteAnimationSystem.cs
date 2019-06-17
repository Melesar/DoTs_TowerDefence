using System;
using DoTs.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace DoTs.Graphics
{
    public class SpriteAnimationSystem : JobComponentSystem
    {
        private AnimationDataProvider _animationDataProvider;
        private NativeMultiHashMap<AnimationTypeWrapper, float4> _sequenceDataMap;
        private EntityQuery _query;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var animateJob = new AnimateJob
            {
                delta = Time.deltaTime,
            };
            
            var animateJobHandle = animateJob.Schedule(_query, inputDeps);
            var assignJob = new AssignUvsJob
            {
                sequenceMap = _sequenceDataMap
            };
            
            return assignJob.Schedule(this, animateJobHandle);
        }

        [BurstCompile]
        private struct AnimateJob : IJobForEach<SpriteAnimationData>
        {
            public float delta;

            public void Execute(ref SpriteAnimationData animationData)
            {
                if (animationData.currentFrameTime < animationData.frameTime)
                {
                    animationData.currentFrameTime += delta;
                    return;
                }

                animationData.currentFrameTime -= animationData.frameTime;
                animationData.currentFrame = (animationData.currentFrame + 1) % animationData.maxFrame;
            }
        }

        [BurstCompile]
        private struct AssignUvsJob : IJobForEach<SpriteAnimationData, Sprite>
        {
            [ReadOnly] public NativeMultiHashMap<AnimationTypeWrapper, float4> sequenceMap;
            
            public void Execute([ReadOnly] ref SpriteAnimationData animationData, [WriteOnly] ref Sprite sprite)
            {
                var targetIndex = animationData.currentFrame;
                var currentIndex = 0;
                var type = new AnimationTypeWrapper(animationData.entityType);

                if (!sequenceMap.TryGetFirstValue(type, out var value, out var it))
                {
                    return;
                }

                if (currentIndex++ == targetIndex)
                {
                    sprite.uv = value;
                    return;
                }
                
                while (sequenceMap.TryGetNextValue(out value, ref it)) 
                {
                    if (currentIndex++ == targetIndex)
                    {
                        sprite.uv = value;
                        return;
                    }    
                }
            }
        }

        protected override void OnStartRunning()
        {
            _query = EntityManager.CreateEntityQuery(typeof(SpriteAnimationData), typeof(Sprite));
            _animationDataProvider = ResourceLocator<AnimationDataProvider>.GetResourceProvider();
            _sequenceDataMap = _animationDataProvider.GetSequenceDataMap();
        }

        protected override void OnStopRunning()
        {
            _sequenceDataMap.Dispose();
            _query.Dispose();
        }
    }
}