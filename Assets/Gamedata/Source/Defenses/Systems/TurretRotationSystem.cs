using DoTs.Graphics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DoTs
{
    public class TurretRotationSystem : ComponentSystem 
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref TurretRotation rotationData, ref Rotation rotationValue, ref Scale scale) =>
            {
                if (rotationData.isTurning)
                {
                    var currentRotation = (Quaternion) rotationValue.Value;
                    var targetRotation = quaternion.RotateZ(rotationData.targetAngle);

                    var rotationDelta = rotationData.turnSpeed * Time.deltaTime;
                    rotationValue.Value = Quaternion.RotateTowards(currentRotation, targetRotation, rotationDelta);
                    rotationData.isTurning =
                        Quaternion.Angle(rotationValue.Value, targetRotation) > Quaternion.kEpsilon;
                }
                else if (rotationData.currentIdleTime < rotationData.idleTime)
                {
                    rotationData.currentIdleTime += Time.deltaTime;
                }
                else
                {
                    rotationData.isTurning = true;
                    rotationData.currentIdleTime = 0f;
                    rotationData.idleTime = Random.Range(0f, rotationData.maxPossibleIdleTime);
                    rotationData.targetAngle = Random.Range(0f, 360f);
                }
            });
        }
    }
}