namespace ServiceConsole.Classes {
    public interface IRecord<T> : IByteData {
        bool EqualsByID(T other);
        T CreateInstance();
        string GetInfo();
    }
}
