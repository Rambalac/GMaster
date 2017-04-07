namespace Tools
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class EnumToValue
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<int, object>> IntToEnum =
            new ConcurrentDictionary<Type, IReadOnlyDictionary<int, object>>();

        public static string GetString(this Enum field)
        {
            return EnumValueAttribute.GetString(field);
        }

        public static T ToEnum<T>(this int i)
            where T : struct, IConvertible
        {
            return (T)IntToEnum.GetOrAdd(typeof(T), t => BuildEnumDict<T>())[i];
        }

        public static T ToEnum<T>(this int i, T defaultValue)
            where T : struct, IConvertible
        {
            if (IntToEnum.GetOrAdd(typeof(T), t => BuildEnumDict<T>()).TryGetValue(i, out object result))
            {
                return (T)result;
            }

            return defaultValue;
        }

        public static T ToEnum<T>(this short i)
            where T : struct, IConvertible
        {
            return ToEnum<T>((int)i);
        }

        public static T ToEnum<T>(this byte i)
            where T : struct, IConvertible
        {
            return ToEnum<T>((int)i);
        }

        public static T ToEnum<T>(this short i, T defaultValue)
            where T : struct, IConvertible
        {
            return ToEnum((int)i, defaultValue);
        }

        public static T ToEnum<T>(this byte i, T defaultValue)
            where T : struct, IConvertible
        {
            return ToEnum((int)i, defaultValue);
        }

        public static T ToValue<T>(this Enum field)
        {
            return EnumValueAttribute.GetValue<T>(field);
        }

        private static IReadOnlyDictionary<int, object> BuildEnumDict<T>()
                    where T : struct, IConvertible
        {
            var result = Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(m => m.ToInt32(null), m => (object)m);

            foreach (var en in Enum.GetValues(typeof(T)))
            {
                var exceptions = typeof(T).GetField(en.ToString()).GetCustomAttributes<EnumExceptionAttribute>();
                if (exceptions != null)
                {
                    foreach (var pair in exceptions)
                    {
                        result[pair.IntValue] = en;
                    }
                }
            }

            return result;
        }
    }
}