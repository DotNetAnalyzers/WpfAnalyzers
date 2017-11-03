namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal class ValueConverter
    {
        internal static bool TryGetDefaultFieldsOrProperties(ITypeSymbol type, out IReadOnlyList<FieldOrProperty> defaults)
        {
            List<FieldOrProperty> temp = null;
            foreach (var member in type.GetMembers())
            {
                if (member.IsStatic &&
                    (member.DeclaredAccessibility == Accessibility.Public ||
                     member.DeclaredAccessibility == Accessibility.Internal))
                {
                    if (FieldOrProperty.TryCreate(member, out var fieldOrProperty) &&
                        fieldOrProperty.Type.Is(KnownSymbol.IValueConverter))
                    {
                        if (temp == null)
                        {
                            temp = new List<FieldOrProperty>();
                        }

                        temp.Add(fieldOrProperty);
                    }
                }
            }

            defaults = temp;
            return defaults != null;
        }
    }
}
