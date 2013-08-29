using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

namespace TA.SharpBooru.Client
{
    public abstract class InteractiveConsole
    {
        public class Command
        {
            private Delegate _Delegate = null;
            private List<string> _CommandParts = null;
            private ParameterInfo[] __Parameters = null;

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

            public Command(string Command, Delegate Delegate)
            {
                _CommandParts = ParseParts(Command);
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

        public InteractiveConsole()
        {
            this.Out = Console.Out;
            this.In = Console.In;
            PopulateCommandList(_Commands, this.Out);
        }

        public InteractiveConsole(TextWriter Out, TextReader In)
        {
            if (Out != null)
                this.Out = Out;
            else this.Out = Console.Out;
            if (In != null)
                this.In = In;
            else this.In = Console.In;
            PopulateCommandList(_Commands, this.Out);
        }

        public void Start()
        {
            Out.Write(Prompt);
            string cmdLine = In.ReadLine();
            if (cmdLine != "exit")
            {
                try
                {
                    if (!TryExecute(cmdLine))
                        throw new EntryPointNotFoundException("Command not found");
                }
                catch (Exception ex)
                {
                    if (ex is TargetInvocationException)
                        ex = ex.InnerException;
                    //TODO Make dat better
                    Out.WriteLine("ERROR: " + ex.Message);
                }
                Start();
            }
        }

        public bool TryExecute(string CmdLine)
        {
            List<string> cmdParts = ParseParts(CmdLine);
            foreach (Command cmd in _Commands)
            {
                if (cmd.IsExecutable(cmdParts))
                {
                    cmd.Execute(cmdParts, Out);
                    return true;
                }
            }
            return false;
        }

        private static List<string> ParseParts(string CmdLine)
        {
            if (!string.IsNullOrWhiteSpace(CmdLine))
            {
                string[] parts = CmdLine.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return parts.ToList();
            }
            else return new List<string>();
        }
    }
}