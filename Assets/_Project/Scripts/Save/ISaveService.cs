namespace Game2048.Save
{
    public interface ISaveService<T> where T : class
    {
        bool TryLoad(out T data);
        void Save(T data);
        void Delete();
    }
}
