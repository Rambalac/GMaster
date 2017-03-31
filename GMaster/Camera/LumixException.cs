using System;

namespace GMaster.Camera
{
    public class LumixException : Exception
    {
        public LumixException(string result)
            : base(result)
        {
        }
    }
}