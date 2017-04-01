namespace GMaster.Tools
{
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumValueAttribute : Attribute
    {
        public EnumValueAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public static string GetString(Enum field)
        {
            return GetValue<string>(field);
        }

        public static T GetValue<T>(object field)
        {
            return (T)GetAttribute(field).Value;
        }

        private static EnumValueAttribute GetAttribute(object field)
        {
            var type = field.GetType();
            var memInfo = type.GetMember(field.ToString());
            return (EnumValueAttribute)memInfo[0].GetCustomAttribute(typeof(EnumValueAttribute));
        }
    }
}