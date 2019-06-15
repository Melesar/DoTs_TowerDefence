using Unity.Entities;
using UnityEngine;

namespace DoTs.Templates
{
    public abstract class EntityTemplate : MonoBehaviour, IEntityTemplate
    {
        public abstract Entity CreateEntity(EntityManager entityManager, Vector3 position);
    }
}