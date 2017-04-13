namespace GMaster.Core.Tools
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    public static class ValueToEnum<T>
        where T : struct, IConvertible
    {
        private static readonly ImmutableDictionary<object, T> Values = Enum.GetValues(typeof(T)).Cast<T>().Where(v => EnumValueAttribute.GetValue<object>(v) != null).ToImmutableDictionary(v => EnumValueAttribute.GetValue<object>(v));

        public static T Parse(object value)
        {
            return Values[value];
        }

        public static T Parse(object value, T defaultValue)
        {
            return Values.TryGetValue(value, out var res) ? res : defaultValue;
        }
    }
}