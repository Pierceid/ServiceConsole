using ServiceConsole.Classes;

namespace ServiceConsole {
    internal class Program {
        static void Main(string[] args) {
            Tester tester = new();

            tester.TestInsert(3000);

            tester.TestPrint();

            tester.TestFind(3000);

            tester.CheckRecordCount("Before");

            tester.TestDelete(3000);

            tester.CheckRecordCount("After");

            tester.TestPrint();

            tester.TestDoublyLinkedListStructure();

            tester.TestSeek();
        }
    }
}
