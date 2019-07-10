using System;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace DoTs.Sandbox
{
    public class Test : MonoBehaviour
    {
        private const int COUNT = 100;

        [SerializeField]
        private Material _material;
        [SerializeField]
        private Mesh _mesh;
        
        private MaterialPropertyBlock _block;
        private Matrix4x4[] _matrices;
        private Vector4[] _colors;
        private float[] _fillValues;

        private void Update()
        {
            UnityEngine.Graphics.DrawMeshInstanced(
                _mesh,
                0,
                _material,
                _matrices,
                COUNT,
                _block
            );
        }

        private void Start()
        {
            _block = new MaterialPropertyBlock();

            _matrices = new Matrix4x4[COUNT];
            _fillValues = new float[COUNT];
            for (int i = 0; i < COUNT; i++)
            {
                var position = new Vector3(Random.Range(0, 10), Random.Range(0, 10), 0f);
                _matrices[i] = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                _fillValues[i] = Random.value;
            }

            _block.SetFloatArray("_Fill", _fillValues);
        }
    }
}