using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace GMaster.Camera
{
    public static class EnumToValue
    {
        public static string GetString(this Enum field)
        {
            return EnumValueAttribute.GetString(field);
        }
    }

    public static class EnumValue<T>
    {
        private static readonly ImmutableDictionary<object, T> Values = Enum.GetValues(typeof(T)).Cast<T>().ToImmutableDictionary(v => EnumValueAttribute.GetValue<object>(v));

        public static T Parse(object value)
        {
            return Values[value];
        }
    }

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