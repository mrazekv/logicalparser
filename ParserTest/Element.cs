using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser;

namespace ParserTest
{
    class Element : ITestable
    {
        public int a { get; set; }
        public int b { get; set; }
        public string c { get; set; }

        public string GetString(int id)
        {
            if(id==3) // variable C
                return this.c;
            throw new PropertyNotFoundException();
        }

        public int GetInteger(int id)
            {
                switch(id)
                {
                    case 1: // variable A
                        return this.a;
                    case 2: // variable B
                        return this.b;
                    default:
                        throw new PropertyNotFoundException();
                }
            }
    }
}
