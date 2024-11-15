using ServiceConsole.Classes;
using System;

namespace ServiceConsole {
    internal class Program {
        static void Main(string[] args) {
            HeapFile<Customer> heapFile = new();
            Random random = new();

            // ----------------------------------------------------------------------------------------------------------

            Console.WriteLine("Testing Insert:\n");

            for (int i = 0; i < 10; i++) {
                Customer customer = new($"Name{i}", $"Surname{i}");

                int servicesCount = random.Next(6);

                for (int j = 0; j < servicesCount; j++) {
                    Service service = new(DateTime.Now.Second, j * 15.0, $"Desc{j}");
                    customer.Services.Add(service);

                    Console.WriteLine($"Inserted Service {service.Date}, {service.Cost}, {service.Description}");
                }

                int address = heapFile.Insert(customer);

                Console.WriteLine($"Inserted Customer {customer.Name} {customer.Surname} at address {address}");
            }

            // ----------------------------------------------------------------------------------------------------------

            Console.WriteLine("\nTesting PrintFile:\n");

            heapFile.PrintFile();

            Console.WriteLine("\nTesting Find:\n");

            int blockIndex = 1;
            int recordIndex = 0;
            int foundAddress = heapFile.Find(blockIndex, recordIndex);

            if (foundAddress != -1) {
                Console.WriteLine($"Customer found at address: {foundAddress}");
            } else {
                Console.WriteLine("Customer not found.");
            }

            // ----------------------------------------------------------------------------------------------------------
            Console.WriteLine("\nTesting Delete:\n");

            blockIndex = 0;
            recordIndex = 0;
            int deleteSuccess = heapFile.Delete(blockIndex, recordIndex);

            Console.WriteLine(deleteSuccess != -1
                ? $"Customer at Block {blockIndex}, Record {recordIndex} deleted successfully."
                : "Failed to delete customer.");

            Console.WriteLine("\nHeap File after Deletion:");
            heapFile.PrintFile();

            Console.WriteLine("\nTesting Doubly Linked List Structure:");
            Console.WriteLine($"First Valid Block: {heapFile.FirstBlockIndex}");
            int lastBlockIndex = heapFile.LastBlockIndex;

            while (lastBlockIndex != -1) {
                Block<Customer> block = heapFile.Blocks[lastBlockIndex];
                Console.WriteLine($"Block {lastBlockIndex}: Previous = {block.PreviousBlock}, Next = {block.NextBlock}");
                lastBlockIndex = block.NextBlock;
            }
        }
    }
}
