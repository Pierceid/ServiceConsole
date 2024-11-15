namespace ServiceConsole.Classes {
    public class Tester {
        private readonly HeapFile<Customer> heapFile;
        private readonly Random random;
        private readonly List<Customer> customers;
        private readonly List<Service> services;
        private readonly List<int> customerIDs;
        private readonly List<int> serviceIDs;

        public Tester() {
            this.heapFile = new();
            this.random = new();
            this.customers = [];
            this.services = [];
            this.customerIDs = [];
            this.serviceIDs = [];
        }

        public void TestInsert(int recordsCount) {
            Console.WriteLine("------------------------------------------------------------------------------------");

            for (int i = 0; i < recordsCount; i++) {
                Customer customer = new($"Name{i}", $"Surname{i}");

                int servicesCount = random.Next(1, 6);

                for (int j = 0; j < servicesCount; j++) {
                    Service service = new(DateTime.Now.AddDays(-random.Next(1000)).ToString("ddMMyy"), Math.Round(random.NextDouble() * 200, 2), $"Desc{j}");
                    customer.Services.Add(service);

                    services.Add(service);
                    serviceIDs.Add(service.Id);
                }

                int insertedAddress = this.heapFile.InsertRecord(customer);

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
                int blockIndex = random.Next(this.heapFile.Blocks.Count);
                Block<Customer> blockToFind = this.heapFile.Blocks[blockIndex];
                IByteData recordData = blockToFind.Records[random.Next(blockToFind.Records.Count)];

                int foundAddress = this.heapFile.FindRecord(blockToFind.Address, recordData);

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
                if (this.heapFile.Blocks.Count == 0) break;

                int blockIndex = random.Next(this.heapFile.Blocks.Count);
                Block<Customer> blockToDelete = this.heapFile.Blocks[blockIndex];

                if (blockToDelete.Records.Count == 0) continue;

                int recordIndex = random.Next(blockToDelete.Records.Count);
                Customer customerToDelete = blockToDelete.Records[recordIndex];

                int deletedAddress = this.heapFile.DeleteRecord(blockToDelete.Address, customerToDelete);

                if (deletedAddress != -1) {
                    Console.WriteLine($"Record deleted from address: [#{deletedAddress}]");

                    customers.Remove(customerToDelete);
                    customerIDs.Remove(customerToDelete.Id);

                    foreach (var service in customerToDelete.Services) {
                        services.Remove(service);
                        serviceIDs.Remove(service.Id);
                    }
                } else {
                    throw new NullReferenceException("Failed to delete record.");
                }
            }
        }

        public void TestDoublyLinkedListStructure() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            foreach (var block in this.heapFile.Blocks) {
                Console.WriteLine($"Block [#{block.Address}]: Previous = [#{block.PreviousBlockAddress}], Next = [#{block.NextBlockAddress}]");
            }
        }

        public void TestPrint() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            this.heapFile.PrintFile();
        }
    }
}
