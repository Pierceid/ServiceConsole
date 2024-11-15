using System.Text;

namespace ServiceConsole.Classes {
    public class Customer : Item<Customer> {
        public string Name { get; set; }
        public string Surname { get; set; }
        public List<Service> Services { get; set; }

        private const int MaxNameLength = 15;
        private const int MaxSurnameLength = 20;
        private const int MaxServicesCount = 5;
        private const int ServiceSize = 40;

        public Customer() {
            this.Name = string.Empty;
            this.Surname = string.Empty;
            this.Services = [];
        }

        public Customer(string name, string surname) {
            this.Name = name.Length > MaxNameLength ? name[..MaxNameLength] : name;
            this.Surname = surname.Length > MaxSurnameLength ? surname[..MaxSurnameLength] : surname;
            this.Services = [];
        }

        public override Customer CreateInstance() {
            return new Customer();
        }

        public override bool EqualsByID(Customer other) {
            return this.Id == other.Id;
        }

        public override void FromByteArray(byte[] data) {
            if (data.Length < this.GetSize()) throw new ArgumentException("Invalid byte array size.");

            // Deserialize ID (4 bytes)
            this.Id = BitConverter.ToInt32(data, 0);

            // Deserialize name (1 + 15 bytes)
            byte nameLength = data[4];
            this.Name = Encoding.ASCII.GetString(data, 5, nameLength);

            // Deserialize Surname (1 + 20 bytes)
            byte SurnameLength = data[5 + nameLength];
            this.Surname = Encoding.ASCII.GetString(data, 6 + nameLength, SurnameLength);

            // Deserialize services (1 + 37 * servicesCount bytes)
            byte serviceCount = data[6 + nameLength + SurnameLength];
            this.Services = [];

            for (int i = 0; i < serviceCount; i++) {
                byte[] serviceData = new byte[ServiceSize];
                Array.Copy(data, 7 + nameLength + SurnameLength + i * ServiceSize, serviceData, 0, ServiceSize);

                Service service = new();
                service.FromByteArray(serviceData);

                this.Services.Add(service);
            }
        }

        public override byte[] GetByteArray() {
            byte[] data = new byte[this.GetSize()];

            // Serialize ID (4 bytes)
            BitConverter.GetBytes(this.Id).CopyTo(data, 0);

            // Serialize name (1 + 15 bytes)
            byte[] nameBytes = Encoding.ASCII.GetBytes(this.Name);
            data[4] = (byte)nameBytes.Length;
            Array.Copy(nameBytes, 0, data, 5, nameBytes.Length);

            // Serialize Surname (1 + 20 bytes)
            byte[] SurnameBytes = Encoding.ASCII.GetBytes(this.Surname);
            data[5 + nameBytes.Length] = (byte)SurnameBytes.Length;
            Array.Copy(SurnameBytes, 0, data, 6 + nameBytes.Length, SurnameBytes.Length);

            // Serialize services (1 + 37 * servicesCount bytes)
            byte serviceCount = (byte)Math.Min(this.Services.Count, MaxServicesCount);
            data[6 + nameBytes.Length + SurnameBytes.Length] = serviceCount;

            for (int i = 0; i < serviceCount; i++) {
                byte[] serviceData = this.Services[i].GetByteArray();
                Array.Copy(serviceData, 0, data, 7 + nameBytes.Length + SurnameBytes.Length + i * ServiceSize, ServiceSize);
            }

            return data;
        }

        public override int GetSize() {
            // ID (4) + Namelength (1) + Name (15) + SurnameLength (1) + Surname (20) + ServicesCount (1) + Services (5*40)
            return 4 + 1 + MaxNameLength + 1 + MaxSurnameLength + 1 + MaxServicesCount * ServiceSize;
        }

        public override string GetInfo() {
            string result = $"Customer - {this.Name} {this.Surname}:";

            foreach (var service in this.Services) {
                result += $"\n  {service.GetInfo()}";
            }

            return result;
        }
    }
}
