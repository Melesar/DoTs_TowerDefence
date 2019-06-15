using Unity.Entities;
using UnityEngine;

namespace DoTs.Templates
{
    public static class EntityManagerExtensions
    {
        public static Entity CreateFromTemplate(this EntityManager manager, IEntityTemplate template, Vector3 position)
        {
            return template.CreateEntity(manager, position);
        }
    }
}