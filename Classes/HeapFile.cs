namespace ServiceConsole.Classes {
    public class HeapFile<T> where T : IRecord<T> {
        public int FirstValidBlock { get; set; }
        public int FirstInvalidBlock { get; set; }
        public List<Block<T>> Blocks { get; set; }

        public HeapFile() {
            this.FirstValidBlock = -1;
            this.FirstInvalidBlock = -1;
            this.Blocks = [];
        }

        public int Insert(IByteData data) {
            // Try to insert into the first block with space
            foreach (var block in this.Blocks) {
                if (block.ValidCount < block.Records.Count) {
                    T newRecord = Activator.CreateInstance<T>();
                    newRecord.FromByteArray(data.GetByteArray());
                    block.Records.Add(newRecord);
                    block.ValidCount++;
                    return block.Records.Count - 1;
                }
            }

            // If no valid block is found, create a new block and insert the record
            Block<T> newBlock = new(1, this.FirstValidBlock, -1);
            T newRecordInBlock = Activator.CreateInstance<T>();
            newRecordInBlock.FromByteArray(data.GetByteArray());
            newBlock.Records.Add(newRecordInBlock);
            newBlock.ValidCount = 1;

            // Add new block to the heap file
            this.Blocks.Add(newBlock);
            this.FirstValidBlock = this.Blocks.Count - 1;

            return 0;
        }


        public int Find(int address, IByteData data) {
            foreach (var block in Blocks) {
                if (address < block.Records.Count) {
                    data.FromByteArray(block.Records[address].GetByteArray());
                    return address;
                }
            }

            return -1;
        }

        public int Delete(int address, IByteData data) {
            foreach (var block in Blocks) {
                if (address < block.Records.Count) {
                    block.Records.RemoveAt(address);
                    block.ValidCount--;
                    return address;
                }
            }

            return -1;
        }

        public void PrintFile() {
            if (this.Blocks.Count == 0) return;

            for (int i = 0; i < this.Blocks.Count; i++) {
                Console.Write($"{i}. ");
                this.Blocks[i].PrintData();
            }
        }
    }
}
