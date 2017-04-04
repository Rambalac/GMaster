namespace Tools
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnumExceptionAttribute : Attribute
    {
        public EnumExceptionAttribute(int intValue)
        {
            IntValue = intValue;
        }

        public int IntValue { get; }
    }
}