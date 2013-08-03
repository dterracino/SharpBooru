using System;
using System.IO;

namespace TA.SharpBooru
{
    public class Logger
    {
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

        public void LogException(Exception Ex) { LogException(null, Ex); }
        public void LogException(string JobName, Exception Ex)
        {
            lock (_Lock)
            {
                WriteANSI(0);
                WriteDate();
                if (!string.IsNullOrWhiteSpace(JobName))
                    _Writer.Write(JobName + " failed: ");
                WriteANSI(1, 31);
                _Writer.Write(Ex.GetType().Name + ": ");
                _Writer.WriteLine(Ex.Message);
                _Writer.Flush();
            }
        }
    }
}
