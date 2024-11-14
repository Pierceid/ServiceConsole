namespace ServiceConsole.Classes {
    public interface IByteData {
        byte[] GetByteArray();
        void FromByteArray(byte[] data);
        int GetSize();
    }
}
