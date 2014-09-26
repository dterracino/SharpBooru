using System;

namespace TA.SharpBooru
{
    public class BooruException : Exception
    {
        public enum ErrorCodes : byte
        {
            Success = 0,
            ResourceNotFound,
            NoPermission,
            LoginFailed,
            FingerprintNotAccepted,
            ProtocolVersionMismatch,
            UnknownError
        }

        private ErrorCodes? _ErrorCode;

        public ErrorCodes ErrorCode
        {
            get
            {
                if (_ErrorCode.HasValue)
                    return _ErrorCode.Value;
                else return BooruException.ErrorCodes.UnknownError;
            }
        }

        public BooruException(string Message)
            : base(Message) { }

        public BooruException(ErrorCodes ErrorCode)
            : base(string.Format("ErrorCode {0}: {1}", (byte)ErrorCode, ErrorCode))
        { _ErrorCode = ErrorCode; }

        public BooruException(ErrorCodes ErrorCode, string Details)
            : base(string.Format("ErrorCode {0}: {1} - {3}", (byte)ErrorCode, ErrorCode, Environment.NewLine, Details))
        { _ErrorCode = ErrorCode; }
    }
}
