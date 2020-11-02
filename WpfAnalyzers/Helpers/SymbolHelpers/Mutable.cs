namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;

    internal static class Mutable
    {
        internal static bool HasMutableInstanceMembers(ITypeSymbol type)
        {
            if (type is null)
            {
                return false;
            }

            while (type is { } &&
                   type != KnownSymbols.Object)
            {
                foreach (var member in type.GetMembers())
                {
                    if (FieldOrProperty.TryCreate(member, out var fieldOrProperty) &&
                        !fieldOrProperty.IsStatic)
                    {
                        switch (member)
                        {
                            case IFieldSymbol { IsConst: false, IsReadOnly: false }:
                            case IPropertySymbol { SetMethod: { } }:
                                return true;
                        }

                        if (fieldOrProperty.Type.Is(KnownSymbols.IEnumerable) &&
                            fieldOrProperty.Type.TryFindFirstMethod("Add", out _))
                        {
                            return true;
                        }
                    }
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
