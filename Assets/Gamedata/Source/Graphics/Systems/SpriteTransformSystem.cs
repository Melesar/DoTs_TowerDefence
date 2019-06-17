using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DoTs.Graphics
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public class SpriteTransformSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle deps)
        {
            return new ApplyTransformJob().Schedule(this, deps);
        }
        
        [BurstCompile]
        private struct ApplyTransformJob : IJobForEach<Sprite, Translation, Rotation, Scale>
        {
            public void Execute([WriteOnly] ref Sprite sprite, 
                [ReadOnly] ref Translation translation, 
                [ReadOnly] ref Rotation rotation, 
                [ReadOnly] ref Scale scale)
            {
                sprite.matrix = float4x4.TRS(translation.Value, rotation.Value, scale.Value);
            }
        }
    }
}