﻿using ServiceConsole.Classes;

namespace ServiceConsole {
    internal class Program {
        static void Main(string[] args) {
            Tester tester = new();

            tester.TestInsert(100);

            tester.TestPrint();

            tester.TestFind(100);

            //tester.TestDelete(990);

            tester.TestDoublyLinkedListStructure();

            tester.TestSeek();
        }
    }
}
