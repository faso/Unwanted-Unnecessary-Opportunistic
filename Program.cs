using System;
using System.Collections.Generic;
using System.Text;
using lngo.ParameterParsers;
using lngo.Evaluators;
using System.Linq;

namespace lngo
{
    public interface IOperationToken { }
    public interface IOperation { }

    public enum ExecutionMode
    {
        Iterative,
        Sequential
    }

    public class Token : IOperationToken
    {
        public string Command { get; set; }
        public List<string> Parameters { get; set; }
        public bool IsControlFlowCommand { get; set; }
        public Dictionary<string, object> MetaStorage { get; set; }
    }

    public class Interpreter
    {
        public Dictionary<string, Func<string, List<string>>> ParameterParsers = new Dictionary<string, Func<string, List<string>>>();
        public Dictionary<string, Func<Token, string, string>> Evaluators = new Dictionary<string, Func<Token, string, string>>();
        public List<string> ControlFlowOperations = new List<string>();

        public Interpreter()
        {
            // Control flow operations
            RegisterControlFlowOperation("G");

            // Parameter parsers
            RegisterParameterParser("R", ParameterParserFunctions.Replace);
            RegisterParameterParser("G", ParameterParserFunctions.Goto);
            RegisterParameterParser("S", ParameterParserFunctions.Substring);
            RegisterParameterParser("M", ParameterParserFunctions.Multiply);

            // Evaluators
            RegisterEvaluator("R", OperationEvaluators.Replace);
            RegisterEvaluator("V", OperationEvaluators.Reverse);
            RegisterEvaluator("S", OperationEvaluators.Substring);
            RegisterEvaluator("M", OperationEvaluators.Multiply);
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
                    Parameters = t.Length == 1 ? null : ParameterParsers[operation](t.Substring(1)),
                    IsControlFlowCommand = ControlFlowOperations.Contains(operation),
                    MetaStorage = new Dictionary<string, object>()
                });
            }

            return tokens;
        }

        //public List<IOperation> Parse(List<Token> tokens) { return null; }
        public string Evalute(List<Token> operations, string input)
        {
            string cur = "";
            bool executing = true;
            int i = 0;
            string register = "";
            string outputBuffer = "";
            ExecutionMode mode = ExecutionMode.Sequential;

            if (operations[0].Command == "I")
            {
                mode = ExecutionMode.Iterative;
                operations.RemoveAt(0);
            }

            List<string> inputs = new List<string>();
            if (mode == ExecutionMode.Iterative)
                inputs = input.ToCharArray().Select(o => o.ToString()).ToList();
            else
            {
                inputs.Add(input);
            }

            foreach (var inp in inputs)
            {
                if (mode == ExecutionMode.Iterative)
                    i = 0;

                while (executing)
                {
                    var curToken = operations[i];

                    if (curToken.IsControlFlowCommand)
                    {
                        if (curToken.Command == "G")
                        {
                            if (curToken.Parameters.ElementAt(1) != "0")
                                i = Int32.Parse(curToken.Parameters.First());
                            else
                                i++;

                            var gotoCount = Int32.Parse(curToken.Parameters.ElementAt(1));
                            if (gotoCount > 0)
                                curToken.Parameters[1] = (gotoCount - 1).ToString();
                        }
                    }
                    else
                    {
                        cur = Evaluators[curToken.Command](curToken, inp);
                        i++;
                    }

                    outputBuffer += cur;

                    if (i == operations.Count)
                        break;
                }
            }

            return outputBuffer;
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

        private void RegisterControlFlowOperation(string op)
        {
            ControlFlowOperations.Add(op);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string program = "IM3";
            string input = "ab";

            var interpreter = new Interpreter();
            var tokens = interpreter.Parse(program);
            Console.WriteLine(interpreter.Evalute(tokens, input));
            Console.ReadKey();
        }
    }
}