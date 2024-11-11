namespace ServiceConsole {
    public class HeapFile : IData {
        public int FirstValidBlock { get; set; }
        public int FirstInValidBlock { get; set; }
        public List<IRecord> Records { get; set; }

        public HeapFile() {
            this.FirstValidBlock = -1;
            this.FirstInValidBlock = -1;
            this.Records = [];
        }

        public int Insert() {
            throw new NotImplementedException();
        }

        public int Find(int address, IData data) {
            throw new NotImplementedException();
        }

        public int Delete(int address, IData data) {
            throw new NotImplementedException();
        }

        public void PrintFile() {
            if (this.Records.Count == 0) return;

            // TODO update later
            this.Records.ForEach(x => Console.WriteLine(x.ToString()));
        }
    }
}
