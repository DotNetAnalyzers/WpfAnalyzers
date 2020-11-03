namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct RegisterInvocation
    {
        internal readonly InvocationExpressionSyntax Invocation;
        internal readonly IMethodSymbol Method;

        internal RegisterInvocation(InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            this.Invocation = invocation;
            this.Method = method;
        }

        internal static bool TryFindRecursive(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out RegisterInvocation result)
        {
            if (DependencyProperty.TryGetDependencyPropertyKeyFieldOrProperty(fieldOrProperty, semanticModel, cancellationToken, out var keyField))
            {
                return TryFindRecursive(keyField, semanticModel, cancellationToken, out result);
            }

            if (DependencyProperty.TryGetDependencyAddOwnerSourceField(fieldOrProperty, semanticModel, cancellationToken, out var addOwnerSource))
            {
                return TryFindRecursive(addOwnerSource, semanticModel, cancellationToken, out result);
            }

            return TryGetRegisterInvocation(fieldOrProperty, semanticModel, cancellationToken, out result);
        }

        internal static bool TryGetRegisterInvocation(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out RegisterInvocation result)
        {
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation)
            {
                if (TryMatchRegister(invocation, semanticModel, cancellationToken, out result) ||
                    TryMatchRegisterReadOnly(invocation, semanticModel, cancellationToken, out result) ||
                    TryMatchRegisterAttached(invocation, semanticModel, cancellationToken, out result) ||
                    TryMatchRegisterAttachedReadOnly(invocation, semanticModel, cancellationToken, out result))
                {
                    return true;
                }
            }

            result = default;
            return false;
        }

        internal static bool TryMatchRegisterAny(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out RegisterInvocation result)
        {
            return TryMatchRegister(invocation, semanticModel, cancellationToken, out result) ||
                   TryMatchRegisterReadOnly(invocation, semanticModel, cancellationToken, out result) ||
                   TryMatchRegisterAttached(invocation, semanticModel, cancellationToken, out result) ||
                   TryMatchRegisterAttachedReadOnly(invocation, semanticModel, cancellationToken, out result);
        }

        internal static bool TryMatchRegister(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out RegisterInvocation result)
        {
            if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.Register, cancellationToken, out var method))
            {
                result = new RegisterInvocation(invocation, method);
                return true;
            }

            result = default;
            return false;
        }

        internal static bool TryMatchRegisterReadOnly(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out RegisterInvocation result)
        {
            if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterReadOnly, cancellationToken, out var method))
            {
                result = new RegisterInvocation(invocation, method);
                return true;
            }

            result = default;
            return false;
        }

        internal static bool TryMatchRegisterAttached(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out RegisterInvocation result)
        {
            if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttached, cancellationToken, out var method))
            {
                result = new RegisterInvocation(invocation, method);
                return true;
            }

            result = default;
            return false;
        }

        internal static bool TryMatchRegisterAttachedReadOnly(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out RegisterInvocation result)
        {
            if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttachedReadOnly, cancellationToken, out var method))
            {
                result = new RegisterInvocation(invocation, method);
                return true;
            }

            result = default;
            return false;
        }

        internal ArgumentSyntax? NameArgument() => this.FindArgument("name");

        internal ArgumentSyntax? PropertyTypeArgument() => this.FindArgument("propertyType");

        internal ArgumentSyntax? FindArgument(string name)
        {
            if (this.Method.TryFindParameter(name, out var parameter))
            {
                return this.Invocation.FindArgument(parameter);
            }

            return null;
        }

        internal ArgumentSyntax? FindArgument(QualifiedType type)
        {
            if (this.Method.TryFindParameter(type, out var parameter))
            {
                return this.Invocation.FindArgument(parameter);
            }

            return null;
        }

        internal ITypeSymbol? PropertyType(INamedTypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (this.PropertyTypeArgument() is { Expression: TypeOfExpressionSyntax { Type: { } typeSyntax } } &&
                TypeSymbol.TryGet(typeSyntax, containingType, semanticModel, cancellationToken) is { } type)
            {
                if (type.IsReferenceType &&
                    (semanticModel.GetNullableContext(this.Invocation.SpanStart) & NullableContext.Enabled) == NullableContext.Enabled &&
                    IsDefaultValueNull(this.FindArgument(KnownSymbols.PropertyMetadata)))
                {
                    return type.WithNullableAnnotation(NullableAnnotation.Annotated);
                }

                return type;

                bool IsDefaultValueNull(ArgumentSyntax? metadataArg)
                {
                    if (metadataArg is { Expression: ObjectCreationExpressionSyntax propertyMetaData } &&
                        PropertyMetadata.TryGetDefaultValue(propertyMetaData, semanticModel, cancellationToken, out var defaultValue))
                    {
                        return defaultValue switch
                        {
                            { Expression: DefaultExpressionSyntax _ } => true,
                            { Expression: LiteralExpressionSyntax { Token: { ValueText: "null" } } } => true,
                            { Expression: CastExpressionSyntax { Expression: LiteralExpressionSyntax { Token: { ValueText: "null" } } } } => true,
                            _ => false,
                        };
                    }

                    return true;
                }
            }

            return null;
        }

        internal string? PropertyName(SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return this.NameArgument() is { } argument &&
                   argument.TryGetStringValue(semanticModel, cancellationToken, out var name)
                ? name
                : null;
        }
    }
}
