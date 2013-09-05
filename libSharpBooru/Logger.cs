using System;
using System.IO;

namespace TA.SharpBooru
{
    public class Logger
    {
        public static Logger Null { get { return new Logger(TextWriter.Null); } }

        private TextWriter _Writer;
        private object _Lock = new object();

        public Logger(TextWriter Writer) { _Writer = Writer; }

        private void WriteDate() { _Writer.Write("[{0:MM-dd HH:mm:ss.fff}] ", DateTime.Now); }

        private void WriteANSI(params int[] Code)
        {
            if (Helper.IsUnix())
                foreach (int code in Code)
                    _Writer.Write("\x1b[{0}m", code);
        }

        public void LogLine(string Format, params object[] Args)
        {
            lock (_Lock)
            {
                WriteANSI(0);
                WriteDate();
                WriteANSI(1, 37);
                _Writer.WriteLine(Format, Args);
                _Writer.Flush();
            }
        }

        [Obsolete("Please use LogException(string, Eception)")]
        public void LogException(Exception Ex) { LogException(null, Ex); }

        public void LogException(string JobName, Exception Ex)
        {
            lock (_Lock)
            {
                WriteANSI(0);
                WriteDate();
                WriteANSI(1, 31);
                if (!string.IsNullOrWhiteSpace(JobName))
                    _Writer.WriteLine(JobName + " failed: ");
                Exception theException = Ex;
                for (int ec = 1; true; ec++)
                {
                    WriteANSI(0);
                    WriteDate();
                    WriteANSI(1, 31);
                    _Writer.Write(new string('-', ec));
                    WriteANSI(1, 37);
                    _Writer.Write(" " + theException.GetType().Name + ": ");
                    _Writer.WriteLine(theException.Message);
                    if (theException.InnerException != null)
                        theException = theException.InnerException;
                    else break;
                }
                _Writer.Flush();
            }
        }
    }
}
