using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DoTs.Graphics
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public class SpriteTransformSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var query = EntityManager.CreateEntityQuery(
                typeof(Sprite),
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale));
            
            var job = new ApplyTransformJob();
            var handle = job.Schedule(query);
            handle.Complete();
        }
        
        [BurstCompile]
        private struct ApplyTransformJob : IJobForEach<Sprite, Translation, Rotation, Scale>
        {
            public void Execute(ref Sprite sprite, ref Translation translation, ref Rotation rotation, ref Scale scale)
            {
                sprite.matrix = float4x4.TRS(translation.Value, rotation.Value, scale.Value);
            }
        }
    }
}