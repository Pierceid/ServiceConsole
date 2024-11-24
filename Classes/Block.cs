namespace ServiceConsole.Classes {
    public class Block<T> where T : IRecord<T>, IByteData {
        public int Address { get; set; }
        public int PreviousAddress { get; set; }
        public int NextAddress { get; set; }
        public int ValidCount { get; set; }
        public int Factor { get; set; }
        public int LocalDepth { get; set; }
        public List<T> Records { get; set; }

        public Block(int factor) {
            if (factor <= 0) throw new ArgumentException("Factor must be greater than 0.");

            this.Address = 0;
            this.PreviousAddress = -1;
            this.NextAddress = -1;
            this.ValidCount = 0;
            this.Factor = factor;
            this.LocalDepth = 1;
            this.Records = [];
        }

        public Block(int address, int previousBlock, int nextBlock, int validCount, int factor, int localDepth) {
            if (factor <= 0) throw new ArgumentException("Factor must be greater than 0.");

            if (validCount < 0 || validCount > factor) throw new ArgumentException($"ValidCount must be between 0 and Factor ({factor}).");

            this.Address = address;
            this.PreviousAddress = previousBlock;
            this.NextAddress = nextBlock;
            this.ValidCount = validCount;
            this.Factor = factor;
            this.LocalDepth = localDepth;
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

            // Serialize Factor (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.Factor));

            // Serialize LocalDepth (4 bytes)
            data.AddRange(BitConverter.GetBytes(this.LocalDepth));

            // Serialize Records
            foreach (var record in this.Records) {
                byte[] recordData = record.GetByteArray();

                if (recordData.Length != Activator.CreateInstance<T>().GetSize()) throw new InvalidOperationException("Record size mismatch.");

                data.AddRange(recordData);
            }

            // Pad with empty records if less than Factor
            int emptyRecords = this.Factor - this.Records.Count;
            if (emptyRecords > 0) {
                byte[] emptyRecord = new byte[Activator.CreateInstance<T>().GetSize()];

                for (int i = 0; i < emptyRecords; i++) {
                    data.AddRange(emptyRecord);
                }
            }

            return [.. data];
        }

        public void FromByteArray(byte[] data) {
            if (data.Length < 24) throw new ArgumentException("Byte array too small to represent a block.");

            // Deserialize Address (4 bytes)
            this.Address = BitConverter.ToInt32(data, 0);

            // Deserialize PreviousBlock (4 bytes)
            this.PreviousAddress = BitConverter.ToInt32(data, 4);

            // Deserialize NextBlock (4 bytes)
            this.NextAddress = BitConverter.ToInt32(data, 8);

            // Deserialize ValidCount (4 bytes)
            this.ValidCount = BitConverter.ToInt32(data, 12);

            // Deserialize Factor (4 bytes)
            this.Factor = BitConverter.ToInt32(data, 16);

            // Deserialize LocalDepth (4 bytes)
            this.LocalDepth = BitConverter.ToInt32(data, 20);

            if (this.ValidCount < 0 || this.ValidCount > this.Factor) throw new ArgumentException("ValidCount out of range.");

            // Deserialize Records
            this.Records = [];
            int offset = 20;
            int recordSize = Activator.CreateInstance<T>().GetSize();

            for (int i = 0; i < this.Factor; i++) {
                if (offset + recordSize > data.Length) break;

                byte[] recordData = new byte[recordSize];
                Array.Copy(data, offset, recordData, 0, recordSize);

                T record = Activator.CreateInstance<T>();
                record.FromByteArray(recordData);

                if (i < this.ValidCount) {
                    this.Records.Add(record);
                }

                offset += recordSize;
            }
        }

        public int GetSize() {
            // Address (4) + PreviousAddress (4) + NextAddress (4) + ValidCount (4) + Factor (4) + LocalDepth (4) + Factor * RecordSize
            return 24 + this.Factor * Activator.CreateInstance<T>().GetSize();
        }

        public void PrintData() {
            Console.WriteLine($"Block [Address: {this.Address}, Prev: {this.PreviousAddress}, Next: {this.NextAddress}, Valid: {this.ValidCount}/{this.Factor}]");

            for (int i = 0; i < this.ValidCount; i++) {
                Console.WriteLine($"  Record #{i + 1}: {this.Records[i].GetInfo()}");
            }

            Console.WriteLine();
        }
    }
}
