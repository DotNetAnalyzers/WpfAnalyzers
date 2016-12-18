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
            No,
            Yes,
            Maybe
        }

        internal static InvokesPropertyChanged InvokesPropertyChangedFor(this AccessorDeclarationSyntax setter, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var invokes = InvokesPropertyChanged.No;
            using (var pooled = InvocationWalker.Create(setter))
            {
                foreach (var invocation in pooled.Item.Invocations)
                {
                    var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
                    if (method == null)
                    {
                        continue;
                    }

                    if (method == KnownSymbol.PropertyChangedEventHandler.Invoke)
                    {
                        ArgumentSyntax argument;
                        if (invocation.ArgumentList.Arguments.TryGetAtIndex(1, out argument))
                        {
                            if (argument.Expression.IsCreatePropertyChangedEventArgsFor(property, semanticModel, cancellationToken))
                            {
                                return InvokesPropertyChanged.Yes;
                            }

                            var cached = semanticModel.GetSymbolSafe(argument.Expression, cancellationToken);
                            if (cached is IFieldSymbol)
                            {
                                foreach (var syntaxReference in cached.DeclaringSyntaxReferences)
                                {
                                    var declarator = syntaxReference.GetSyntax(cancellationToken) as VariableDeclaratorSyntax;
                                    if (declarator?.Initializer?.Value?.IsCreatePropertyChangedEventArgsFor(property, semanticModel, cancellationToken) == true)
                                    {
                                        return InvokesPropertyChanged.Yes;
                                    }
                                }
                            }

                            continue;
                        }
                    }

                    switch (method.InvokesPropertyChangedFor(property, invocation, semanticModel, cancellationToken))
                    {
                        case InvokesPropertyChanged.No:
                            break;
                        case InvokesPropertyChanged.Yes:
                            return InvokesPropertyChanged.Yes;
                        case InvokesPropertyChanged.Maybe:
                            if (invokes == InvokesPropertyChanged.No)
                            {
                                invokes = InvokesPropertyChanged.Maybe;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return invokes;
        }

        internal static bool TryGetInvoker(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol invoker)
        {
            invoker = null;

            foreach (var member in type.GetMembers())
            {
                var method = member as IMethodSymbol;

                if (method?.Parameters.Length != 1)
                {
                    continue;
                }

                var parameter = method.Parameters[0];

                if (method.DeclaringSyntaxReferences.Length == 0)
                {
                    if (parameter.Type == KnownSymbol.String &&
                        method.Name.Contains("PropertyChnaged"))
                    {
                        // A bit speculative here
                        // for handling the case when inheriting a ViewModelBase class from a binary reference.
                        invoker = method;
                    }

                    continue;
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
                                        invoker = method;
                                        return true;
                                    }

                                    var objectCreation = argument.Expression as ObjectCreationExpressionSyntax;
                                    if (objectCreation != null)
                                    {
                                        var nameArgument = objectCreation.ArgumentList.Arguments[0];
                                        if ((nameArgument.Expression as IdentifierNameSyntax)?.Identifier.ValueText == parameter.Name)
                                        {
                                            invoker = method;
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return invoker != null;
        }

        private static bool IsCreatePropertyChangedEventArgsFor(this ExpressionSyntax newPropertyChangedEventArgs, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var objectCreation = newPropertyChangedEventArgs as ObjectCreationExpressionSyntax;
            if (objectCreation == null)
            {
                return false;
            }

            ArgumentSyntax nameArg = null;
            if (objectCreation.ArgumentList?.Arguments.TryGetSingle(out nameArg) == true)
            {
                string name;
                if (nameArg.TryGetStringValue(semanticModel, cancellationToken, out name))
                {
                    // raising with null or empty means that all properties notifies
                    // this is how a wpf binding sees it.
                    if (string.IsNullOrEmpty(name) ||
                        name == property.Name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static InvokesPropertyChanged InvokesPropertyChangedFor(
            this IMethodSymbol method,
            IPropertySymbol property,
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (method.IsStatic)
            {
                return InvokesPropertyChanged.No;
            }

            var invokes = InvokesPropertyChanged.No;
            for (var i = 0; i < method.Parameters.Length; i++)
            {
                var parameter = method.Parameters[i];
                if (parameter.Type == KnownSymbol.String)
                {
                    ArgumentSyntax argument;
                    if (invocation.ArgumentList.Arguments.TryGetAtIndex(i, out argument))
                    {
                        string value;
                        if (argument.TryGetStringValue(semanticModel, cancellationToken, out value))
                        {
                            if (string.IsNullOrEmpty(value) ||
                                value == property.Name)
                            {
                                switch (Invokes(method, method.Parameters[i], semanticModel, cancellationToken))
                                {
                                    case InvokesPropertyChanged.No:
                                        continue;
                                    case InvokesPropertyChanged.Yes:
                                        return InvokesPropertyChanged.Yes;
                                    case InvokesPropertyChanged.Maybe:
                                        invokes = InvokesPropertyChanged.Maybe;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                        }

                        continue;
                    }

                    if (parameter.HasExplicitDefaultValue)
                    {
                        foreach (var attribute in parameter.GetAttributes())
                        {
                            if (attribute.AttributeClass == KnownSymbol.CallerMemberNameAttribute)
                            {
                                switch (Invokes(method, method.Parameters[i], semanticModel, cancellationToken))
                                {
                                    case InvokesPropertyChanged.No:
                                        continue;
                                    case InvokesPropertyChanged.Yes:
                                        return InvokesPropertyChanged.Yes;
                                    case InvokesPropertyChanged.Maybe:
                                        invokes = InvokesPropertyChanged.Maybe;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                        }
                    }
                }

                if (parameter.Type == KnownSymbol.PropertyChangedEventArgs)
                {
                    ArgumentSyntax argument;
                    if (invocation.ArgumentList.Arguments.TryGetAtIndex(i, out argument))
                    {
                        var objectCreation = argument.Expression as ObjectCreationExpressionSyntax;
                        if (objectCreation == null)
                        {
                            continue;
                        }

                        var argsArgument = objectCreation.ArgumentList.Arguments[0];
                        string name;
                        if (argsArgument.TryGetStringValue(semanticModel, cancellationToken, out name))
                        {
                            if (name == property.Name ||
                                string.IsNullOrEmpty(name))
                            {
                                switch (Invokes(method, method.Parameters[i], semanticModel, cancellationToken))
                                {
                                    case InvokesPropertyChanged.No:
                                        continue;
                                    case InvokesPropertyChanged.Yes:
                                        return InvokesPropertyChanged.Yes;
                                    case InvokesPropertyChanged.Maybe:
                                        invokes = InvokesPropertyChanged.Maybe;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                        }
                    }
                }
            }

            return invokes;
        }

        private static InvokesPropertyChanged Invokes(IMethodSymbol method, IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (method.DeclaringSyntaxReferences.Length == 0)
            {
                return InvokesPropertyChanged.Maybe;
            }

            foreach (var reference in method.DeclaringSyntaxReferences)
            {
                var methodDeclaration = (MethodDeclarationSyntax)reference.GetSyntax(cancellationToken);
                using (var pooled = InvocationWalker.Create(methodDeclaration))
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
    }
}
