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

                int address = this.heapFile.InsertRecord(customer);

                Console.WriteLine($"Inserted Customer {customer.Name} {customer.Surname} at address {address}");

                if (address == -1) {
                    Console.WriteLine("Test failed: Customer insertion returned an invalid address.");
                    throw new NullReferenceException();
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
                    Console.WriteLine($"Customer found at address: {foundAddress}");
                } else {
                    Console.WriteLine("Customer not found.");
                    throw new NullReferenceException();
                }
            }
        }

        public void TestDelete(int recordsCount) {
            Console.WriteLine("------------------------------------------------------------------------------------");

            for (int i = 0; i < recordsCount; i++) {
                RefreshLists();

                int blockIndex = random.Next(this.heapFile.Blocks.Count);
                Block<Customer> blockToDelete = this.heapFile.Blocks[blockIndex];
                int recordIndex = random.Next(blockToDelete.Records.Count);
                IByteData recordData = blockToDelete.Records[recordIndex];

                int deletedAddress = this.heapFile.DeleteRecord(blockToDelete.Address, recordData);

                if (deletedAddress != -1) {
                    Console.WriteLine($"Block {deletedAddress}, Record {recordIndex} was deleted successfully.");
                } else {
                    Console.WriteLine("Failed to delete customer.");
                    throw new NullReferenceException();
                }
            }
        }

        public void TestDoublyLinkedListStructure() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            foreach (var block in this.heapFile.Blocks) {
                Console.WriteLine($"Block {block.Address}: Previous = {block.PreviousBlock}, Next = {block.NextBlock}");
            }
        }

        public void TestPrint() {
            Console.WriteLine("------------------------------------------------------------------------------------");

            this.heapFile.PrintFile();
        }

        private void RefreshLists() {
            customers.Clear();
            services.Clear();
            customerIDs.Clear();
            serviceIDs.Clear();

            foreach (var block in heapFile.Blocks) {
                foreach (var customer in block.Records) {
                    customers.Add(customer);
                    customerIDs.Add(customer.Id);

                    foreach (var service in customer.Services) {
                        services.Add(service);
                        serviceIDs.Add(service.Id);
                    }
                }
            }
        }
    }
}
