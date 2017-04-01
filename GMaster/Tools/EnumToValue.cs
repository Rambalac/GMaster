namespace GMaster.Tools
{
    using System;

    public static class EnumToValue
    {
        public static string GetString(this Enum field)
        {
            return EnumValueAttribute.GetString(field);
        }
    }
}