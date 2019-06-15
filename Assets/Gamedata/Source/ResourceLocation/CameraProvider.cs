using System;
using UnityEngine;

namespace DoTs.Resources
{
    public class CameraProvider : MonoBehaviour, IResourceProvider
    {
        [SerializeField]
        private Camera _entitiesRenderingCamera;

        public Camera EntitiesRenderingCamera => _entitiesRenderingCamera;

        private void Start()
        {
            ResourceLocator<CameraProvider>.SetResourceProvider(this);
        }

        private void OnDestroy()
        {
            ResourceLocator<CameraProvider>.SetResourceProvider(null);
        }
    }
}