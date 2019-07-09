using System;
using System.Collections.Generic;

namespace DoTs.ObjectPooling
{
    public abstract class ObjectPool<T> : IObjectPool<T> where T : class
    {
        protected int Count => _buffer.Count;
        
        protected readonly List<T> _buffer;

        public event Action<T> resetAction = delegate {  };
        
        public T Get()
        {
            T obj;
            if (Count > 0)
            {
                obj = _buffer[Count - 1];
                _buffer.RemoveAt(Count - 1);
            }
            else
            {
                obj = SpawnNewObject();
            }

            return obj;
        }

        public void Recycle(T obj)
        {
            Reset(obj);
            _buffer.Add(obj);
        }

        public void Prepare(int objectsCount)
        {
            for (int i = 0; i < objectsCount; i++)
            {
                var obj = SpawnNewObject();
                _buffer.Add(obj);
            }
        }

        protected void Reset(T obj)
        {
            resetAction.Invoke(obj);
        }

        protected abstract T SpawnNewObject();
        
        protected ObjectPool ()
        {
            _buffer = new List<T>();
        }

        protected ObjectPool(int capacity)
        {
            _buffer = new List<T>(capacity);
        }
    }
}