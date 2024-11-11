namespace ServiceConsole {
    public class Block {
        public int ValidCount { get; set; }
        public int PreviousBlock { get; set; }
        public int NextBlock { get; set; }

        public Block(int validCount, int previousBlock, int nextBlock) {
            this.ValidCount = validCount;
            this.PreviousBlock = previousBlock;
            this.NextBlock = nextBlock;
        }


    }
}
