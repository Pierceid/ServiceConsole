namespace ServiceConsole.Classes {
    public abstract class Item<T> : IRecord<T> {
        private int id = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);

        public abstract T CreateInstance();

        public abstract bool EqualsByID(T other);

        public abstract void FromByteArray(byte[] data);

        public abstract byte[] GetByteArray();

        public abstract int GetSize();

        public abstract string GetInfo();

        public int Id { get => id; set => id = value; }
    }
}
