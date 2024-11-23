﻿namespace ServiceConsole.Classes {
    public class HeapFile<T> where T : IRecord<T>, IByteData, new() {
        private readonly int Factor;
        private readonly string FilePath;
        private readonly int BlockSize;

        public int FirstPartiallyFullBlock { get; set; } = -1;
        public int FirstFullBlock { get; set; } = -1;
        public List<int> PartiallyFullBlocks { get; set; } = new();
        public List<int> FullBlocks { get; set; } = new();

        public HeapFile(int factor, string filePath) {
            this.Factor = factor;
            this.FilePath = filePath;
            this.BlockSize = new Block<T>(this.Factor).GetSize();
        }

        public int InsertRecord(IByteData recordData) {
            T record = new();
            record.FromByteArray(recordData.GetByteArray());

            // Prioritize partially full blocks
            if (this.PartiallyFullBlocks.Count > 0) {
                var blockAddress = this.PartiallyFullBlocks[0];
                var block = ReadBlock(blockAddress);

                if (block != null) {
                    block.Records.Add(record);
                    block.ValidCount++;

                    // If the block is now full, move it to the full blocks list
                    if (block.ValidCount == this.Factor) {
                        RemoveFromPartiallyFullBlocks(block);
                        AddToFullBlocks(block);
                    }

                    WriteBlock(block);

                    return block.Address;
                }
            }

            // If no partially full block exists, create a new block
            int newBlockAddress = Seek();
            Block<T> newBlock = new(newBlockAddress, -1, this.FirstPartiallyFullBlock, 1, this.Factor);
            newBlock.Records.Add(record);

            AddToPartiallyFullBlocks(newBlock);
            WriteBlock(newBlock);

            return newBlock.Address;
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
                        return currentAddress;
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
            block.ValidCount--;

            RemoveFromFullBlocks(block);
            AddToPartiallyFullBlocks(block);

            WriteBlock(block);

            return block.Address;
        }

        public int Seek() {
            return this.PartiallyFullBlocks.Count * this.BlockSize + this.FullBlocks.Count * this.BlockSize;
        }

        public void PrintFile() {
            Console.ForegroundColor = ConsoleColor.Yellow;

            PrintBlocks(this.FirstPartiallyFullBlock);

            Console.ForegroundColor = ConsoleColor.Green;

            PrintBlocks(this.FirstFullBlock);

            Console.ResetColor();
        }

        private void PrintBlocks(int startAddress) {
            int currentAddress = startAddress;

            while (currentAddress != -1) {
                var block = ReadBlock(currentAddress);

                if (block == null) break;

                block.PrintData();
                currentAddress = block.NextAddress;
            }
        }

        public void CheckStructure() {
            int currentAddress = this.FirstPartiallyFullBlock;

            while (currentAddress != -1) {
                var block = ReadBlock(currentAddress);

                if (block == null) break;

                Console.WriteLine($"Block: #{block.Address}, Prev: #{block.PreviousAddress}, Next: #{block.NextAddress}");

                currentAddress = block.NextAddress;
            }

            currentAddress = this.FirstFullBlock;

            while (currentAddress != -1) {
                var block = ReadBlock(currentAddress);

                if (block == null) break;

                Console.WriteLine($"Block: #{block.Address}, Prev: #{block.PreviousAddress}, Next: #{block.NextAddress}");

                currentAddress = block.NextAddress;
            }
        }

        private Block<T>? ReadBlock(int address) {
            if (!File.Exists(this.FilePath)) return null;

            try {
                using var fileStream = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read);

                byte[] buffer = new byte[this.BlockSize];
                fileStream.Seek(address, SeekOrigin.Begin);

                if (fileStream.Read(buffer, 0, this.BlockSize) != this.BlockSize) return null;

                Block<T> block = new(this.Factor);
                block.FromByteArray(buffer);

                return block;
            } catch {
                return null;
            }
        }

        private void WriteBlock(Block<T> block) {
            try {
                using var fileStream = new FileStream(this.FilePath, FileMode.OpenOrCreate, FileAccess.Write);

                byte[] buffer = block.GetByteArray();
                fileStream.Seek(block.Address, SeekOrigin.Begin);
                fileStream.Write(buffer, 0, buffer.Length);
            } catch {
                throw new InvalidDataException();
            }
        }

        private void AddToPartiallyFullBlocks(Block<T> block) {
            if (this.FirstPartiallyFullBlock != -1) {
                var nextBlock = ReadBlock(this.FirstPartiallyFullBlock);
                if (nextBlock != null) {
                    nextBlock.PreviousAddress = block.Address;
                    WriteBlock(nextBlock);
                }
            }

            block.NextAddress = this.FirstPartiallyFullBlock;
            block.PreviousAddress = -1;
            this.FirstPartiallyFullBlock = block.Address;

            this.PartiallyFullBlocks.Add(block.Address);
        }

        private void RemoveFromPartiallyFullBlocks(Block<T> block) {
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

            if (this.FirstPartiallyFullBlock == block.Address) {
                this.FirstPartiallyFullBlock = block.NextAddress;
            }

            this.PartiallyFullBlocks.Remove(block.Address);
        }

        private void AddToFullBlocks(Block<T> block) {
            if (this.FirstFullBlock != -1) {
                var nextBlock = ReadBlock(this.FirstFullBlock);

                if (nextBlock != null) {
                    nextBlock.PreviousAddress = block.Address;
                    WriteBlock(nextBlock);
                }
            }

            block.NextAddress = this.FirstFullBlock;
            block.PreviousAddress = -1;

            this.FirstFullBlock = block.Address;
            this.FullBlocks.Add(block.Address);
        }

        private void RemoveFromFullBlocks(Block<T> block) {
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

            if (this.FirstFullBlock == block.Address) {
                this.FirstFullBlock = block.NextAddress;
            }

            this.FullBlocks.Remove(block.Address);
        }
    }
}
