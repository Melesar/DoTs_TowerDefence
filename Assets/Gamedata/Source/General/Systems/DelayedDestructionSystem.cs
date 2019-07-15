using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DoTs
{
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public class DelayedDestructionSystem : JobComponentSystem
    {
        private struct DelayedDestructionJob : IJobForEachWithEntity<Lifetime>
        {
            public EntityCommandBuffer.Concurrent commandBuffer;
            public float deltaTime;
            
            public void Execute(Entity entity, int index, ref Lifetime lifetime)
            {
                if (lifetime.value > 0f)
                {
                    lifetime.value -= deltaTime;
                }
                else
                {
                    commandBuffer.DestroyEntity(index, entity);
                }
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var system = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            
            var job = new DelayedDestructionJob
            {
                deltaTime = Time.deltaTime,
                commandBuffer = system.CreateCommandBuffer().ToConcurrent() 
            };

            inputDeps = job.Schedule(this, inputDeps);
            system.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }
    }
}