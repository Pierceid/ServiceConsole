namespace ServiceConsole.Classes {
    public class Block<T> where T : IRecord<T>, IByteData {
        public int ValidCount { get; set; }
        public int Address { get; set; }
        public int PreviousBlock { get; set; }
        public int NextBlock { get; set; }
        public List<T> Records { get; set; }
        public int MaxRecordsCount { get => 8; }

        public Block() {
            this.ValidCount = 0;
            this.Address = -1;
            this.PreviousBlock = -1;
            this.NextBlock = -1;
            this.Records = [];
        }

        public Block(int validCount, int address, int previousBlock, int nextBlock) {
            this.ValidCount = validCount;
            this.Address = address;
            this.PreviousBlock = previousBlock;
            this.NextBlock = nextBlock;
            this.Records = [];
        }

        public byte[] GetByteArray() {
            List<byte> data = [];

            // Serialize ValidCount (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.ValidCount));

            // Serialize PreviousBlock (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.PreviousBlock));

            // Serialize NextBlock (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.NextBlock));

            // Serialize Records
            foreach (var record in this.Records) {
                byte[] recordData = record.GetByteArray();
                data.AddRange(recordData);
            }

            return data.ToArray();
        }

        public void FromByteArray(byte[] data) {
            if (data.Length < 12) throw new ArgumentException("Invalid byte array size.");

            // Deserialize ValidCount (4 bytes)
            this.ValidCount = BitConverter.ToInt32(data, 0);

            // Deserialize PreviousBlock (4 bytes)
            this.PreviousBlock = BitConverter.ToInt32(data, 4);

            // Deserialize NextBlock (4 bytes)
            this.NextBlock = BitConverter.ToInt32(data, 8);

            // Deserialize Records
            int offset = 12;
            this.Records = [];

            while (offset < data.Length) {
                T record = Activator.CreateInstance<T>();
                int recordSize = record.GetSize();
                byte[] recordData = new byte[recordSize];
                Array.Copy(data, offset, recordData, 0, recordSize);

                record.FromByteArray(recordData);
                this.Records.Add(record);

                offset += recordSize;
            }
        }

        public int GetSize() {
            // ValidCount (4 bytes) + PreviousBlock (4 bytes) + NextBlock (4 bytes)
            int size = 12;

            // Add size of all records
            foreach (var record in this.Records) {
                size += record.GetSize();
            }

            return size;
        }

        public void PrintData() {
            Console.WriteLine($"Block [{this.Address}]");

            foreach (var record in this.Records) {
                Console.WriteLine($"{record.GetInfo()}");
            }
        }
    }
}
