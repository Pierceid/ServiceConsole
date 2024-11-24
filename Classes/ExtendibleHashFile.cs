namespace ServiceConsole.Classes {
    public class ExtendibleHashFile<T> : HeapFile<T> where T : IRecord<T>, new() {
        private List<int> Directory;
        private int GlobalDepth;
        private const int MaxDirectorySize = 1024;

        public ExtendibleHashFile(int factor, int cacheLimit, string filePath) : base(factor, cacheLimit, filePath) {
            this.GlobalDepth = 1;
            this.Directory = [-1, -1];
        }

        public override int InsertRecord(IByteData recordData) {
            T record = new();
            record.FromByteArray(recordData.GetByteArray());

            // Calculate the hash value and directory index using global depth
            int hashValue = record.GetHashCode();
            int directoryIndex = GetDirectoryIndex(hashValue);

            // Get the block address for the directory entry
            int blockAddress = this.Directory[directoryIndex];
            var block = blockAddress == -1 ? CreateNewBlock(directoryIndex) : ReadBlock(blockAddress);

            if (block == null) return -1;

            // Insert the record into the block
            block.Records.Add(record);
            block.ValidCount++;

            // Handle overflow (when block exceeds factor)
            if (block.ValidCount > block.Factor) HandleOverflow(directoryIndex, block);

            WriteBlock(block);

            base.InsertRecord(recordData);

            return block.Address;
        }

        private void HandleOverflow(int directoryIndex, Block<T> block) {
            block.ValidCount--;
            block.LocalDepth++;

            // If the block's local depth exceeds the global depth, double the directory
            if (block.LocalDepth > this.GlobalDepth) {
                DoubleDirectory();
            }

            // Create a new block and redistribute the records
            int newBlockAddress = Seek();
            Block<T> newBlock = new(newBlockAddress, block.PreviousAddress, block.NextAddress, 0, block.Factor, block.LocalDepth);

            RedistributeRecords(block, newBlock);
            UpdateDirectoryPointers(directoryIndex, block, newBlock);

            WriteBlock(block);
            WriteBlock(newBlock);
        }

        private void RedistributeRecords(Block<T> block, Block<T> newBlock) {
            List<T> allRecords = new(block.Records);
            block.Records.Clear();
            block.ValidCount = 0;

            foreach (var record in allRecords) {
                int hashValue = record.GetHashCode();
                int directoryIndex = GetDirectoryIndex(hashValue);

                // Depending on the directory index, move the record to the right block
                if (this.Directory[directoryIndex] == block.Address) {
                    block.Records.Add(record);
                    block.ValidCount++;
                } else {
                    newBlock.Records.Add(record);
                    newBlock.ValidCount++;
                }
            }
        }

        private void UpdateDirectoryPointers(int directoryIndex, Block<T> block, Block<T> newBlock) {
            int mask = (1 << block.LocalDepth) - 1;

            for (int i = 0; i < this.Directory.Count; i++) {
                if ((i & mask) == directoryIndex) {
                    this.Directory[i] = newBlock.Address;
                }
            }
        }

        private void DoubleDirectory() {
            if (this.Directory.Count * 2 > MaxDirectorySize) {
                Console.WriteLine("Warning: Directory size exceeded maximum allowable size.");
                return;
            }

            this.GlobalDepth++;
            int currentSize = this.Directory.Count;
            List<int> newDirectory = new(this.Directory);

            // Duplicate the directory entries
            for (int i = 0; i < currentSize; i++) {
                newDirectory.Add(this.Directory[i]);
            }

            this.Directory = newDirectory;

            Console.WriteLine($"Directory doubled. New global depth: {this.GlobalDepth}");
        }

        private Block<T> CreateNewBlock(int directoryIndex) {
            int newBlockAddress = Seek();
            Block<T> newBlock = new(newBlockAddress, -1, -1, 0, this.Factor, this.GlobalDepth);

            this.Directory[directoryIndex] = newBlock.Address;

            return newBlock;
        }

        public override int FindRecord(int blockAddress, IByteData recordData) {
            T recordToFind = new();
            recordToFind.FromByteArray(recordData.GetByteArray());

            int hashValue = recordToFind.GetHashCode();
            int directoryIndex = GetDirectoryIndex(hashValue);
            int address = this.Directory[directoryIndex];

            if (address == -1) return -1;

            var block = ReadBlock(address);

            if (block == null) return -1;

            foreach (var record in block.Records) {
                if (record.EqualsByID(recordToFind)) {
                    return block.Address;
                }
            }

            return -1;
        }

        public override int DeleteRecord(int blockAddress, IByteData recordData) {
            T recordToDelete = new();
            recordToDelete.FromByteArray(recordData.GetByteArray());

            int hashValue = recordToDelete.GetHashCode();
            int directoryIndex = GetDirectoryIndex(hashValue);
            int address = this.Directory[directoryIndex];

            if (address == -1) return -1;

            var block = ReadBlock(address);

            if (block == null) return -1;

            var record = block.Records.Find(r => r.EqualsByID(recordToDelete));

            if (record == null) return -1;

            block.Records.Remove(record);
            block.ValidCount--;

            WriteBlock(block);

            if (block.ValidCount == 0) {
                // Clear directory entry if the block is empty
                this.Directory[directoryIndex] = -1;
                ShrinkFile(block.Address);
            }

            return block.Address;
        }

        private int GetDirectoryIndex(int hashValue) {
            return hashValue & ((1 << this.GlobalDepth) - 1);
        }

        public override void PrintFile() {
            Console.WriteLine($"Global Depth: {this.GlobalDepth}");
            Console.WriteLine("Directory:");

            for (int i = 0; i < this.Directory.Count; i++) {
                Console.WriteLine($"Index {i:D2}: Block Address {this.Directory[i]}");
            }

            base.PrintFile();
        }
    }
}
