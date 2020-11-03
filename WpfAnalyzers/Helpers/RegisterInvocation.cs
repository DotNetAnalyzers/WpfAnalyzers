namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct RegisterInvocation
    {
        internal readonly InvocationExpressionSyntax Invocation;
        internal readonly IMethodSymbol Target;

        internal RegisterInvocation(InvocationExpressionSyntax invocation, IMethodSymbol target)
        {
            this.Invocation = invocation;
            this.Target = target;
        }

        internal static RegisterInvocation? FindRecursive(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (DependencyProperty.TryGetDependencyPropertyKeyFieldOrProperty(fieldOrProperty, semanticModel, cancellationToken, out var keyField))
            {
                return FindRecursive(keyField, semanticModel, cancellationToken);
            }

            if (DependencyProperty.TryGetDependencyAddOwnerSourceField(fieldOrProperty, semanticModel, cancellationToken, out var addOwnerSource))
            {
                return FindRecursive(addOwnerSource, semanticModel, cancellationToken);
            }

            return FindRegisterInvocation(fieldOrProperty, semanticModel, cancellationToken);
        }

        internal static RegisterInvocation? FindRegisterInvocation(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation)
            {
                return MatchRegisterAny(invocation, semanticModel, cancellationToken);
            }

            return null;
        }

        internal static RegisterInvocation? MatchRegisterAny(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return MatchRegister(invocation, semanticModel, cancellationToken) ??
                   MatchRegisterReadOnly(invocation, semanticModel, cancellationToken) ??
                   MatchRegisterAttached(invocation, semanticModel, cancellationToken) ??
                   MatchRegisterAttachedReadOnly(invocation, semanticModel, cancellationToken);
        }

        internal static RegisterInvocation? MatchRegister(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.Register, cancellationToken, out var method))
            {
                return new RegisterInvocation(invocation, method);
            }

            return null;
        }

        internal static RegisterInvocation? MatchRegisterReadOnly(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterReadOnly, cancellationToken, out var method))
            {
                return new RegisterInvocation(invocation, method);
            }

            return null;
        }

        internal static RegisterInvocation? MatchRegisterAttached(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttached, cancellationToken, out var method))
            {
                return new RegisterInvocation(invocation, method);
            }

            return null;
        }

        internal static RegisterInvocation? MatchRegisterAttachedReadOnly(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttachedReadOnly, cancellationToken, out var method))
            {
                return new RegisterInvocation(invocation, method);
            }

            return null;
        }

        internal ArgumentSyntax? NameArgument() => this.FindArgument("name");

        internal ArgumentSyntax? PropertyTypeArgument() => this.FindArgument("propertyType");

        internal ArgumentSyntax? FindArgument(string name)
        {
            if (this.Target.TryFindParameter(name, out var parameter))
            {
                return this.Invocation.FindArgument(parameter);
            }

            return null;
        }

        internal ArgumentSyntax? FindArgument(QualifiedType type)
        {
            if (this.Target.TryFindParameter(type, out var parameter))
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
