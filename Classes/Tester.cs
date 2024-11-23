namespace ServiceConsole.Classes {
    public class Tester {
        private readonly HeapFile<Customer> heapFile;
        private readonly Random random;
        private readonly List<Customer> customers;
        private readonly List<CarService> services;
        private readonly List<int> customerIDs;
        private readonly List<int> serviceIDs;

        public Tester() {
            this.heapFile = new HeapFile<Customer>(8, Path.GetFullPath(Path.Combine("..", "..", "..", "Resources", "heap-file.bin")));
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
                int insertedAddress = this.heapFile.InsertRecord(recordData);

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

                int foundAddress = this.heapFile.FindRecord(this.heapFile.FirstPartiallyFullBlock, recordData);

                if (foundAddress == -1) foundAddress = this.heapFile.FindRecord(this.heapFile.FirstFullBlock, recordData);

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

                int deletedAddress = this.heapFile.DeleteRecord(this.heapFile.FirstPartiallyFullBlock, recordData);

                if (deletedAddress == -1) deletedAddress = this.heapFile.DeleteRecord(this.heapFile.FirstFullBlock, recordData);

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

            this.heapFile.CheckStructure();
        }

        public void TestPrint() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            this.heapFile.PrintFile();
        }

        public void TestSeek() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            Console.WriteLine($"Seek: {this.heapFile.Seek()}");
        }
    }
}
