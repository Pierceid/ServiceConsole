namespace ServiceConsole.Classes {
    public class HeapFile<T> where T : IRecord<T>, IByteData, new() {
        public int FirstBlockAddress { get; set; } = -1;
        public int LastBlockAddress { get; set; } = -1;
        public List<Block<T>> Blocks { get; set; } = [];

        private const int RecordSize = 227;
        private const int BlockSize = 1816;

        public int InsertRecord(IByteData recordData) {
            if (this.FirstBlockAddress == -1) {
                this.FirstBlockAddress = 0;
                this.LastBlockAddress = 0;
                this.Blocks.Add(new(0, this.FirstBlockAddress, -1, -1));
            }

            T recordToInsert = new();
            recordToInsert.FromByteArray(recordData.GetByteArray());

            foreach (var block in this.Blocks) {
                if (block.Records.Count < block.MaxRecordsCount) {
                    block.Records.Add(recordToInsert);
                    block.ValidCount++;

                    return this.CalculateRecordAddress(block, block.Records[^1]);
                }
            }

            int blockAddress = this.Blocks.Count > 0 ? this.CalculateBlockAddress(this.Blocks[^1]) + BlockSize : 0;

            Block<T> blockToInsert = new(1, blockAddress, this.LastBlockAddress, -1);
            blockToInsert.Records.Add(recordToInsert);
            this.Blocks.Add(blockToInsert);
            this.LastBlockAddress = blockToInsert.Address;

            if (this.Blocks.Count > 1) this.Blocks[^2].NextBlock = blockToInsert.Address;

            return this.CalculateRecordAddress(blockToInsert, blockToInsert.Records[^1]);
        }

        public int FindRecord(int blockAddress, IByteData recordData) {
            T recordToFind = new();
            recordToFind.FromByteArray(recordData.GetByteArray());

            if (blockAddress < 0 || blockAddress > this.LastBlockAddress) return -1;

            foreach (var block in this.Blocks) {
                if (block.Address == blockAddress) {
                    foreach (var record in block.Records) {
                        if (record.EqualsByID(recordToFind)) {
                            return this.CalculateRecordAddress(block, record);
                        }
                    }
                }
            }

            return -1;
        }

        public Block<T>? FindBlock(int blockAddress) {
            if (blockAddress < 0 || blockAddress > this.LastBlockAddress) return null;

            foreach (var block in this.Blocks) {
                if (block.Address == blockAddress) {
                    return block;
                }
            }

            return null;
        }

        public int DeleteRecord(int blockAddress, IByteData recordData) {
            if (blockAddress < 0 || blockAddress > this.LastBlockAddress) return -1;

            T recordToDelete = new();
            recordToDelete.FromByteArray(recordData.GetByteArray());

            Block<T>? block = this.FindBlock(blockAddress);

            if (block == null) return -1;

            block.Records.RemoveAll(x => x.EqualsByID(recordToDelete));
            block.ValidCount = block.Records.Count;

            // If the block is now empty, remove it
            if (block.ValidCount == 0) {
                // Update links for previous and next blocks
                if (block.PreviousBlock != -1) {
                    this.Blocks[block.PreviousBlock].NextBlock = block.NextBlock;
                }

                if (block.NextBlock != -1) {
                    this.Blocks[block.NextBlock].PreviousBlock = block.PreviousBlock;
                }

                // Update heap file head/tail pointers
                if (blockAddress == this.FirstBlockAddress) {
                    this.FirstBlockAddress = block.NextBlock;
                }

                if (blockAddress == this.LastBlockAddress) {
                    this.LastBlockAddress = block.PreviousBlock;
                }

                // Remove block from the list
                this.Blocks.RemoveAt(blockAddress);

                // Adjust indices of subsequent blocks
                for (int i = 0; i < this.Blocks.Count; i++) {
                    Block<T> updatedBlock = this.Blocks[i];

                    if (updatedBlock.PreviousBlock > blockAddress) updatedBlock.PreviousBlock--;
                    if (updatedBlock.NextBlock > blockAddress) updatedBlock.NextBlock--;
                }
            }

            return blockAddress;
        }

        public void PrintFile() {
            foreach (var block in this.Blocks) {
                block.PrintData();
            }
        }

        private int CalculateBlockAddress(Block<T> block) {
            int result = 0;

            for (int i = 0; i < this.Blocks.Count; i++) {
                if (block == this.Blocks[i]) {
                    result += i * BlockSize;
                    break;
                }
            }

            return result;
        }

        private int CalculateRecordAddress(Block<T> block, T record) {
            int result = 0;

            result += this.CalculateBlockAddress(block);

            for (int i = 0; i < block.Records.Count; i++) {
                if (record.EqualsByID(block.Records[i])) {
                    result += i * RecordSize;
                    break;
                }
            }

            return result;
        }
    }
}
