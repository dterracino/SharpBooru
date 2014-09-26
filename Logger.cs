using System;
using System.IO;
using System.Reflection;

namespace TA.SharpBooru
{
    public class Logger : IDisposable
    {
        public static Logger Null { get { return new Logger(TextWriter.Null); } }

        private TextWriter _Writer;
        private object _Lock = new object();

        public Logger(TextWriter Writer) { _Writer = Writer; }

        private void WriteDate() { _Writer.Write("[{0:MM-dd HH:mm:ss.fff}] ", DateTime.Now); }

        private void WriteANSI(params int[] Code)
        {
            for (byte i = 0; i < Code.Length; i++)
                _Writer.Write("\x1b[{0}m", Code[i]);
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
                    _Writer.Write(" {0}: ", theException.GetType().Name);
                    _Writer.WriteLine(theException.Message);
                    _Writer.WriteLine(theException.StackTrace);
                    if (theException.InnerException != null)
                        theException = theException.InnerException;
                    else break;
                }
                _Writer.Flush();
            }
        }

        public void LogPublicFields(string ObjectName, object Object)
        {
            if (Object != null)
            {
                Type objType = Object.GetType();
                lock (_Lock)
                {
                    WriteANSI(0);
                    WriteDate();
                    WriteANSI(1, 37);
                    _Writer.WriteLine("Fields/Properties: {0}", ObjectName);
                    foreach (PropertyInfo pInfo in objType.GetProperties())
                    {
                        WriteANSI(0);
                        WriteDate();
                        WriteANSI(1, 37);
                        _Writer.WriteLine("- {0} = {1}", pInfo.Name, pInfo.CanRead ? pInfo.GetValue(Object, null) : "not readable");
                    }
                    foreach (FieldInfo fInfo in objType.GetFields())
                    {
                        WriteANSI(0);
                        WriteDate();
                        WriteANSI(1, 37);
                        _Writer.WriteLine("- {0} = {1}", fInfo.Name, fInfo.GetValue(Object));
                    }
                }
            }
        }

        public void Dispose() { _Writer.Dispose(); }
    }
}
