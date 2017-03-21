namespace GMaster.Camera
{
    using System;

    public class LumixException : Exception
    {
        public LumixException(string result) : base(result)
        {
        }
    }
}