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
    }
}
