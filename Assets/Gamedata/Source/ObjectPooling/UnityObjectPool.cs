using UnityEngine;

namespace DoTs.ObjectPooling
{
    public class UnityObjectPool<T> : ObjectPool<T> where T : Object
    {
        private readonly T _template;

        public UnityObjectPool(T template)
        {
            _template = template;
        }

        public UnityObjectPool(T template, int capacity) : base(capacity)
        {
            _template = template;
        }

        protected override T SpawnNewObject()
        {
            return Object.Instantiate(_template);
        }
    }
}