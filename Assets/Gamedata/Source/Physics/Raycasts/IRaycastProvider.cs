using DoTs.Resources;
using Unity.Mathematics;

namespace DoTs.Physics
{
    public interface IRaycastProvider : IResourceProvider
    {
        RaycastResult Raycast(float3 origin, float3 direction);
        RaycastResult Raycast(float3 origin, float3 direction, LayerMask layerMask);
    }
}