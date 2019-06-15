using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DoTs.Resources
{
    public interface IResourceProvider { }

    public static class ResourceLocator<T> where T : class, IResourceProvider
    {
        private static T _resourceProvider;
        
        public static T GetResourceProvider()
        {
            Assert.IsNotNull(_resourceProvider);
            return _resourceProvider;
        }

        public static void SetResourceProvider(T provider)
        {
            _resourceProvider = provider;
        }
    }
}