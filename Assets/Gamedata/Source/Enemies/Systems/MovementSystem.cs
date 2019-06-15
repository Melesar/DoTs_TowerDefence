using DoTs.Graphics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs
{
    public class MovementSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref TargetOwnership target, ref Movement movement, ref Translation translation, ref Rotation rotation) =>
            {
                Vector3 from = (Quaternion) rotation.Value * Vector2.right;
                Vector3 to = target.targetPosition - translation.Value;
                rotation.Value = Quaternion.FromToRotation(from, to);

                var position = Vector3.MoveTowards(translation.Value, target.targetPosition,
                    movement.speed * Time.deltaTime);
                translation.Value = position;
            });
        }
    }
}