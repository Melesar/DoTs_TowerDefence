using System;
using Unity.Entities;
using UnityEngine;

namespace DoTs.Sandbox
{
    public class QuadrantsTest : MonoBehaviour
    {
        [SerializeField]
        private float _radius;
        
        private QuadrantSystem _quadrantSystem;
        private Camera _camera;

        private void Update()
        {
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _quadrantSystem.ShowEnemiesInRadius(mousePos, _radius);
        }

        private void Awake()
        {
            _camera = Camera.main;
            _quadrantSystem = World.Active.GetExistingSystem<QuadrantSystem>();
        }
    }
}