using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser;

namespace ParserTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var l = new LexAnalyzer();
            l.Parse("a>14 and not not (a=0x23) and b=12 and c=HELLO");
            var ll = new LLAnalyzer<Element>(l, new Finder());
            var expr = ll.Analyze();
            Console.WriteLine(expr);
            Console.WriteLine(expr.TestValidity(new Element()) ? "true" : "false");

            Console.ReadKey();
        }



    }
}
