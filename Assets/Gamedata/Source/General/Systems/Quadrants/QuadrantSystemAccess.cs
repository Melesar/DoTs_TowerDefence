using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DoTs.Quadrants
{
    public partial class QuadrantSystem
    {
        public struct QuadrantSystemAccess<T> where T : struct
        {
            [ReadOnly]
            private NativeMultiHashMap<int, T> _quadrantMap;

            public NativeList<T> GetActorsInQuadrant(float3 position, Allocator allocator = Allocator.TempJob)
            {
                return GetActorsInQuadrant(GetQuadrantHash(position), allocator);
            }

            public NativeList<T> GetActorsWithinRadius(float3 position, float radius,
                Allocator allocator = Allocator.TempJob)
            {
                const int randomPointsCount = 30;
                var points = GeneratePointsInCircle(randomPointsCount, position, radius);
                var actors = GetActorsFromRandomPoints(points, allocator);
                points.Dispose();
                return actors;
            }

            public NativeList<T> GetActorsAlongTheRay(float3 origin, float3 direction, float maxDistance, Allocator allocator = Allocator.Temp)
            {
                const int pointsOnTheRay = 30;
                var endPoint = origin + math.normalize(direction) * maxDistance;
                var points = GeneratePointsOnTheLine(pointsOnTheRay, origin, endPoint);
                var actors = GetActorsFromRandomPoints(points, allocator);
                points.Dispose();
                return actors;
            }

            private NativeList<T> GetActorsFromRandomPoints(NativeArray<float3> points, Allocator allocator)
            {
                var hashMap = new NativeHashMap<int, bool>(points.Length, Allocator.Temp);
                for (int i = 0; i < points.Length; i++)
                {
                    var hash = GetQuadrantHash(points[i]);
                    hashMap.TryAdd(hash, true);
                }

                var actors = new NativeList<T>(allocator);
                var uniqueHashes = hashMap.GetKeyArray(Allocator.Temp);
                hashMap.Dispose();

                for (int i = 0; i < uniqueHashes.Length; i++)
                {
                    var hash = uniqueHashes[i];
                    if (!_quadrantMap.TryGetFirstValue(hash, out var data, out var it))
                    {
                        continue;
                    }

                    do
                    {
                        actors.Add(data);
                    } while (_quadrantMap.TryGetNextValue(out data, ref it));
                }

                uniqueHashes.Dispose();
                return actors;
            }

            private NativeList<T> GetActorsInQuadrant(int hash, Allocator allocator = Allocator.TempJob)
            {
                var enemies = new NativeList<T>(allocator);
                if (!_quadrantMap.TryGetFirstValue(hash, out var data, out var it))
                {
                    return enemies;
                }

                do
                {
                    enemies.Add(data);
                } 
                while (_quadrantMap.TryGetNextValue(out data, ref it));

                return enemies;
            }

            private NativeArray<float3> GeneratePointsOnTheLine(int count, float3 start, float3 finish)
            {
                return new NativeArray<float3>(count, Allocator.Temp);
            }

            private NativeArray<float3> GeneratePointsInCircle(int count, float3 center, float radius)
            {
                var points = new NativeArray<float3>(count, Allocator.Temp);
                var seed = (uint) math.abs(math.ceil(center.x + center.y));
                var random = new Random(seed != 0 ? seed : 100);

                for (int i = 0; i < count; i++)
                {
                    var point = 2f * random.NextFloat2() - 1f;
                    point *= radius;
                    point += new float2(center.x, center.y);

                    points[i] = new float3(point.x, point.y, 0);
                }

                return points;
            }

            public QuadrantSystemAccess(NativeMultiHashMap<int, T> quadrantMap)
            {
                _quadrantMap = quadrantMap;
            }
        }
    }
}