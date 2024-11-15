using ServiceConsole.Classes;

namespace ServiceConsole {
    internal class Program {
        static void Main(string[] args) {
            Tester tester = new();

            tester.TestInsert(100);

            tester.TestPrint();

            tester.TestFind(100);

            try {
                tester.TestDelete(50);
            } catch (NullReferenceException) { }

            tester.TestDoublyLinkedListStructure();

            tester.TestPrint();
        }
    }
}
