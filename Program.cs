using ServiceConsole.Classes;
using System.Text;

namespace ServiceConsole {
    internal class Program {
        static void Main(string[] args) {
            HeapFile<Customer> customerFile = new();
            HeapFile<Service> servicefile = new();

            for (int i = 0; i < 20; i++) {
                string name = "a" + i;
                string surname = "a" + 3 * i;
                Customer customer = new(name, surname);
                customerFile.Insert(customer);
            }

            for (int i = 0; i < 20; i++) {
                int date = DateTime.Now.Second + i;
                double cost = Math.Round(i * 1.2, 2);
                string desc = "xxx" + i;
                Service service = new(date, cost, desc);
                servicefile.Insert(service);
            }

            customerFile.PrintFile();
            servicefile.PrintFile();
        }
    }
}
