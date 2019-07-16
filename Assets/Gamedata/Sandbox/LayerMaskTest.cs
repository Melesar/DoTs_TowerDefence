using System;
using DoTs.Utilites;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using Random = UnityEngine.Random;

namespace DoTs.Sandbox
{
    public class LayerMaskTest : MonoBehaviour
    {
        [ContextMenu("Set layer")]
        private void TestSetLayer()
        {
            var mask = new LayerMask();
            mask = mask.SetLayer(Layer.Building);
            
            Debug.Assert(mask.value == 2);

            mask = mask.SetLayer(Layer.Enemy);

            Debug.Assert(mask.value == 3);
        }

        [ContextMenu("Unset layer")]
        private void TestUnsetLayer()
        {
            var mask = new LayerMask {value = 3};
            mask = mask.UnsetLayer(Layer.Building);
            
            Debug.Assert(mask.value == 1);
        }

        [ContextMenu("Has layer")]
        private void TestHasLayer()
        {
            var mask = new LayerMask {value = 1};
            
            Debug.Assert(mask.HasLayer(Layer.Enemy));
            Debug.Assert(!mask.HasLayer(Layer.Building));
        }
    }
}