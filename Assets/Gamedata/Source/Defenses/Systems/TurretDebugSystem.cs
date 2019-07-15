using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    [UpdateInGroup(typeof(TurretsSystemGroup))]
    public class TurretDebugSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAllReadOnly<TurretRotation>().ForEach((ref TargetOwnership target) =>
            {
                Debug.DrawLine(target.targetPosition, target.targetPosition + new float3(0, -1f, 01f));
                Debug.DrawLine(target.targetPosition, target.targetPosition + new float3(0, 1f, 0f));
                Debug.DrawLine(target.targetPosition, target.targetPosition + new float3(1f, 0f, 0f));
                Debug.DrawLine(target.targetPosition, target.targetPosition + new float3(-1f, 0f, 0f));
            });
        }
    }
}