using DoTs.Graphics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    [UpdateBefore(typeof(SpriteAnimationSystem))]
    public class MovementSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct MovementJob : IJobForEach<TargetOwnership, Movement, Translation, Rotation>
        {
            public float delta;
            
            public void Execute(
                [ReadOnly] ref TargetOwnership target,
                [ReadOnly] ref Movement movement,
                ref Translation translation, 
                ref Rotation rotation)
            {
                Vector3 from = (Quaternion) rotation.Value * Vector2.right;
                Vector3 to = target.targetPosition - translation.Value;
//                rotation.Value = Quaternion.FromToRotation(from, to);

                var position = Vector3.MoveTowards(translation.Value, target.targetPosition,
                    movement.speed * delta);
                translation.Value = position;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new MovementJob {delta = Time.deltaTime}.Schedule(this, inputDeps);
        }
    }
}