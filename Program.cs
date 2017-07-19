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
        Sequential,
        Enumerative
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
            RegisterControlFlowOperation("A");
            RegisterControlFlowOperation("<");
            RegisterControlFlowOperation("+");

            // Parameter parsers
            RegisterParameterParser("R", ParameterParserFunctions.Replace);
            RegisterParameterParser("G", ParameterParserFunctions.Goto);
            RegisterParameterParser("S", ParameterParserFunctions.Substring);
            RegisterParameterParser("M", ParameterParserFunctions.Multiply);
            RegisterParameterParser("*", ParameterParserFunctions.GetRegister);
            RegisterParameterParser("V", ParameterParserFunctions.Reverse);
            RegisterParameterParser("P", ParameterParserFunctions.Append);

            // Evaluators
            RegisterEvaluator("R", OperationEvaluators.Replace);
            RegisterEvaluator("V", OperationEvaluators.Reverse);
            RegisterEvaluator("S", OperationEvaluators.Substring);
            RegisterEvaluator("M", OperationEvaluators.Multiply);
            RegisterEvaluator("P", OperationEvaluators.Append);
        }

        public List<List<Token>> Parse(string input)
        {
            var splits = new List<List<string>>();
            // Pass 1 - splitting the tokens
            var split = new List<string>();

            StringBuilder buffer = new StringBuilder();
            foreach (var c in input)
            {
                if (c == '|')
                {
                    split.Add(buffer.ToString());
                    buffer = new StringBuilder();

                    splits.Add(split);
                    split = new List<string>();
                    continue;
                }
                if (Char.IsUpper(c) || c == '<' || c == '+')
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
            splits.Add(split);

            // Pass 2 - splitting the command from parameters + splitting parameters from each other
            var allTokens = new List<List<Token>>();

            foreach (var tokenSplit in splits)
            {
                var tokens = new List<Token>();
                foreach (var t in tokenSplit)
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
                allTokens.Add(tokens);
            }

            return allTokens;
        }

        //public List<IOperation> Parse(List<Token> tokens) { return null; }
        public string Evaluate(List<List<Token>> operations, string input, string register)
        {
            string originalRegister = register;
            string indexRegister = "0";
            string cur = "";
            bool executing = true;
            int i = 0;
            string outputBuffer = "";
            ExecutionMode mode = ExecutionMode.Sequential;

            foreach (var part in operations)
            {
                i = 0;
                indexRegister = "0";
                if (part[0].Command == "I")
                {
                    mode = ExecutionMode.Iterative;
                    part.RemoveAt(0);
                }
                else if (part[0].Command == "E")
                {
                    mode = ExecutionMode.Enumerative;
                    part.RemoveAt(0);
                }
                else
                {
                    mode = ExecutionMode.Sequential;
                }

                List<string> inputs = new List<string>();
                if (mode == ExecutionMode.Iterative)
                    inputs = input.ToCharArray().Select(o => o.ToString()).ToList();
                else if (mode == ExecutionMode.Enumerative)
                {
                    inputs = Enumerable.Repeat(input, input.Length).ToList();
                }
                else
                {
                    inputs.Add(input);
                }

                foreach (var inp in inputs)
                {
                    if (mode == ExecutionMode.Iterative || mode == ExecutionMode.Enumerative)
                        i = 0;

                    cur = inp;

                    while (executing)
                    {
                        var curToken = part[i];

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

                            if (curToken.Command == "<")
                            {
                                register = indexRegister;
                                i++;
                            }

                            if (curToken.Command == "+")
                            {
                                register = (Int32.Parse(register) + 1).ToString();
                                i++;
                            }

                            if (curToken.Command == "A")
                            {
                                var num = Int32.Parse(register);
                                num = Math.Abs(num);
                                register = num.ToString();
                                i++;
                            }
                        }
                        else
                        {
                            int j = 0;
                            var newParams = new List<string>(curToken.Parameters);
                            var newToken = new Token()
                            {
                                Command = curToken.Command,
                                IsControlFlowCommand = curToken.IsControlFlowCommand,
                                MetaStorage = curToken.MetaStorage
                            };

                            foreach (var p in curToken.Parameters)
                            {
                                if (p == "*")
                                    newParams[j] = register;

                                if (p == "^")
                                    newParams[j] = originalRegister;

                                if (p == "#")
                                    newParams[j] = indexRegister;

                                j++;
                            }
                            newToken.Parameters = newParams;

                            cur = Evaluators[curToken.Command](newToken, cur);
                            i++;
                        }

                        if (i == part.Count)
                            break;
                    }
                    if (mode == ExecutionMode.Iterative || mode == ExecutionMode.Enumerative)
                    {
                        outputBuffer += cur;
                    }
                    else
                        outputBuffer = cur;

                    cur = "";

                    indexRegister = (Int32.Parse(indexRegister) + 1).ToString();
                }

                input = outputBuffer;
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
            string program = "";
            string input = "boi";
            string register = "0";

            if (program.Length != 0)
            {
                var interpreter = new Interpreter();
                var tokens = interpreter.Parse(program);
                var output = interpreter.Evaluate(tokens, input, register);
                Console.Write(output);
            }
            else
            {
                Console.Write(input);
            }
            Console.ReadKey();
        }
    }
}