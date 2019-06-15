using System;
using DoTs.Templates;
using Unity.Entities;
using UnityEngine;

namespace DoTs
{
    public class FortressSpawnPoint : MonoBehaviour
    {
        [SerializeField]
        private EntityTemplate _fortressTemplate;

        private void Start()
        {
            var entityManager = World.Active.EntityManager;
            entityManager.CreateFromTemplate(_fortressTemplate, transform.position);
        }
    }
}