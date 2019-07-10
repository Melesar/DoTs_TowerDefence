using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace DoTs.UI
{
    [UpdateInGroup(typeof(UISystemGroup))]
    public class UIDataSystem : JobComponentSystem
    {
        private EndInitializationEntityCommandBufferSystem _cbSystem;

        [ExcludeComponent(typeof(UIHealthBar))]
        private struct UpdateUIDataJob : IJobForEachWithEntity<Scale, Health>
        {
            public EntityCommandBuffer.Concurrent commands;
            
            public void Execute(Entity entity, int index, [ReadOnly]ref Scale scale, [ReadOnly] ref Health health)
            {
//                if (health.value >= health.maxValue)
//                {
//                    return;
//                }
                var healthBar = new UIHealthBar
                {
                    offsetY = 0.8f,
                    scale = scale.Value
                };
                commands.AddComponent(index, entity, healthBar);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = _cbSystem.CreateCommandBuffer();
            var job = new UpdateUIDataJob
            {
                commands = commandBuffer.ToConcurrent()
            };

            inputDeps = job.Schedule(this, inputDeps);
            _cbSystem.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }

        protected override void OnCreate()
        {
            _cbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
        }
    }
}