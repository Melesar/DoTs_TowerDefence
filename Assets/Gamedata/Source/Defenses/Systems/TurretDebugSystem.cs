using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    public class TurretDebugSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref TurretRotation rotationData, ref Translation translation, ref Rotation rotation) =>
            {
                var targetDirection = math.mul(Quaternion.Euler(0f, 0f, rotationData.targetAngle), math.up());
                Debug.DrawLine(translation.Value, targetDirection * 4f, Color.green);
            });
            
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