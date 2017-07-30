namespace GMaster.Core.Camera.Panasonic
{
    using System;

    public class LumixException : Exception
    {
        public LumixException(LumixError error, string result, Exception exception = null)
            : base(result, exception)
        {
            Error = error;
        }

        public LumixException(string result, Exception exception = null)
            : this(LumixError.Unknown, result, exception)
        {
        }

        public LumixError Error { get; }
    }
}