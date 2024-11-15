namespace ServiceConsole.Classes {
    public class HeapFile<T> where T : IRecord<T>, IByteData, new() {
        public int FirstBlockAddress { get; set; } = -1;
        public int LastBlockAddress { get; set; } = -1;
        public List<Block<T>> Blocks { get; set; } = [];

        private const int RecordSize = 242;
        private const int BlockSize = 1936;

        public int InsertRecord(IByteData recordData) {
            if (this.FirstBlockAddress == -1) {
                this.FirstBlockAddress = 0;
                this.LastBlockAddress = 0;
                this.Blocks.Add(new(0, this.FirstBlockAddress, -1, -1));
            }

            T recordToInsert = new();
            recordToInsert.FromByteArray(recordData.GetByteArray());

            int currentAddress = this.FirstBlockAddress;

            // Try to find a suitable block for inserting the new record
            while (currentAddress != -1) {
                var block = this.FindBlock(currentAddress);

                if (block == null) break;

                if (block.Records.Count < block.MaxRecordsCount) {
                    block.Records.Add(recordToInsert);
                    block.ValidCount++;

                    return HeapFile<T>.CalculateRecordAddress(block, recordToInsert);
                }

                currentAddress = block.NextBlockAddress;
            }

            // If no suitable block is found, create a new block
            int newBlockAddress = this.Blocks.Count > 0 ? this.LastBlockAddress + BlockSize : 0;

            Block<T> newBlock = new(1, newBlockAddress, this.LastBlockAddress, -1);
            newBlock.Records.Add(recordToInsert);
            this.Blocks.Add(newBlock);

            if (this.LastBlockAddress != -1) {
                var lastBlock = this.FindBlock(this.LastBlockAddress);

                if (lastBlock != null) lastBlock.NextBlockAddress = newBlock.Address;
            }

            this.LastBlockAddress = newBlock.Address;

            return HeapFile<T>.CalculateRecordAddress(newBlock, recordToInsert);
        }

        public int FindRecord(int blockAddress, IByteData recordData) {
            T recordToFind = new();
            recordToFind.FromByteArray(recordData.GetByteArray());

            int currentAddress = blockAddress;

            while (currentAddress != -1) {
                var block = this.FindBlock(currentAddress);

                if (block == null) break;

                foreach (var record in block.Records) {
                    if (record.EqualsByID(recordToFind)) {
                        return HeapFile<T>.CalculateRecordAddress(block, record);
                    }
                }

                currentAddress = block.NextBlockAddress;
            }

            return -1;
        }

        public int DeleteRecord(int blockAddress, IByteData recordData) {
            T recordToDelete = new();
            recordToDelete.FromByteArray(recordData.GetByteArray());

            int currentAddress = this.FirstBlockAddress;

            while (currentAddress != -1) {
                var block = this.FindBlock(currentAddress);

                if (block == null) break;

                if (block.Address == blockAddress) {
                    block.Records.RemoveAll(record => record.EqualsByID(recordToDelete));
                    block.ValidCount = block.Records.Count;

                    // If the block is empty, handle removal and links
                    if (block.ValidCount == 0) {
                        if (block.PreviousBlockAddress != -1) {
                            var prevBlock = this.FindBlock(block.PreviousBlockAddress);

                            if (prevBlock != null) prevBlock.NextBlockAddress = block.NextBlockAddress;
                        }

                        if (block.NextBlockAddress != -1) {
                            var nextBlock = this.FindBlock(block.NextBlockAddress);

                            if (nextBlock != null) nextBlock.PreviousBlockAddress = block.PreviousBlockAddress;
                        }

                        if (block.Address == this.FirstBlockAddress) this.FirstBlockAddress = block.NextBlockAddress;

                        if (block.Address == this.LastBlockAddress) this.LastBlockAddress = block.PreviousBlockAddress;

                        this.Blocks.Remove(block);
                    }

                    return blockAddress;
                }

                currentAddress = block.NextBlockAddress;
            }

            return -1;
        }

        public int Seek() {
            return (this.Blocks.Count - 1) * BlockSize;
        }

        public void PrintFile() {
            if (this.Blocks.Count == 0) return;

            int currentAddress = this.FirstBlockAddress;

            while (currentAddress != -1) {
                var block = this.FindBlock(currentAddress);

                if (block == null) break;

                block.PrintData();

                currentAddress = block.NextBlockAddress;
            }
        }

        private Block<T>? FindBlock(int blockAddress) {
            return this.Blocks.FirstOrDefault(block => block.Address == blockAddress);
        }

        private static int CalculateRecordAddress(Block<T> block, T record) {
            int blockAddress = block.Address;
            int recordAddress = block.Records.IndexOf(record) * RecordSize;

            return blockAddress + recordAddress;
        }
    }
}
