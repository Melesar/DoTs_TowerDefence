using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DoTs
{
    public partial class QuadrantSystem
    {
        public NativeList<EnemyData> GetEnemiesInQuadrant(float3 position, Allocator allocator = Allocator.TempJob)
        {
            var enemies = new NativeList<EnemyData>(allocator);
            var hash = GetQuadrantHash(position);
            if (!_quadrantsMap.TryGetFirstValue(hash, out var data, out var it))
            {
                return enemies;
            }

            do
            {
                enemies.Add(data);
            } while (_quadrantsMap.TryGetNextValue(out data, ref it));

            return enemies;
        }

        public NativeList<EnemyData> GetEnemiesWithinRadius(float3 position, float radius,
            Allocator allocator = Allocator.TempJob)
        {
            var leftX = position.x - radius;
            var rightX = position.x + radius;
            var upY = position.y + radius;
            var downY = position.y - radius;
            const float step = CELL_SIZE / 2f;

            var gridSize = UnityEngine.Mathf.FloorToInt(2 * radius / step);
            var points = new NativeArray<float3>(gridSize * gridSize, Allocator.Temp);
            var enemies = new NativeList<EnemyData>(allocator);
            using (points)
            {
                var pointIndex = 0;
                for (float x = leftX; x <= rightX; x += step)
                {
                    for (float y = downY; y <= upY; y += step)
                    {
                        points[pointIndex++] = new float3(x, y, 0);
                    }
                }

                for (int i = 0; i < points.Length; i++)
                {
                    using (var enemiesInQuadrant = GetEnemiesInQuadrant(points[i], Allocator.Temp))
                    {
                        enemies.AddRange(enemiesInQuadrant.AsArray());
                    }
                }

                return enemies;
            }
        }
    }
}