namespace WpfAnalyzers.Test
{
    using System.Collections.Generic;

    using NUnit.Framework;

    internal static class StringHelpers
    {
        internal static string AssertReplace(this string text, string oldValue, string newValue)
        {
            StringAssert.Contains(oldValue, text, $"AssertReplace failed, expected {oldValue} to be in {text}");
            return text.Replace(oldValue, newValue);
        }

        internal static IReadOnlyList<string> Lines(this string text)
        {
            return text.NormalizeNewLine().Split('\n');
        }

        internal static string Line(this string text, int line)
        {
            return text.NormalizeNewLine().Split('\n')[line - 1];
        }

        internal static string NormalizeNewLine(this string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}
