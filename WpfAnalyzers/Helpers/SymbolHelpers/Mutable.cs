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
                   type != KnownSymbols.Object)
            {
                foreach (var member in type.GetMembers())
                {
                    if (FieldOrProperty.TryCreate(member, out var fieldOrProperty) &&
                        !fieldOrProperty.IsStatic)
                    {
                        switch (member)
                        {
                            case IFieldSymbol field when !field.IsConst && !field.IsReadOnly:
                            case IPropertySymbol property when property.SetMethod != null:
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
