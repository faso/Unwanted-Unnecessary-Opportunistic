using System;
using System.Collections.Generic;
using System.Text;
using lngo.ParameterParsers;
using lngo.Evaluators;

namespace lngo
{
    public interface IOperationToken { }
    public interface IOperation { }

    public class Token : IOperationToken
    {
        public string Command { get; set; }
        public List<string> Parameters { get; set; }
    }

    public class Interpreter
    {
        public Dictionary<string, Func<string, List<string>>> ParameterParsers = new Dictionary<string, Func<string, List<string>>>();
        public Dictionary<string, Func<Token, string, string>> Evaluators = new Dictionary<string, Func<Token, string, string>>();

        public Interpreter()
        {
            // Parameter parsers
            RegisterParameterParser("R", ParameterParserFunctions.Replace);

            // Evaluators
            RegisterEvaluator("R", OperationEvaluators.Replace);
        }

        public List<Token> Parse(string input)
        {
            // Pass 1 - splitting the tokens
            var split = new List<string>();

            StringBuilder buffer = new StringBuilder();
            foreach (var c in input)
            {
                if (Char.IsUpper(c))
                {
                    if (buffer.Length != 0)
                    {
                        split.Add(buffer.ToString());
                        buffer = new StringBuilder();
                    }

                    buffer.Append(c);
                }
                else
                {
                    buffer.Append(c);
                }
            }
            split.Add(buffer.ToString());

            // Pass 2 - splitting the command from parameters + splitting parameters from each other
            var tokens = new List<Token>();
            
            foreach (var t in split)
            {
                var operation = t[0].ToString();
                tokens.Add(new Token()
                {
                    Command = operation,
                    Parameters = t.Length == 1 ? null : ParameterParsers[operation](t.Substring(1))
                });
            }

            return tokens;
        }

        //public List<IOperation> Parse(List<Token> tokens) { return null; }
        public string Evalute(List<Token> operations, string input)
        {
            string cur = input;
            foreach (var op in operations)
                cur = Evaluators[op.Command](op, cur);

            return cur;
        }

        // Helpers
        private void RegisterParameterParser(string operation, Func<string, List<string>> par)
        {
            ParameterParsers.Add(operation, par);
        }

        private void RegisterEvaluator(string operation, Func<Token, string, string> par)
        {
            Evaluators.Add(operation, par);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string program = "Rnig;nog";
            string input = "asdnigspado";

            var interpreter = new Interpreter();
            var tokens = interpreter.Parse(program);
            Console.WriteLine(interpreter.Evalute(tokens, input));
            Console.ReadKey();
        }
    }
}