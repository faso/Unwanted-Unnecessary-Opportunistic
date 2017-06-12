using System;
using System.Collections.Generic;
using System.Text;

namespace lngo.ParameterParsers
{
    public static class ParameterParserFunctions
    {
        public static List<string> Replace(string input)
        {
            return new List<string>(input.Split(';'));
        }
    }
}
