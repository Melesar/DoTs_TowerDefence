using System;
using Unity.Entities;
using UnityEngine;

namespace DoTs.Path
{
    public class PathSetup : MonoBehaviour
    {
        [SerializeField]
        private Transform _pathRootObject;

        private void Start()
        {
            var entityManager = World.Active.EntityManager;
            var pathEntity = entityManager.CreateEntity();
            entityManager.AddComponent(pathEntity, typeof(Path));
            var pointsBuffer = entityManager.AddBuffer<EnemyPathPoint>(pathEntity);
            
            foreach (Transform child in _pathRootObject)
            {
                pointsBuffer.Add(new EnemyPathPoint {position = child.position});    
            }
        }
    }
}