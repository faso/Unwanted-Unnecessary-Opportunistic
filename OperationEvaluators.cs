using System;
using System.Collections.Generic;
using System.Text;

namespace lngo.Evaluators
{
    public static class OperationEvaluators
    {
        public static string Replace(Token token, string input)
        {
            return input.Replace(token.Parameters[0], token.Parameters[1]);
        }
    }
}
