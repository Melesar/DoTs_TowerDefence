using Unity.Entities;
using Unity.Jobs;

public class ExampleSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        var commandBuffer = commandBufferSystem.CreateCommandBuffer();

        var job = new Job {buffer = commandBuffer};
        var handle = job.Schedule(inputDeps);

        //Allows to use EntityCommandBuffer in JobComponentSystem
        commandBufferSystem.AddJobHandleForProducer(handle);

        return handle;
    }
    
    private struct Job : IJob
    {
        public EntityCommandBuffer buffer;
        
        public void Execute()
        {
        }
    }
}