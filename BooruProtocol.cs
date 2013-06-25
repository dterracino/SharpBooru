using System;
using System.IO;

namespace TEAM_ALPHA.SharpBooru
{
    public class BooruProtocol
    {
        /*
         * Posts
         *  Edit post
         *  Remove post
         *  Get post
         *  Get image
         *  
         * Tags
         *  Edit tag
         *  (Add tag)
         *  Remove tag
         *  
         * Aliases
         *  Add alias
         *  Remove alias
         */

        public class Request
        {
            private ushort _Command;
            private object _Payload;

            public T Payload<T>()
            {
                if (_Payload != null)
                    return (T)Convert.ChangeType(_Payload, typeof(T));
                else throw new NullReferenceException();
            }

            public ushort Command {get{return _Command;}}

            public Request(ushort Command, object Payload)
            {
                _Command = Command;
                _Payload = Payload;
            }

            public Request(Stream Stream)
            {
                BinaryReader reader = new BinaryReader(Stream);
                uint length = reader.ReadUInt32();
                _Command = reader.ReadUInt16();
            }
        }
    }
}
