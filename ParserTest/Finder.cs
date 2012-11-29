using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserTest
{
    class Finder : IPropertyFinder
    {
        public int GetId(string name, out bool isString)
        {
            isString = false;
            switch (name)
            {
                case "A":
                    isString = false;
                    return 1;
                case "B":
                    isString = false;
                    return 2;
                case "C":
                    isString = true;
                    return 3;
            }
            throw new PropertyNotFoundException();
        }
    }
}
