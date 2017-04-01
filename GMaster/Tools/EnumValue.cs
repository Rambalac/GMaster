namespace GMaster.Tools
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    public static class EnumValue<T>
    {
        private static readonly ImmutableDictionary<object, T> Values = Enum.GetValues(typeof(T)).Cast<T>().ToImmutableDictionary(v => EnumValueAttribute.GetValue<object>(v));

        public static T Parse(object value)
        {
            return Values[value];
        }
    }
}