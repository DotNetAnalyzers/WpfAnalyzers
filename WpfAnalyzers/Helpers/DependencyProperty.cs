namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyProperty
    {
        internal static ArgumentAndValue<string?>? TryGetRegisteredName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (Register.MatchAny(invocation, semanticModel, cancellationToken) is { } register)
            {
                if (register.NameArgument() is { } argument)
                {
                    if (argument.TryGetStringValue(semanticModel, cancellationToken, out var registeredName))
                    {
                        return new ArgumentAndValue<string?>(argument, registeredName);
                    }

                    return new ArgumentAndValue<string?>(argument, null);
                }

                return null;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (AddOwner.Match(invocation, semanticModel, cancellationToken) is { } ||
                 OverrideMetadata.Match(invocation, semanticModel, cancellationToken) is { }))
            {
                if (semanticModel.TryGetSymbol(memberAccess.Expression, cancellationToken, out var symbol) &&
                    BackingFieldOrProperty.Match(symbol) is { } backing)
                {
                    return backing.RegisteredName(semanticModel, cancellationToken);
                }

                return null;
            }

            return null;
        }

        internal readonly struct Register
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            internal Register(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal static Register? FindRecursive(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (FindRegisterInvocation(fieldOrProperty, semanticModel, cancellationToken) is { } register)
                {
                    return register;
                }

                if (fieldOrProperty.FindKey(semanticModel, cancellationToken) is { } keyField)
                {
                    return FindRecursive(keyField, semanticModel, cancellationToken);
                }

                if (fieldOrProperty.FindAddOwnerSource(semanticModel, cancellationToken) is { } addOwnerSource)
                {
                    return FindRecursive(addOwnerSource, semanticModel, cancellationToken);
                }

                return null;
            }

            internal static Register? FindRegisterInvocation(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (fieldOrProperty.Value(cancellationToken) is InvocationExpressionSyntax invocation)
                {
                    return MatchAny(invocation, semanticModel, cancellationToken);
                }

                return null;
            }

            internal static Register? MatchAny(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                return MatchRegister(invocation, semanticModel, cancellationToken) ??
                       MatchRegisterReadOnly(invocation, semanticModel, cancellationToken) ??
                       MatchRegisterAttached(invocation, semanticModel, cancellationToken) ??
                       MatchRegisterAttachedReadOnly(invocation, semanticModel, cancellationToken);
            }

            internal static Register? MatchRegister(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.Register, cancellationToken, out var method))
                {
                    return new Register(invocation, method);
                }

                return null;
            }

            internal static Register? MatchRegisterReadOnly(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterReadOnly, cancellationToken, out var method))
                {
                    return new Register(invocation, method);
                }

                return null;
            }

            internal static Register? MatchRegisterAttached(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttached, cancellationToken, out var method))
                {
                    return new Register(invocation, method);
                }

                return null;
            }

            internal static Register? MatchRegisterAttachedReadOnly(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttachedReadOnly, cancellationToken, out var method))
                {
                    return new Register(invocation, method);
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
                        if (metadataArg is { Expression: ObjectCreationExpressionSyntax objectCreation } &&
                            PropertyMetadata.Match(objectCreation, semanticModel, cancellationToken) is { DefaultValueArgument: { } defaultValue })
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

        internal readonly struct AddOwner
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            internal AddOwner(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal static AddOwner? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.AddOwner, cancellationToken, out var target))
                {
                    return new AddOwner(invocation, target);
                }

                return null;
            }
        }

        internal readonly struct OverrideMetadata
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            internal OverrideMetadata(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal ArgumentSyntax? MetadataArgument
            {
                get
                {
                    if (this.Target.TryFindParameter(KnownSymbols.PropertyMetadata, out var parameter) &&
                        this.Invocation.TryFindArgument(parameter, out var argument))
                    {
                        return argument;
                    }

                    return null;
                }
            }

            internal static OverrideMetadata? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.OverrideMetadata, cancellationToken, out var target))
                {
                    return new OverrideMetadata(invocation, target);
                }

                return null;
            }
        }
    }
}
