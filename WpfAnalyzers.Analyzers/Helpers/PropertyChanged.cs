namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChanged
    {
        internal static AnalysisResult InvokesPropertyChangedFor(this SyntaxNode assignment, IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var invokes = AnalysisResult.No;
            var block = assignment.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.Body ??
                        assignment.FirstAncestorOrSelf<AccessorDeclarationSyntax>()?.Body ??
                        assignment.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>()?.Body;
            if (block == null)
            {
                return AnalysisResult.No;
            }

            using (var pooled = InvocationWalker.Create(block))
            {
                foreach (var invocation in pooled.Item.Invocations)
                {
                    if (invocation.SpanStart < assignment.SpanStart)
                    {
                        continue;
                    }

                    switch (TryGetInvokedPropertyChangedName(invocation, semanticModel, cancellationToken, out ArgumentSyntax _, out string propertyName))
                    {
                        case AnalysisResult.No:
                            continue;
                        case AnalysisResult.Yes:
                            if (string.IsNullOrEmpty(propertyName) ||
                                propertyName == property.Name)
                            {
                                return AnalysisResult.Yes;
                            }

                            continue;
                        case AnalysisResult.Maybe:
                            invokes = AnalysisResult.Maybe;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return invokes;
        }

        internal static AnalysisResult TryGetInvokedPropertyChangedName(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string propertyName)
        {
            nameArg = null;
            propertyName = null;
            var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            if (method == null)
            {
                return AnalysisResult.No;
            }

            if (method == KnownSymbol.PropertyChangedEventHandler.Invoke)
            {
                if (invocation.ArgumentList.Arguments.TryGetAtIndex(1, out ArgumentSyntax propertyChangedArg))
                {
                    if (TryGetCreatePropertyChangedEventArgsFor(propertyChangedArg.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return AnalysisResult.Yes;
                    }

                    if (TryGetCachedArgs(propertyChangedArg, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return AnalysisResult.Yes;
                    }
                }

                return AnalysisResult.Maybe;
            }

            if (IsInvoker(method, semanticModel, cancellationToken) == AnalysisResult.No)
            {
                return AnalysisResult.No;
            }

            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                if (method.Parameters[0].IsCallerMemberName())
                {
                    var member = invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                    if (member == null)
                    {
                        return AnalysisResult.Maybe;
                    }

                    propertyName = semanticModel.GetDeclaredSymbolSafe(member, cancellationToken)?.Name;
                    if (propertyName != null)
                    {
                        return AnalysisResult.Yes;
                    }

                    return AnalysisResult.Maybe;
                }
            }

            if (invocation.ArgumentList.Arguments.TryGetSingle(out ArgumentSyntax argument))
            {
                if (TryGetCreatePropertyChangedEventArgsFor(argument.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                {
                    return AnalysisResult.Yes;
                }

                var symbol = semanticModel.GetTypeInfoSafe(argument.Expression, cancellationToken).Type;
                if (symbol == KnownSymbol.String)
                {
                    if (argument.TryGetStringValue(semanticModel, cancellationToken, out propertyName))
                    {
                        nameArg = argument;
                        return AnalysisResult.Yes;
                    }

                    return AnalysisResult.Maybe;
                }

                if (symbol == KnownSymbol.PropertyChangedEventArgs)
                {
                    if (TryGetCreatePropertyChangedEventArgsFor(argument.Expression as ObjectCreationExpressionSyntax, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return AnalysisResult.Yes;
                    }

                    if (TryGetCachedArgs(argument, semanticModel, cancellationToken, out nameArg, out propertyName))
                    {
                        return AnalysisResult.Yes;
                    }
                }
            }

            return AnalysisResult.Maybe;
        }

        internal static bool TryGetInvoker(ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol invoker)
        {
            invoker = null;
            foreach (var member in type.RecursiveMembers())
            {
                var method = member as IMethodSymbol;
                switch (IsInvoker(method, semanticModel, cancellationToken))
                {
                    case AnalysisResult.No:
                        continue;
                    case AnalysisResult.Yes:
                        invoker = method;
                        return true;
                    case AnalysisResult.Maybe:
                        invoker = method;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return invoker != null;
        }

        internal static AnalysisResult IsInvoker(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken, HashSet<IMethodSymbol> @checked = null)
        {
            if (@checked?.Add(method) == false)
            {
                return AnalysisResult.No;
            }

            if (method == null ||
                method.IsStatic ||
                method.Parameters.Length != 1 ||
                method.AssociatedSymbol != null ||
               (method.Parameters[0].Type != KnownSymbol.String && method.Parameters[0].Type != KnownSymbol.PropertyChangedEventArgs))
            {
                return AnalysisResult.No;
            }

            var parameter = method.Parameters[0];
            if (method.DeclaringSyntaxReferences.Length == 0)
            {
                if (parameter.Type == KnownSymbol.String &&
                    method.Name.Contains("PropertyChanged"))
                {
                    // A bit speculative here
                    // for handling the case when inheriting a ViewModelBase class from a binary reference.
                    return AnalysisResult.Maybe;
                }

                return AnalysisResult.No;
            }

            foreach (var declaration in method.Declarations(cancellationToken))
            {
                using (var pooled = InvocationWalker.Create(declaration))
                {
                    foreach (var invocation in pooled.Item.Invocations)
                    {
                        if (invocation.ArgumentList == null ||
                            invocation.ArgumentList.Arguments.Count == 0)
                        {
                            continue;
                        }

                        var invokedMethod = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
                        if (invokedMethod == null)
                        {
                            continue;
                        }

                        if ((invocation.Expression is MemberAccessExpressionSyntax memberAccess && !(memberAccess.Expression is ThisExpressionSyntax)) ||
                            (invocation.Expression is MemberBindingExpressionSyntax))
                        {
                            if (invokedMethod == KnownSymbol.PropertyChangedEventHandler.Invoke)
                            {
                                if (invocation.ArgumentList.Arguments.TryGetAtIndex(1, out ArgumentSyntax argument))
                                {
                                    var identifier = argument.Expression as IdentifierNameSyntax;
                                    if (identifier?.Identifier.ValueText == parameter.Name)
                                    {
                                        return AnalysisResult.Yes;
                                    }

                                    if (argument.Expression is ObjectCreationExpressionSyntax objectCreation)
                                    {
                                        var nameArgument = objectCreation.ArgumentList.Arguments[0];
                                        if ((nameArgument.Expression as IdentifierNameSyntax)?.Identifier.ValueText == parameter.Name)
                                        {
                                            return AnalysisResult.Yes;
                                        }
                                    }
                                }
                            }

                            return AnalysisResult.No;
                        }

                        if (invokedMethod == KnownSymbol.PropertyChangedEventHandler.Invoke)
                        {
                            if (invocation.ArgumentList.Arguments.TryGetAtIndex(1, out ArgumentSyntax argument))
                            {
                                var identifier = argument.Expression as IdentifierNameSyntax;
                                if (identifier?.Identifier.ValueText == parameter.Name)
                                {
                                    return AnalysisResult.Yes;
                                }

                                if (argument.Expression is ObjectCreationExpressionSyntax objectCreation)
                                {
                                    var nameArgument = objectCreation.ArgumentList.Arguments[0];
                                    if ((nameArgument.Expression as IdentifierNameSyntax)?.Identifier.ValueText == parameter.Name)
                                    {
                                        return AnalysisResult.Yes;
                                    }
                                }
                            }

                            return AnalysisResult.No;
                        }

                        if (!method.ContainingType.Is(invokedMethod.ContainingType))
                        {
                            continue;
                        }

                        using (var argsWalker = ArgumentsWalker.Create(invocation.ArgumentList))
                        {
                            if (argsWalker.Item.Contains(parameter, semanticModel, cancellationToken))
                            {
                                if (@checked == null)
                                {
                                    using (var pooledSet = SetPool<IMethodSymbol>.Create())
                                    {
                                        return IsInvoker(invokedMethod, semanticModel, cancellationToken, pooledSet.Item);
                                    }
                                }

                                return IsInvoker(invokedMethod, semanticModel, cancellationToken, @checked);
                            }
                        }
                    }
                }
            }

            return AnalysisResult.No;
        }

        internal static bool IsCallerMemberName(this IMethodSymbol invoker)
        {
            return invoker != null &&
                   invoker.Parameters.Length == 1 &&
                   invoker.Parameters[0].Type == KnownSymbol.String &&
                   invoker.Parameters[0].IsCallerMemberName();
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

        internal static bool IsNotifyPropertyChanged(StatementSyntax statement, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var expressionStatement = statement as ExpressionStatementSyntax;
            var expression = expressionStatement?.Expression;
            if (expression == null)
            {
                return false;
            }

            if (expression is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                return IsNotifyPropertyChanged(conditionalAccess.WhenNotNull as InvocationExpressionSyntax, semanticModel, cancellationToken);
            }

            return IsNotifyPropertyChanged(expression as InvocationExpressionSyntax, semanticModel, cancellationToken);
        }

        internal static bool IsNotifyPropertyChanged(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation == null)
            {
                return false;
            }

            var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            return method == KnownSymbol.PropertyChangedEventHandler.Invoke ||
                   IsInvoker(method, semanticModel, cancellationToken) != AnalysisResult.No;
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

        private sealed class ArgumentsWalker : CSharpSyntaxWalker
        {
            private static readonly Pool<ArgumentsWalker> Cache = new Pool<ArgumentsWalker>(
                () => new ArgumentsWalker(),
                x => x.identifierNames.Clear());

            private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();

            private ArgumentsWalker()
            {
            }

            public IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

            public static Pool<ArgumentsWalker>.Pooled Create(ArgumentListSyntax arguments)
            {
                var pooled = Cache.GetOrCreate();
                pooled.Item.Visit(arguments);
                return pooled;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                this.identifierNames.Add(node);
                base.VisitIdentifierName(node);
            }

            public bool Contains(IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                foreach (var identifierName in this.IdentifierNames)
                {
                    var symbol = semanticModel.GetSymbolSafe(identifierName, cancellationToken) as IParameterSymbol;
                    if (parameter.MetadataName == symbol?.MetadataName)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
