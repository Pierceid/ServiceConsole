using System.Text;

namespace ServiceConsole.Classes {
    public class Customer : Item<Customer> {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string ECV { get; set; }
        public List<CarService> Services { get; set; }

        private const int MaxNameLength = 15;
        private const int MaxSurnameLength = 20;
        private const int MaxECVLength = 10;
        private const int MaxServicesCount = 5;
        private const int ServiceSize = 40;

        public Customer() {
            this.Name = string.Empty.PadRight(MaxNameLength);
            this.Surname = string.Empty.PadRight(MaxSurnameLength);
            this.ECV = string.Empty.PadRight(MaxECVLength);
            this.Services = [];
        }

        public Customer(string name, string surname, string ecv) {
            this.Name = name.Length > MaxNameLength ? name[..MaxNameLength] : name.PadRight(MaxNameLength);
            this.Surname = surname.Length > MaxSurnameLength ? surname[..MaxSurnameLength] : surname.PadRight(MaxSurnameLength);
            this.ECV = ecv.Length > MaxECVLength ? ecv[..MaxECVLength] : ecv.PadRight(MaxECVLength);
            this.Services = [];
        }

        public override Customer CreateInstance() {
            return new Customer();
        }

        public override bool EqualsByID(Customer other) {
            return this.Id == other.Id;
        }

        public override void FromByteArray(byte[] data) {
            if (data.Length > this.GetSize()) throw new ArgumentException("Invalid byte array size.");

            // Deserialize ID (4 bytes)
            this.Id = BitConverter.ToInt32(data, 0);

            // Deserialize Name (15 bytes, padded)
            this.Name = Encoding.ASCII.GetString(data, 4, MaxNameLength).TrimEnd();

            // Deserialize Surname (20 bytes, padded)
            this.Surname = Encoding.ASCII.GetString(data, 4 + MaxNameLength, MaxSurnameLength).TrimEnd();

            // Deserialize ECV (10 bytes, padded)
            this.ECV = Encoding.ASCII.GetString(data, 4 + MaxNameLength + MaxSurnameLength, MaxECVLength).TrimEnd();

            // Deserialize Services (1 + ServiceSize * number of services bytes)
            int offset = 4 + MaxNameLength + MaxSurnameLength + MaxECVLength;
            byte serviceCount = data[offset];
            this.Services = [];

            for (int i = 0; i < serviceCount; i++) {
                if (offset + 1 + i * ServiceSize >= data.Length) break;

                byte[] serviceData = new byte[ServiceSize];
                Array.Copy(data, offset + 1 + i * ServiceSize, serviceData, 0, ServiceSize);

                CarService service = new();
                service.FromByteArray(serviceData);
                this.Services.Add(service);
            }
        }

        public override byte[] GetByteArray() {
            byte[] data = new byte[this.GetSize()];

            // Serialize ID (4 bytes)
            BitConverter.GetBytes(this.Id).CopyTo(data, 0);

            // Serialize Name (15 bytes, padded)
            byte[] nameBytes = Encoding.ASCII.GetBytes(this.Name.PadRight(MaxNameLength));
            Array.Copy(nameBytes, 0, data, 4, MaxNameLength);

            // Serialize Surname (20 bytes, padded)
            byte[] surnameBytes = Encoding.ASCII.GetBytes(this.Surname.PadRight(MaxSurnameLength));
            Array.Copy(surnameBytes, 0, data, 4 + MaxNameLength, MaxSurnameLength);

            // Serialize ECV (10 bytes, padded)
            byte[] ecvBytes = Encoding.ASCII.GetBytes(this.ECV.PadRight(MaxECVLength));
            Array.Copy(ecvBytes, 0, data, 4 + MaxNameLength + MaxSurnameLength, MaxECVLength);

            // Serialize Services (1 byte for service count + ServiceSize * number of services bytes)
            int offset = 4 + MaxNameLength + MaxSurnameLength + MaxECVLength;
            byte serviceCount = (byte)this.Services.Count;
            data[offset] = serviceCount;

            for (int i = 0; i < serviceCount; i++) {
                byte[] serviceData = this.Services[i].GetByteArray();
                Array.Copy(serviceData, 0, data, offset + 1 + i * ServiceSize, ServiceSize);
            }

            return data;
        }

        public override int GetSize() {
            // ID (4) + Name (15) + Surname (20) + ECV (10) + ServicesCount (1) + Services (5 * 40)
            return 4 + MaxNameLength + MaxSurnameLength + MaxECVLength + 1 + MaxServicesCount * ServiceSize;
        }

        public override string GetInfo() {
            string result = $"Customer - {this.Name.TrimEnd()} {this.Surname.TrimEnd()} (ECV: {this.ECV.TrimEnd()}):";

            foreach (var service in this.Services) {
                result += $"\n{service.GetInfo()}";
            }

            return result;
        }
    }
}
