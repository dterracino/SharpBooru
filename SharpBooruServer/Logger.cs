using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TA.SharpBooru.Server
{
    public class Logger
    {
        private const string COLOR_FORMAT = "\x1b[1;3{0}m";
        private const string COLOR_FORMAT_RESET = "\x1b[0m";

        private TextWriter _Writer;
        private bool _UseColor = false;
        private string _NewLine = Environment.NewLine;
        private object _Lock = new object();

        public TextWriter Writer { get { return _Writer; } }
        public bool UseColor { get { return _UseColor; } set { _UseColor = value; } }
        private string NewLine { get { return _NewLine; } set { _NewLine = value ?? Environment.NewLine; } }

        public Logger() { _Writer = TextWriter.Null; }
        public Logger(TextWriter Writer) { _Writer = Writer; }
        public Logger(Stream Stream) { _Writer = new StreamWriter(Stream); }
        public Logger(string File) { _Writer = new StreamWriter(System.IO.File.Open(File, FileMode.Create, FileAccess.Write, FileShare.Read)); }
        public Logger(TextWriter Writer, bool UseColor) : this(Writer) { this.UseColor = UseColor; }
        public Logger(Stream Stream, bool UseColor) : this(Stream) { this.UseColor = UseColor; }
        public Logger(string File, bool UseColor) : this(File) { this.UseColor = UseColor; }

        public string GetColorCodeString(int? Color)
        {
            if (Color.HasValue)
                return string.Format(COLOR_FORMAT, Color.Value);
            else return COLOR_FORMAT_RESET;
        }

        public void Write(string Format, params object[] Args)
        {
            string result = string.Format(Format, Args);
            result = Regex.Replace(result, "\\[[Cc][0-9rR]\\]", (x) =>
            {
                if (UseColor)
                {
                    char colorChar = x.Value.Substring(2, 1).ToLower()[0];
                    int? colorInt = null;
                    if (char.IsNumber(colorChar))
                        colorInt = (int)char.GetNumericValue(colorChar);
                    return GetColorCodeString(colorInt);
                }
                else return string.Empty;
            });
            lock (_Lock)
            {
                _Writer.Write(result);
                if (UseColor)
                    _Writer.Write(GetColorCodeString(null));
            }
        }

        public void WriteLine(string Format, params object[] Args)
        {
            lock (_Lock)
            {
                Write(Format, Args);
                Write(_NewLine);
            }
        }

        public void LogLine() { _Writer.WriteLine(); }
        public void LogLine(string Format, params object[] Args) { WriteLine(Format, Args); }
        public void LogBegin(string Format, params object[] Args) { Write(Format + "... ", Args); }

        public void LogBool(bool Result)
        {
            lock (_Lock)
                WriteLine("[c7][[c{0}]{1}[c7]]", Result ? 2 : 1, Result ? "OK" : "FAIL");
        }

        public void LogOK() { LogBool(true); }
        public void LogFAIL() { LogBool(false); }

        public void LogFAILAndLine(string Format, params object[] Args)
        {
            lock (_Lock)
            {
                LogBool(false);
                LogLine(Format, Args);
            }
        }

        public int LogFAILAndLineAndReturn(int ReturnCode, string Format, params object[] Args)
        {
            LogFAILAndLine(Format, Args);
            return ReturnCode;
        }

        public int LogFAILAndReturn(int ReturnCode)
        {
            LogBool(false);
            return ReturnCode;
        }

        private T internalThrow<T>(Exception Ex)
        {
            if (Ex == null)
                throw new ArgumentNullException("No exception provided");
            else throw Ex;
        }

        public int LogFAILAndThrow(Exception Exception)
        {
            LogBool(false);
            return internalThrow<int>(Exception);
        }

        public int LogFAILAndLineAndThrow(Exception Exception, string Format, params object[] Args)
        {
            LogFAILAndLine(Format, Args);
            return internalThrow<int>(Exception);
        }

        public T LogFAILAndThrow<T>(Exception Exception)
        {
            LogBool(false);
            return internalThrow<T>(Exception);
        }

        public T LogFAILAndLineAndThrow<T>(Exception Exception, string Format, params object[] Args)
        {
            LogFAILAndLine(Format, Args);
            return internalThrow<T>(Exception);
        }
    }
}