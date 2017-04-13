namespace GMaster.Core.Tools
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class Debug
    {
        private static readonly ConcurrentDictionary<string, bool> CategoryEnabled = new ConcurrentDictionary<string, bool>();

        public static IDictionary<string, bool> Categories => CategoryEnabled;

        [Conditional("DEBUG")]
        public static void AddCategory(string category, bool b)
        {
            CategoryEnabled.AddOrUpdate(category, b, (s, b1) => b);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(Exception ex, string category) => WriteLine(ex.ToString(), category);

        [Conditional("DEBUG")]
        public static void WriteLine(string message, string category)
        {
            if (CategoryEnabled.GetOrAdd(category, true))
            {
                System.Diagnostics.Debug.WriteLine(message);
            }
        }

        [Conditional("DEBUG")]
        public static void WriteLine(Func<string> func, string category)
        {
            if (CategoryEnabled.GetOrAdd(category, true))
            {
                System.Diagnostics.Debug.WriteLine(func());
            }
        }
    }
}