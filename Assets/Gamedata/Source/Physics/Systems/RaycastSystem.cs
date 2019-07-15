using DoTs.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs.Physics
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class RaycastSystem : ComponentSystem, IRaycastProvider
    {
        private EntityQuery _query;
        
        [BurstCompile]
        private struct RaycastJob : IJobParallelFor
        {
            public float3 origin;
            public float3 direction;
            
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<AABB> aabbs;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Scale> scales;

            [ReadOnly]
            public NativeArray<Translation> positions;

            public NativeArray<float> outDistances;
            
            public void Execute(int index)
            {
                var position = positions[index].Value;
                var size = scales[index].Value * 2f * aabbs[index].extents;
                var bounds = new Bounds(position, size);
                var ray = new Ray(origin, direction);

                outDistances[index] = bounds.IntersectRay(ray, out var distance) ? distance : float.NegativeInfinity;
            }
        }
        
        [BurstCompile]
        private struct RaycastResultJob : IJob
        {
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Translation> positions;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Entity> entities;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<float> distances;

            public NativeArray<RaycastResult> outResult;
            
            public void Execute()
            {
                var minDistance = float.MaxValue;
                var minIndex = -1;
                for (int i = 0; i < distances.Length; i++)
                {
                    var distance = distances[i];
                    if (distance >= 0 && distance < minDistance)
                    {
                        minDistance = distance;
                        minIndex = i;
                    }
                }

                outResult[0] = new RaycastResult
                {
                    entity = entities[minIndex],
                    distance = minDistance,
                    position = positions[minIndex].Value
                };
            }
        }
        
        public RaycastResult Raycast(float3 origin, float3 direction)
        {
            var count = _query.CalculateLength();
            var aabbs = _query.ToComponentDataArray<AABB>(Allocator.TempJob);
            var positions = _query.ToComponentDataArray<Translation>(Allocator.TempJob);
            var scales = _query.ToComponentDataArray<Scale>(Allocator.TempJob);
            var entities = _query.ToEntityArray(Allocator.TempJob);
            
            var distances = new NativeArray<float>(count, Allocator.TempJob);
            var results = new NativeArray<RaycastResult>(1, Allocator.TempJob);

            var raycast = new RaycastJob
            {
                origin = origin,
                direction = direction,
                aabbs = aabbs,
                positions = positions,
                scales = scales,
                outDistances = distances
            };

            var getResult = new RaycastResultJob
            {
                distances = distances,
                entities = entities,
                positions = positions,
                outResult = results
            };

            var handle = raycast.Schedule(count, 16);
            handle = getResult.Schedule(handle);
            handle.Complete();
            
            var result = results[0];
            results.Dispose();
            return result;
        }

        protected override void OnCreate()
        {
            ResourceLocator<IRaycastProvider>.SetResourceProvider(this);
            
            _query = Entities.WithAllReadOnly<AABB, Translation, Scale>().ToEntityQuery();
        }
        

        protected override void OnUpdate()
        {
        }
    }
}