namespace WpfAnalyzers
{
    using System;

    internal static class StringHelper
    {
        internal static bool IsParts(this string text, string start, string end, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (text == null)
            {
                return start == null && end == null;
            }

            if (start == null)
            {
                return string.Equals(text, end, stringComparison);
            }

            if (end == null)
            {
                return string.Equals(text, start, stringComparison);
            }

            if (text.Length != start.Length + end.Length)
            {
                return false;
            }

            return text.StartsWith(start, stringComparison) &&
                   text.EndsWith(end, stringComparison);
        }

        internal static bool IsParts(this string text, string start, string middle, string end, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (text == null)
            {
                return start == null && middle == null && end == null;
            }

            if (start == null)
            {
                return text.IsParts(middle, end, stringComparison);
            }

            if (middle == null)
            {
                return text.IsParts(start, end, stringComparison);
            }

            if (end == null)
            {
                return text.IsParts(start, middle, stringComparison);
            }

            if (text.Length != start.Length + middle.Length + end.Length)
            {
                return false;
            }

            return text.StartsWith(start, stringComparison) &&
                   text.IndexOf(middle, start.Length, stringComparison) == start.Length &&
                   text.EndsWith(end, stringComparison);
        }
    }
}
