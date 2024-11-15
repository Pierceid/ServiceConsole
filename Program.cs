using ServiceConsole.Classes;

namespace ServiceConsole {
    internal class Program {
        static void Main(string[] args) {
            Tester tester = new();

            tester.TestInsert(20);

            tester.TestPrint();

            tester.TestFind(30);

            tester.TestDelete(10);

            tester.TestDoublyLinkedListStructure();

            tester.TestPrint();
        }
    }
}
