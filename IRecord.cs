namespace ServiceConsole {
    public interface IRecord {
        int GetSize();
        bool Equals(IRecord other);
        byte[] GetByteArray();
        IRecord FromByteArray(byte[] byteArray);
    }
}
