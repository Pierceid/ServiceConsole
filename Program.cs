using ServiceConsole.Classes;

namespace ServiceConsole {
    internal class Program {
        static void Main(string[] args) {
            Tester tester = new();

            tester.TestInsert(100);

            tester.TestPrint();

            tester.TestFind(100);

            tester.CheckRecordCount("Before");

            tester.TestDelete(100);

            tester.CheckRecordCount("After");

            tester.TestPrint();

            tester.TestDoublyLinkedListStructure();

            tester.TestSeek();
        }
    }
}
