namespace DoTs.ObjectPooling
{
    public interface IObjectPool<T> where T : class
    {
        T Get();

        void Recycle(T obj);
    }
}