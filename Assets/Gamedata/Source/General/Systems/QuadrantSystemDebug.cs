using Unity.Collections;
using UnityEngine;

namespace DoTs
{
    public partial class QuadrantSystem
    {
        private Camera _camera;

        private Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = Camera.main;
                }
                
                return _camera;
            }
        }

        public void ShowEnemiesInRadius(Vector3 position, float radius)
        {
            DrawCircle(position, radius, Color.red);

            var access = GetQuadrantsAccess();
            using (var enemies = access.GetEnemiesWithinRadius(position, radius, Allocator.Temp))
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    var enemyPosition = enemies[i].position;
                    DrawEnemyBoundary(enemyPosition, Color.green);
                }
            }
        }

        private static void DrawCircle(Vector3 center, float radius, Color color)
        {
            const int segmentsCount = 25;
            const float step = 2f * Mathf.PI / segmentsCount;
            
            for(var angle = 0f; angle < 2 * Mathf.PI; angle += step)
            {
                var x1 = center.x + radius * Mathf.Cos(angle);
                var y1 = center.y + radius * Mathf.Sin(angle);
                
                var x2 = center.x + radius * Mathf.Cos(angle + step);
                var y2 = center.y + radius * Mathf.Sin(angle + step);

                Debug.DrawLine(new Vector3(x1, y1), new Vector3(x2, y2), color);
            }
        }

        private static void DrawEnemyBoundary(Vector3 position, Color color)
        {
            Debug.DrawLine(position + new Vector3(-0.5f, -0.5f), position + new Vector3(-0.5f, +0.5f), color);
            Debug.DrawLine(position + new Vector3(-0.5f, +0.5f), position + new Vector3(+0.5f, +0.5f), color);
            Debug.DrawLine(position + new Vector3(+0.5f, +0.5f), position + new Vector3(+0.5f, -0.5f), color);
            Debug.DrawLine(position + new Vector3(+0.5f, -0.5f), position + new Vector3(-0.5f, +0.5f), color);
        }

        private static void DrawPoint(Vector3 position, Color color)
        {
            Debug.DrawLine(position + Vector3.down * 0.3f, position + Vector3.up * 0.3f, color);
            Debug.DrawLine(position + Vector3.left * 0.3f, position + Vector3.right * 0.3f, color);
        }
        
        private static void DrawQuadrant(Vector3 position)
        {
            var lowerLeft = new Vector3(Mathf.FloorToInt(position.x / CELL_SIZE) * CELL_SIZE,
                Mathf.FloorToInt(position.y / CELL_SIZE) * CELL_SIZE, 0f);
            var upperLeft = lowerLeft + CELL_SIZE * new Vector3(0, 1);
            var upperRight = lowerLeft + CELL_SIZE * new Vector3(1, 1);
            var lowerRight = lowerLeft + CELL_SIZE * new Vector3(1, 0);
            
            Debug.DrawLine(lowerLeft, upperLeft);
            Debug.DrawLine(lowerLeft, lowerRight);
            Debug.DrawLine(upperLeft, upperRight);
            Debug.DrawLine(upperRight, lowerRight);
        }
    }
}