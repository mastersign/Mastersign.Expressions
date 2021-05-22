using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace de.mastersign.expressions.functions
{
    internal static class Strings
    {
        public static string ToLower(string s) { return s.ToLowerInvariant(); }
        public static string ToUpper(string s) { return s.ToUpperInvariant(); }
        public static string Trim(string s) { return s.Trim(); }
        public static string TrimStart(string s) { return s.TrimStart(); }
        public static string TrimEnd(string s) { return s.TrimEnd(); }
        public static string Substr(string s, int start) { return s.Substring(start); }
        public static string Substr(string s, int start, int length) { return s.Substring(start, length); }
        public static string Remove(string s, int start) { return s.Remove(start); }
        public static string Remove(string s, int start, int length) { return s.Remove(start, length); }
        public static string Replace(string s, string oldValue, string newValue) { return s.Replace(oldValue, newValue); }

        public static int Find(string s, string value) { return s.IndexOf(value); }
        public static int Find(string s, string value, int start) { return s.IndexOf(value, start); }
        public static int Find(string s, string value, int start, int count) { return s.IndexOf(value, start, count); }

        public static int FindIgnoreCase(string s, string value) { return s.IndexOf(value, StringComparison.InvariantCultureIgnoreCase); }
        public static int FindIgnoreCase(string s, string value, int start) { return s.IndexOf(value, start, StringComparison.InvariantCultureIgnoreCase); }
        public static int FindIgnoreCase(string s, string value, int start, int count) { return s.IndexOf(value, start, count, StringComparison.InvariantCultureIgnoreCase); }

        public static int FindLast(string s, string value) { return s.LastIndexOf(value); }
        public static int FindLast(string s, string value, int start) { return s.LastIndexOf(value, start); }
        public static int FindLast(string s, string value, int start, int count) { return s.LastIndexOf(value, start, count); }

        public static int FindLastIgnoreCase(string s, string value) { return s.LastIndexOf(value, StringComparison.InvariantCultureIgnoreCase); }
        public static int FindLastIgnoreCase(string s, string value, int start) { return s.LastIndexOf(value, start, StringComparison.InvariantCultureIgnoreCase); }
        public static int FindLastIgnoreCase(string s, string value, int start, int count) { return s.LastIndexOf(value, start, count, StringComparison.InvariantCultureIgnoreCase); }

        public static bool Contains(string s, string value) { return s.Contains(value); }
        public static bool StartsWith(string s, string value) { return s.StartsWith(value); }
        public static bool EndsWith(string s, string value) { return s.EndsWith(value); }

        public static string Min(string a, string b) { return string.Compare(a, b, false, CultureInfo.InvariantCulture) > 0 ? b : a; }
        public static string Max(string a, string b) { return string.Compare(a, b, false, CultureInfo.InvariantCulture) < 0 ? b : a; }
        public static string MinIgnoreCase(string a, string b) { return string.Compare(a, b, true, CultureInfo.InvariantCulture) > 0 ? b : a; }
        public static string MaxIgnoreCase(string a, string b) { return string.Compare(a, b, true, CultureInfo.InvariantCulture) < 0 ? b : a; }

        public static string RegexMatch(string str, string regex)
        {
            var m = Regex.Match(str, regex);
            return m.Success ? m.Value : null;
        }
    }
}
