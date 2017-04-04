namespace GMaster.Camera
{
    using System;

    public class LumixException : Exception
    {
        public LumixException(LumixError error, string result)
            : base(result)
        {
            Error = error;
        }

        public LumixException(string result)
            : this(LumixError.Unknown, result)
        {
        }

        public LumixError Error { get; }
    }
}