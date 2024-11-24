namespace ServiceConsole.Classes {
    public class Tester {
        private readonly ExtendibleHashFile<Customer> extensibleHashFile;
        private readonly Random random;
        private readonly List<Customer> customers;
        private readonly List<CarService> services;
        private readonly List<int> customerIDs;
        private readonly List<int> serviceIDs;
        private readonly string filePath = Path.GetFullPath(Path.Combine("..", "..", "..", "Resources", "data.bin"));

        public Tester() {
            this.extensibleHashFile = new ExtendibleHashFile<Customer>(8, 100, this.filePath);
            this.random = new Random();
            this.customers = [];
            this.services = [];
            this.customerIDs = [];
            this.serviceIDs = [];
        }

        public void TestInsert(int recordsCount) {
            Console.WriteLine("------------------------------------------------------------------------------------");

            for (int i = 0; i < recordsCount; i++) {
                Customer customer = new($"Name{i}", $"Surname{i}", $"ECV{i}");

                int servicesCount = random.Next(1, 6);

                for (int j = 0; j < servicesCount; j++) {
                    CarService service = new(DateTime.Now.AddDays(-random.Next(1000)), Math.Round(random.NextDouble() * 200, 2), [$"Desc{j}", $"Desc{j + 1}"]);

                    customer.Services.Add(service);
                    services.Add(service);
                    serviceIDs.Add(service.Id);
                }

                IByteData recordData = customer;
                int insertedAddress = this.extensibleHashFile.InsertRecord(recordData);

                if (insertedAddress != -1) {
                    Console.WriteLine($"Record inserted at address: [#{insertedAddress}]");
                } else {
                    throw new NullReferenceException("Failed to insert record.");
                }

                customers.Add(customer);
                customerIDs.Add(customer.Id);
            }
        }

        public void TestFind(int recordsCount) {
            Console.WriteLine("------------------------------------------------------------------------------------");

            for (int i = 0; i < recordsCount; i++) {
                Customer customerToFind = customers[random.Next(customers.Count)];
                IByteData recordData = customerToFind;

                int foundAddress = this.extensibleHashFile.FindRecord(this.extensibleHashFile.FirstPartiallyFullBlock, recordData);

                if (foundAddress == -1) foundAddress = this.extensibleHashFile.FindRecord(this.extensibleHashFile.FirstFullBlock, recordData);

                if (foundAddress != -1) {
                    Console.WriteLine($"Record found at address: [#{foundAddress}]");
                } else {
                    throw new NullReferenceException("Failed to find record.");
                }
            }
        }

        public void TestDelete(int recordsCount) {
            Console.WriteLine("------------------------------------------------------------------------------------");

            for (int i = 0; i < recordsCount; i++) {
                if (this.customers.Count == 0) break;

                Customer customerToDelete = customers[random.Next(customers.Count)];
                IByteData recordData = customerToDelete;

                int deletedAddress = this.extensibleHashFile.DeleteRecord(this.extensibleHashFile.FirstPartiallyFullBlock, recordData);

                if (deletedAddress == -1) deletedAddress = this.extensibleHashFile.DeleteRecord(this.extensibleHashFile.FirstFullBlock, recordData);

                if (deletedAddress != -1) {
                    Console.WriteLine($"Record deleted from address: [#{deletedAddress}]");
                } else {
                    throw new NullReferenceException("Failed to delete record.");
                }
            }
        }

        public void TestDoublyLinkedListStructure() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            this.extensibleHashFile.CheckStructure();
        }

        public void TestPrint() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            this.extensibleHashFile.PrintFile();
        }

        public void TestSeek() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            Console.WriteLine($"Seek: {this.extensibleHashFile.Seek()}");
        }

        public void CheckRecordCount(string message) {
            Console.WriteLine("------------------------------------------------------------------------------------");
            
            int count = 0;

            foreach (var blockAddress in this.extensibleHashFile.PartiallyFullBlocks) {
                var block = extensibleHashFile.ReadBlock(blockAddress);

                if (block == null) continue;

                count += block.ValidCount;
            }

            foreach (var blockAddress in this.extensibleHashFile.FullBlocks) {
                var block = extensibleHashFile.ReadBlock(blockAddress);

                if (block == null) continue;

                count += block.ValidCount;
            }

            Console.WriteLine($"{message}: {count}");
        }
    }
}
