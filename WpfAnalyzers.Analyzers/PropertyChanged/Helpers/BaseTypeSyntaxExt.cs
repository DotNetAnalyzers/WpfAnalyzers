namespace WpfAnalyzers.PropertyChanged.Helpers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class BaseTypeSyntaxExt
    {
        internal static bool IsINotifyPropertyChanged(this BaseTypeSyntax baseType)
        {
            return (baseType.Type as IdentifierNameSyntax)?.Identifier.ValueText.EndsWith("INotifyPropertyChanged") == true;
        }
    }
}