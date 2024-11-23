using System.Runtime.CompilerServices;

namespace ServiceConsole.Classes {
    public class HeapFile<T> where T : IRecord<T>, IByteData, new() {
        private readonly int Factor;
        private readonly string FilePath;
        private readonly int HeaderSize;
        private readonly int BlockSize;
        private readonly int RecordSize;
        public int FirstBlockAddress { get; set; } = -1;
        public int LastBlockAddress { get; set; } = -1;
        public int FirstPartiallyEmptyBlockAddress { get; set; } = -1;
        public int FirstEmptyBlockAddress { get; set; } = -1;

        public HeapFile(int factor, string filePath) {
            this.Factor = factor;
            this.FilePath = filePath;
            this.HeaderSize = 16;
            this.RecordSize = new T().GetSize();
            this.BlockSize = this.HeaderSize + this.Factor * this.RecordSize;
        }

        public int InsertRecord(IByteData recordData) {
            T record = new();
            record.FromByteArray(recordData.GetByteArray());

            // Prioritize partially empty blocks
            if (this.FirstPartiallyEmptyBlockAddress != -1) {
                var block = ReadBlock(this.FirstPartiallyEmptyBlockAddress);

                if (block != null) {
                    block.Records.Add(record);
                    block.ValidCount++;

                    // If block is now full, update partially empty list
                    if (block.ValidCount == this.BlockSize / this.RecordSize) {
                        RemoveFromPartiallyEmptyList(block);
                    }

                    WriteBlock(block);

                    return CalculateRecordAddress(block, record);
                }
            }

            // Use an empty block if available
            if (this.FirstEmptyBlockAddress != -1) {
                var block = ReadBlock(this.FirstEmptyBlockAddress);

                if (block != null) {
                    block.Records.Add(record);
                    block.ValidCount = 1;

                    RemoveFromEmptyList(block);
                    AddToPartiallyEmptyList(block);
                    WriteBlock(block);

                    return CalculateRecordAddress(block, record);
                }
            }

            // Create a new block
            int newBlockAddress = this.LastBlockAddress == -1 ? 0 : this.LastBlockAddress + this.BlockSize;
            Block<T> newBlock = new(newBlockAddress, this.LastBlockAddress, -1, 1);
            newBlock.Records.Add(record);

            // Update doubly linked list
            if (this.LastBlockAddress != -1) {
                var lastBlock = ReadBlock(this.LastBlockAddress);

                if (lastBlock != null) {
                    lastBlock.NextAddress = newBlock.Address;
                    WriteBlock(lastBlock);
                }
            }

            this.LastBlockAddress = newBlock.Address;

            if (this.FirstBlockAddress == -1) this.FirstBlockAddress = newBlock.Address;

            AddToPartiallyEmptyList(newBlock);
            WriteBlock(newBlock);

            return CalculateRecordAddress(newBlock, record);
        }

        public int FindRecord(int blockAddress, IByteData recordData) {
            T recordToFind = new();
            recordToFind.FromByteArray(recordData.GetByteArray());

            int currentAddress = blockAddress;

            while (currentAddress != -1) {
                var block = ReadBlock(currentAddress);

                if (block == null) break;

                foreach (var record in block.Records) {
                    if (record.EqualsByID(recordToFind)) {
                        return CalculateRecordAddress(block, record);
                    }
                }

                currentAddress = block.NextAddress;
            }

            return -1;
        }

        public int DeleteRecord(int blockAddress, IByteData recordData) {
            T recordToDelete = new();
            recordToDelete.FromByteArray(recordData.GetByteArray());

            var block = ReadBlock(blockAddress);

            if (block == null) return -1;

            var foundRecord = block.Records.Find(r => r.EqualsByID(recordToDelete));

            if (foundRecord == null) return -1;

            block.Records.Remove(foundRecord);
            block.Records.Add(recordToDelete);
            block.ValidCount--;

            if (block.ValidCount == 0) {
                AddToEmptyList(block);

                // Update links in doubly linked list
                if (block.PreviousAddress != -1) {
                    var prevBlock = ReadBlock(block.PreviousAddress);

                    if (prevBlock != null) {
                        prevBlock.NextAddress = block.NextAddress;
                        WriteBlock(prevBlock);
                    }
                }

                if (block.NextAddress != -1) {
                    var nextBlock = ReadBlock(block.NextAddress);

                    if (nextBlock != null) {
                        nextBlock.PreviousAddress = block.PreviousAddress;
                        WriteBlock(nextBlock);
                    }
                }

                if (blockAddress == FirstBlockAddress) FirstBlockAddress = block.NextAddress;

                if (blockAddress == LastBlockAddress) LastBlockAddress = block.PreviousAddress;
            } else {
                AddToPartiallyEmptyList(block);
            }

            WriteBlock(block);

            return blockAddress;
        }

        public int Seek() {
            int result = 0;

            int currentAddress = this.FirstBlockAddress;

            while (currentAddress != -1) {
                var block = ReadBlock(currentAddress);

                if (block == null) break;

                result += this.BlockSize;

                currentAddress = block.NextAddress;
            }

            return result - this.BlockSize;
        }

        public void PrintFile() {
            int currentAddress = this.FirstBlockAddress;

            while (currentAddress != -1) {
                var block = ReadBlock(currentAddress);

                if (block == null) break;

                Console.ForegroundColor = block.Records.Count == this.Factor ? ConsoleColor.Green : ConsoleColor.Red;

                block.PrintData();

                currentAddress = block.NextAddress;
            }

            Console.ResetColor();
        }

        public void CheckStructure() {
            int currentAddress = this.FirstBlockAddress;

            while (currentAddress != -1) {
                var block = ReadBlock(currentAddress);

                if (block == null) break;

                Console.WriteLine($"Block: #{block.Address}, Prev: #{block.PreviousAddress}, Next: #{block.NextAddress}");

                currentAddress = block.NextAddress;
            }
        }

        private Block<T>? ReadBlock(int address) {
            using var fileStream = new FileStream(this.FilePath, FileMode.OpenOrCreate, FileAccess.Read);
            byte[] buffer = new byte[this.BlockSize];

            fileStream.Seek(address, SeekOrigin.Begin);
            int bytesRead = fileStream.Read(buffer, 0, this.BlockSize);

            if (bytesRead < this.BlockSize) return null;

            Block<T> block = new();
            block.FromByteArray(buffer);

            return block;
        }

        private void WriteBlock(Block<T> block) {
            using var fileStream = new FileStream(this.FilePath, FileMode.OpenOrCreate, FileAccess.Write);
            byte[] buffer = block.GetByteArray();

            fileStream.Seek(block.Address, SeekOrigin.Begin);
            fileStream.Write(buffer, 0, buffer.Length);
        }

        private void AddToPartiallyEmptyList(Block<T> block) {
            block.NextAddress = this.FirstPartiallyEmptyBlockAddress;

            if (this.FirstPartiallyEmptyBlockAddress != -1) {
                var nextBlock = ReadBlock(this.FirstPartiallyEmptyBlockAddress);

                if (nextBlock != null) nextBlock.PreviousAddress = block.Address;
            }

            block.PreviousAddress = -1;
            this.FirstPartiallyEmptyBlockAddress = block.Address;
        }

        private void RemoveFromPartiallyEmptyList(Block<T> block) {
            if (block.PreviousAddress != -1) {
                var prevBlock = ReadBlock(block.PreviousAddress);

                if (prevBlock != null) prevBlock.NextAddress = block.NextAddress;
            }

            if (block.NextAddress != -1) {
                var nextBlock = ReadBlock(block.NextAddress);

                if (nextBlock != null) nextBlock.PreviousAddress = block.PreviousAddress;
            }

            if (this.FirstPartiallyEmptyBlockAddress == block.Address) {
                this.FirstPartiallyEmptyBlockAddress = block.NextAddress;
            }
        }

        private void AddToEmptyList(Block<T> block) {
            block.NextAddress = this.FirstEmptyBlockAddress;
            if (this.FirstEmptyBlockAddress != -1) {
                var nextBlock = ReadBlock(this.FirstEmptyBlockAddress);
                if (nextBlock != null) nextBlock.PreviousAddress = block.Address;
            }

            block.PreviousAddress = -1;
            this.FirstEmptyBlockAddress = block.Address;
        }

        private void RemoveFromEmptyList(Block<T> block) {
            if (block.PreviousAddress != -1) {
                var prevBlock = ReadBlock(block.PreviousAddress);

                if (prevBlock != null) prevBlock.NextAddress = block.NextAddress;
            }

            if (block.NextAddress != -1) {
                var nextBlock = ReadBlock(block.NextAddress);

                if (nextBlock != null) nextBlock.PreviousAddress = block.PreviousAddress;
            }

            if (this.FirstEmptyBlockAddress == block.Address) {
                this.FirstEmptyBlockAddress = block.NextAddress;
            }
        }

        private int CalculateRecordAddress(Block<T> block, T record) {
            int blockAddress = block.Address + this.HeaderSize;
            int recordIndex = block.Records.IndexOf(record);

            return blockAddress + recordIndex * record.GetSize();
        }
    }
}
