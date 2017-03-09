using System;

namespace GMaster
{
    public class LumixException : Exception
    {
        public LumixException(string result) : base(result)
        {
        }
    }
}