namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;

    internal static class Mutable
    {
        internal static bool HasMutableInstanceMembers(ITypeSymbol type)
        {
            if (type == null)
            {
                return false;
            }

            while (type != null &&
                   type != KnownSymbol.Object)
            {
                foreach (var member in type.GetMembers())
                {
                    if (member is IFieldSymbol field &&
                        !field.IsStatic &&
                        !field.IsConst &&
                        !field.IsReadOnly)
                    {
                        return true;
                    }

                    if (member is IPropertySymbol property &&
                        !property.IsStatic &&
                        !property.IsGetOnly())
                    {
                        return true;
                    }
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
