using System;
using Unity.Entities;
using UnityEngine;

namespace DoTs
{
    public class ESCBehaviour : MonoBehaviour
    {
        protected EntityManager _entityManager;

        protected virtual void Awake()
        {
            _entityManager = World.Active.EntityManager;
        }
    }
}