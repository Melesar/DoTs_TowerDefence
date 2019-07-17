using DoTs.Resources;
using DoTs.Utilites;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;

namespace DoTs.Physics
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class RaycastSystem : ComponentSystem, IRaycastProvider
    {
        private EntityQuery _query;
        
        [BurstCompile]
        private struct RaycastForEachJob : IJobForEachWithEntity<AABB, Scale, LayerMask, Translation>
        {
            public float3 origin;
            public float3 direction;
            public LayerMask targetMask;

            public NativeArray<float> outDistances;
            
            public void Execute(
                Entity entity,
                int index, 
                [ReadOnly] ref AABB aabb, 
                [ReadOnly] ref Scale scale,
                [ReadOnly] ref LayerMask layerMask,
                [ReadOnly] ref Translation translation)
            {
                if (!ValidateLayer(layerMask))
                {
                    outDistances[index] = float.NegativeInfinity;
                    return;
                }
                
                var size = scale.Value * 2f * aabb.extents;
                var bounds = new Bounds(translation.Value, size);
                var ray = new Ray(origin, direction);

                outDistances[index] = bounds.IntersectRay(ray, out var distance) ? distance : float.NegativeInfinity;
            }

            private bool ValidateLayer(LayerMask mask)
            {
                var targetLayer = targetMask.ToLayer();
                return mask.HasLayer(targetLayer);
            }
        }
        
        [BurstCompile]
        private struct RaycastJob : IJobParallelFor
        {
            public float3 origin;
            public float3 direction;
            public LayerMask targetMask;

            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<AABB> aabbs;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Scale> scales;
            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<LayerMask> layers;

            [ReadOnly]
            public NativeArray<Translation> positions;

            public NativeArray<float> outDistances;
            
            public void Execute(int index)
            {
                if (!ValidateLayer(index))
                {
                    outDistances[index] = float.NegativeInfinity;
                    return;
                }
                
                var position = positions[index].Value;
                var size = scales[index].Value * 2f * aabbs[index].extents;
                var bounds = new Bounds(position, size);
                var ray = new Ray(origin, direction);

                outDistances[index] = bounds.IntersectRay(ray, out var distance) ? distance : float.NegativeInfinity;
            }

            private bool ValidateLayer(int index)
            {
                var targetLayer = targetMask.ToLayer();
                return layers[index].HasLayer(targetLayer);
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
                    entity = minIndex >= 0 ? entities[minIndex] : Entity.Null,
                    distance = minIndex >= 0 ? minDistance : float.NegativeInfinity,
                    position = minIndex >= 0 ? positions[minIndex].Value : float3.zero
                };
            }
        }

        public RaycastResult Raycast(float3 origin, float3 direction)
        {
            return Raycast(origin, direction, LayerMask.Create(Layer.Default));
        }

        public RaycastResult Raycast(float3 origin, float3 direction, LayerMask layerMask)
        {
            Profiler.BeginSample("Raycast");
            var count = _query.CalculateLength();
            var positions = _query.ToComponentDataArray<Translation>(Allocator.TempJob);
            var entities = _query.ToEntityArray(Allocator.TempJob);

            var distances = new NativeArray<float>(count, Allocator.TempJob);
            var results = new NativeArray<RaycastResult>(1, Allocator.TempJob);

            var raycast = new RaycastForEachJob
            {
                direction = direction,
                origin = origin,
                outDistances = distances,
                targetMask = layerMask
            };

            var getResult = new RaycastResultJob
            {
                distances = distances,
                entities = entities,
                positions = positions,
                outResult = results
            };

            var handle = raycast.Schedule(this);
            handle = getResult.Schedule(handle);
            handle.Complete();
            
            var result = results[0];
            results.Dispose();
            Profiler.EndSample();
            return result;
        }

        protected override void OnCreate()
        {
            ResourceLocator<IRaycastProvider>.SetResourceProvider(this);
            
            _query = Entities.WithAllReadOnly<AABB, LayerMask, Translation, Scale>().ToEntityQuery();
        }
        

        protected override void OnUpdate()
        {
        }
    }
}