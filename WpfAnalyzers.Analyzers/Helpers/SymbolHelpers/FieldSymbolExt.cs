namespace WpfAnalyzers
{
    using System;

    using Microsoft.CodeAnalysis;

    internal static class FieldSymbolExt
    {
        internal static bool TryGetRegisteredType(this IFieldSymbol field, out ITypeSymbol result)
        {
            result = null;
            var indexOf = field?.Name?.LastIndexOf("Property", StringComparison.Ordinal) ?? -1;
            if (field == null ||
                field.Type?.Name != Names.DependencyProperty ||
                indexOf < 0)
            {
                return false;
            }

            var type = field.ContainingType;
            if (type.IsStatic)
            {
                // ReSharper disable once PossibleNullReferenceException
                var getMethodName = "Get" + field.Name.Substring(0, indexOf);
                var members = type.GetMembers(getMethodName);
                if (members.Length != 1)
                {
                    return false;
                }

                var property = members[0] as IMethodSymbol;
                result = property?.ReturnType;
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                var propertyName = field.Name.Substring(0, indexOf);
                var members = type.GetMembers(propertyName);
                if (members.Length != 1)
                {
                    return false;
                }

                var property = members[0] as IPropertySymbol;
                result = property?.Type;
            }

            return result != null;
        }
    }
}