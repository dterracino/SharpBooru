using System;

namespace TA.SharpBooru.NetIO
{
    public class ServerException : Exception
    {
        public ServerException()
            : base() { }

        public ServerException(string message)
            : base(message) { }

        public ServerException(string message, Exception innerException)
            : base(message, innerException) { }

        //TODO Network serializable?
    }
}