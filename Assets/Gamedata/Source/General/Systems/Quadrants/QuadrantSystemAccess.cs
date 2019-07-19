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

            public NativeList<T> GetEnemiesInQuadrant(float3 position, Allocator allocator = Allocator.TempJob)
            {
                return GetEnemiesInQuadrant(GetQuadrantHash(position), allocator);
            }

            public NativeList<T> GetEnemiesWithinRadius(float3 position, float radius,
                Allocator allocator = Allocator.TempJob)
            {
                const int randomPointsCount = 30;
                using (var points = GeneratePointsInCircle(randomPointsCount, position, radius))
                using (var hashMap = new NativeHashMap<int, float3>(randomPointsCount, Allocator.Temp))
                {
                    for (int i = 0; i < randomPointsCount; i++)
                    {
                        var hash = GetQuadrantHash(points[i]);
                        hashMap.TryAdd(hash, points[i]);
                    }

                    var enemies = new NativeList<T>(allocator);
                    var uniqueHashes = hashMap.GetKeyArray(Allocator.Temp);
                    for (int i = 0; i < uniqueHashes.Length; i++)
                    {
                        var hash = uniqueHashes[i];
                        if (!_quadrantMap.TryGetFirstValue(hash, out var data, out var it))
                        {
                            continue;
                        }

                        do
                        {
                            enemies.Add(data);
                        } 
                        while (_quadrantMap.TryGetNextValue(out data, ref it));
                    }

                    uniqueHashes.Dispose();
                    return enemies;
                }
            }

            private NativeList<T> GetEnemiesInQuadrant(int hash, Allocator allocator = Allocator.TempJob)
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