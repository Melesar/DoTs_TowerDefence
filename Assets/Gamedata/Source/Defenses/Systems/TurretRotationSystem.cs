using DoTs.Graphics;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DoTs
{
    public class TurretRotationSystem : JobComponentSystem
    {
        private const float ROTATION_EPSILON = 3f;
        
        protected override JobHandle OnUpdate(JobHandle deps)
        {
            var seed = (uint) Random.Range(0, 1000000);
            var random = new Unity.Mathematics.Random(seed);

            var job = new TurretRotationJob
            {
                delta = Time.deltaTime,
                accuracy = ROTATION_EPSILON,
                rnd = random
            };
            
            return job.Schedule(this, deps);
        }
        
        [BurstCompile]
        private struct TurretRotationJob : IJobForEach<TurretRotation, Rotation>
        {
            public float delta;
            public float accuracy;
            public Unity.Mathematics.Random rnd;
            
            public void Execute(ref TurretRotation rotationData, ref Rotation rotationValue)
            {
                if (rotationData.isTurning)
                {
                    var currentRotation = (Quaternion) rotationValue.Value;
                    var targetRotation = Quaternion.Euler(0f, 0f, rotationData.targetAngle);

                    var rotationDelta = rotationData.turnSpeed * delta;
                    rotationValue.Value = Quaternion.RotateTowards(currentRotation, targetRotation, rotationDelta);
                    rotationData.isTurning =
                        Quaternion.Angle(rotationValue.Value, targetRotation) > accuracy;
                }
                else if (rotationData.currentIdleTime < rotationData.idleTime)
                {
                    rotationData.currentIdleTime += delta;
                }
                else
                {
                    rotationData.isTurning = true;
                    rotationData.currentIdleTime = 0f;
                    rotationData.idleTime = rnd.NextFloat(0f, rotationData.maxPossibleIdleTime);
                    rotationData.targetAngle = rnd.NextFloat(0f, 360f);
                }
            }
        }
    }
}