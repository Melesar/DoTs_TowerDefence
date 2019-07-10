using DoTs.Graphics;
using DoTs.Resources;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DoTs.UI
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(SpriteRenderingSystem))]
    public class HealthBarRenderingSystem : ComponentSystem
    {
        private EntityQuery _query;
        private MaterialPropertyBlock _propertyBlock;
        private HealthBarGraphicsProvider _graphicsProvider;
        
        private const int BATCH_SIZE = 1023;
        private const string FILL_PROPERTY = "_Fill";

        private readonly int _fillPropIndex = Shader.PropertyToID(FILL_PROPERTY);
        private readonly Matrix4x4[] _matrices = new Matrix4x4[BATCH_SIZE];
        private readonly float[] _healthValues = new float[BATCH_SIZE];
        
        protected override void OnUpdate()
        {
            var dataSize = _query.CalculateLength();
            var healthBarData = _query.ToComponentDataArray<UIHealthBar>(Allocator.TempJob);
            var healthData = _query.ToComponentDataArray<Health>(Allocator.TempJob);
            var positionData = _query.ToComponentDataArray<Translation>(Allocator.TempJob);
            var scaleData = _query.ToComponentDataArray<Scale>(Allocator.TempJob);

            var material = _graphicsProvider.Material;
            var mesh = _graphicsProvider.GetMesh(1f);

            for (int i = 0; i < dataSize; i += BATCH_SIZE)
            {
                var endIndex = Mathf.Min(i + BATCH_SIZE, dataSize);
                var batchSize = endIndex - i;
                for (int j = 0; j < batchSize; j++)
                {
                    var index = i + j;
                    _matrices[j] = GetTransform(positionData[index], scaleData[index], healthBarData[index]);
                    _healthValues[j] = GetHealthValue(healthData[index]);
                }

                _propertyBlock.SetFloatArray(_fillPropIndex, _healthValues);
                UnityEngine.Graphics.DrawMeshInstanced(
                    mesh,
                    0,
                    material,
                    _matrices,
                    batchSize,
                    _propertyBlock
                );
            }
            
            healthBarData.Dispose();
            healthData.Dispose();
            positionData.Dispose();
            scaleData.Dispose();
        }
        
        private static Matrix4x4 GetTransform(Translation translation, Scale scaleData, UIHealthBar healthBarData)
        {
            var position = translation.Value + new float3(healthBarData.offsetX, healthBarData.offsetY, -5f);
            var scale = new Vector3(1f, 1f, 1f);

            return Matrix4x4.TRS(position, Quaternion.identity, scale);
        }

        private static float GetHealthValue(Health health)
        {
            return health.value / health.maxValue;
        }
        
        protected override void OnCreate()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _graphicsProvider = ResourceLocator<HealthBarGraphicsProvider>.GetResourceProvider();
            
            _query = Entities
                .WithAllReadOnly<UIHealthBar, Health, Translation, Scale>()
                .ToEntityQuery();
        }
    }
}