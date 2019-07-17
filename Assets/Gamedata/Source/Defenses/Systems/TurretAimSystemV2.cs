using Unity.Entities;
using Unity.Jobs;

namespace DoTs
{
    [DisableAutoCreation]
    public class TurretAimSystemV2 : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return inputDeps;
        }
    }
}