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
            Prepare(capacity);
        }

        protected override T SpawnNewObject()
        {
            var obj = Object.Instantiate(_template);
            Reset(obj);
            return obj;
        }
    }
}