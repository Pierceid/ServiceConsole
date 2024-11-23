namespace ServiceConsole.Classes {
    public class Block<T> where T : IRecord<T>, IByteData {
        public int Address { get; set; }
        public int PreviousAddress { get; set; }
        public int NextAddress { get; set; }
        public int ValidCount { get; set; }
        public List<T> Records { get; set; }

        public Block() {
            this.Address = 0;
            this.PreviousAddress = 0;
            this.NextAddress = 0;
            this.ValidCount = 0;
            this.Records = [];
        }

        public Block(int address, int previousBlock, int nextBlock, int validCount) {
            this.Address = address;
            this.PreviousAddress = previousBlock;
            this.NextAddress = nextBlock;
            this.ValidCount = validCount;
            this.Records = [];
        }

        public byte[] GetByteArray() {
            List<byte> data = [];

            // Serialize Address (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.Address));

            // Serialize PreviousBlock (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.PreviousAddress));

            // Serialize NextBlock (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.NextAddress));

            // Serialize ValidCount (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.ValidCount));

            // Serialize Records
            foreach (var record in this.Records) {
                byte[] recordData = record.GetByteArray();
                data.AddRange(recordData);
            }

            return data.ToArray();
        }

        public void FromByteArray(byte[] data) {
            if (data.Length < 16) throw new ArgumentException("Invalid byte array size.");

            // Deserialize Address (4 bytes)
            this.Address = BitConverter.ToInt32(data, 0);

            // Deserialize PreviousBlock (4 bytes)
            this.PreviousAddress = BitConverter.ToInt32(data, 4);

            // Deserialize NextBlock (4 bytes)
            this.NextAddress = BitConverter.ToInt32(data, 8);

            // Deserialize ValidCount (4 bytes)
            this.ValidCount = BitConverter.ToInt32(data, 12);

            // Deserialize Records
            int offset = 16;
            this.Records = [];

            for (int i = 0; i < this.ValidCount; i++) {
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
            // Address (4 bytes) + PreviousAddress (4 bytes) + NextAddress (4 bytes) + ValidCount (4 bytes)
            int size = 16;

            // Add size of all records
            foreach (var record in this.Records) {
                size += record.GetSize();
            }

            return size;
        }

        public void PrintData() {
            Console.WriteLine($"Block [#{this.Address}]");

            for (int i = 0; i < Math.Min(this.ValidCount, this.Records.Count); i++) {
                Console.WriteLine($"{this.Records[i].GetInfo()}");
            }
        }
    }
}
