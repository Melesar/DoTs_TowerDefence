using System;
using System.Collections;
using UnityEngine;
using DoTs.Resources;

namespace DoTs.Sandbox
{
    public class Test : MonoBehaviour
    {

        [SerializeField]
        private Mesh _mesh;
        [SerializeField]
        private Material _material;
        
//        private void Start()
//        {
//            var renderer = GetComponent<MeshRenderer>();
//            var block = new MaterialPropertyBlock();
//            renderer.GetPropertyBlock(block);
//            block.SetVectorArray("_MainTex_UV", new [] {new Vector4(1, 1, 0, 0)});
//            renderer.SetPropertyBlock(block);
//        }

        private void Update()
        {
            var matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
            var uv = new Vector4(1/23f, 1/13f, 15f/23, 2f/13);
            var propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetVectorArray("_MainTex_UV", new [] {uv});

            UnityEngine.Graphics.DrawMeshInstanced(_mesh, 0, _material, new[] {matrix}, 1, propertyBlock);
        }
    }
}