using System.Text;

namespace ServiceConsole.Classes {
    public class Service : Item<Service> {
        public int Date { get; set; }
        public double Cost { get; set; }
        public string Description { get; set; }

        private const int MaxDescriptionLength = 20;

        public Service() {
            this.Date = 0;
            this.Cost = 0.0;
            this.Description = string.Empty;
        }

        public Service(int date, double cost, string description) {
            this.Date = date;
            this.Cost = cost;
            this.Description = description.Length > MaxDescriptionLength ? description[..MaxDescriptionLength] : description;
        }

        public override Service CreateInstance() {
            return new Service();
        }

        public override bool EqualsByID(Service other) {
            return this.Id == other.Id;
        }

        public override void FromByteArray(byte[] data) {
            if (data.Length < this.GetSize()) throw new ArgumentException("Invalid byte array size.");

            // Deserialize ID (4 bytes)
            this.Id = BitConverter.ToInt32(data, 0);

            // Deserialize date (4 bytes)
            this.Date = BitConverter.ToInt32(data, 4);

            // Deserialize cost (8 bytes)
            this.Cost = BitConverter.ToDouble(data, 8);

            // Deserialize description (1 + 20 bytes)
            byte descLength = data[16];
            this.Description = Encoding.ASCII.GetString(data, 17, descLength);
        }

        public override byte[] GetByteArray() {
            byte[] data = new byte[this.GetSize()];

            // Serialize ID (4 bytes)
            BitConverter.GetBytes(this.Id).CopyTo(data, 0);

            // Serialize date (4 bytes)
            BitConverter.GetBytes(this.Date).CopyTo(data, 4);

            // Serialize cost (8 bytes)
            BitConverter.GetBytes(this.Cost).CopyTo(data, 8);

            // Serialize description (1 + 20 bytes)
            byte[] descriptionBytes = Encoding.ASCII.GetBytes(this.Description);
            data[16] = (byte)descriptionBytes.Length;
            Array.Copy(descriptionBytes, 0, data, 17, descriptionBytes.Length);

            return data;
        }

        public override int GetSize() {
            // id + date + cost + descLength + description
            return 4 + 4 + 8 + 1 + MaxDescriptionLength;
        }

        public override string GetInfo() {
            return $"Service - Date: {this.Date}, Cost: {this.Cost}, Desc: {this.Description}";
        }
    }
}
