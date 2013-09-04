using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public abstract class ConsoleEx
    {
        public class Command
        {
            private Delegate _Delegate = null;
            private List<string> _CommandParts = null;
            private ParameterInfo[] __Parameters = null;
            private string _HelpText;

            public string HelpText { get { return _HelpText ?? string.Empty; } }

            private ParameterInfo[] _Parameters
            {
                get
                {
                    if (__Parameters == null)
                    {
                        MethodInfo method = _Delegate.GetType().GetMethod("Invoke");
                        __Parameters = method.GetParameters();
                    }
                    return __Parameters;
                }
            }

            public Command(string Command, string HelpText, Delegate Delegate)
            {
                _CommandParts = ParseParts(Command);
                _HelpText = HelpText;
                _Delegate = Delegate;
            }

            public bool IsExecutable(List<string> CmdParts)
            {
                if (CmdParts.Count == _CommandParts.Count + _Parameters.Length)
                {
                    for (int i = 0; i < _CommandParts.Count; i++)
                        if (CmdParts[i] != _CommandParts[i])
                            return false;
                    for (int i = 0; i < _Parameters.Length; i++)
                    {
                        TypeConverter conv = TypeDescriptor.GetConverter(_Parameters[i].ParameterType);
                        if (conv != null)
                            if (conv.IsValid(CmdParts[i + _CommandParts.Count]))
                                continue;
                        return false;
                    }
                    return true;
                }
                return false;
            }

            public object Execute(List<string> CmdParts, TextWriter Output = null)
            {
                if (Output == null)
                    Output = Console.Out;
                object[] args = new object[_Parameters.Length];
                for (int i = 0; i < _Parameters.Length; i++)
                {
                    Type paramType = _Parameters[i].ParameterType;
                    args[i] = Convert.ChangeType(CmdParts[i + _CommandParts.Count], paramType);
                }
                return _Delegate.DynamicInvoke(args);
            }
        }

        private List<Command> _Commands = new List<Command>();

        public List<Command> Commands { get { return _Commands; } }
        public readonly TextWriter Out;
        public readonly TextReader In;

        public abstract string Prompt { get; }
        protected abstract void PopulateCommandList(List<Command> Commands, TextWriter Out);

        public ConsoleEx()
        {
            this.Out = Console.Out;
            this.In = Console.In;
            PopulateCommandList(_Commands, this.Out);
        }

        public ConsoleEx(TextWriter Out, TextReader In)
        {
            if (Out != null)
                this.Out = Out;
            else this.Out = Console.Out;
            if (In != null)
                this.In = In;
            else this.In = Console.In;
            PopulateCommandList(_Commands, this.Out);
        }

        public void StartInteractive()
        {
            while (true)
            {
                Out.Write(Prompt);
                string cmdLine = In.ReadLine();
                if (!string.IsNullOrWhiteSpace(cmdLine))
                {
                    if (cmdLine == "help")
                        foreach (var command in _Commands)
                            Out.WriteLine(command.HelpText);
                    else if (cmdLine != "exit")
                        ExecuteCmdLine(cmdLine);
                    else break;
                }
            }
        }

        public void ExecuteCmdLine(string CmdLine)
        {
            try
            {
                List<string> cmdParts = ParseParts(CmdLine);
                foreach (Command cmd in _Commands)
                    if (cmd.IsExecutable(cmdParts))
                    {
                        cmd.Execute(cmdParts, Out);
                        return;
                    }
                throw new EntryPointNotFoundException("Command not found");
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                    if (ex.InnerException != null)
                        ex = ex.InnerException;
                for (int exN = 0; true; exN++)
                {
                    Out.WriteLine("{0}{1}: {2}", new string(' ', exN * 2), ex.GetType().Name, ex.Message);
                    if (ex.InnerException != null)
                        ex = ex.InnerException;
                    else break;
                }
            }
        }

        private static List<string> ParseParts(string CmdLine)
        {
            if (!string.IsNullOrWhiteSpace(CmdLine))
                return SplitCommandLine(CmdLine).ToList();
            else return new List<string>();
        }

        private static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;
            return SplitEx(commandLine, c =>
                {
                    if (c == '\"')
                        inQuotes = !inQuotes;
                    return !inQuotes && c == ' ';
                }).Select(arg => TrimMatchingQuotes(arg.Trim(), '\"'))
                  .Where(arg => !string.IsNullOrEmpty(arg));
        }

        private static IEnumerable<string> SplitEx(string str, Func<char, bool> controller)
        {
            int nextPiece = 0;
            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }
            yield return str.Substring(nextPiece);
        }

        private static string TrimMatchingQuotes(string input, char quote)
        {
            if ((input.Length >= 2) && (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);
            return input;
        }
    }
}