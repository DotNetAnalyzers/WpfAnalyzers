namespace WpfAnalyzers.Test
{
    using NUnit.Framework;

    internal static class StringHelpers
    {
        internal static string AssertReplace(this string text, string oldValue, string newValue)
        {
            StringAssert.Contains(oldValue, text, $"AssertReplace failed, expected {oldValue} to be in {text}");
            return text.Replace(oldValue, newValue);
        }

        internal static string NormalizeNewLine(this string text)
        {
            return text.Replace("\r\n", "\n");
        }
    }
}
