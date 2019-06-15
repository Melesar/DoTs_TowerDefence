using Unity.Entities;
using UnityEngine;

namespace DoTs.Templates
{
    public interface IEntityTemplate
    {
        Entity CreateEntity(EntityManager entityManager, Vector3 position);
    }
}