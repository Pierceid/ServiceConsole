using ServiceConsole.Classes;

namespace ServiceConsole {
    internal class Program {
        static void Main(string[] args) {
            Tester tester = new();

            tester.TestInsert(10000);

            tester.TestPrint();

            tester.TestFind(10000);

            tester.TestDelete(9997);

            tester.TestDoublyLinkedListStructure();

            tester.TestPrint();
        }
    }
}
