namespace ServiceConsole.Classes {
    public class HeapFile<T> where T : IRecord<T>, IByteData, new() {
        public int FirstBlockIndex { get; set; } = -1;
        public int LastBlockIndex { get; set; } = -1;
        public List<Block<T>> Blocks { get; set; } = [];

        public int Insert(IByteData data) {
            T newRecord = new();
            newRecord.FromByteArray(data.GetByteArray());

            // Try to insert into an existing block with free space
            foreach (var block in this.Blocks) {
                if (block.ValidCount < block.Records.Count) {
                    block.Records.Add(newRecord);
                    block.ValidCount++;

                    return block.Records.Count - 1;
                }
            }

            // Create a new block and add it to the list
            Block<T> newBlock = new(1, this.LastBlockIndex, -1);
            newBlock.Records.Add(newRecord);

            if (this.LastBlockIndex != -1) {
                this.Blocks[this.LastBlockIndex].NextBlock = this.Blocks.Count;
            }

            this.Blocks.Add(newBlock);

            if (this.FirstBlockIndex == -1) {
                this.FirstBlockIndex = 0;
            }

            this.LastBlockIndex = this.Blocks.Count - 1;

            return this.LastBlockIndex;
        }

        public int Find(int blockIndex, int recordIndex) {
            if (blockIndex < 0 || blockIndex >= this.Blocks.Count) {
                return -1;
            }

            Block<T> block = this.Blocks[blockIndex];

            if (recordIndex < 0 || recordIndex >= block.Records.Count) {
                return -1;
            }

            int address = 0;

            for (int i = 0; i < blockIndex; i++) {
                address += this.Blocks[i].Records.Count;
            }

            address += recordIndex;

            return address;
        }

        public int Delete(int blockIndex, int recordIndex) {
            if (blockIndex < 0 || blockIndex >= this.Blocks.Count) return -1;

            Block<T> block = this.Blocks[blockIndex];

            if (recordIndex < 0 || recordIndex >= block.Records.Count) return -1;

            // Mark the record as invalid
            block.Records[recordIndex] = default!;
            block.ValidCount--;

            if (block.ValidCount == 0) {
                // Remove block if empty
                if (block.PreviousBlock != -1) {
                    this.Blocks[block.PreviousBlock].NextBlock = block.NextBlock;
                }

                if (block.NextBlock != -1) {
                    this.Blocks[block.NextBlock].PreviousBlock = block.PreviousBlock;
                }

                if (blockIndex == this.FirstBlockIndex) {
                    this.FirstBlockIndex = block.NextBlock;
                }

                if (blockIndex == this.LastBlockIndex) {
                    this.LastBlockIndex = block.PreviousBlock;
                }
            }

            return blockIndex;
        }

        public void PrintFile() {
            int currentBlockIndex = this.FirstBlockIndex;

            while (currentBlockIndex != -1) {
                Console.WriteLine($"Block {currentBlockIndex}:");
                this.Blocks[currentBlockIndex].PrintData();
                currentBlockIndex = this.Blocks[currentBlockIndex].NextBlock;
            }
        }
    }
}
