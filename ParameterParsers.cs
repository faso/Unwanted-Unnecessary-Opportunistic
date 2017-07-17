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

        public static List<string> Goto(string input)
        {
            var args = input.Split(';');
            return new List<string>(args);
        }

        public static List<string> Substring(string input)
        {
            return new List<string>(input.Split(';'));
        }

        public static List<string> Multiply(string input)
        {
            return new List<string>(input.Split(';'));
        }
    }
}
