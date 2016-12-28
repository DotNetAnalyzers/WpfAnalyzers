namespace WpfAnalyzers.PropertyChanged.Helpers
{
    using System;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChanged
    {
        internal enum InvokesPropertyChanged
        {
            /// <summary>
            /// Analysis determined that PropertyChanged was not invoked in this path.
            /// </summary>
            No,

            /// <summary>
            /// Analysis determined that PropertyChanged was invoked in this path.
            /// </summary>
            Yes,

            /// <summary>
            /// Analysis determined that PropertyChanged is potentially invoked in this path.
            /// </summary>
            Maybe
        }

        internal static InvokesPropertyChanged InvokesPropertyChangedFor(this SyntaxNode assignment, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var invokes = InvokesPropertyChanged.No;
            var block = assignment.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.Body ??
                        assignment.FirstAncestorOrSelf<AccessorDeclarationSyntax>()?.Body ??
                        assignment.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>()?.Body;
            if (block == null)
            {
                return InvokesPropertyChanged.No;
            }

            using (var pooled = InvocationWalker.Create(block))
            {
                foreach (var invocation in pooled.Item.Invocations)
                {
                    if (invocation.SpanStart < assignment.SpanStart)
                    {
                        continue;
                    }

                    ArgumentSyntax nameArg;
                    string propertyName;
                    switch (TryGetInvokedPropertyChangedName(invocation, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        case InvokesPropertyChanged.No:
                            continue;
                        case InvokesPropertyChanged.Yes:
                            if (string.IsNullOrEmpty(propertyName) ||
                                propertyName == property.Name)
                            {
                                return InvokesPropertyChanged.Yes;
                            }

                            continue;
                        case InvokesPropertyChanged.Maybe:
                            invokes = InvokesPropertyChanged.Maybe;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return invokes;
        }

        internal static InvokesPropertyChanged TryGetInvokedPropertyChangedName(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string propertyName)
        {
            nameArg = null;
            propertyName = null;
            var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            if (method == null)
            {
                return InvokesPropertyChanged.No;
            }

            if (method == KnownSymbol.PropertyChangedEventHandler.Invoke)
            {
                ArgumentSyntax propertyChangedArg;
                if (invocation.ArgumentList.Arguments.TryGetAtIndex(1, out propertyChangedArg))
                {
                    if (TryGetCreatePropertyChangedEventArgsFor(propertyChangedArg.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return InvokesPropertyChanged.Yes;
                    }

                    if (TryGetCachedArgs(propertyChangedArg, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return InvokesPropertyChanged.Yes;
                    }
                }

                return InvokesPropertyChanged.Maybe;
            }

            if (IsInvoker(method, semanticModel, cancellationToken) == InvokesPropertyChanged.No)
            {
                return InvokesPropertyChanged.No;
            }

            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                if (method.Parameters[0].IsCallerMemberName())
                {
                    var member = invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                    if (member == null)
                    {
                        return InvokesPropertyChanged.Maybe;
                    }

                    propertyName = semanticModel.GetDeclaredSymbolSafe(member, cancellationToken)?.Name;
                    if (propertyName != null)
                    {
                        return InvokesPropertyChanged.Yes;
                    }

                    return InvokesPropertyChanged.Maybe;
                }
            }

            ArgumentSyntax argument;
            if (invocation.ArgumentList.Arguments.TryGetSingle(out argument))
            {
                if (TryGetCreatePropertyChangedEventArgsFor(argument.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                {
                    return InvokesPropertyChanged.Yes;
                }

                var symbol = semanticModel.GetTypeInfoSafe(argument.Expression, cancellationToken).Type;
                if (symbol == KnownSymbol.String)
                {
                    if (argument.TryGetStringValue(semanticModel, cancellationToken, out propertyName))
                    {
                        nameArg = argument;
                        return InvokesPropertyChanged.Yes;
                    }

                    return InvokesPropertyChanged.Maybe;
                }

                if (symbol == KnownSymbol.PropertyChangedEventArgs)
                {
                    if (TryGetCreatePropertyChangedEventArgsFor(argument.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return InvokesPropertyChanged.Yes;
                    }

                    if (TryGetCachedArgs(argument, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return InvokesPropertyChanged.Yes;
                    }
                }
            }

            return InvokesPropertyChanged.Maybe;
        }

        internal static bool TryGetInvoker(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol invoker)
        {
            invoker = null;
            foreach (var member in type.RecursiveMembers())
            {
                var method = member as IMethodSymbol;
                switch (IsInvoker(method, semanticModel, cancellationToken))
                {
                    case InvokesPropertyChanged.No:
                        continue;
                    case InvokesPropertyChanged.Yes:
                        invoker = method;
                        return true;
                    case InvokesPropertyChanged.Maybe:
                        invoker = method;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return invoker != null;
        }

        internal static InvokesPropertyChanged IsInvoker(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (method == null ||
                method.IsStatic ||
                method.Parameters.Length != 1 ||
               (method.Parameters[0].Type != KnownSymbol.String && method.Parameters[0].Type != KnownSymbol.PropertyChangedEventArgs))
            {
                return InvokesPropertyChanged.No;
            }

            var parameter = method.Parameters[0];
            if (method.DeclaringSyntaxReferences.Length == 0)
            {
                if (parameter.Type == KnownSymbol.String &&
                    method.Name.Contains("PropertyChnaged"))
                {
                    // A bit speculative here
                    // for handling the case when inheriting a ViewModelBase class from a binary reference.
                    return InvokesPropertyChanged.Maybe;
                }

                return InvokesPropertyChanged.No;
            }

            foreach (var declaration in method.Declarations(cancellationToken))
            {
                using (var pooled = InvocationWalker.Create(declaration))
                {
                    foreach (var invocation in pooled.Item.Invocations)
                    {
                        var invokedMethod = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
                        if (invokedMethod == null)
                        {
                            continue;
                        }

                        if (invokedMethod == KnownSymbol.PropertyChangedEventHandler.Invoke)
                        {
                            ArgumentSyntax argument;
                            if (invocation.ArgumentList.Arguments.TryGetAtIndex(1, out argument))
                            {
                                var identifier = argument.Expression as IdentifierNameSyntax;
                                if (identifier?.Identifier.ValueText == parameter.Name)
                                {
                                    return InvokesPropertyChanged.Yes;
                                }

                                var objectCreation = argument.Expression as ObjectCreationExpressionSyntax;
                                if (objectCreation != null)
                                {
                                    var nameArgument = objectCreation.ArgumentList.Arguments[0];
                                    if ((nameArgument.Expression as IdentifierNameSyntax)?.Identifier.ValueText == parameter.Name)
                                    {
                                        return InvokesPropertyChanged.Yes;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return InvokesPropertyChanged.No;
        }

        internal static bool IsCallerMemberName(this IParameterSymbol parameter)
        {
            if (parameter.HasExplicitDefaultValue)
            {
                foreach (var attribute in parameter.GetAttributes())
                {
                    if (attribute.AttributeClass == KnownSymbol.CallerMemberNameAttribute)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryGetCachedArgs(
            ArgumentSyntax propertyChangedArg,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out ArgumentSyntax nameArg,
            out string propertyName)
        {
            var cached = semanticModel.GetSymbolSafe(propertyChangedArg.Expression, cancellationToken);
            if (cached is IFieldSymbol)
            {
                foreach (var syntaxReference in cached.DeclaringSyntaxReferences)
                {
                    var declarator = syntaxReference.GetSyntax(cancellationToken) as VariableDeclaratorSyntax;
                    if (TryGetCreatePropertyChangedEventArgsFor(
                        declarator?.Initializer?.Value as ObjectCreationExpressionSyntax,
                        semanticModel,
                        cancellationToken,
                        out nameArg,
                        out propertyName))
                    {
                        {
                            return true;
                        }
                    }
                }
            }

            if (cached is IPropertySymbol)
            {
                foreach (var syntaxReference in cached.DeclaringSyntaxReferences)
                {
                    var propertyDeclaration = syntaxReference.GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
                    if (TryGetCreatePropertyChangedEventArgsFor(
                        propertyDeclaration?.Initializer?.Value as ObjectCreationExpressionSyntax,
                        semanticModel,
                        cancellationToken,
                        out nameArg,
                        out propertyName))
                    {
                        {
                            return true;
                        }
                    }
                }
            }

            nameArg = null;
            propertyName = null;
            return false;
        }

        private static bool TryGetCreatePropertyChangedEventArgsFor(this ExpressionSyntax newPropertyChangedEventArgs, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string propertyName)
        {
            nameArg = null;
            propertyName = null;
            var objectCreation = newPropertyChangedEventArgs as ObjectCreationExpressionSyntax;
            if (objectCreation == null)
            {
                return false;
            }

            if (objectCreation.ArgumentList?.Arguments.TryGetSingle(out nameArg) == true)
            {
                return nameArg.TryGetStringValue(semanticModel, cancellationToken, out propertyName);
            }

            return false;
        }
    }
}
