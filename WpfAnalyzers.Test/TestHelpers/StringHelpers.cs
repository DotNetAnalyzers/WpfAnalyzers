namespace WpfAnalyzers.Test
{
    using System.Collections.Generic;

    internal static class StringHelpers
    {
        internal static IReadOnlyList<string> Lines(this string text)
        {
            return text.NormalizeNewLine().Split('\n');
        }

        internal static string NormalizeNewLine(this string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}
