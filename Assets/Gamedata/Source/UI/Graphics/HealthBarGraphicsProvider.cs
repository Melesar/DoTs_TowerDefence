using System.Collections.Generic;
using DoTs.Resources;
using UnityEngine;

namespace DoTs.UI
{
    [CreateAssetMenu(menuName = "DoTs/UI/Health bar graphics provider")]
    public class HealthBarGraphicsProvider : ResourceProviderAsset<HealthBarGraphicsProvider>
    {
        [SerializeField]
        private Material _material;
        
        public Material Material =>_material;

        private readonly Dictionary<float, Mesh> _meshesCache = new Dictionary<float, Mesh>();
         
        public Mesh GetMesh(float scale)
        {
            if (!_meshesCache.TryGetValue(scale, out var mesh))
            {
                mesh = CreateMesh(scale);
            }

            return mesh;
        }

        private Mesh CreateMesh(float scale)
        {
            var m = new Mesh();

            var verts = new Vector3[4];
            verts[0] = new Vector3(-0.5f, -0.12f, 0f);
            verts[1] = new Vector3(-0.5f, 0.12f, 0f);
            verts[2] = new Vector3(0.5f, 0.12f, 0f);
            verts[3] = new Vector3(0.5f, -0.12f, 0f);

            var triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 3;

            triangles[3] = 1;
            triangles[4] = 2;
            triangles[5] = 3;

            var uv = new Vector2[4];
            uv[0] = new Vector2(0f, 0f);
            uv[1] = new Vector2(0f, 1f);
            uv[2] = new Vector2(1f, 1f);
            uv[3] = new Vector2(1f, 0f);

            m.vertices = verts;
            m.triangles = triangles;
            m.uv = uv;

            _meshesCache[scale] = m;
            
            return m;
        }
    }
}