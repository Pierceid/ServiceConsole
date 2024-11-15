using System.Text;

namespace ServiceConsole.Classes {
    public class Service : Item<Service> {
        public string Date { get; set; }
        public double Cost { get; set; }
        public string Description { get; set; }

        private const int MaxDateLength = 6;
        private const int MaxDescriptionLength = 20;

        public Service() {
            this.Date = string.Empty;
            this.Cost = 0.0;
            this.Description = string.Empty;
        }

        public Service(string date, double cost, string description) {
            this.Date = date.Length > MaxDateLength ? date[..MaxDateLength] : date;
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

            // Deserialize date (1 + MaxDateLength bytes)
            byte dateLength = data[4];
            this.Date = Encoding.ASCII.GetString(data, 5, dateLength);

            // Deserialize cost (8 bytes)
            this.Cost = BitConverter.ToDouble(data, 5 + MaxDateLength);

            // Deserialize description (1 + MaxDescriptionLength bytes)
            byte descriptionLength = data[13 + MaxDateLength];
            this.Description = Encoding.ASCII.GetString(data, 14 + MaxDateLength, descriptionLength);
        }

        public override byte[] GetByteArray() {
            byte[] data = new byte[this.GetSize()];

            // Serialize ID (4 bytes)
            BitConverter.GetBytes(this.Id).CopyTo(data, 0);

            // Serialize date (1 + MaxDateLength bytes)
            byte[] dateBytes = Encoding.ASCII.GetBytes(this.Date);
            data[4] = (byte)dateBytes.Length;
            Array.Copy(dateBytes, 0, data, 5, dateBytes.Length);

            // Serialize cost (8 bytes)
            BitConverter.GetBytes(this.Cost).CopyTo(data, 5 + MaxDateLength);

            // Serialize description (1 + MaxDescriptionLength bytes)
            byte[] descriptionBytes = Encoding.ASCII.GetBytes(this.Description);
            data[13 + MaxDateLength] = (byte)descriptionBytes.Length;
            Array.Copy(descriptionBytes, 0, data, 14 + MaxDateLength, descriptionBytes.Length);

            return data;
        }

        public override int GetSize() {
            // ID (4) + DateLength (1) + Date (6) + Cost (8) + DescriptionLength (1) + Description (20)
            return 4 + 1 + MaxDateLength + 8 + 1 + MaxDescriptionLength;
        }

        public override string GetInfo() {
            return $"Service - Date: {this.ConvertToDate(this.Date)}, Cost: {this.Cost.ToString().Replace(',','.')}, Desc: {this.Description}";
        }

        private string ConvertToDate(string date) {
            string result = string.Empty;

            if (date.Length != 6) return result;

            string day = date[..2];
            string month = date[2..4];
            string year = date[4..6];

            bool isInCurrentCentury = new DateTime(2000 + int.Parse(year), int.Parse(month), int.Parse(day)) <= DateTime.Now;
            int yearInt = int.Parse(year);
            year = (isInCurrentCentury ? 2000 + yearInt : 1900 + yearInt).ToString();

            return $"{day}.{month}.{year}";
        }
    }
}
