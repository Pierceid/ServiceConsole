using System.Text;

namespace ServiceConsole.Classes {
    public class CarService : Item<CarService> {
        public DateTime Date { get; set; }
        public double Cost { get; set; }
        public string[] Descriptions { get; set; }

        private const int MaxDescriptionCount = 10;
        private const int MaxDescriptionLength = 20;

        public CarService() {
            this.Date = DateTime.MinValue;
            this.Cost = 0.0;
            this.Descriptions = Enumerable.Repeat(string.Empty, MaxDescriptionCount).ToArray();
        }

        public CarService(DateTime date, double cost, string[] descriptions) {
            this.Date = date;
            this.Cost = cost;

            this.Descriptions = new string[MaxDescriptionCount];
            for (int i = 0; i < MaxDescriptionCount; i++) {
                this.Descriptions[i] = i < descriptions.Length
                    ? descriptions[i].Length > MaxDescriptionLength
                        ? descriptions[i][..MaxDescriptionLength]
                        : descriptions[i].PadRight(MaxDescriptionLength)
                    : string.Empty.PadRight(MaxDescriptionLength);
            }
        }

        public override CarService CreateInstance() {
            return new CarService();
        }

        public override bool EqualsByID(CarService other) {
            return this.Id == other.Id;
        }

        public override void FromByteArray(byte[] data) {
            if (data.Length > this.GetSize()) throw new ArgumentException("Invalid byte array size.");

            // Deserialize ID (4 bytes)
            this.Id = BitConverter.ToInt32(data, 0);

            // Deserialize Date (8 bytes)
            long binaryDate = BitConverter.ToInt64(data, 4);
            this.Date = DateTime.FromBinary(binaryDate);

            // Deserialize Cost (8 bytes)
            this.Cost = BitConverter.ToDouble(data, 12);

            // Deserialize Descriptions (10 * MaxDescriptionLength bytes)
            this.Descriptions = new string[MaxDescriptionCount];
            for (int i = 0; i < MaxDescriptionCount; i++) {
                int offset = 20 + i * MaxDescriptionLength;

                if (offset >= data.Length) break;

                string description = Encoding.ASCII.GetString(data, offset, MaxDescriptionLength);
                this.Descriptions[i] = description.TrimEnd();
            }
        }

        public override byte[] GetByteArray() {
            byte[] data = new byte[this.GetSize()];

            // Serialize ID (4 bytes)
            BitConverter.GetBytes(this.Id).CopyTo(data, 0);

            // Serialize Date (8 bytes)
            BitConverter.GetBytes(this.Date.ToBinary()).CopyTo(data, 4);

            // Serialize Cost (8 bytes)
            BitConverter.GetBytes(this.Cost).CopyTo(data, 12);

            // Serialize Descriptions (10 * MaxDescriptionLength bytes)
            for (int i = 0; i < MaxDescriptionCount; i++) {
                byte[] descriptionBytes = Encoding.ASCII.GetBytes((this.Descriptions[i] ?? string.Empty).PadRight(MaxDescriptionLength));
                Array.Copy(descriptionBytes, 0, data, 20 + i * MaxDescriptionLength, MaxDescriptionLength);
            }

            return data;
        }

        public override int GetSize() {
            // ID (4) + Date (8) + Cost (8) + Descriptions (10 * 20)
            return 4 + 8 + 8 + MaxDescriptionCount * MaxDescriptionLength;
        }

        public override string GetInfo() {
            var descriptions = string.Join(", ", this.Descriptions.Where(d => !string.IsNullOrWhiteSpace(d)));
            return $"Service - Date: {this.Date:dd.MM.yyyy}, Cost: {this.Cost.ToString().Replace(',', '.')}, Descriptions: {descriptions}";
        }
    }
}
