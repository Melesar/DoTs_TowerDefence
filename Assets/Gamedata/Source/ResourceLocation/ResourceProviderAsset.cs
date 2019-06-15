using System;
using UnityEngine;

namespace DoTs.Resources
{
    public abstract class ResourceProviderAsset : ScriptableObject, IResourceProvider { }
    public abstract class ResourceProviderAsset<T> : ResourceProviderAsset where T : ResourceProviderAsset
    {
        protected virtual void OnEnable()
        {
            ResourceLocator<T>.SetResourceProvider(this as T);
        }

        protected virtual void OnDisable()
        {
            ResourceLocator<T>.SetResourceProvider(null);
        }
    }
}