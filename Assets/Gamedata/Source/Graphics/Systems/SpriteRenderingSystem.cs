using DoTs.Resources;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DoTs.Graphics
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpriteRenderingSystem : ComponentSystem
    {
        private Mesh _mesh;
        private Material _material;
        
        private Matrix4x4[] _matrices;
        private Vector4[] _uvs;

        private readonly int _shaderUV = Shader.PropertyToID(UV_PROPERTY_NAME);
        private readonly MaterialPropertyBlock _propertyBlock = new MaterialPropertyBlock();
        private EntityQuery _query;

        private const int DRAW_MESH_BATCH = 1023;
        private const string UV_PROPERTY_NAME = "_MainTex_UV";
        
        protected override void OnUpdate()
        {
            using (var spritesArray = _query.ToComponentDataArray<Sprite>(Allocator.TempJob))
            {
                var length = spritesArray.Length;

                for (var i = 0; i < length; i += DRAW_MESH_BATCH)
                {
                    var sliceSize = math.min(length - i, DRAW_MESH_BATCH);
                    for (int j = 0; j < sliceSize; j++)
                    {
                        var spriteData = spritesArray[i + j];
                        _matrices[j] = spriteData.matrix;
                        _uvs[j] = spriteData.uv;
                    }
                
                    _propertyBlock.SetVectorArray(_shaderUV, _uvs);
                    UnityEngine.Graphics.DrawMeshInstanced(
                        _mesh,
                        0,
                        _material,
                        _matrices,
                        sliceSize,
                        _propertyBlock
                        );
                }
            }
        }

        protected override void OnStartRunning()
        {
            _matrices = new Matrix4x4[DRAW_MESH_BATCH];
            _uvs = new Vector4[DRAW_MESH_BATCH];
            
            var animationDataProvider = ResourceLocator<AnimationDataProvider>.GetResourceProvider();
            _mesh = animationDataProvider.Mesh;
            _material = animationDataProvider.Material;
            
            _query = Entities.WithAllReadOnly<Sprite>().ToEntityQuery();
        }
    }
}