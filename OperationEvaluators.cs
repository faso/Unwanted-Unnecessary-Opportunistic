using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lngo.Evaluators
{
    public static class OperationEvaluators
    {
        public static string Replace(Token token, string input)
        {
            return input.Replace(token.Parameters[0], token.Parameters[1]);
        }

        public static string Reverse(Token token, string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string Substring(Token token, string input)
        {
            if (token.Parameters.Count == 1)
                return input.Substring(Int32.Parse(token.Parameters[0]));
            else
                return input.Substring(Int32.Parse(token.Parameters[0]), Int32.Parse(token.Parameters[1]));
        }
    }
}
