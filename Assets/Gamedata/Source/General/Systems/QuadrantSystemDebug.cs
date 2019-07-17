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

        private void DrawQuadrants()
        {
            var mousePos = Camera.ScreenToWorldPoint(Input.mousePosition);

            DrawQuadrant(mousePos);
            using (var enemies = GetEnemiesInQuadrant(mousePos, Allocator.Temp))
            {
                Debug.Log($"Quadrant {GetQuadrantHash(mousePos)}, {enemies.Length} enemies");
            }
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