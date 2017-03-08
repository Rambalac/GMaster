using System;

namespace LumixMaster
{
    public class LumixException : Exception
    {
        public LumixException(string result) : base(result)
        {
        }
    }
}