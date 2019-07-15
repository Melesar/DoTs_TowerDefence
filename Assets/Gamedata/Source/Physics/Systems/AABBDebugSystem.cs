using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs.Physics
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public class AABBDebugSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<AABB, Translation, Scale>()
                .WithNone<PhysicsStatic>()
                .ForEach(
                (ref AABB aabb, ref Translation translation, ref Scale scale) =>
                {
                    var center = translation.Value;
                    var extents = aabb.extents * scale.Value;
                    
                    DrawAABB(center, extents, Color.green);
                });
            
            Entities
                .WithAllReadOnly<AABB, PhysicsStatic, Translation, Scale>()
                .ForEach(
                    (Entity e, ref AABB aabb, ref Translation translation, ref Scale scale) =>
                    {
                        var center = translation.Value;
                        var extents = aabb.extents * scale.Value;

//                        Debug.Log($"{EntityManager.GetName(e)}: {scale.Value}");
                        
                        DrawAABB(center, extents, Color.red);
                    });
        }

        private static void DrawAABB(float3 center, float3 extents, Color color)
        {
            var lleft = center + new float3(-extents.x, -extents.y, 0f);
            var uleft = center + new float3(-extents.x, extents.y, 0f);
            var lright = center + new float3(extents.x, -extents.y, 0f);
            var uright = center + new float3(extents.x, extents.y, 0f);

            Debug.DrawLine(uleft, uright, color);
            Debug.DrawLine(uright, lright, color);
            Debug.DrawLine(lright, lleft, color);
            Debug.DrawLine(lleft, uleft, color);
        }
    }
}