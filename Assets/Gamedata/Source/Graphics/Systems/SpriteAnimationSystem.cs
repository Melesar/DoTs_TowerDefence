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
    public class SpriteAnimationSystem : ComponentSystem
    {
        private UVData _uvData;
        private AnimationDataProvider _animationDataProvider;
        private NativeMultiHashMap<AnimationTypeWrapper, float4> _sequenceDataMap;

        protected override void OnUpdate()
        {
            var query = EntityManager.CreateEntityQuery(typeof(SpriteAnimationData), typeof(Sprite));

            var animateJob = new AnimateJob
            {
                delta = Time.deltaTime,
            };

//            var animationsMap = new NativeMultiHashMap<AnimationTypeWrapper, AnimationDataWrapper>();
//            var sortingJob = new SortAnimationsByTypesJob
//            {
//                map = animationsMap.ToConcurrent()
//            };

            

            //TODO Set animation go free??? Assign readonly attribute to SpriteAnimationData in other jobs
            var animateJobHandle = animateJob.Schedule(query);
//            var sortingHandle = sortingJob.Schedule(query, animateJobHandle);
//            sortingHandle.Complete();

            var assignJob = new AssignUvsJob
            {
                sequenceMap = _sequenceDataMap
            };
            assignJob.Schedule(this, animateJobHandle).Complete();
            
//            var setPropertiesJob = new SetSpriteUvJob
//            {
//                uvData = _uvData
//            };
//            var setPropertiesHandle = setPropertiesJob.Schedule(query, animateJobHandle);
//            setPropertiesHandle.Complete();
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
        private struct SortAnimationsByTypesJob : IJobForEach<SpriteAnimationData, Sprite>
        {
            public NativeMultiHashMap<AnimationTypeWrapper, AnimationDataWrapper>.Concurrent map;
            
            public void Execute(ref SpriteAnimationData animationData, ref Sprite sprite)
            {
            }
        }

        [BurstCompile]
        private struct SetSpriteUvJob : IJobForEach<Sprite, SpriteAnimationData>
        {
            public UVData uvData;
            [ReadOnly, DeallocateOnJobCompletion] 
            public NativeArray<float4> _uvs;
            
            public void Execute(ref Sprite sprite, ref SpriteAnimationData animationData)
            {
                var offsetX = uvData.offsetX + animationData.currentFrame * uvData.stepX * uvData.sizeX;
                var offsetY = uvData.offsetY + animationData.currentFrame * uvData.stepY * uvData.sizeY;
                var sizeX = uvData.sizeX;
                var sizeY = uvData.sizeY;
                sprite.uv = new float4(sizeX, sizeY, offsetX, offsetY);
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
        
        private struct AnimationDataWrapper
        {
            public SpriteAnimationData animationData;
            public Sprite sprite;
        }

        protected override void OnStartRunning()
        {
            _animationDataProvider = ResourceLocator<AnimationDataProvider>.GetResourceProvider();
            _sequenceDataMap = _animationDataProvider.GetSequenceDataMap();
            _uvData = _animationDataProvider.UvData;
        }

        protected override void OnStopRunning()
        {
            _sequenceDataMap.Dispose();
        }
    }
}